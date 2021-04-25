using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace AppRevitEventArgs
{
    [Transaction(TransactionMode.Manual)]
    class CMD : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            // 事件方法
            RevitSavingArgs revitSaving = new RevitSavingArgs();

            // 订阅事件
            uiapp.Idling
                += revitSaving.Application_Idling;

            // 保存文件
            doc.DocumentSaving
                += revitSaving.DocSavingEventArgs;

            return Result.Succeeded;
        }
    }
}
