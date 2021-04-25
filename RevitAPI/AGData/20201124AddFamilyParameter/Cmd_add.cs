using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace AddFamilyParameter
{
    [Transaction(TransactionMode.Manual)]
    public class Cmd_add : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // 定义变量
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;


            AddParameter addParameter = new AddParameter(doc);
            addParameter.Set("测试key", BuiltInParameterGroup.PG_DATA, ParameterType.Length, true, 1);

            TaskDialog.Show("0", "good");

            return Result.Succeeded;
        }
    }
}
