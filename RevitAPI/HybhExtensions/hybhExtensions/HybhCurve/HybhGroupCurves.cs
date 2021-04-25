using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace Xml.HybhCurve
{
    public static class HybhGroupCurves
    {
        /// <summary>
        /// 线段成组-获取闭合多段线
        /// </summary>
        /// <param name="curves"></param>
        /// <param name="ignore"></param>
        /// <returns></returns>
        public static List<CADModel> GroupCurves(this List<Curve> curves, double ignore = 0.001)
        {
            List<CADModel> curvesModels = new List<CADModel>();
            var queue = new Queue<Curve>();
            while (curves.Count > 0)
            {
                queue.Enqueue(curves.LastOrDefault());
                List<Curve> shape = new List<Curve>();
                while (queue.Count > 0)
                {
                    var current_curve = queue.Dequeue();
                    shape.Add(current_curve);
                    var current_points = current_curve.Tessellate();
                    foreach (var mathch in curves)
                    {
                        var P1 = mathch.Tessellate()[0];
                        foreach (var P2 in current_points)
                        {
                            var distance = P1.DistanceTo(P2);
                            if (distance <= ignore && !shape.Contains(mathch))
                            {
                                queue.Enqueue(mathch);
                                break;
                            }
                        }
                    }
                    var _curves = new List<Curve>();
                    foreach (var item in curves)
                    {
                        if (!shape.Contains(item))
                        {
                            _curves.Add(item);
                        }
                    }
                    curves = _curves;
                }
                if (shape.Count > 1)
                {
                    curvesModels.Add(new CADModel() { Curves = shape });
                }
            }
            return curvesModels;
        }

    }
}
