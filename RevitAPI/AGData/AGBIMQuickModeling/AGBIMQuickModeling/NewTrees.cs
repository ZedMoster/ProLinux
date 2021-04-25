using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using CADReader.View;

using RvtTxt;

namespace CADReader
{
    [Transaction(TransactionMode.Manual)]
    public class NewTrees : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // 定义变量
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            _ = new CGeoNode();
            // 创建基于点的模型
            WPFHowNewTree howNewTree = new WPFHowNewTree(uidoc);
            _ = new System.Windows.Interop.WindowInteropHelper(howNewTree)
            {
                Owner = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle
            };
            howNewTree.Show();

            return Result.Succeeded;
        }
    }
}
