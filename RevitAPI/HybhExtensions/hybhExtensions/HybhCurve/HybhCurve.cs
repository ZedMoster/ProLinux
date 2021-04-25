using Autodesk.Revit.DB;

namespace Xml
{
    public static class CurvesExtensions
    {
        /// <summary>
        /// 基于两点创建线段
        /// </summary>
        /// <param name="start"> 起点</param>
        /// <param name="end"> 终点</param>
        /// <returns> 起点到终点线段Line</returns>
        public static Line ByStartPointEndPoint(this XYZ start, XYZ end)
        {
            return Line.CreateBound(start, end);
        }

        /// <summary>
        /// 基于起点与方向创建射线
        /// </summary>
        /// <param name="origin"> 起点</param>
        /// <param name="direction"> 方向</param>
        /// <returns> 基于点的射线 Line</returns>
        public static Line ByStartPointDirectionLength(this XYZ origin, XYZ direction)
        {
            return Line.CreateUnbound(origin, direction);
        }

        /// <summary>
        /// 获取line 基于BasisX 的旋转弧度值
        /// </summary>
        /// <param name="line"> 线段</param>
        /// <returns> 获取线段方向与 BasisX 方向的弧度值</returns>
        public static double BasisXRotation(this Line line)
        {
            return (line.GetEndPoint(1) - line.GetEndPoint(0)).Flatten().AngleTo(XYZ.BasisX);
        }

        /// <summary>
        /// 获取line 基于BasisY 的旋转弧度值
        /// </summary>
        /// <param name="line"></param>
        /// <returns> 获取线段方向与 BasisY 方向的弧度值</returns>
        public static double BasisYRotation(this Line line)
        {
            return (line.GetEndPoint(1) - line.GetEndPoint(0)).Flatten().AngleTo(XYZ.BasisY);
        }

        /// <summary>
        /// 获取Line 基于给定Basis方向的旋转弧度值
        /// </summary>
        /// <param name="line"></param>
        /// <returns>获取线段方向与 Basis 方向的弧度值</returns>
        public static double BasisRotation(this Line line, XYZ Basis)
        {
            return (line.GetEndPoint(1) - line.GetEndPoint(0)).Flatten().AngleTo(Basis);
        }

        /// <summary>
        /// Line 坐标转换
        /// </summary>
        /// <param name="line"> Line 坐标准换</param>
        /// <returns></returns>
        public static Line OfLine(this Transform transform, Line line)
        {
            return Line.CreateBound(line.GetEndPoint(0).Transform(transform), line.GetEndPoint(1).Transform(transform));
        }

        /// <summary>
        /// Curve-Line 拍平到平面
        /// </summary>
        /// <param name="curve"> Curve 线段拍平到面Z上</param>
        /// <param name="z"> 平面Z轴坐标值，默认：0</param>
        /// <returns> 拍平后的Line</returns>
        public static Curve Flatten(this Curve curve, double z = 0)
        {
            if(curve is Line line)
            {
                return Line.CreateBound(line.GetEndPoint(0).Flatten(z), line.GetEndPoint(1).Flatten(z));
            }
            else
            {
                return curve;
            }
        }

        /// <summary>
        /// 两个点是否可以创建直线 Line
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <returns></returns>
        public static bool CanCurve(this XYZ p0, XYZ p1)
        {
            return p0.DistanceTo(p1) > 0.001;
        }
    }
}
