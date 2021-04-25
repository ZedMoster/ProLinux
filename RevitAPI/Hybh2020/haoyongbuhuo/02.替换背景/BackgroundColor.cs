using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class BackgroundColor : IExternalCommand
    {
        readonly RegistryStorage Registry = new RegistryStorage();
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;

            // 程序是否注册可运行
            if (Run.Running(Strings.key))
            {
                // 背景色颜色设置  B0_r, B0_g, B0_b    B1_r, B1_g, B1_b
                GetRegistryBackgroundColor backgroundColor = new GetRegistryBackgroundColor();
                var change = Registry.OpenAfterStart("Bchange");

                if (change != "0" || change == null)
                {
                    backgroundColor._r = 8;
                    backgroundColor._g = 8;
                    backgroundColor._b = 8;

                    uiapp.Application.BackgroundColor = backgroundColor.BackgroundColor("B0_r", "B0_g", "B0_b");
                    Registry.SaveBeforeExit("Bchange", "0");
                }
                else
                {
                    backgroundColor._r = 245;
                    backgroundColor._g = 245;
                    backgroundColor._b = 245;
                    uiapp.Application.BackgroundColor = backgroundColor.BackgroundColor("B1_r", "B1_g", "B1_b");
                    Registry.SaveBeforeExit("Bchange", "1");
                }
            }

            return Result.Succeeded;
        }
    }
}
