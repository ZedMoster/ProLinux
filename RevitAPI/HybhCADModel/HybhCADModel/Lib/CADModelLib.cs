using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace HybhCADModel.Lib
{
    class CADModelLib
    {
        /// <summary>
        /// 通过定位点及半径创建两段圆弧
        /// </summary>
        public static List<Curve> CreateArcByCenterR(XYZ center, double radius, XYZ xAxis, XYZ yAxis)
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
        public static List<Curve> CreateEllipseByCenter(XYZ center, double radiusX, double radiusY, XYZ xAxis, XYZ yAxis)
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
