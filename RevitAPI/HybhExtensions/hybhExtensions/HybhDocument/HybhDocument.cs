using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace Xml.HybhDocument
{
    public static class HybhDocument
    {
        /// <summary>
        /// 通过点位创建地形
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="xyzs"></param>
        /// <returns></returns>
        public static bool CreateTopography(this Document doc, List<XYZ> xyzs)
        {
            // PolyCurve.GetCoordinates 点位去重
            if (xyzs.FirstOrDefault().IsAlmostEqualTo(xyzs.LastOrDefault()))
            {
                xyzs.RemoveAt(xyzs.Count - 1);
            }
            Transaction t = new Transaction(doc, "创建地形");
            t.Start();
            try
            {
                Autodesk.Revit.DB.Architecture.TopographySurface.Create(doc, xyzs);
                t.Commit();
                return true;
            }
            catch (Exception e)
            {
                _ = e.Message;
                t.RollBack();
                return false;
            }
        }
    }
}
