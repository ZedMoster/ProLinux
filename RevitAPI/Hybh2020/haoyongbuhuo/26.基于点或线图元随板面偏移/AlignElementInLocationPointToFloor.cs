using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Linq;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class AlignElementInLocationPointToFloor : IExternalCommand
    {
        readonly RegistryStorage Registry = new RegistryStorage();
        public XYZ P0 { get; set; }
        public bool SelectFace { get; set; }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            // 程序是否注册可运行
            if (Run.Running(Strings.key))
            {
                #region 选择构件
                // 创建一个梁列表
                List<Element> elementSel = new List<Element>();
                // 选择过滤器
                var newfilter = new PickByCategorySelectionFilter();

                // 判断当前视口是否已选中构件
                if (sel.GetElementIds().Count != 0)
                {
                    foreach (var item in sel.GetElementIds())
                    {
                        Element el = doc.GetElement(item);
                        if (el.Location is LocationPoint)
                        {
                            elementSel.Add(el);
                        }
                    }
                }
                else
                {
                    // 右键完成多选
                    ToFinish toFinish = new ToFinish();
                    // 选择基于定位点的构件（不包含：结构柱、柱）
                    PickByLocationPointSelectionFilter pickByLocationPoint = new PickByLocationPointSelectionFilter();
                    try
                    {
                        toFinish.Subscribe();
                        SelPick.SelRefsList = sel.PickObjects(ObjectType.Element, pickByLocationPoint, "<选择需要对其板面的基于点的构件>").ToList();
                        toFinish.Unsubscribe();
                    }
                    catch { SelPick.SelRefsList = new List<Reference>(); }

                    if (SelPick.SelRefsList.Count == 0)
                    {
                        return Result.Cancelled;
                    }
                    foreach (var elId in SelPick.SelRefsList)
                    {
                        Element el = doc.GetElement(elId);
                        elementSel.Add(el);
                    }
                }
                // 判断是列表是否存在 图元
                if (elementSel.Count == 0)
                {
                    return Result.Failed;
                }

                // 选择平面
                newfilter.CategoryName = "楼板";
                try
                {
                    SelPick.SelRef = sel.PickObject(ObjectType.Element, newfilter, "<选择构件需对齐到的楼板>");
                }
                catch { }
                if (SelPick.SelRef == null)
                {
                    return Result.Cancelled;
                }
                #endregion

                var el_B = doc.GetElement(SelPick.SelRef);
                var levelId = el_B.LevelId;
                // 模型显示颜色
                ColorWithModel colorWithModel = new ColorWithModel();

                // 设置事务组
                TransactionGroup T = new TransactionGroup(doc);
                T.Start("基于点图元调整");

                // 创建一个存储计算错误构件的 列表ids
                List<ElementId> elementError = new List<ElementId>();
                var readOffset = Registry.OpenAfterStart("OffsetValue") ?? "0";
                double.TryParse(readOffset, out double offset);

                foreach (var el in elementSel)
                {
                    #region 更新el 标高
                    Transaction tLevel = new Transaction(doc, "更新标高");
                    try
                    {
                        tLevel.Start();
                        el.get_Parameter(BuiltInParameter.FAMILY_LEVEL_PARAM).Set(levelId);
                        tLevel.Commit();
                    }
                    catch { tLevel.RollBack(); }
                    #endregion

                    try
                    {
                        // 获取结构柱的定位点
                        P0 = (el.Location as LocationPoint).Point;
                    }
                    catch { }

                    if (P0 == null)
                    {
                        elementError.Add(el.Id);
                        continue;
                    }

                    Transaction t = new Transaction(doc);
                    t.Start("计算距离");
                    // 关闭警告
                    FailureHandlingOptions fho = t.GetFailureHandlingOptions();
                    fho.SetFailuresPreprocessor(new FailuresPreprocessor());
                    t.SetFailureHandlingOptions(fho);

                    #region 计算垂直方向
                    PointDistanceSolidFace pointDistanceSolidFace = new PointDistanceSolidFace();
                    var dir = new XYZ(0, 0, 1);
                    // 点的投影结果 IntersectionResult  false：楼板顶面
                    var index = Registry.OpenAfterStart("FloorFaceIndex") ?? "0";
                    if (index == "0")
                    {
                        SelectFace = false;
                    }
                    else
                    {
                        SelectFace = true;
                    }
                    var result0 = pointDistanceSolidFace.GetPointDistanceFace(el_B, P0, dir, SelectFace);
                    #endregion

                    #region 投影结果
                    if (result0 != null)
                    {
                        // 自标高的高度偏移  偏移：INSTANCE_FREE_HOST_OFFSET_PARAM
                        var _Parameter = el.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM);

                        #region false-0-对齐板面
                        if (SelectFace)
                        {
                            #region 对齐底面
                            if (result0.PointZ > P0.Z)
                            {
                                // 在底下的情况 + 距离大的值
                                _Parameter.Set(_Parameter.AsDouble() + result0.Distance - offset / 304.8);
                            }
                            else
                            {
                                // 在上方的情况 - 距离小的值
                                _Parameter.Set(_Parameter.AsDouble() - result0.Distance - offset / 304.8);
                            }
                            #endregion
                        }
                        else
                        {
                            #region 对齐顶面
                            if (result0.PointZ > P0.Z)
                            {
                                // 在底下的情况 + 距离大的值
                                _Parameter.Set(_Parameter.AsDouble() + result0.Distance + offset / 304.8);
                            }
                            else
                            {
                                // 在上方的情况 - 距离小的值
                                _Parameter.Set(_Parameter.AsDouble() - result0.Distance + offset / 304.8);
                            }
                            #endregion
                        }
                        #endregion

                        // 恢复图形当前视图替换
                        var overrideGraphicSettings = new OverrideGraphicSettings();
                        var activeView = uidoc.ActiveView;
                        // 图形替换恢复
                        activeView.SetElementOverrides(el.Id, overrideGraphicSettings);

                        // 完成 提交事务
                        t.Commit();
                    }
                    else
                    {
                        // 错误
                        t.RollBack();
                        //Color color = new Color(255, 69, 0);
                        // 构件添加颜色
                        GetRegistryBackgroundColor backgroundColor = new GetRegistryBackgroundColor
                        {
                            // No -- 橘色
                            _r = 255,
                            _g = 70,
                            _b = 0
                        };

                        Color color = backgroundColor.BackgroundColor("F1_r", "F1_g", "F1_b");
                        // 设置注释参数 可空
                        //var value = "梁参数计算错误";
                        // 计算失败 标记颜色
                        colorWithModel.ElementSetError(uidoc, doc, el, color);
                        // 计算错误的构件
                        elementError.Add(el.Id);
                    }
                    #endregion
                }
                // 正常运行
                T.Assimilate();
                // UI 选中运行完成的构件
                sel.SetElementIds(elementError);
            }
            return Result.Succeeded;
        }
    }
}
