using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class ReloadDwg : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // 定义变量
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            ReloadCAD(uidoc, "重新载入");
            return Result.Succeeded;
        }

        private void ReloadCAD(UIDocument uidoc, string tranName)
        {
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;
            // 选择链接图纸信息
            Reference reference = null;

            #region 选择CAD图纸
            try
            {
                reference = sel.PickObject(ObjectType.Subelement, new PickCADFilter(), "选择需要重新载入的CAD图纸");
            }
            catch { }
            if (reference == null)
            {
                return;
            }
            #endregion

            // 获取图纸 CADLinkType
            var dwg = doc.GetElement(reference);
            var link = doc.GetElement(dwg.GetTypeId()) as CADLinkType;
            // 重新链接图纸
            Transaction t = new Transaction(doc, tranName);
            t.Start();
            link.Reload(new CADLinkOptions(true, doc.ActiveView.Id));
            t.Commit();

            MessageBox.Show(tranName + ":\n" + link.Name, "提示");
        }
    }
}
