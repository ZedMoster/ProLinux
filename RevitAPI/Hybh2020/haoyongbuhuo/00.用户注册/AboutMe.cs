using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class AboutMe : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            WPFAbout about = new WPFAbout();
            about.ShowDialog();

            return Result.Succeeded;
        }
    }
}
