using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Linq;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class AlignWallToBottomLink : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            // 程序是否注册可运行
            if (Run.Running(Strings.key))
            {
                // 获取所有选择的墙
                List<Element> sels = new List<Element>();

                #region UI选中的构件不是 墙 或者 就没有选中构件
                // 获取当前UI是否选中了墙体
                List<ElementId> SelIdsUI = sel.GetElementIds().ToList();
                if (SelIdsUI.Count != 0)
                {
                    foreach (var item in SelIdsUI)
                    {
                        var _el = doc.GetElement(item);
                        if (_el.Category.Name == "墙")
                        {
                            sels.Add(doc.GetElement(item));
                        }
                    }
                }
                // UI选中的构件不是 墙 或者 就没有选中构件
                if (sels.Count == 0)
                {
                    try
                    {
                        ToFinish toFinish = new ToFinish();
                        PickByCategorySelectionFilter pickBy = new PickByCategorySelectionFilter() { CategoryName = "墙" };
                        toFinish.Subscribe();
                        SelPick.SelRefsList = sel.PickObjects(ObjectType.Element, pickBy, "<选择楼板或梁下需要对齐的墙>").ToList();
                        toFinish.Unsubscribe();
                    }
                    catch { SelPick.SelRefsList = new List<Reference>(); }

                    if (SelPick.SelRefsList.Count == 0)
                    {
                        return Result.Cancelled;
                    }
                    foreach (var elID in SelPick.SelRefsList)
                    {
                        sels.Add(doc.GetElement(elID));
                    }
                }
                #endregion

                #region 对齐到的构件梁或楼板
                // 对齐的板/梁
                try
                {
                    SelectionlinkFilter selectionlink = new SelectionlinkFilter() { ListCategoryName = new List<string>() { "结构框架", "楼板" } };
                    SelPick.SelRef = sel.PickObject(ObjectType.LinkedElement, selectionlink, "<选择墙底部的楼板或梁>");
                }
                catch { }

                if (SelPick.SelRef == null)
                {
                    return Result.Cancelled;
                }
                #endregion

                var elLink = doc.GetElement(SelPick.SelRef);
                RevitLinkInstance linkIns = elLink as RevitLinkInstance;
                Document linkdoc = linkIns.GetLinkDocument();
                var el = linkdoc.GetElement(SelPick.SelRef.LinkedElementId);

                // 是否存在调整成功的情况
                List<bool> GoodJob = new List<bool>();

                #region 墙底齐板面
                if (el is Floor)
                {
                    var elF = el as Floor;
                    if (elF.SlabShapeEditor != null && !elF.SlabShapeEditor.IsEnabled)
                    {
                        TransactionGroup T = new TransactionGroup(doc, "墙底齐板面");
                        T.Start();

                        // 自标高的高度偏移值 FLOOR_HEIGHTABOVELEVEL_PARAM
                        var floorLevel = elF.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).AsDouble();
                        // 楼板 标高高程值
                        var levelFId = elF.LevelId;
                        var levelF = linkdoc.GetElement(levelFId) as Level;
                        var ElevationT = levelF.ProjectElevation;

                        foreach (var item in sels)
                        {
                            Transaction t = new Transaction(doc, "墙参数调整");
                            t.Start();
                            // 关闭警告
                            FailureHandlingOptions fho = t.GetFailureHandlingOptions();
                            fho.SetFailuresPreprocessor(new FailuresPreprocessor());
                            t.SetFailureHandlingOptions(fho);
                            try
                            {
                                // 获取墙体 底部约束
                                var wallTopId = item.get_Parameter(BuiltInParameter.WALL_BASE_CONSTRAINT).AsElementId();
                                // 墙体标高高程值
                                var levelWallTop = doc.GetElement(wallTopId) as Level;
                                var ElevationWall = levelWallTop.ProjectElevation;
                                // 获取 建筑结构标高高差
                                var Elevation = ElevationWall - ElevationT;

                                if (item.get_Parameter(BuiltInParameter.WALL_TOP_OFFSET).IsReadOnly)
                                {
                                    // 修改 无连接高度
                                    var oldHeightParam = item.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble();
                                    var SetValue = oldHeightParam - floorLevel;
                                    item.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).Set(SetValue);
                                    // 底部偏移 WALL_BASE_OFFSET
                                    item.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET).Set(floorLevel - Elevation);
                                }
                                else
                                {
                                    // 修改 底部偏移
                                    var SetValue = floorLevel - Elevation;
                                    // 底部偏移 WALL_BASE_OFFSET
                                    item.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET).Set(SetValue);
                                }
                                t.Commit();
                                GoodJob.Add(true);
                            }
                            catch
                            {
                                t.RollBack();
                            }
                        }

                        if (GoodJob.Count > 0)
                        {
                            T.Assimilate();
                        }
                        else
                        {
                            T.RollBack();
                        }
                    }
                    else
                    {
                        TaskDialog.Show("提示", "注意:\n斜板不支持此自动更新墙体参数的功能" );
                    }
                }
                #endregion

                #region 墙底齐梁面
                else
                {
                    TransactionGroup T = new TransactionGroup(doc, "墙底齐梁面");
                    T.Start();

                    var _el = el as FamilyInstance;
                    // Z轴偏移值 Z_OFFSET_VALUE
                    var SetValue = _el.get_Parameter(BuiltInParameter.Z_OFFSET_VALUE).AsDouble();

                    // 楼板 标高高程值
                    var levelF = _el.Host as Level;
                    var ElevationT = levelF.ProjectElevation;
                    foreach (var item in sels)
                    {
                        Transaction t = new Transaction(doc, "墙参数调整");
                        t.Start();
                        // 关闭警告
                        FailureHandlingOptions fho = t.GetFailureHandlingOptions();
                        fho.SetFailuresPreprocessor(new FailuresPreprocessor());
                        t.SetFailureHandlingOptions(fho);

                        try
                        {
                            // 获取墙体 底部约束
                            var wallTopId = item.get_Parameter(BuiltInParameter.WALL_BASE_CONSTRAINT).AsElementId();
                            // 墙体标高高程值
                            var levelWallTop = doc.GetElement(wallTopId) as Level;
                            var ElevationWall = levelWallTop.ProjectElevation;
                            // 获取 建筑结构标高高差
                            var Elevation = ElevationWall - ElevationT;

                            // 修改 底部偏移
                            item.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET).Set(SetValue - Elevation);
                            t.Commit();
                            GoodJob.Add(true);
                        }
                        catch
                        {
                            t.RollBack();
                        }
                    }
                    // 输入结构
                    if (GoodJob.Count > 0)
                    {
                        T.Assimilate();
                    }
                    else
                    {
                        T.RollBack();
                        TaskDialog.Show("提示", "注意:\n斜梁不支持此自动更新墙体参数的功能" );
                    }
                }
                #endregion
            }

            return Result.Succeeded;
        }
    }
}
