using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Linq;
using System.Reflection;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class OutdoorPipe : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // 定义变量
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            // 程序是否注册可运行
            if (Run.Running(Strings.key))
            {
                TransactionGroup transactionGroup = new TransactionGroup(doc, "风管开洞");
                transactionGroup.Start();

                // 载入所需的族文件
                DoLoadFamily doLoadFamily = new DoLoadFamily();
                try
                {
                    var Rfa = "OutPipe";
                    var have = doLoadFamily.IsHaveFamily(doc, BuiltInCategory.OST_GenericModel, Rfa);
                    if (!have)
                    {
                        doLoadFamily.LoadFamily(doc, Rfa + ".rfa", "室外管井");
                        TaskDialog.Show("提示", "加载指定<室外管井>族文件\n请先创建族实例");
                        transactionGroup.Assimilate();
                        return Result.Succeeded;
                    }
                }
                catch
                {
                    TaskDialog.Show("提示", "请重新安装安装包找不到指定族文件");
                    transactionGroup.RollBack();
                    return Result.Failed;
                }

                // 获取系统类型
                var pipingSystemType = new FilteredElementCollector(doc).OfClass(
                                        typeof(Autodesk.Revit.DB.Plumbing.PipingSystemType)).WhereElementIsElementType().ToElements();
                // 获取管道类型
                var pipeTypes = new FilteredElementCollector(doc).OfCategory(
                                        BuiltInCategory.OST_PipeCurves).WhereElementIsElementType().ToElements();
                var window = new WPFOutdoorPipe(pipingSystemType, pipeTypes);
                window.ShowDialog();
                if (window.IsHitTestVisible)
                {
                    try
                    {
                        // 选择水井族文件
                        var newfilter = new PickByCategorySelectionFilter
                        {
                            CategoryName = "常规模型"
                        };
                        ToFinish toFinish = new ToFinish();
                        toFinish.Subscribe();
                        SelPick.SelRefsList = sel.PickObjects(ObjectType.Element, newfilter, "选择两个室外雨水井族").ToList();
                        toFinish.Unsubscribe();
                    }
                    catch { }

                    if (SelPick.SelRefsList.Count == 0)
                    {
                        return Result.Cancelled;
                    }
                    else
                    {
                        if (SelPick.SelRefsList.Count == 1)
                        {
                            TaskDialog.Show("提示", "需要选择两个水井族实例");
                            return Result.Failed;
                        }

                        // 获取两个水井族
                        var el0 = doc.GetElement(SelPick.SelRefsList[0]);
                        var el1 = doc.GetElement(SelPick.SelRefsList[1]);
                        // 创建管段的参数
                        var pipeSystem = window.PipingSystemBox.SelectedValue as Element;
                        var pipeType = window.PipeTypeBox.SelectedValue as Element;
                        double.TryParse(window.pipeD.Text, out double pipeDiameter);
                        var levelId = el0.LevelId;
                        var level = doc.GetElement(levelId) as Level;
                        // 获取定位点
                        GetNewPoint GetNewPoint = new GetNewPoint();
                        var p1 = GetNewPoint.CreatNewPoint(el0, pipeDiameter, level);
                        var p2 = GetNewPoint.CreatNewPoint(el1, pipeDiameter, level);
                        if (p1 != null && p2 != null)
                        {
                            using (Transaction transaction = new Transaction(doc))
                            {
                                if (transaction.Start("创建水井管段") == TransactionStatus.Started)
                                {
                                    try
                                    {
                                        // 创建管段 doc, pipeSystem.Id, pipeType.Id, levelId, p1, p2
                                        var pipe = Autodesk.Revit.DB.Plumbing.Pipe.Create(
                                                        doc, pipeSystem.Id, pipeType.Id, levelId, p1, p2);
                                        pipe.LookupParameter("直径").Set(pipeDiameter / 304.8);
                                        transaction.Commit();
                                    }
                                    catch (Exception e)
                                    {
                                        transaction.RollBack();
                                        TaskDialog.Show("提示", e.Message + Strings.error);
                                    }
                                }
                            }
                        }
                        else
                        {
                            TaskDialog.Show("提示", "选择的水井族实例不是参数化的族");
                        }
                    }
                    transactionGroup.Assimilate();
                }
                else
                {
                    transactionGroup.RollBack();
                }
            }

            return Result.Succeeded;
        }
    }

    // Class方法：获取elment 点位 创建管段定位点
    class GetNewPoint
    {
        XYZ newlocationPoint;
        double hightD;
        public XYZ CreatNewPoint(Element el, double pipe_param, Level level)
        {
            var Elevation = level.Elevation;
            var oldLocation = el.Location as LocationPoint;
            if (null != oldLocation)
            {
                var oldPoint = oldLocation.Point;
                var hightT = el.LookupParameter("管道出口").AsDouble();
                try
                {
                    hightD = el.LookupParameter("偏移").AsDouble();
                }
                catch (Exception)
                {
                    hightD = el.LookupParameter("主体中的偏移").AsDouble();
                }
                var valueParameter = hightD - hightT + (100 + pipe_param/2)/304.8;
                newlocationPoint = new XYZ(oldPoint.X, oldPoint.Y, valueParameter+ Elevation);
                return newlocationPoint;
            }
            else
            {
                return null;
            }
        }
    }
}
