using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace hybh
{
    /// <summary>
    /// 梁端点齐柱中心定位点
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    class AlignBeamtoColumnLocation : IExternalCommand
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
                // 创建一个梁列表
                List<Element> elementsBeams = new List<Element>();
                List<Element> elementsColmun = new List<Element>();

                // 判断窗口是否已经选中模型
                if (sel.GetElementIds().Count != 0)
                {
                    foreach (var item in sel.GetElementIds())
                    {
                        Element el = doc.GetElement(item);
                        if (el.Category.Name == "结构框架")
                        {
                            elementsBeams.Add(el);
                        }
                        else if (el.Category.Name == "结构柱")
                        {
                            elementsColmun.Add(el);
                        }
                    }
                }

                if (elementsBeams.Count == 0 || elementsColmun.Count == 0)
                {
                    // 重新定义结构柱列表
                    elementsColmun.Clear();
                    try
                    {
                        ToFinish toFinish = new ToFinish();
                        var filter = new PickByListCategorySelectionFilter() { ListCategoryName = new List<string> { "结构框架", "结构柱" } };
                        // 手动选择梁柱
                        toFinish.Subscribe();
                        SelPick.SelRefsList = sel.PickObjects(ObjectType.Element, filter, "<选择需要对其板底的梁>").ToList();
                        toFinish.Unsubscribe();
                    }
                    catch { SelPick.SelRefsList = new List<Reference>(); }

                    if (SelPick.SelRefsList.Count == 0)
                    {
                        return Result.Cancelled;
                    }
                    // 获取结构柱列表
                    foreach (var item in SelPick.SelRefsList)
                    {
                        Element el = doc.GetElement(item);
                        if (el.Category.Name == "结构框架")
                        {
                            elementsBeams.Add(el);
                        }
                        else if (el.Category.Name == "结构柱")
                        {
                            elementsColmun.Add(el);
                        }
                    }
                }
                // 判断各个类型列表是否是存在空值
                if (elementsBeams.Count == 0 || elementsColmun.Count == 0)
                {
                    TaskDialog.Show("提示", "至少应该选择一个梁和一个柱子" + Strings.error);
                    return Result.Failed;
                }
                else
                {
                    // 模型显示颜色
                    ColorWithModel colorWithModel = new ColorWithModel();
                    // 修改模型参数部分
                    using (Transaction t = new Transaction(doc))
                    {
                        t.Start("梁端齐柱中心");
                        // 关闭警告
                        FailureHandlingOptions fho = t.GetFailureHandlingOptions();
                        fho.SetFailuresPreprocessor(new FailuresPreprocessor());
                        t.SetFailureHandlingOptions(fho);

                        var column = elementsColmun.First();
                        foreach (var beam in elementsBeams)
                        {
                            try
                            {
                                // 移动梁端点到柱定位点
                                MakeBeamLocation(column, beam);
                            }
                            catch
                            {
                                Color color = new Color(255, 69, 0);
                                // 设置注释参数 可空
                                //var value = "梁参数计算错误";
                                // 计算失败 标记颜色
                                colorWithModel.ElementSetError(uidoc, doc, beam, color);
                            }
                        }
                        t.Commit();
                    }
                }
            }

            return Result.Succeeded;
        }

        /// <summary>
        /// 移动梁距离柱端点较近的点位到柱子定位中心
        /// </summary>
        /// <param name="column"></param>
        /// <param name="beam"></param>
        public void MakeBeamLocation(Element column, Element beam)
        {
            // 柱定位点
            var locationPoint = column.Location as LocationPoint;
            var point = locationPoint.Point;
            // 梁定位线
            LocationCurve locationCurve = beam.Location as LocationCurve;
            Line line = locationCurve.Curve as Line;
            // 获取 结构框架 定位线的两个点
            XYZ ptStart = line.GetEndPoint(0);
            XYZ ptEnd = line.GetEndPoint(1);
            // 判断移动的点
            var res0 = point.DistanceTo(ptStart);
            var res1 = point.DistanceTo(ptEnd);

            if (res0 < res1)
            {
                var _point = new XYZ(point.X, point.Y, 0) + new XYZ(0, 0, ptStart.Z);
                Line newLine0 = Line.CreateBound(_point, ptEnd);
                locationCurve.Curve = newLine0;
            }
            else
            {
                var _point = new XYZ(point.X, point.Y, 0) + new XYZ(0, 0, ptEnd.Z);
                Line newLine0 = Line.CreateBound(ptStart, _point);
                locationCurve.Curve = newLine0;
            }
        }

    }
}
