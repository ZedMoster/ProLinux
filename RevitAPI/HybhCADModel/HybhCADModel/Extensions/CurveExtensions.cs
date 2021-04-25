using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using HybhCADModel.Model;

namespace HybhCADModel.Extensions
{
    public static class CurveExtensions
    {
        /// <summary>
        /// Line 拍平
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static Line Flatten(this Curve line, double z = 0)
        {
            return Line.CreateBound(line.GetEndPoint(0).Flatten(z), line.GetEndPoint(1).Flatten(z));
        }

        public static List<Curve> GetCurves(this List<Model.CAD_Model> models)
        {
            var curves = new List<Curve>();
            foreach(var mo in models)
            {
                foreach(var curve in mo.Curves)
                {
                    curves.Add(curve);
                }
            }
            return curves;
        }

        /// <summary>
        /// 定位线是否平行
        /// </summary>
        /// <param name="line"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool IsParallel(this Curve curve, Curve other_curve)
        {
            if(curve is Line line && other_curve is Line other)
            {
                return line.Direction.IsAlmostEqualTo(other.Direction) || line.Direction.IsAlmostEqualTo(other.Direction.Negate());
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 获取闭合多段线
        /// </summary>
        /// <param name="curves"></param>
        /// <param name="ignore"></param>
        /// <returns></returns>
        public static List<CAD_Model> GroupCurves(this List<Curve> curves, double ignore = 0.001)
        {
            List<CAD_Model> curvesModels = new List<CAD_Model>();
            var queue = new Queue<Curve>();
            while(curves.Count > 0)
            {
                queue.Enqueue(curves.LastOrDefault());
                List<Curve> shape = new List<Curve>();
                while(queue.Count > 0)
                {
                    var current_curve = queue.Dequeue();
                    shape.Add(current_curve);
                    var current_points = current_curve.Tessellate();
                    foreach(var mathch in curves)
                    {
                        var P1 = mathch.Tessellate()[0];
                        foreach(var P2 in current_points)
                        {
                            var distance = P1.DistanceTo(P2);
                            if(distance <= ignore && !shape.Contains(mathch))
                            {
                                queue.Enqueue(mathch);
                                break;
                            }
                        }
                    }
                    var _curves = new List<Curve>();
                    foreach(var item in curves)
                    {
                        if(!shape.Contains(item))
                        {
                            _curves.Add(item);
                        }
                    }
                    curves = _curves;
                }
                if(shape.Count > 1)
                {
                    curvesModels.Add(new CAD_Model() { Curves = shape });
                }
            }
            return curvesModels;
        }
    }
}
