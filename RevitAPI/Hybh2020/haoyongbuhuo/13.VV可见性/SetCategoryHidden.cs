using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace hybh
{
    [Regeneration(RegenerationOption.Manual)]
    [Transaction(TransactionMode.Manual)]
    class SetCategoryHidden:IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            if (Run.Running(Strings.key))
            {
                WPFSetCategoryHidden windows = new WPFSetCategoryHidden(commandData);
                _ = new System.Windows.Interop.WindowInteropHelper(windows)
                {
                    Owner = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle
                };
                windows.Show();
            }

            return Result.Succeeded;
        }
    }

   
}
