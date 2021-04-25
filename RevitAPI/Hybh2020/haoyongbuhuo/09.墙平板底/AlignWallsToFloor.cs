using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Linq;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class AlignWallsToFloor : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            // 程序是否注册可运行
            if (Run.Running(Strings.key))
            {
                List<Element> elsFloor = new List<Element>();

                #region 选择构件
                if (sel.GetElementIds().Count != 0)
                {
                    foreach (var item in sel.GetElementIds())
                    {
                        Element el = doc.GetElement(item);
                        if (el.Category.Name == "楼板")
                        {
                            elsFloor.Add(el);
                        }
                    }
                }
                else
                {
                    try
                    {
                        ToFinish toFinish = new ToFinish();
                        PickByCategorySelectionFilter selectFilter = new PickByCategorySelectionFilter() { CategoryName = "楼板" };
                        toFinish.Subscribe();
                        SelPick.SelRefsList = sel.PickObjects(ObjectType.Element, selectFilter, "<选择楼板自动平齐与其相交的墙体 右键或空格完成选择>").ToList();
                        toFinish.Unsubscribe();
                    }
                    catch { SelPick.SelRefsList = new List<Reference>(); }

                    // 判断选择是否为空
                    if (SelPick.SelRefsList.Count == 0)
                    {
                        return Result.Cancelled;
                    }

                    foreach (var elId in SelPick.SelRefsList)
                    {
                        var elF = doc.GetElement(elId);
                        elsFloor.Add(elF);
                    }
                }
                #endregion

                // 是否存在调整成功的情况
                List<bool> GoodJob = new List<bool>();

                TransactionGroup T = new TransactionGroup(doc, "板底墙自动对齐");
                T.Start();

                foreach (var elfloor in elsFloor)
                {
                    #region 自动判断与楼板相交的墙体并调整墙顶部偏移参数
                    // 获取与楼板相交的墙体
                    GetInstersectsElements getInstersectsElements = new GetInstersectsElements();
                    var category = BuiltInCategory.OST_Walls;
                    var walls = getInstersectsElements.ElementByType(doc, elfloor, category).ToList();
                    // 没有相交的墙体 跳过循环
                    if (walls.Count == 0)
                    {
                        continue;
                    }
                    // 判断楼板是不是斜板
                    var el = elfloor as Floor;
                    if (el.SlabShapeEditor != null && !el.SlabShapeEditor.IsEnabled)
                    {
                        // 厚度 FLOOR_ATTR_THICKNESS_PARAM
                        var floorHight = el.get_Parameter(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM).AsDouble();
                        // 自标高的高度偏移值 FLOOR_HEIGHTABOVELEVEL_PARAM
                        var floorLevel = el.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).AsDouble();
                        foreach (var item in walls)
                        {
                            Transaction t = new Transaction(doc, "调整墙参数");
                            t.Start();

                            // 关闭警告
                            FailureHandlingOptions fho = t.GetFailureHandlingOptions();
                            fho.SetFailuresPreprocessor(new FailuresPreprocessor());
                            t.SetFailureHandlingOptions(fho);
                            try
                            {
                                // 楼板标高高程值
                                var levelFId = el.LevelId;
                                var levelF = doc.GetElement(levelFId) as Level;
                                var ElevationT = levelF.ProjectElevation;
                                if (item.get_Parameter(BuiltInParameter.WALL_TOP_OFFSET).IsReadOnly)
                                {
                                    // 墙体标高高程值
                                    var levelWallId = item.LevelId;
                                    var levelW = doc.GetElement(levelWallId) as Level;
                                    var ElevationB = levelW.ProjectElevation;
                                    // 计算墙体初始高度
                                    var old = ElevationT - ElevationB;
                                    // 墙底部偏移值
                                    var wallBaseOffset = item.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET).AsDouble();
                                    // 修改 无连接高度
                                    var SetValue = old + floorLevel - floorHight - wallBaseOffset;
                                    item.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).Set(SetValue);
                                }
                                else
                                {
                                    // 获取墙体 顶部约束
                                    var wallTopId = item.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).AsElementId();
                                    // 墙体标高高程值
                                    var levelWallTop = doc.GetElement(wallTopId) as Level;
                                    var ElevationWall = levelWallTop.ProjectElevation;
                                    // 获取建筑结构标高高差
                                    var Elevation = ElevationWall - ElevationT;
                                    // 修改 顶部偏移
                                    var SetValue = floorLevel - floorHight - Elevation;
                                    item.get_Parameter(BuiltInParameter.WALL_TOP_OFFSET).Set(SetValue);
                                }
                                t.Commit();
                                GoodJob.Add(true);
                            }
                            catch
                            {
                                t.RollBack();
                            }
                        }
                    }
                    else
                    {
                        GoodJob.Add(false);
                    }
                    #endregion
                }

                #region 判断是否存在正确的调整并提交修改模型
                if (GoodJob.All(x => x == false))
                {
                    T.RollBack();
                }
                else
                {
                    T.Assimilate();
                }
                #endregion

                #region 存在运行出错的 提示斜板不支持
                if (GoodJob.Any(x => x == false))
                {
                    TaskDialog.Show("提示", "注意:斜板不支持此自动更新墙体参数的功能" );
                }
                #endregion

            }

            return Result.Succeeded;
        }
    }
}
