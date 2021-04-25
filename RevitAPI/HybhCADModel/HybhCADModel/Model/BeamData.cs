using Autodesk.Revit.DB;

namespace HybhCADModel.Model
{
    class Data
    {
        public Curve LocationCurve { get; set; }
        public XYZ LocationPoint { get; set; }
        public string Name { get; set; }
    }
}
