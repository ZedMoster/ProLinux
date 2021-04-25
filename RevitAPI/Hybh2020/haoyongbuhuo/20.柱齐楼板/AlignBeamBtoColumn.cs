using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace hybh
{
    /// <summary>
    /// 墙或梁齐柱面
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    class AlignBeamBtoColumn : IExternalCommand
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
                // 选择的构件列表
                List<Element> elementsList = new List<Element>();

                #region 选择构件
                // 判断当前ui是否选中指定类型的构件
                if (sel.GetElementIds().Count != 0)
                {
                    foreach (var item in sel.GetElementIds())
                    {
                        Element element = doc.GetElement(item);
                        if (element.Category.Name == "结构框架" || element.Category.Name == "墙")
                        {
                            elementsList.Add(element);
                        }
                    }
                }
                // 如果没有构件手动选择
                if (elementsList.Count == 0)
                {
                    try
                    {
                        // 右键完成多选
                        ToFinish toFinish = new ToFinish();
                        // 选择过滤器
                        var newfilterList = new PickByListCategorySelectionFilter
                        {
                            ListCategoryName = new List<string>() { "结构框架", "墙" }
                        };
                        toFinish.Subscribe();
                        // 选择梁 墙
                        SelPick.SelRefsList = sel.PickObjects(ObjectType.Element, newfilterList, "<选择需要对结构柱的梁或墙>").ToList();
                        toFinish.Unsubscribe();
                    }
                    catch { SelPick.SelRefsList = new List<Reference>(); }

                    if (SelPick.SelRefsList.Count == 0)
                    {
                        return Result.Cancelled;
                    }
                    foreach (var item in SelPick.SelRefsList)
                    {
                        Element element = doc.GetElement(item);
                        elementsList.Add(element);
                    }
                }
                #endregion

                // 选择过滤器
                try
                {
                    var newfilter = new PickByCategorySelectionFilter() { CategoryName = "结构柱" };
                    SelPick.SelRef = sel.PickObject(ObjectType.Element, newfilter, "<选择需平齐的结构柱>");
                }
                catch { }

                if (SelPick.SelRef == null)
                {
                    return Result.Cancelled;
                }


                if (elementsList.Count > 0)
                {
                    TransactionGroup transactionGroup = new TransactionGroup(doc, "端部齐柱面");
                    transactionGroup.Start();
                    foreach (var el in elementsList)
                    {
                        LocationCurve locationCurve = el.Location as LocationCurve;
                        Line line = locationCurve.Curve as Line;
                        // 获取 结构框架 定位线的两个点
                        XYZ ptStart = line.GetEndPoint(0);
                        XYZ ptEnd = line.GetEndPoint(1);
                        // 获取中点
                        XYZ midPoint = (ptStart + ptEnd) / 2;
                        // 获取结构柱实例
                        var el_Z = doc.GetElement(SelPick.SelRef);

                        #region 获取结构柱距离el 最近的相交面
                        var res = GetColumnFace(el_Z, line, midPoint);
                        #endregion

                        if (res == null)
                        {
                            continue;
                            //return Result.Failed;
                        }
                        using (Transaction t = new Transaction(doc))
                        {
                            t.Start("对齐柱面");

                            // 获取模型与 选择的面的交点
                            var face = res.Face;
                            var result0 = res.IntersectPoint;

                            // 判断移动的点
                            var res0 = face.Project(ptStart).Distance;
                            var res1 = face.Project(ptEnd).Distance;

                            if (res0 < res1)
                            {
                                Line newLine0 = Line.CreateBound(result0, ptEnd);
                                locationCurve.Curve = newLine0;
                            }
                            else
                            {
                                Line newLine0 = Line.CreateBound(ptStart, result0);
                                locationCurve.Curve = newLine0;
                            }

                            // 完成提交事务
                            t.Commit();
                        }
                    }
                    transactionGroup.Assimilate();
                }

            }
            // 返回成功
            return Result.Succeeded;
        }

        /// <summary>
        /// 获取定位线与柱相交最近的面
        /// </summary>
        /// <param name="el"></param>
        /// <param name="line"></param>
        /// <param name="xYZ"></param>
        /// <returns></returns>
        private DictDoubleFaces GetColumnFace(Element el, Line line, XYZ xYZ)
        {
            List<DictDoubleFaces> dictDoubleFaces = new List<DictDoubleFaces>();
            // 创建几何选项
            var options = new Options();
            var geometry = el.get_Geometry(options);
            line.MakeUnbound();
            try
            {
                foreach (var geometryInstance in geometry)
                {
                    Solid solid = geometryInstance as Solid;
                    if (solid == null || solid.Faces.Size == 0 || solid.Edges.Size == 0)
                    {
                        continue;
                    }
                    FaceArray faceArray = solid.Faces;
                    if (faceArray.Size == 0 || faceArray == null)
                    {
                        continue;
                    }
                    // 相交OUT结果
                    IntersectionResultArray result = new IntersectionResultArray();
                    foreach (var faceI in faceArray)
                    {
                        var face = faceI as Face;
                        var intersect = face.Intersect(line, out result);
                        if (intersect == SetComparisonResult.Overlap)
                        {
                            var intersectPoint = result.get_Item(0).XYZPoint;
                            var distanceP = intersectPoint.DistanceTo(xYZ);
                            //TaskDialog.Show("dis", (distanceP * 304.8).ToString());
                            dictDoubleFaces.Add(new DictDoubleFaces { Distance = distanceP, Face = face, IntersectPoint = intersectPoint });
                        }
                    }
                }
            }

            catch (Exception e)
            {
                TaskDialog.Show("提示", e.Message + Strings.error); 
            }

            // 有些结构柱solid 为空 仅有GeometryInstance()
            if (dictDoubleFaces.Count == 0)
            {
                foreach (GeometryObject obj in geometry)
                {
                    var ins = obj as GeometryInstance;
                    foreach (GeometryObject _solid in ins.GetInstanceGeometry())
                    {
                        Solid solid = _solid as Solid;
                        if (solid == null || solid.Faces.Size == 0 || solid.Edges.Size == 0)
                        {
                            continue;
                        }
                        FaceArray faceArray = solid.Faces;
                        if (faceArray.Size == 0 || faceArray == null)
                        {
                            continue;
                        }
                        // 相交OUT结果
                        IntersectionResultArray result = new IntersectionResultArray();
                        foreach (var faceI in faceArray)
                        {
                            var face = faceI as Face;
                            var intersect = face.Intersect(line, out result);
                            if (intersect == SetComparisonResult.Overlap)
                            {
                                var intersectPoint = result.get_Item(0).XYZPoint;
                                var distanceP = intersectPoint.DistanceTo(xYZ);
                                //TaskDialog.Show("dis", (distanceP * 304.8).ToString());
                                dictDoubleFaces.Add(new DictDoubleFaces { Distance = distanceP, Face = face, IntersectPoint = intersectPoint });
                            }
                        }
                    }
                }
            }

            // 判断本次是否获取到相交的数据
            if (dictDoubleFaces.Count > 0)
            {
                // List<DictDoubleFaces> 按distance 值进行排序 并获取第一个 DictDoubleFaces
                var res = dictDoubleFaces.OrderBy(i => i.Distance).First();
                return res;
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// 定义list 参数
    /// </summary>
    class DictDoubleFaces
    {
        public double Distance { get; set; }
        public Face Face { get; set; }
        public XYZ IntersectPoint { get; set; }
    }
}
