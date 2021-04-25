using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace hybh
{
    static class HybhExtensions
    {
        /// <summary>
        /// 过滤器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static List<T> TCollector<T>(this Document doc)
        {
            return new FilteredElementCollector(doc).OfClass(typeof(T)).Cast<T>().ToList();
        }

        /// <summary>
        /// 过滤器-当前视图
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="doc"></param>
        /// <param name="viewId"></param>
        /// <returns></returns>
        public static List<T> TCollector<T>(this Document doc, ElementId viewId)
        {
            return new FilteredElementCollector(doc, viewId).OfClass(typeof(T)).Cast<T>().ToList();
        }

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
        /// 毫米 转 英尺
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double MillimeterToFeet(this double value)
        {
            return UnitUtils.Convert(value, DisplayUnitType.DUT_MILLIMETERS, DisplayUnitType.DUT_DECIMAL_FEET);
        }

        /// <summary>
        /// 英尺 转 毫米
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double FeetToMillimeter(this double value)
        {
            return UnitUtils.Convert(value, DisplayUnitType.DUT_DECIMAL_FEET, DisplayUnitType.DUT_MILLIMETERS);
        }

        /// <summary>
        /// 向量拍平到平面
        /// </summary>
        /// <param name="xyz"></param>
        /// <returns></returns>
        public static XYZ Flatten(this XYZ xyz)
        {
            return new XYZ(xyz.X, xyz.Y, 0);
        }
    }
}
