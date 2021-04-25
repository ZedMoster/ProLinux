using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class ColorSplasherOk : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            if (Run.Running(Strings.key))
            {
                GetRegistryBackgroundColor backgroundColor = new GetRegistryBackgroundColor
                {
                    // OK -- 绿色
                    _r = 0,
                    _g = 128,
                    _b = 0
                };

                Color color = backgroundColor.BackgroundColor("F0_r", "F0_g", "F0_b");
                var value = "OK";
                try
                {
                    ColorWithModel model = new ColorWithModel();
                    model.ElementOverrideGraphicSetting(uidoc, doc, sel, color, value);
                }
                catch (Exception e)
                {

                    TaskDialog.Show("提示", e.Message + Strings.error);
                }
            }

            return Result.Succeeded;
        }
    }
}
