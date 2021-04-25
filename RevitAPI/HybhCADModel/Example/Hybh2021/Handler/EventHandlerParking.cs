using System.Collections.Generic;
using System.Windows;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace CADReader.Handler
{
    class EventHandlerParking : IExternalEventHandler
    {
        private readonly string tranName = "边线停车";
        public ToDo ToDoData { get; set; }
        public void Execute(UIApplication app)
        {
            // 全局参数
            Document doc = app.ActiveUIDocument.Document;
            if (doc.ActiveView is View3D)
            {
                MessageBox.Show("不支持在三维视图创建", "警告");
                return;
            }

            // 图形
            CADElement CADModel = new CADElement();
            var models = CADModel.GetLayerCurves(doc, ToDoData.ReferenceCurves);
            // 获取停车位边线
            var parkLine = ParkingCurve(models);
            if (parkLine.Count == 0)
            {
                MessageBox.Show("图纸图层信息读取错误", "提示");
                return;
            }
            // 类型
            if (!(ToDoData.SelectElement.Element is FamilySymbol familySymbol))
            {
                MessageBox.Show("未指定族类型", "提示");
                return;
            }

            TransactionGroup T = new TransactionGroup(doc, "创建车位");
            T.Start();
            // 更新参数
            UpdataParameter(doc, familySymbol, ToDoData);
            // 创建车位
            var count = doc.NewInstanceCurves(familySymbol, parkLine, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
            _ = count > 0 ? T.Assimilate() : T.RollBack();

            MessageBox.Show($"创建个数：{count}");
        }
        public string GetName()
        {
            return tranName;
        }

        /// <summary>
        /// 更新类型参数
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="elementType"></param>
        /// <param name="ToDoData"></param>
        private void UpdataParameter(Document doc, ElementType elementType, ToDo ToDoData)
        {
            var t = new Transaction(doc, "update");
            t.Start();
            elementType.LookupParameter("车位宽").Set(ToDoData.Width.MillimeterToFeet());
            elementType.LookupParameter("车位长").Set(ToDoData.Lengh.MillimeterToFeet());
            t.Commit();
        }

        /// <summary>
        /// 获取停车位定位边线
        /// </summary>
        private List<Curve> ParkingCurve(List<CADCurveModel> models)
        {
            List<Curve> curves = new List<Curve>();
            foreach (var m in models)
            {
                foreach (var c in m.Curves)
                {
                    if (c is Line && Runing(c.Length.FeetToMillimeter(), 100))
                    {
                        curves.Add(c.Flatten());
                    }
                }
            }
            return curves;
        }

        /// <summary>
        /// 车位长 误差
        /// </summary>
        /// <param name="a"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        private bool Runing(double a, double t)
        {
            return a >= ToDoData.Lengh - t && a <= ToDoData.Lengh + t;
        }
    }
}
