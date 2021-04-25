using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using CADReader.View;

using RvtTxt;

namespace CADReader
{
    [Transaction(TransactionMode.Manual)]
    public class NewBoxExterior : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // 定义变量
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            _ = new CGeoNode();
            // 创建建筑轮廓
            WPFHowNewBoxExterior howNewBoxExterior = new WPFHowNewBoxExterior(uidoc);
            _ = new System.Windows.Interop.WindowInteropHelper(howNewBoxExterior)
            {
                Owner = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle
            };
            howNewBoxExterior.Show();

            return Result.Succeeded;
        }
    }
}
