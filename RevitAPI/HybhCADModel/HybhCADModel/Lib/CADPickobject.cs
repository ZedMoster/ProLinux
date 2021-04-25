using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace HybhCADModel.Lib
{
    public class CADPickobject
    {
        /// <summary>
        /// 选择CAD对象返回 Reference
        /// </summary>
        public bool GetReference(Selection sel, out Reference reference, string status = "请点选<CAD>图层图元")
        {
            try
            {
                reference = sel.PickObject(ObjectType.PointOnElement, new FilterCAD(), status);
                return true;
            }
            catch
            {
                reference = null;
                return false;
            }
        }
    }
}
