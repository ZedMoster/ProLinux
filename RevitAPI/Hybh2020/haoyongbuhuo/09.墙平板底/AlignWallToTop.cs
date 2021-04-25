using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Linq;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class AlignWallToTop : IExternalCommand
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

                #region 对齐到的对象梁或板
                try
                {
                    PickByListCategorySelectionFilter selectFilter = new PickByListCategorySelectionFilter() { ListCategoryName = new List<string>() { "结构框架", "楼板" } };
                    SelPick.SelRef = sel.PickObject(ObjectType.Element, selectFilter, "<选择结构梁或楼板>");
                }
                catch { }

                if (SelPick.SelRef == null)
                {
                    return Result.Cancelled;
                }

                #endregion

                var el = doc.GetElement(SelPick.SelRef);

                // 是否存在调整成功的情况
                List<bool> GoodJob = new List<bool>();

                #region 墙顶齐板底
                if (el is Floor)
                {
                    var elF = el as Floor;
                    if (elF.SlabShapeEditor != null && !elF.SlabShapeEditor.IsEnabled)
                    {
                        // 读取梁参数名称
                        TransactionGroup T = new TransactionGroup(doc, "墙顶齐梁底");
                        T.Start();

                        // 厚度 FLOOR_ATTR_THICKNESS_PARAM
                        var floorHight = elF.get_Parameter(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM).AsDouble();
                        // 自标高的高度偏移值 FLOOR_HEIGHTABOVELEVEL_PARAM
                        var floorLevel = elF.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).AsDouble();
                        // 楼板标高高程值
                        var levelFId = elF.LevelId;
                        var levelF = doc.GetElement(levelFId) as Level;
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
                                // 判断墙体顶部约束是否存在
                                if (item.get_Parameter(BuiltInParameter.WALL_TOP_OFFSET).IsReadOnly)
                                {
                                    // 墙体标高高程值
                                    var levelWallId = item.LevelId;
                                    var levelW = doc.GetElement(levelWallId) as Level;
                                    var ElevationB = levelW.Elevation;

                                    var old = ElevationT - ElevationB;
                                    // 墙底部偏移值
                                    var wallBaseOffset = item.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET).AsDouble();
                                    // 修改 无连接高度
                                    var SetValue = old + floorLevel - floorHight - wallBaseOffset;
                                    item.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).Set(SetValue);
                                }
                                else
                                {
                                    // 获取墙体顶部约束
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

                        // 更新模型
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
                        TaskDialog.Show("提示", "注意:斜板不支持此自动更新墙体参数的功能" );
                    }

                }
                #endregion

                #region 墙顶齐梁底
                else
                {
                    RegistryStorage Registry = new RegistryStorage();
                    // 读取梁参数名称
                    var h_string = Registry.OpenAfterStart("BeamH") ?? "h";
                    TransactionGroup T = new TransactionGroup(doc, "墙顶齐梁底");
                    T.Start();

                    // 将 el（梁） 转为 FamilyInstance
                    FamilyInstance elInstance = el as FamilyInstance;
                    try
                    {
                        // 获取梁类型参数 H
                        var elType = elInstance.Symbol;
                        HightValue = elType.LookupParameter(h_string).AsDouble();
                    }
                    catch
                    {
                        // 获取梁实例参数 高度
                        HightValue = elInstance.LookupParameter(h_string).AsDouble();
                    }
                    var old_Z = elInstance.get_Parameter(BuiltInParameter.Z_OFFSET_VALUE).AsDouble();

                    // 梁 标高高程值
                    if (elInstance.Host == null)
                    {
                        TaskDialog.Show("提示", "梁主体标高Host不存在!" );
                        return Result.Failed;
                    }
                    var levelB = elInstance.Host as Level;
                    var ElevationT = levelB.ProjectElevation;

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
                            // 获取墙体顶部约束
                            var wallTopId = item.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).AsElementId();
                            // 墙体标高高程值
                            var levelWallTop = doc.GetElement(wallTopId) as Level;
                            var ElevationWall = levelWallTop.ProjectElevation;
                            // 获取建筑结构标高高差
                            var Elevation = ElevationWall - ElevationT;

                            // 计算得出 顶部偏移值
                            var SetValue = old_Z - HightValue - Elevation;
                            // 修改 顶部偏移
                            item.get_Parameter(BuiltInParameter.WALL_TOP_OFFSET).Set(SetValue);
                            t.Commit();
                            GoodJob.Add(true);
                        }
                        catch
                        {
                            t.RollBack();
                            GoodJob.Add(false);
                        }
                    }

                    // 存在运行出错的 更新梁高度参数名称
                    if (GoodJob.Any(x => x == false))
                    {
                        TaskDialog.Show("提示", "在全局参数中设置梁高度的参数名称" );
                    }
                    // 判断是否存在正确的调整并提交修改模型
                    if (GoodJob.All(x => x == false))
                    {
                        T.RollBack();
                    }
                    else
                    {
                        T.Assimilate();
                    }
                }
                #endregion
            }

            return Result.Succeeded;
        }
        // 梁高度
        public double HightValue { get; set; }
    }
}
