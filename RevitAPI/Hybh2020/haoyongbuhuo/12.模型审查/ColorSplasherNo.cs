using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MongoDB.Bson;
using System;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class ColorSplasherNo : IExternalCommand
    {
        // 注册列表文件读写方法
        private readonly RegistryStorage registryStorage = new RegistryStorage();
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            if (Run.Running(Strings.key))
            {
                try
                {
                    WPFColorSplasherNo wPF = new WPFColorSplasherNo();
                    wPF.ShowDialog();
                    if (wPF.IsHitTestVisible)
                    {
                        var ques = wPF.text.Text;
                        // 构件添加颜色
                        GetRegistryBackgroundColor backgroundColor = new GetRegistryBackgroundColor
                        {
                            // No -- 橘色
                            _r = 255,
                            _g = 70,
                            _b = 0
                        };

                        Color color = backgroundColor.BackgroundColor("F1_r", "F1_g", "F1_b");
                        var value = "NO";
                        ColorWithModel model = new ColorWithModel();
                        model.ElementOverrideGraphicSetting(uidoc, doc, sel, color, value, ques);

                        try
                        {
                            // 作者 文档 问题 时间
                            BsonDocument data = new BsonDocument();
                            var author = registryStorage.OpenAfterStart("name");
                            data.Add("author", author);
                            data.Add("document", doc.Title.Split('.')[0]);
                            data.Add("question", ques);
                            data.Add("time", DateTime.Now.ToString());

                            var collection = ClientMongoDB.GetClient("message");
                            collection.InsertOne(data);
                        }
                        catch { }
                    }
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
