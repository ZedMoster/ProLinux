using System.Collections.Generic;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using HybhCADModel.Extensions;
using HybhCADModel.Lib;
using HybhCADModel.Units;

namespace HybhCADModel.App
{
    [Transaction(TransactionMode.Manual)]
    class Text : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;
            var sel = uidoc.Selection;

            CADPickobject pickobject = new CADPickobject();
            var result_model = pickobject.GetReference(sel, out Reference reference_model);
            if(!result_model)
            {
                ShowResult.Print("No pick reference_model");
                return Result.Failed;
            }
            CADModel model = new CADModel();
            List<Model.CAD_Model> models = model.GetLayerCurves(doc, reference_model);
            var curves = models.GetCurves();
            var group = curves.GroupCurves();

            ShowResult.Print($"合并后获取闭合区域个数：{group.Count}");

            return Result.Succeeded;
        }
    }
}
