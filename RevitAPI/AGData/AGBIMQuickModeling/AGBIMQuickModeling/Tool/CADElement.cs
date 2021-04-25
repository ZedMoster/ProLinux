using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace CADReader
{
    class CADElement
    {
        /// <summary>
        /// 获取线段
        /// </summary>
        public List<Line> GetLayerLines(Document doc, Reference refer)
        {
            // 定义变量
            List<Line> lines = new List<Line>();
            // 获取链接图纸
            ImportInstance dwg = doc.GetElement(refer) as ImportInstance;
            GeometryElement geoElement = dwg.get_Geometry(new Options());
            GeometryObject geoObj = dwg.GetGeometryObjectFromReference(refer);
            // 读取图层中的所有图形
            foreach(var gObj in geoElement)
            {
                GeometryInstance geomInstance = gObj as GeometryInstance;
                if(null != geomInstance)
                {
                    foreach(var insObj in geomInstance.GetInstanceGeometry())
                    {
                        try
                        {
                            // 图层是否相同
                            if(doc.IsNotEqualLayerName(geoObj, insObj))
                            {
                                continue;
                            }
                            // curves 变量
                            if(insObj is Line line)
                            {
                                if(line.Length < 0.001)
                                {
                                    continue;
                                }
                                lines.Add(line);
                            }
                            else if(insObj is PolyLine polyLine)
                            {
                                var coordinates = polyLine.GetCoordinates().ToList();
                                for(int i = 0; i < coordinates.Count; i++)
                                {
                                    if(i < coordinates.Count - 1)
                                    {
                                        if(coordinates[i].DistanceTo(coordinates[i + 1]) < 0.001)
                                        {
                                            continue;
                                        }
                                        lines.Add(Line.CreateBound(coordinates[i], coordinates[i + 1]));
                                    }
                                }
                            }
                        }
                        catch(Exception e)
                        {
                            _ = e.Message;
                        }
                    }
                }
            }

            return lines;
        }

        /// <summary>
        /// 获取CAD图纸中选择图层的所有定位线
        /// </summary>
        public List<CADCurveModel> GetLayerCurves(Document doc, Reference refer)
        {
            // 定义变量
            List<CADCurveModel> curveModels = new List<CADCurveModel>();
            // 获取链接图纸
            ImportInstance dwg = doc.GetElement(refer) as ImportInstance;
            GeometryElement geoElement = dwg.get_Geometry(new Options());
            GeometryObject geoObj = dwg.GetGeometryObjectFromReference(refer);
            // 读取图层中的所有图形
            foreach(var gObj in geoElement)
            {
                GeometryInstance geomInstance = gObj as GeometryInstance;
                if(null != geomInstance)
                {
                    foreach(var insObj in geomInstance.GetInstanceGeometry())
                    {
                        try
                        {
                            // 图层是否相同
                            if(doc.IsNotEqualLayerName(geoObj, insObj))
                            {
                                continue;
                            }
                            // curves 变量
                            List<Curve> Curves = new List<Curve>();

                            #region 判断图形
                            if(insObj is Line line)
                            {
                                if(line.Length < 0.001)
                                {
                                    continue;
                                }
                                Curves.Add(line);
                            }
                            if(insObj is PolyLine polyLine)
                            {
                                var coordinates = polyLine.GetCoordinates().ToList();
                                for(int i = 0; i < coordinates.Count; i++)
                                {
                                    if(i < coordinates.Count - 1)
                                    {
                                        if(coordinates[i].DistanceTo(coordinates[i + 1]) < 0.001)
                                        {
                                            continue;
                                        }
                                        Curves.Add(Line.CreateBound(coordinates[i], coordinates[i + 1]));
                                    }
                                }
                            }
                            if(insObj is Arc arc)
                            {
                                if(arc.IsBound)
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
                            if(insObj is Ellipse ellipse)
                            {
                                if(ellipse.IsBound)
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

                            if(Curves.Count == 0)
                            {
                                continue;
                            }
                            curveModels.Add(new CADCurveModel() { Curves = Curves });
                        }
                        catch(Exception e)
                        {
                            _ = e.Message;
                        }

                    }
                }
            }

            return curveModels;
        }

        /// <summary>
        /// 获取 Arc 中心点
        /// </summary>
        public List<CADCurveModel> GetLayerArcCenterPoints(Document doc, Reference refer)
        {
            // 定义变量
            List<CADCurveModel> curveModels = new List<CADCurveModel>();
            // 获取链接图纸
            ImportInstance dwg = doc.GetElement(refer) as ImportInstance;
            GeometryElement geoElement = dwg.get_Geometry(new Options());
            GeometryObject geoObj = dwg.GetGeometryObjectFromReference(refer);
            // 读取图层中的所有图形
            foreach(var gObj in geoElement)
            {
                GeometryInstance geomInstance = gObj as GeometryInstance;
                if(null != geomInstance)
                {
                    foreach(var insObj in geomInstance.GetInstanceGeometry())
                    {
                        try
                        {
                            // 图层是否相同
                            if(doc.IsNotEqualLayerName(geoObj, insObj))
                            {
                                continue;
                            }
                            // curves 变量
                            List<XYZ> xyzs = new List<XYZ>();

                            #region 判断图形
                            if(insObj is Arc arc)
                            {
                                xyzs.Add(arc.Center);
                            }
                            #endregion

                            if(xyzs.Count == 0)
                            {
                                continue;
                            }
                            curveModels.Add(new CADCurveModel() { XYZs = xyzs });
                        }
                        catch(Exception e)
                        {
                            _ = e.Message;
                        }
                    }
                }
            }

            return curveModels;
        }


        /// <summary>
        /// 通过定位点及半径创建两段圆弧
        /// </summary>
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
