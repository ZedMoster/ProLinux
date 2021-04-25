using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Linq;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class AlignElementInLocationCurveToLinkFloor : IExternalCommand
    {
        public XYZ Point01 { get; set; }
        public XYZ Point02 { get; set; }
        public XYZ IntersectPoint { get; set; }
        public double Distance01 { get; set; }
        public double Distance02 { get; set; }
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
                // 判断是列表是否存在 结构柱
                if (elementSel.Count == 0)
                {
                    return Result.Failed;
                }

                try
                {
                    // 选择平面
                    SelectionlinkFilter selectionlink = new SelectionlinkFilter
                    {
                        ListCategoryName = new List<string>() { "楼板" }
                    };
                    SelPick.SelRef = sel.PickObject(ObjectType.LinkedElement, selectionlink, "<选择构件需对齐到的楼板>");
                }
                catch { }
                if (SelPick.SelRef == null)
                {
                    return Result.Cancelled;
                }
                #endregion

                // 获取链接文件的 doc - el
                RevitLinkInstance linkIns = doc.GetElement(SelPick.SelRef) as RevitLinkInstance;
                Document linkdoc = linkIns.GetLinkDocument();
                var el_B = linkdoc.GetElement(SelPick.SelRef.LinkedElementId);
                #region 读取参数获取构件信息

                // 注册列表操作
                RegistryStorage Registry = new RegistryStorage();
                var dis = Registry.OpenAfterStart("OffsetValue") ?? "0";
                var dia = double.TryParse(dis, out double Distan);
                if (!dia)
                {
                    TaskDialog.Show("提示", "参数设置错误！");
                    return Result.Failed;
                }
                var index = Registry.OpenAfterStart("FloorFaceIndex") ?? "0";
                if (index == "0")
                {
                    SelectFace = false;  // 顶面
                }
                else
                {
                    SelectFace = true;  // 底面
                }
                // 模型显示颜色
                ColorWithModel colorWithModel = new ColorWithModel();
                #endregion

                // 设置事务组
                TransactionGroup T = new TransactionGroup(doc);
                T.Start("基于线图元调整");

                // 创建一个存储计算错误构件的 列表ids
                List<ElementId> elementError = new List<ElementId>();

                foreach (var el in elementSel)
                {
                    #region 更新el 参照标高
                    //Transaction tLevel = new Transaction(doc, "更新参照标高");
                    //try
                    //{
                    //    tLevel.Start();
                    //    el.get_Parameter(BuiltInParameter.RBS_START_LEVEL_PARAM).Set(levelId);
                    //    tLevel.Commit();
                    //}
                    //catch { tLevel.RollBack(); }
                    #endregion

                    try
                    {
                        // 获取结构柱的定位点
                        var curve = (el.Location as LocationCurve).Curve;
                        Point01 = curve.GetEndPoint(0);
                        Point02 = curve.GetEndPoint(1);
                        //TaskDialog.Show("res", string.Format("P0：  {0}\nP1：  {0}", P0.ToString(), P1.ToString()));
                    }
                    catch { }

                    if (Point01 == null && Point02 == null)
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
                    // 点的投影结果 IntersectionResult  false:楼板顶面
                    var result0 = pointDistanceSolidFace.GetPointDistanceFace(el_B, Point01, dir, SelectFace);
                    var result1 = pointDistanceSolidFace.GetPointDistanceFace(el_B, Point02, dir, SelectFace);
                    #endregion

                    #region 投影结果 (判断交点个数[2, 1, 0]三种情况，判断位于面的上方还是下方，判断是顶面还是底面。应用不同的计算规则)
                    if (result0 != null && result1 != null)
                    {
                        if (SelectFace)
                        {
                            #region 2: 对齐底面
                            if (result0.PointZ < result1.PointZ)
                            {
                                // 判断当前构件在与相交点的位置关系
                                if (result0.PointZ > Point01.Z)
                                {
                                    // 在底下的情况 + 距离大的值
                                    OffsetParameter.Set(OffsetParameter.AsDouble() + result0.Distance - Distan / 304.8);
                                }
                                else
                                {
                                    // 在上方的情况 - 距离小的值
                                    OffsetParameter.Set(OffsetParameter.AsDouble() - result0.Distance - Distan / 304.8);
                                }
                            }
                            else
                            {
                                // 判断当前构件在与相交点的位置关系
                                if (result1.PointZ > Point02.Z)
                                {
                                    // 在底下的情况 + 距离大的值
                                    OffsetParameter.Set(OffsetParameter.AsDouble() + result1.Distance - Distan / 304.8);
                                }
                                else
                                {
                                    // 在上方的情况 - 距离小的值
                                    OffsetParameter.Set(OffsetParameter.AsDouble() - result1.Distance - Distan / 304.8);
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            #region 2: 对齐顶面
                            if (result0.PointZ < result1.PointZ)
                            {
                                // 判断当前构件在与相交点的位置关系
                                if (result1.PointZ > Point02.Z)
                                {
                                    // 在底下的情况 + 距离大的值
                                    OffsetParameter.Set(OffsetParameter.AsDouble() + result1.Distance + Distan / 304.8);
                                }
                                else
                                {
                                    // 在上方的情况 - 距离小的值
                                    OffsetParameter.Set(OffsetParameter.AsDouble() - result1.Distance + Distan / 304.8);
                                }
                            }
                            else
                            {
                                // 判断当前构件在与相交点的位置关系
                                if (result0.PointZ > Point01.Z)
                                {
                                    // 在底下的情况 + 距离大的值
                                    OffsetParameter.Set(OffsetParameter.AsDouble() + result0.Distance + Distan / 304.8);
                                }
                                else
                                {
                                    // 在上方的情况 - 距离小的值
                                    OffsetParameter.Set(OffsetParameter.AsDouble() - result0.Distance + Distan / 304.8);
                                }
                            }
                            #endregion
                        }

                        // 恢复图形当前视图替换
                        var overrideGraphicSettings = new OverrideGraphicSettings();
                        var activeView = uidoc.ActiveView;
                        // 图形替换恢复
                        activeView.SetElementOverrides(el.Id, overrideGraphicSettings);
                        // 完成 提交事务
                        t.Commit();
                    }
                    // P0 在范围内
                    else if (result0 != null)
                    {
                        if (SelectFace)
                        {
                            #region 1: 对齐底面
                            if (result0.PointZ > Point01.Z)
                            {
                                // 在底下的情况 + 距离大的值
                                OffsetParameter.Set(OffsetParameter.AsDouble() + result0.Distance - Distan / 304.8);
                            }
                            else
                            {
                                // 在上方的情况 - 距离小的值
                                OffsetParameter.Set(OffsetParameter.AsDouble() - result0.Distance - Distan / 304.8);
                            }
                            #endregion
                        }
                        else
                        {
                            #region 1: 对齐顶面
                            if (result0.PointZ > Point01.Z)
                            {
                                // 在底下的情况 + 距离大的值
                                OffsetParameter.Set(OffsetParameter.AsDouble() + result0.Distance + Distan / 304.8);
                            }
                            else
                            {
                                // 在上方的情况 - 距离小的值
                                OffsetParameter.Set(OffsetParameter.AsDouble() - result0.Distance + Distan / 304.8);
                            }
                            #endregion
                        }

                        // 恢复图形当前视图替换
                        var overrideGraphicSettings = new OverrideGraphicSettings();
                        var activeView = uidoc.ActiveView;
                        // 图形替换恢复
                        activeView.SetElementOverrides(el.Id, overrideGraphicSettings);
                        // 完成 提交事务
                        t.Commit();
                    }
                    // P1 在范围内
                    else if (result1 != null)
                    {
                        #region 1: 对齐板面
                        if (SelectFace)
                        {
                            #region 1：对齐底面
                            if (result1.PointZ > Point02.Z)
                            {
                                // 在底下的情况 + 距离大的值
                                OffsetParameter.Set(OffsetParameter.AsDouble() + result1.Distance - Distan / 304.8);
                            }
                            else
                            {
                                // 在上方的情况 - 距离小的值
                                OffsetParameter.Set(OffsetParameter.AsDouble() - result1.Distance - Distan / 304.8);
                            }
                            #endregion
                        }
                        else
                        {
                            #region 1：对齐顶面
                            if (result1.PointZ > Point01.Z)
                            {
                                // 在底下的情况 + 距离大的值
                                OffsetParameter.Set(OffsetParameter.AsDouble() + result1.Distance + Distan / 304.8);
                            }
                            else
                            {
                                // 在上方的情况 - 距离小的值
                                OffsetParameter.Set(OffsetParameter.AsDouble() - result1.Distance + Distan / 304.8);
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
                    // 运行失败，一个定位点都不在板面范围内
                    else
                    {
                        #region 0：计算点
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
                        #endregion
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