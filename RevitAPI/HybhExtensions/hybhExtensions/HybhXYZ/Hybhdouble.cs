using System;

namespace Xml
{
    public static class DoubleExtensions
    {
        /// <summary>
        /// double 误差范围内相等
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static bool IsAlmostEqualTo(this double p0, double p1, double tolerance = 0.0001)
        {
            return Math.Abs(p0 - p1) < tolerance;
        }
    }
}
