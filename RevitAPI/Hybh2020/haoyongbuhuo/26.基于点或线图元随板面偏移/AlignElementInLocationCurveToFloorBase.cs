using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Linq;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class AlignElementInLocationCurveToFloorBase : IExternalCommand
    {
        public XYZ P0 { get; set; }
        public XYZ P1 { get; set; }
        public XYZ IntersectPoint { get; set; }
        public double Distance01 { get; set; }
        public double Distance02 { get; set; }
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
                        if (el.Location is LocationCurve)
                        {
                            elementSel.Add(el);
                        }
                    }
                }
                else
                {
                    // 右键完成多选
                    ToFinish toFinish = new ToFinish();
                    // 选择基于定位线的构件（不包含：墙）
                    PickByLocationCurveSelectionFilter pickByLocationCurve = new PickByLocationCurveSelectionFilter();
                    try
                    {
                        toFinish.Subscribe();
                        SelPick.SelRefsList = sel.PickObjects(ObjectType.Element, pickByLocationCurve, "<选择需要对其板面的基于线的构件>").ToList();
                        toFinish.Unsubscribe();
                    }
                    catch { }

                    if (SelPick.SelRefsList.Count == 0)
                    {
                        return Result.Failed;
                    }
                    foreach (var elId in SelPick.SelRefsList)
                    {
                        Element el = doc.GetElement(elId);
                        elementSel.Add(el);
                    }
                }
                // 判断是列表是否存在 结构柱
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
                    return Result.Failed;
                }
                #endregion

                var el_B = doc.GetElement(SelPick.SelRef);
                // 注册列表操作
                RegistryStorage Registry = new RegistryStorage();
                var dis = Registry.OpenAfterStart("OffsetValue") ?? "0";
                var dia = double.TryParse(dis, out double Distan);
                if (!dia)
                {
                    TaskDialog.Show("error", "参数设置错误！");
                    return Result.Failed;
                }
                // 模型显示颜色
                ColorWithModel colorWithModel = new ColorWithModel();

                // 设置事务组
                TransactionGroup T = new TransactionGroup(doc);
                T.Start("基于线图元调整");

                // 创建一个存储计算错误构件的 列表ids
                List<ElementId> elementError = new List<ElementId>();

                foreach (var el in elementSel)
                {
                    try
                    {
                        // 获取结构柱的定位点
                        var curve = (el.Location as LocationCurve).Curve;
                        P0 = curve.GetEndPoint(0);
                        P1 = curve.GetEndPoint(1);
                    }
                    catch { }

                    if (P0 == null && P1 == null)
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

                    // 偏移
                    var OffsetParameter = el.get_Parameter(BuiltInParameter.RBS_OFFSET_PARAM);
                    //var StartOffset = el.get_Parameter(BuiltInParameter.RBS_START_OFFSET_PARAM).AsDouble();   // 开始偏移
                    //var EndOffset = el.get_Parameter(BuiltInParameter.RBS_END_OFFSET_PARAM).AsDouble();       // 端点偏移
                    if (OffsetParameter == null)
                    {
                        elementError.Add(el.Id);
                        continue;
                    }

                    #region 计算垂直方向
                    PointDistanceSolidFace pointDistanceSolidFace = new PointDistanceSolidFace();
                    var dir = new XYZ(0, 0, 1);
                    // 点的投影结果 IntersectionResult  true:楼板底面
                    var result0 = pointDistanceSolidFace.GetPointDistanceFace(el_B, P0, dir, true);
                    var result1 = pointDistanceSolidFace.GetPointDistanceFace(el_B, P1, dir, true);
                    #endregion

                    #region 投影结果
                    if (result0 != null || result1 != null)
                    {
                        try
                        {
                            Distance01 = result0.Distance;
                        }
                        catch { Distance01 = 0; }
                        try
                        {
                            Distance02 = result1.Distance;
                        }
                        catch { Distance02 = 0; }
                        // 至少存在一个交点的情况
                        if (Distance01 < Distance02 && Distance01 != 0)
                        {
                            IntersectPoint = result1.IntersectPoint;
                        }
                        else
                        {
                            IntersectPoint = result0.IntersectPoint;
                        }
                        // 设置梁参数
                        if (IntersectPoint.Z < P0.Z)
                        {
                            if (Distance01 != 0)
                            {
                                OffsetParameter.Set(OffsetParameter.AsDouble() - Distance01 + Distan / 304.8);
                            }
                            else
                            {
                                OffsetParameter.Set(OffsetParameter.AsDouble() - Distance02 + Distan / 304.8);
                            }
                        }
                        else
                        {
                            if (Distance01 != 0)
                            {
                                OffsetParameter.Set(Distance01 + OffsetParameter.AsDouble() - Distan / 304.8);
                            }
                            else
                            {
                                OffsetParameter.Set(Distance02 + OffsetParameter.AsDouble() - Distan / 304.8);
                            }
                        }

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