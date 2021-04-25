using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using CADReader.WPF;

namespace CADReader.Command
{
    [Transaction(TransactionMode.Manual)]
    public class AppParking : IExternalCommand
    {
        /// <summary>
        /// 基于线创建停车位
        /// </summary>
        /// <param name="commandData"></param>
        /// <param name="message"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // 定义变量
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            // 窗口实例
            WPFParking wpfParking = new WPFParking(uidoc.Document);

            // 窗口停靠顶部
            new System.Windows.Interop.WindowInteropHelper(wpfParking)
            {
                Owner = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle
            };

            // 非模式窗体
            wpfParking.Show();

            return Result.Succeeded;
        }
    }
}