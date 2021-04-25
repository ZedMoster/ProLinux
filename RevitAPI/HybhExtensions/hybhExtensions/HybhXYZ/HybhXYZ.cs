using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace Xml
{
    public static class XYZExtensions
    {
        /// <summary>
        /// 定位点坐标转换
        /// </summary>
        /// <param name="xyz"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static XYZ Transform(this XYZ xyz, Transform transform)
        {
            return transform.OfPoint(xyz);
        }

        /// <summary>
        /// 逆时针旋转90° /*且Z轴置为0*/
        /// </summary>
        /// <param name="xyz"></param>
        /// <returns></returns>
        public static XYZ Rotation(this XYZ xyz)
        {
            return new XYZ(-xyz.Y, xyz.X, xyz.Z);
        }

        /// <summary>
        /// 向量拍平到平面
        /// </summary>
        /// <param name="xyz"></param>
        /// <returns></returns>
        public static XYZ Flatten(this XYZ xyz, double z = 0)
        {
            return new XYZ(xyz.X, xyz.Y, z);
        }

        /// <summary>
        /// 判断点是否在一个闭合轮廓内
        /// </summary>
        /// <returns> true 在内部</returns>
        public static bool IsInsideCurves(this XYZ TargetPoint, List<Curve> curves, double tolerance = 10000)
        {
            // 计数
            int insertCount = 0;

            #region  判断点是否在一个闭合轮廓内
            var TarPoint = new XYZ(TargetPoint.X, TargetPoint.Y, 0);
            Line rayLine = Line.CreateBound(TarPoint, TarPoint.Add(XYZ.BasisX * tolerance));
            foreach (var item in curves)
            {
                var interResult = rayLine.Flatten().Intersect(item.Flatten());
                if (interResult == SetComparisonResult.Overlap)
                {
                    insertCount += 1;
                }
            }
            #endregion

            return insertCount % 2 == 1;
        }

        /// <summary>
        /// 获取矩形旋转的角度
        /// </summary>
        /// <param name="YXZs"> 图形给定位点</param>
        /// <returns> 图形旋转角度</returns>
        public static double GetRoation(this List<XYZ> YXZs)
        {
            // 定义参数
            double angle;

            // 获取尺寸参数
            var p0 = YXZs[0];
            var p1 = YXZs[1];
            var p2 = YXZs[2];
            var localPoint = (p0 + p2) / 2;
            var LineMidPoint = (p1 + p2) / 2;

            #region 选择定位构件
            Line line = Line.CreateBound(localPoint, LineMidPoint);
            var lineDirection = line.Direction;
            var _angle = lineDirection.AngleTo(XYZ.BasisX);
            if (_angle > Math.PI / 4)
            {
                angle = localPoint.Y > LineMidPoint.Y ? lineDirection.AngleTo(XYZ.BasisX.Negate()) * -1 :
                        lineDirection.AngleTo(XYZ.BasisX.Negate());
            }
            else
            {
                angle = localPoint.Y > LineMidPoint.Y ? _angle : -_angle;
            }
            #endregion

            return angle;
        }
    }
}
