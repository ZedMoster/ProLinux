using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class AlignColumnBaseWithFace : IExternalCommand
    {
        public List<Reference> SelBeamIDs { get; set; }
        public Reference SelFloorID { get; set; }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            // 程序是否注册可运行
            if (Run.Running(Strings.key))
            {
                try
                {
                    #region 选择构件
                    // 创建一个梁列表
                    List<Element> elementColumns = new List<Element>();
                    // 选择过滤器
                    var newfilter = new PickByCategorySelectionFilter();

                    // 判断当前视口是否已选中构件
                    if (sel.GetElementIds().Count != 0)
                    {
                        foreach (var item in sel.GetElementIds())
                        {
                            Element el = doc.GetElement(item);
                            if (el.Category.Name == "结构柱")
                            {
                                elementColumns.Add(el);
                            }
                        }
                    }
                    else
                    {
                        // 右键完成多选
                        ToFinish toFinish = new ToFinish();
                        // 选择梁
                        newfilter.CategoryName = "结构柱";
                        try
                        {
                            toFinish.Subscribe();
                            SelBeamIDs = sel.PickObjects(ObjectType.Element, newfilter, "<选择需要对其板面的结构柱>").ToList();
                            toFinish.Unsubscribe();
                        }
                        catch { }

                        if (SelBeamIDs.Count == 0)
                        {
                            return Result.Failed;
                        }
                        foreach (var elId in SelBeamIDs)
                        {
                            Element el = doc.GetElement(elId);
                            elementColumns.Add(el);
                        }
                    }
                    // 判断是列表是否存在 结构柱
                    if (elementColumns.Count == 0)
                    {
                        TaskDialog.Show("error", "至少选择一个结构柱才能进行下一步" + Strings.error);
                        return Result.Failed;
                    }

                    // 选择平面
                    newfilter.CategoryName = "楼板";
                    try
                    {
                        SelFloorID = sel.PickObject(ObjectType.Face, newfilter, "<选择梁需平齐的楼板板面>");
                    }
                    catch { }

                    if (SelFloorID == null)
                    {
                        return Result.Failed;
                    }
                    #endregion

                    var el_B = doc.GetElement(SelFloorID);
                    var face = el_B.GetGeometryObjectFromReference(SelFloorID) as PlanarFace;

                    // 模型显示颜色
                    ColorWithModel colorWithModel = new ColorWithModel();

                    // 设置事务组
                    TransactionGroup T = new TransactionGroup(doc);
                    T.Start("柱底平板面");
                    try
                    {
                        // 创建一个 列表ids
                        List<ElementId> elementIds = new List<ElementId>();

                        foreach (var el in elementColumns)
                        {
                            // 获取结构柱分析模型的定位点 列表
                            var twoPoints = GetAnalyCurve.GetCurveTessellate(doc, el);
                            if (twoPoints.Count == 0)
                            {
                                continue;
                            }
                            Transaction t = new Transaction(doc);
                            t.Start("计算柱距板面");
                            try
                            {
                                // 关闭警告
                                FailureHandlingOptions fho = t.GetFailureHandlingOptions();
                                fho.SetFailuresPreprocessor(new FailuresPreprocessor());
                                t.SetFailureHandlingOptions(fho);

                                // 柱底平板面
                                XYZ p0 = twoPoints[0];

                                #region 计算垂直方向
                                XYZ dir = new XYZ(0, 0, 1);
                                Line line = Line.CreateUnbound(p0, dir);
                                IntersectionResultArray result = new IntersectionResultArray();
                                var intersect = face.Intersect(line, out result);
                                #endregion

                                #region 投影结果
                                if (intersect == SetComparisonResult.Overlap)
                                {
                                    var intersectPoint = result.get_Item(0).XYZPoint;
                                    var distanceP0 = intersectPoint.DistanceTo(p0);

                                    // 待修改梁参数 底部偏移 SCHEDULE_BASE_LEVEL_OFFSET_PARAM
                                    var _Parameter = el.get_Parameter(BuiltInParameter.SCHEDULE_BASE_LEVEL_OFFSET_PARAM);
                                    var oldTOP = _Parameter.AsDouble();

                                    if (p0.Z < intersectPoint.Z)
                                    {
                                        var _value = oldTOP + distanceP0;
                                        _Parameter.Set(_value);
                                    }
                                    else
                                    {
                                        var _value = oldTOP - distanceP0;
                                        _Parameter.Set(_value);
                                    }

                                    // 恢复图形当前视图替换
                                    var overrideGraphicSettings = new OverrideGraphicSettings();
                                    var activeView = uidoc.ActiveView;
                                    // 图形替换恢复
                                    activeView.SetElementOverrides(el.Id, overrideGraphicSettings);

                                    // 完成 提交事务
                                    t.Commit();

                                    // 正确的id 集合
                                    elementIds.Add(el.Id);
                                }
                                // 计算错误自动未模型添加颜色
                                else
                                {
                                    // 错误 返回事务
                                    t.RollBack();
                                    Color color = new Color(255, 69, 0);
                                    // 设置注释参数 可空
                                    //var value = "梁参数计算错误";
                                    // 计算失败 标记颜色
                                    colorWithModel.ElementSetError(uidoc, doc, el, color);
                                }
                                #endregion
                            }
                            catch (Exception e)
                            {
                                // 错误 返回事务
                                t.RollBack();
                                TaskDialog.Show("error2", e.Message + Strings.error);
                            }

                            // UI 选中运行完成的构件
                            sel.SetElementIds(elementIds);
                        }
                        // 正常运行
                        T.Assimilate();
                    }
                    catch (Exception e)
                    {
                        T.RollBack();
                        TaskDialog.Show("error1", e.Message + Strings.error);
                    }
                }
                catch (Exception e)
                {
                    // 取消对齐
                    TaskDialog.Show("error0", e.Message + Strings.error);
                    return Result.Failed;
                }
            }

            return Result.Succeeded;
        }
    }
}