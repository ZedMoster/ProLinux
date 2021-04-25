using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace CADReader
{
    /// <summary>
    /// CAD图形类
    /// </summary>
    public class CADElement
    {
        /// <summary>
        /// 获取选择的多段线
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="refer"></param>
        /// <returns></returns>
        public List<CADCurveModel> GetPickCurves(Document doc, Reference refer)
        {
            // 定义变量
            List<CADCurveModel> curveModels = new List<CADCurveModel>();
            // 获取图纸
            ImportInstance dwg = doc.GetElement(refer) as ImportInstance;
            var geoObj = dwg.GetGeometryObjectFromReference(refer);
            Transform transform = dwg.GetTransform();
            var dir = new XYZ(0, 0, doc.ActiveView.GenLevel.ProjectElevation);
            List<Curve> Curves = new List<Curve>();
            #region 判断图形
            if (geoObj is Line line)
            {
                var end0 = line.GetEndPoint(0).TransForm(transform);
                var end1 = line.GetEndPoint(1).TransForm(transform);
                try
                {
                    Curves.Add(Line.CreateBound(end0, end1));
                }
                catch (Exception)
                {
                    //throw new Exception("Curve length is too small for Revit's tolerance");
                }

                curveModels.Add(new CADCurveModel() { Curves = Curves });
            }
            if (geoObj is PolyLine polyLine)
            {
                var coordinates = polyLine.GetCoordinates().ToList();
                for (int i = 0; i < coordinates.Count; i++)
                {
                    // 循环直线的点为
                    if (i < coordinates.Count - 1)
                    {
                        XYZ start = transform.OfPoint(coordinates[i]) + dir;
                        XYZ end = transform.OfPoint(coordinates[i + 1]) + dir;
                        Curves.Add(Line.CreateBound(start, end));
                    }
                }
                if (Curves.Count != 0)
                {
                    curveModels.Add(new CADCurveModel() { Curves = Curves });
                }
            }
            if (geoObj is Arc arc)
            {
                var center = arc.Center.TransForm(transform);
                var radius = arc.Radius;
                var xAxis = arc.XDirection;
                var yAxis = arc.YDirection;
                // 添加识别结果到返回列表
                curveModels.Add(new CADCurveModel() { Curves = CreateArcByCenterR(center, radius, xAxis, yAxis) });
            }
            if (geoObj is Ellipse ellipse)
            {
                var center = ellipse.Center.TransForm(transform);
                var radiusX = ellipse.RadiusX;
                var radiusY = ellipse.RadiusY;
                var xAxis = ellipse.XDirection;
                var yAxis = ellipse.YDirection;
                curveModels.Add(new CADCurveModel() { Curves = CreateEllipseByCenter(center, radiusX, radiusY, xAxis, yAxis) });
            }
            #endregion

            return curveModels;
        }

        /// <summary>
        /// 获取选择的多段线的定位点
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="refer"></param>
        /// <returns></returns>
        public List<CADCurveModel> GetPickGetCoordinates(Document doc, Reference refer)
        {
            // 定义变量
            List<CADCurveModel> CurveModel = new List<CADCurveModel>();
            List<XYZ> xyzs = new List<XYZ>();
            #region 获取Curve线段
            ImportInstance dwg = doc.GetElement(refer) as ImportInstance;
            var geoObj = dwg.GetGeometryObjectFromReference(refer);
            Transform transform = dwg.GetTransform();
            var dir = new XYZ(0, 0, doc.ActiveView.GenLevel.ProjectElevation);
            if (geoObj is Line line)
            {
                var start = line.GetEndPoint(0).TransForm(transform).Add(dir);
                var end = line.GetEndPoint(1).TransForm(transform).Add(dir);
                xyzs.Add(start);
                xyzs.Add(end);
            }
            if (geoObj is PolyLine polyLine)
            {
                var coordinates = polyLine.GetCoordinates().ToList();
                foreach (var item in coordinates)
                {
                    XYZ xyz = transform.OfPoint(item).Add(dir);
                    xyzs.Add(xyz);
                }
            }
            if (geoObj is Arc arc)
            {
                if (arc.IsBound)
                {
                    var start = arc.Tessellate().FirstOrDefault().TransForm(transform).Add(dir);
                    var end = arc.Tessellate().LastOrDefault().TransForm(transform).Add(dir);
                    xyzs.Add(start);
                    xyzs.Add(end);
                }
                var center = arc.Center.TransForm(transform).Add(dir);
                xyzs.Add(center);
            }
            if (geoObj is Ellipse ellpise)
            {
                if (ellpise.IsBound)
                {
                    var start = ellpise.Tessellate().FirstOrDefault().TransForm(transform).Add(dir);
                    var end = ellpise.Tessellate().LastOrDefault().TransForm(transform).Add(dir);
                    xyzs.Add(start);
                    xyzs.Add(end);
                }
                var center = ellpise.Center.TransForm(transform).Add(dir);
                xyzs.Add(center);
            }
            #endregion
            // 添加点位
            CurveModel.Add(new CADCurveModel() { XYZs = xyzs });
            return CurveModel;
        }

        /// <summary>
        /// 获取CAD图纸中选择图层的所有定位线
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="refer"></param>
        /// <returns></returns>
        public List<CADCurveModel> GetLayerCurves(Document doc, Reference refer)
        {
            // 定义变量
            List<CADCurveModel> curveModels = new List<CADCurveModel>();
            // 获取链接图纸
            ImportInstance dwg = doc.GetElement(refer) as ImportInstance;
            GeometryElement geoElement = dwg.get_Geometry(new Options());
            GeometryObject geoObj = dwg.GetGeometryObjectFromReference(refer);
            // 获取图层
            GraphicsStyle graphicsStyle = doc.GetElement(geoObj.GraphicsStyleId) as GraphicsStyle;
            // 读取图层中的所有图形
            foreach (var gObj in geoElement)
            {
                GeometryInstance geomInstance = gObj as GeometryInstance;
                if (null != geomInstance)
                {
                    foreach (var insObj in geomInstance.GetInstanceGeometry())
                    {
                        // 获取当前图层的内容
                        if (insObj.GraphicsStyleId.IntegerValue != graphicsStyle.Id.IntegerValue)
                            continue;
                        // curves 变量
                        List<Curve> Curves = new List<Curve>();
                        #region 判断图形
                        if (insObj is Line line)
                        {
                            Curves.Add(line);
                        }
                        if (insObj is PolyLine polyLine)
                        {
                            var coordinates = polyLine.GetCoordinates().ToList();
                            for (int i = 0; i < coordinates.Count; i++)
                            {
                                if (i < coordinates.Count - 1)
                                {
                                    try
                                    {
                                        Curves.Add(Line.CreateBound(coordinates[i], coordinates[i + 1]));
                                    }
                                    catch (Exception)
                                    {
                                        //throw new Exception("Curve length is too small for Revit's tolerance");
                                    }
                                }
                            }
                        }
                        if (insObj is Arc arc)
                        {
                            if (arc.IsBound)
                            {
                                Curves.Add(arc);
                            }
                            else
                            {
                                var center = arc.Center;
                                var radius = arc.Radius;
                                var xAxis = arc.XDirection;
                                var yAxis = arc.YDirection;
                                Curves = CreateArcByCenterR(center, radius, xAxis, yAxis);
                            }

                        }
                        if (insObj is Ellipse ellipse)
                        {
                            if (ellipse.IsBound)
                            {
                                Curves.Add(ellipse);
                            }
                            else
                            {
                                var center = ellipse.Center;
                                var radiusX = ellipse.RadiusX;
                                var radiusY = ellipse.RadiusY;
                                var xAxis = ellipse.XDirection;
                                var yAxis = ellipse.YDirection;
                                Curves = CreateEllipseByCenter(center, radiusX, radiusY, xAxis, yAxis);
                            }
                        }
                        #endregion
                        if (Curves.Count != 0)
                        {
                            curveModels.Add(new CADCurveModel() { Curves = Curves });
                        }
                    }
                }
            }

            return curveModels;
        }

        public List<CADCurveModel> GetLayerPolyCurves(Document doc, Reference refer)
        {
            // 定义变量
            List<CADCurveModel> curveModels = new List<CADCurveModel>();
            // 获取链接图纸
            ImportInstance dwg = doc.GetElement(refer) as ImportInstance;
            GeometryElement geoElement = dwg.get_Geometry(new Options());
            GeometryObject geoObj = dwg.GetGeometryObjectFromReference(refer);
            // 获取图层
            GraphicsStyle graphicsStyle = doc.GetElement(geoObj.GraphicsStyleId) as GraphicsStyle;
            // 读取图层中的所有图形
            foreach (var gObj in geoElement)
            {
                GeometryInstance geomInstance = gObj as GeometryInstance;
                if (null != geomInstance)
                {
                    foreach (var insObj in geomInstance.GetInstanceGeometry())
                    {
                        // 获取当前图层的内容
                        if (insObj.GraphicsStyleId.IntegerValue != graphicsStyle.Id.IntegerValue)
                            continue;
                        // curves 变量
                        List<Curve> Curves = new List<Curve>();
                        #region 判断图形
                        if (insObj is PolyLine polyLine)
                        {
                            var coordinates = polyLine.GetCoordinates().ToList();
                            for (int i = 0; i < coordinates.Count; i++)
                            {
                                if (i < coordinates.Count - 1)
                                {
                                    try
                                    {
                                        Curves.Add(Line.CreateBound(coordinates[i], coordinates[i + 1]));
                                    }
                                    catch (Exception)
                                    {
                                        //throw new Exception("Curve length is too small for Revit's tolerance");
                                    }
                                }
                            }
                        }
                        #endregion
                        if (Curves.Count != 0)
                        {
                            curveModels.Add(new CADCurveModel() { Curves = Curves });
                        }
                    }
                }
            }

            return curveModels;
        }

        /// <summary>
        /// 获取图层中所有多段线的定位点
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="refer"></param>
        /// <returns></returns>
        public List<CADCurveModel> GetLayerGetCoordinates(Document doc, Reference refer)
        {
            // 定义变量
            List<CADCurveModel> CurveModel = new List<CADCurveModel>();
            // 获取链接图纸
            ImportInstance dwg = doc.GetElement(refer) as ImportInstance;
            GeometryElement geoElement = dwg.get_Geometry(new Options());
            GeometryObject geoObj = dwg.GetGeometryObjectFromReference(refer);
            GraphicsStyle graphicsStyle = doc.GetElement(geoObj.GraphicsStyleId) as GraphicsStyle;
            foreach (var gObj in geoElement)
            {
                GeometryInstance geomInstance = gObj as GeometryInstance;
                if (null != geomInstance)
                {
                    foreach (var insObj in geomInstance.GetInstanceGeometry())
                    {
                        // 获取当前图层的内容
                        if (insObj.GraphicsStyleId.IntegerValue != graphicsStyle.Id.IntegerValue)
                            continue;
                        if (insObj is PolyLine)
                        {
                            var polyLine = insObj as PolyLine;
                            var coordinates = polyLine.GetCoordinates().ToList();
                            CurveModel.Add(new CADCurveModel() { XYZs = coordinates, Curves = CoordinatesToCurves(coordinates) });
                        }
                    }
                }
            }

            return CurveModel;
        }

        /// <summary>
        /// 获取满足宽度条件平行线的中线
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="refer"></param>
        /// <returns></returns>
        public List<CADCurveModel> GetLayerMidCurves(Document doc, Reference refer)
        {
            List<CADCurveModel> curveModels = new List<CADCurveModel>();
            var CurveModel = GetLayerCurves(doc, refer);
            List<Curve> curves = new List<Curve>();
            if (CurveModel.Count == 0)
            {
                return curveModels;
            }
            foreach (var item in CurveModel)
            {
                if (item.Curves.Count == 0)
                {
                    continue;
                }
                foreach (var curve in item.Curves)
                {
                    curves.Add(curve);
                }
            }
            #region 获取定位中线
            // 计算中线
            List<int> set = new List<int>();
            EnumCAD enumCAD = new EnumCAD();
            foreach (var Distan in enumCAD.EnumValues)
            {
                for (int i = 0; i < curves.Count; i++)
                {
                    for (int j = 0; j < curves.Count; j++)
                    {
                        // 是否相等
                        if (i == j)
                        {
                            continue;
                        }
                        // 重复计算
                        if (set.Contains(i) && set.Contains(j))
                        {
                            continue;
                        }
                        if (curves[i] is Line line0 && curves[j] is Line line1)
                        {
                            XYZ dir0 = line0.Direction;
                            XYZ dir1 = line1.Direction;
                            if (dir0.IsAlmostEqualTo(dir1) || dir0.IsAlmostEqualTo(dir1.Negate()))
                            {
                                var model = ParallelAndEquidistant(line0, line1, Distan);
                                if (model.MidCurve != null)
                                {
                                    curveModels.Add(model);
                                    set.Add(i);
                                    set.Add(j);
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            return curveModels;
        }

        /// /////////////////////////////////////////////////////////////////////////////// ///
        ///

        /// <summary>
        /// 创建平行线的中线
        /// </summary>
        /// <param name="Line0"> </param>
        /// <param name="Line1"> </param>
        /// <param name="Distan"> 间距最大值</param>
        /// <returns></returns>
        private CADCurveModel ParallelAndEquidistant(Line Line0, Line Line1, double Distan)
        {
            CADCurveModel curveModel = new CADCurveModel();
            var mid = (Line0.GetEndPoint(0) + Line0.GetEndPoint(1)) * 0.5;
            var width = Line1.Project(mid).Distance;
            var x0 = new XYZ(0, 0, width);
            var x1 = new XYZ(0, 0, Convert.ToDouble(Distan).MillimeterToFeet());
            if (!x0.IsAlmostEqualTo(x1, 0.0001))
            {
                return curveModel;
            }
            var widthValueString = Distan.ToString();
            Curve newLine = null;
            #region 确定方向
            XYZ dir0 = Line0.Direction;
            XYZ dir1 = Line1.Direction;
            if (dir0.IsAlmostEqualTo(dir1))
            {
                var p0_0 = Line0.GetEndPoint(0);
                var p0_1 = Line0.GetEndPoint(1);
                var p1_0 = Line1.GetEndPoint(0);
                var p1_1 = Line1.GetEndPoint(1);
                try
                {
                    newLine = MidSortLine(p0_0, p0_1, p1_0, p1_1, Line0, Line1, Distan);
                }
                catch { }
            }
            else
            {
                var p0_0 = Line0.GetEndPoint(0);
                var p0_1 = Line0.GetEndPoint(1);
                var p1_0 = Line1.GetEndPoint(1);
                var p1_1 = Line1.GetEndPoint(0);
                try
                {
                    newLine = MidSortLine(p0_0, p0_1, p1_0, p1_1, Line0, Line1, Distan);
                }
                catch { }
            }
            #endregion

            curveModel.MidCurve = newLine;
            curveModel.ValueString = widthValueString;
            curveModel.Width = width;

            // 返回平行线中线
            return curveModel;
        }
        /// <summary>
        /// 获取短中线
        /// </summary>
        /// <param name="p0_0"></param>
        /// <param name="p0_1"></param>
        /// <param name="p1_0"></param>
        /// <param name="p1_1"></param>
        /// <param name="Line0"></param>
        /// <param name="Line1"></param>
        /// <param name="Distan"></param>
        /// <returns></returns>
        private Line MidSortLine(XYZ p0_0, XYZ p0_1, XYZ p1_0, XYZ p1_1, Line Line0, Line Line1, double Distan)
        {
            XYZ p2_0, p2_1;
            XYZ dir0 = Line0.Direction;
            XYZ dir1 = Line1.Direction;
            if (p0_0.DistanceTo(p1_0) <= Distan.MillimeterToFeet())
            {
                // 间距满足情况
                p2_0 = (p0_0 + p1_0) / 2;
            }
            else
            {
                // 间距不满足情况
                if (Line0.Length < Line1.Length)
                {
                    // 将 line1 点 换算到 line0
                    var _cLine = Line.CreateUnbound(p0_0.Flatten(), dir0.Rotation());
                    _cLine.MakeUnbound();
                    IntersectionResultArray resultArray = new IntersectionResultArray();
                    var result = _cLine.Intersect(Line1.Flatten(), out resultArray);
                    if (result != SetComparisonResult.Disjoint)
                    {
                        p1_0 = resultArray.get_Item(0).XYZPoint;
                    }
                }
                else
                {
                    // 将 line0 点 换算到 line1
                    var _cLine = Line.CreateUnbound(p1_0.Flatten(), dir1.Rotation());
                    _cLine.MakeUnbound();
                    IntersectionResultArray resultArray = new IntersectionResultArray();
                    var result = _cLine.Intersect(Line0.Flatten(), out resultArray);
                    if (result != SetComparisonResult.Disjoint)
                    {
                        p0_0 = resultArray.get_Item(0).XYZPoint;
                    }
                }
                p2_0 = (p0_0 + p1_0) / 2;
            }
            // p2
            if (p0_1.DistanceTo(p1_1) <= Distan.MillimeterToFeet())
            {
                // 间距满足情况
                p2_1 = (p0_1 + p1_1) / 2;
            }
            else
            {
                // 间距不满足情况
                if (Line0.Length < Line1.Length)
                {
                    // 将 line1 点 换算到 line0
                    var _cLine = Line.CreateUnbound(p0_1.Flatten(), dir0.Rotation());
                    _cLine.MakeUnbound();
                    IntersectionResultArray resultArray = new IntersectionResultArray();
                    var result = _cLine.Intersect(Line1.Flatten(), out resultArray);
                    if (result != SetComparisonResult.Disjoint)
                    {
                        p1_1 = resultArray.get_Item(0).XYZPoint;
                    }
                }
                else
                {
                    // 将 line0 点 换算到 line1
                    var _cLine = Line.CreateUnbound(p1_1.Flatten(), dir1.Rotation());
                    _cLine.MakeUnbound();
                    IntersectionResultArray resultArray = new IntersectionResultArray();
                    var result = _cLine.Intersect(Line0.Flatten(), out resultArray);
                    if (result != SetComparisonResult.Disjoint)
                    {
                        p0_1 = resultArray.get_Item(0).XYZPoint;
                    }
                }
                p2_1 = (p0_1 + p1_1) / 2;
            }
            return Line.CreateBound(p2_0.Flatten(), p2_1.Flatten());
        }
        /// <summary>
        /// 定位点转多段线
        /// </summary>
        /// <param name="coordinates"></param>
        /// <returns></returns>
        private List<Curve> CoordinatesToCurves(List<XYZ> coordinates)
        {
            List<Curve> curves = new List<Curve>();
            for (int i = 0; i < coordinates.Count; i++)
            {
                // 循环直线的点为
                if (i < coordinates.Count - 1)
                {
                    XYZ start = coordinates[i];
                    XYZ end = coordinates[i + 1];
                    try
                    {
                        curves.Add(Line.CreateBound(start, end));
                    }
                    catch (Exception)
                    {
                        //throw new Exception("Curve length is too small for Revit's tolerance");
                    }
                }
            }
            return curves;
        }
        /// <summary>
        /// 通过定位点及半径创建两段圆弧
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        private List<Curve> CreateArcByCenterR(XYZ center, double radius, XYZ xAxis, XYZ yAxis)
        {
            List<Curve> curves = new List<Curve>();
            var arc0 = Arc.Create(center, radius, 0, Math.PI, xAxis, yAxis);
            var arc1 = Arc.Create(center, radius, Math.PI, Math.PI * 2, xAxis, yAxis);
            curves.Add(arc0);
            curves.Add(arc1);
            return curves;
        }
        /// <summary>
        /// 通过定位点及半径创建椭圆
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        private List<Curve> CreateEllipseByCenter(XYZ center, double radiusX, double radiusY, XYZ xAxis, XYZ yAxis)
        {
            List<Curve> curves = new List<Curve>();
            var Ellipse0 = Ellipse.CreateCurve(center, radiusX, radiusY, xAxis, yAxis, 0, Math.PI);
            var Ellipse1 = Ellipse.CreateCurve(center, radiusX, radiusY, xAxis, yAxis, Math.PI, Math.PI * 2);
            curves.Add(Ellipse0);
            curves.Add(Ellipse1);
            return curves;
        }
    }
}
