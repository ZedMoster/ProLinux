using Autodesk.Revit.DB;

namespace HybhCADModel.Extensions
{
    public static class XYZExtensions
    {
        /// <summary>
        /// 向量拍平
        /// </summary>
        /// <param name="xyz"></param>
        /// <returns></returns>
        public static XYZ Flatten(this XYZ xyz, double z = 0)
        {
            return new XYZ(xyz.X, xyz.Y, z);
        }

        /// <summary>
        /// 是否可以创建 Line
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
