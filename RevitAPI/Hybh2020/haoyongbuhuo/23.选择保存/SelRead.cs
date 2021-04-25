using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Linq;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class SelRead : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // 定义变量
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            // 程序是否注册可运行
            if (Run.Running(Strings.key))
            {
                // 文件名
                var title = doc.Title.Split('.')[0];
                // 获取历史保存的 IDs
                var sels = TempFile.ReadSelIdsFileToList(title);

                if (sels.Count == 0)
                {
                    TaskDialog.Show("提示", "不存在选择过的构件\n需先使用<新增选择>" );
                    return Result.Failed;
                }
                else
                {
                    // 选中构件
                    sel.SetElementIds(sels);
                }
            }
            

            return Result.Succeeded;
        }
    }
}
