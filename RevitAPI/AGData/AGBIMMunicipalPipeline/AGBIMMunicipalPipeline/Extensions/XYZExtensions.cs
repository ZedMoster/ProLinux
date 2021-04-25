using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace AGBIMMunicipalPipeline.Extensions
{
    public static class XYZExtensions
    {
        /// <summary>
        /// 拍平
        /// </summary>
        /// <param name="point"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static XYZ GetXYZ(this XYZ point, double z)
        {
            return point.Add(new XYZ(0, 0, z));
        }
    }
}
