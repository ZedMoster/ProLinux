using System.Collections.Generic;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Xml
{
    [Transaction(TransactionMode.Manual)]
    class App : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;
            var sel = uidoc.Selection;

            SelectionFilterByCategoryList categoryList = new SelectionFilterByCategoryList(new List<string> { "墙", "门", "窗" });

            var els = sel.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element, categoryList);
            ShowResult.Print(els.Count);

#if Revit2016
                        TaskDialog.Show("提示", "版本:2016");
#elif Revit2017
                        TaskDialog.Show("提示", "版本:2017");
#elif Revit2018
                        TaskDialog.Show("提示", "版本:2018");
#elif Revit2019
                        TaskDialog.Show("提示", "版本:2019");
#elif Revit2020
            TaskDialog.Show("提示", "版本:2020");
#elif Revit2021
                        TaskDialog.Show("提示", "版本:2021");
#else
                        TaskDialog.Show("提示", "版本:None");
#endif

            return 0;
        }
    }
}
