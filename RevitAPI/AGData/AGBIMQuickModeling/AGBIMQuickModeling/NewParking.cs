using System.Linq;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using CADReader.View;

using RvtTxt;

namespace CADReader
{
    [Transaction(TransactionMode.Manual)]
    public class NewParking : IExternalCommand
    {
        private const string familyname = "停车位基于线";
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // 定义变量
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            #region 载入族
            var toloading = doc.TCollector<FamilySymbol>().FirstOrDefault(i => i.FamilyName == familyname) == null;
            if(toloading)
            {
                //var path = @"C:\temp\停车位基于线.rfa";
                var path = RevitExtensions.GetRfaFilePath(familyname);
                uidoc.Document.LoadRfaFilePath(path);
            }
            #endregion

            _ = new CGeoNode();
            // 创建区域楼板
            WPFHowNewParking howNewParking = new WPFHowNewParking(uidoc);
            _ = new System.Windows.Interop.WindowInteropHelper(howNewParking)
            {
                Owner = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle
            };
            howNewParking.Show();

            return Result.Succeeded;
        }
    }
}
