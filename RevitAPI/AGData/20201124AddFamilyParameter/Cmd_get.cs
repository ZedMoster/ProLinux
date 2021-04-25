using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace AddFamilyParameter
{
    [Transaction(TransactionMode.Manual)]
    public class Cmd_get : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // 定义变量
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            GetParameters getParameters = new GetParameters(doc);

            var res = getParameters.GetData();
            TaskDialog.Show("0", res.Types.ToString());

            return Result.Succeeded;
        }
    }
}
