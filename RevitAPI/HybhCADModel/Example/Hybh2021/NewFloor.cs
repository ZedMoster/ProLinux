using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using CADReader.View;

using RvtTxt;

namespace CADReader.Command
{
    [Transaction(TransactionMode.Manual)]
    class NewFloor : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // 定义变量
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            _ = new CGeoNode();
            // 创建区域楼板
            WPFHowNewFloor howNewFloor = new WPFHowNewFloor(uidoc);
            _ = new System.Windows.Interop.WindowInteropHelper(howNewFloor)
            {
                Owner = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle
            };
            howNewFloor.Show();

            return Result.Succeeded;
        }
    }
}
