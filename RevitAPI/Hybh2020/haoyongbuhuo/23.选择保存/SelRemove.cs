using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Linq;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class SelRemove : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // 定义变量
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            if (Run.Running(Strings.key))
            {
                // 文件名
                var title = doc.Title.Split('.')[0];
                // 获取当前窗口选中的构件 IDs
                var sels = sel.GetElementIds().ToList();

                if (sels.Count == 0)
                {
                    TaskDialog.Show("提示", "需要选中至少一个构件!");
                    return Result.Failed;
                }
                else
                {
                    // -= 删除
                    TempFile.DelSelIdsToFile(sels, title);
                }
            }
            

            return Result.Succeeded;
        }
    }
}
