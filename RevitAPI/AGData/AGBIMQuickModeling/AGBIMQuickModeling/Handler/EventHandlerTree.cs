using System.Windows;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace CADReader.Handler
{
    class EventHandlerTree : IExternalEventHandler
    {
        private readonly string tranName = "创建植物";
        public ToDo ToDoData { get; set; }
        public void Execute(UIApplication app)
        {
            Document doc = app.ActiveUIDocument.Document;

            if(doc.ActiveView is View3D)
            {
                $"{tranName}-debug-0001-{doc.ActiveView.Id}".ToDebug();
                MessageBox.Show("不支持在3D视图创建!", "警告");
                return;
            }

            // 图形
            CADElement CADModel = new CADElement();
            var models = CADModel.GetLayerArcCenterPoints(doc, ToDoData.ReferenceCurves);
            $"{tranName}-debug-0002-{models.Count}".ToDebug();
            if(models.Count == 0)
            {
                MessageBox.Show("图纸图层信息读取错误", "提示");
                return;
            }

            int count = 0;
            TransactionGroup T = new TransactionGroup(doc, "基于点模型");
            T.Start();
            foreach(var cad in models)
            {
                var c = doc.NewInstancePoints((FamilySymbol)ToDoData.SelectElement.Element, cad.XYZs);
                count += c;
            }
            _ = count > 0 ? T.Assimilate() : T.RollBack();
            $"{tranName}-debug-0003-{count}".ToDebug();
            MessageBox.Show($"总计创建：{count}");
        }
        public string GetName()
        {
            return tranName;
        }
    }
}
