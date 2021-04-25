using System;

using Autodesk.Revit.DB;

namespace Xml
{
    public static class UnitUtilsExtensions
    {
        /// <summary>
        /// 毫米 --> 英尺
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double MillimeterToFeet(this double value)
        {
            return UnitUtils.Convert(value, DisplayUnitType.DUT_MILLIMETERS, DisplayUnitType.DUT_DECIMAL_FEET);
        }

        /// <summary>
        /// 英尺 --> 毫米
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double FeetToMillimeter(this double value)
        {
            return UnitUtils.Convert(value, DisplayUnitType.DUT_DECIMAL_FEET, DisplayUnitType.DUT_MILLIMETERS);
        }

        /// <summary>
        /// 将数字字符串转为数字
        /// </summary>
        /// <param name="strNumber"> 数字字符串</param>
        /// <param name="size"> 放大倍数</param>
        /// <returns> 转换失败返回:0.00</returns>
        public static double Parse(this string strNumber, double size = 1)
        {
            var t = double.TryParse(strNumber, out double res);
            return t ? res * size : 0.0;
        }

        /// <summary>
        /// 角度 --> 弧度
        /// </summary>
        /// <param name="value"> 角度值</param>
        /// <returns></returns>
        public static double AngleToRadians(this double value)
        {
            return (Math.PI / 180.0) * value;
        }

        /// <summary>
        /// 弧度 --> 角度
        /// </summary>
        /// <param name="value"> 弧度值</param>
        /// <returns></returns>
        public static double RadiansToAngle(this double value)
        {
            return (180.0 / Math.PI) * value;
        }
    }
}
