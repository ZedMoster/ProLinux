using Autodesk.Revit.DB;

namespace HybhCADModel.Extensions
{
    public static class DoubleExtensions
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
        /// 角度 --> 弧度
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double AngleToRadians(this double value)
        {
            return (System.Math.PI / 180.0) * value;
        }

        /// <summary>
        /// 数字字符串转换
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double ToFloat(this string value)
        {
            double.TryParse(value, out double result);
            return result;
        }
    }
}
