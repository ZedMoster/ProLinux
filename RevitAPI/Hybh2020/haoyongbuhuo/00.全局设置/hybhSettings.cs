using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class HybhSettings : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            if (Run.Running(Strings.key))
            {
                WPFhybhSettings wPFhybh = new WPFhybhSettings();
                new System.Windows.Interop.WindowInteropHelper(wPFhybh)
                {
                    Owner = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle
                };
                wPFhybh.ShowDialog();
            }
            return Result.Succeeded;
        }
    }
}
