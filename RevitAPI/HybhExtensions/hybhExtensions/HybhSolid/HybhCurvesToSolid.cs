using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace Xml.HybhSolid
{
    public static class HybhCurvesToSolid
    {
        /// <summary>
        /// curves 转 solid
        /// </summary>
        public static Solid CurvesToSolid(this List<Curve> curves, double height = 10)
        {
            Solid solid = null;
            CurveLoop baseLoop = CurveLoop.Create(curves);
            // 判断curveloop 是否闭合
            if (baseLoop.IsOpen())
            {
                return solid;
            }
            try
            {
                List<CurveLoop> loopList = new List<CurveLoop>() { baseLoop };
                solid = GeometryCreationUtilities.CreateExtrusionGeometry(loopList, XYZ.BasisZ, height);
            }
            catch (Exception e)
            {
                _ = e.Message;
                // Curve Loops do not satisfy the input requirements.
            }
            return solid;
        }

        /// <summary>
        /// 判断 point 是否再 Solid 内
        /// </summary>
        public static bool IsPointInSolid(this Solid solid, XYZ point, double tolerance = 0.0001)
        {
            if (solid != null)
            {
                SolidCurveIntersectionOptions sco = new SolidCurveIntersectionOptions
                {
                    ResultType = SolidCurveIntersectionMode.CurveSegmentsInside
                };
                Line line = Line.CreateBound(point, point.Add(XYZ.BasisZ));
                SolidCurveIntersection sci = solid.IntersectWithCurve(line, sco);
                for (int i = 0; i < sci.SegmentCount; i++)
                {
                    Curve curve = sci.GetCurveSegment(i);
                    curve.MakeUnbound();
                    var result = curve.Project(point);
                    if (point.IsAlmostEqualTo(result.XYZPoint, tolerance))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
