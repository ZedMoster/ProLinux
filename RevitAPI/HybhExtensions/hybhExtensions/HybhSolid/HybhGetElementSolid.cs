using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace Xml
{
    public static class HybhGetElementSolid
    {
        /// <summary>
        /// 获取Element 对象 Solid
        /// </summary>
        /// <param name="element"> Element对象</param>
        /// <returns> 获取对象的 Solid 实体</returns>
        public static List<Solid> CurvesToSolid(this Element element)
        {
            List<Solid> solids = new List<Solid>();
            var geo = element.get_Geometry(new Options() { ComputeReferences = true });
            foreach (var obj in geo)
            {
                // 判断对象是否为 instance 对象
                if (obj is GeometryInstance instanceObj)
                {
                    foreach (var ins_obj in instanceObj.GetInstanceGeometry())
                    {
                        if (ins_obj is Solid ins_solid && ins_solid.SurfaceArea > 0)
                        {
                            solids.Add(ins_solid);
                        }
                    }
                }
                else
                {
                    if (obj is Solid solid && solid.SurfaceArea > 0)
                    {
                        solids.Add(solid);
                    }
                }
            }
            return solids;
        }
    }
}
