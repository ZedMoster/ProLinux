using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class LoginInUser : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Window window = new WPFLogin();
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
