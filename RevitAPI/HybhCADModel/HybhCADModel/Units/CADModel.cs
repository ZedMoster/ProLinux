using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using HybhCADModel.Extensions;
using HybhCADModel.Lib;
using HybhCADModel.Model;

namespace HybhCADModel.Units
{
    class CADModel
    {
        /// <summary>
        /// 获取CAD图纸中选择图层的所有定位线
        /// </summary>
        public List<CAD_Model> GetLayerCurves(Document doc, Reference refer)
        {
            // 定义变量
            List<CAD_Model> curveModels = new List<CAD_Model>();
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
                        // 图层是否相同
                        if(doc.IsNotEqualLayerName(geoObj, insObj))
                        {
                            continue;
                        }
                        // curves 变量
                        List<Curve> Curves = new List<Curve>();

                        // 判断图形形状
                        if(insObj is Line line)
                        {
                            Curves.Add(line);
                        }
                        if(insObj is PolyLine polyLine)
                        {
                            var coordinates = polyLine.GetCoordinates();
                            for(int i = 0; i < coordinates.Count; i++)
                            {
                                if(i < coordinates.Count - 1)
                                {
                                    if(coordinates[i].CanCurve(coordinates[i + 1]))
                                    {
                                        Curves.Add(Line.CreateBound(coordinates[i], coordinates[i + 1]));
                                    }
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
                                Curves = CADModelLib.CreateArcByCenterR(center, radius, xAxis, yAxis);
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
                                Curves = CADModelLib.CreateEllipseByCenter(center, radiusX, radiusY, xAxis, yAxis);
                            }
                        }

                        if(Curves.Count == 0)
                        {
                            continue;
                        }
                        curveModels.Add(new CAD_Model() { Curves = Curves });
                    }
                }
            }

            return curveModels;
        }

        /// <summary>
        /// 获取 Arc 中心点
        /// </summary>
        public List<CAD_Model> GetLayerArcCenters(Document doc, Reference refer)
        {
            // 定义变量
            List<CAD_Model> curveModels = new List<CAD_Model>();
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
                        // 图层是否相同
                        if(doc.IsNotEqualLayerName(geoObj, insObj))
                        {
                            continue;
                        }
                        // curves 变量
                        List<XYZ> xyzs = new List<XYZ>();

                        // 判断图形
                        if(insObj is Arc arc)
                        {
                            xyzs.Add(arc.Center);
                        }

                        if(xyzs.Count == 0)
                        {
                            continue;
                        }
                        curveModels.Add(new CAD_Model() { LocationPoints = xyzs });
                    }
                }
            }

            return curveModels;
        }
    }
}
