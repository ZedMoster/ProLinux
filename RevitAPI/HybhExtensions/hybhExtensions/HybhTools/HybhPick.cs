using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace Xml
{
    /// <summary>
    /// 选择图纸中的图元
    /// </summary>
    public static class PickRefer
    {
        /// <summary>
        /// 选择CAD图元
        /// </summary>
        public static void Refer(this Selection sel, out Reference reference)
        {
            try
            {
                SelectionFilterCategoryEndsWith categoryEndsWith = new SelectionFilterCategoryEndsWith(".dwg");
                reference = sel.PickObject(ObjectType.PointOnElement, categoryEndsWith, "选择CAD图元");
            }
            catch { reference = null; }
        }

        /// <summary>
        /// 选择CAD图元列表
        /// </summary>
        public static void RefList(this Selection sel, out List<Reference> ReferList)
        {
            try
            {
                SelectionFilterCategoryEndsWith categoryEndsWith = new SelectionFilterCategoryEndsWith(".dwg");
                ReferList = sel.PickObjects(ObjectType.PointOnElement, categoryEndsWith, "选择多个CAD图元").ToList();
            }
            catch { ReferList = new List<Reference>(); }
        }
    }
}
