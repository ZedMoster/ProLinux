using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace HybhCADModel.Model
{
    public class CAD_Model
    {
        /// <summary>
        /// 模型定位线
        /// </summary>
        public IList<Curve> Curves { get; set; }
        /// <summary>
        /// 模型定位点
        /// </summary>
        public IList<XYZ> LocationPoints { get; set; }
    }
}
