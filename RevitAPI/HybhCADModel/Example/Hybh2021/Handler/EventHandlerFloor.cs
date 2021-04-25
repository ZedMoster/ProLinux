using System.Collections.Generic;
using System.Windows;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using CADReader.Model;

namespace CADReader.Handler
{
    class EventHandlerFloor : IExternalEventHandler
    {
        private readonly string tranName = "区域楼板";
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

            // 获取定位边线
            CADElement CADModel = new CADElement();
            var models = CADModel.GetLayerCurves(doc, ToDoData.ReferenceCurves);
            if (models.Count == 0)
            {
                MessageBox.Show("图纸图层信息读取错误", "提示");
                return;
            }

            // 依据名称创建楼板
            GetFloorTypeByName floorTypeByName = new GetFloorTypeByName();
            TransactionGroup T = new TransactionGroup(doc, "识别楼板");
            T.Start();
            FloorType floorType = floorTypeByName.Get(doc, ToDoData.SelectElement);
            var Push = CreateFloor(doc, models, floorType);
            // 处理事务组
            _ = Push.Count == 0 ? T.RollBack() : T.Assimilate();

            MessageBox.Show($"创建个数：{Push.Count}");
        }
        public string GetName()
        {
            return tranName;
        }

        /// <summary>
        /// 创建楼板
        /// </summary>
        private List<bool> CreateFloor(Document doc, List<CADCurveModel> curveModels, FloorType floorType,
             double elva = 0.0, bool isStructure = false)
        {
            List<bool> Push = new List<bool>();
            Transaction t = new Transaction(doc);
            t.Start("newFloor");
            t.NoFailure();
            foreach (var model in curveModels)
            {
                CurveArray curveArray = model.Curves.ToCurveArray();
                if (curveArray.IsEmpty)
                {
                    continue;
                }
                try
                {
                    var floor = doc.Create.NewFloor(curveArray, floorType, doc.ActiveView.GenLevel, isStructure);
                    // 设置标高偏移值
                    floor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).Set(elva.MillimeterToFeet());
                    Push.Add(true);
                }
                catch { }
            }
            _ = Push.Count == 0 ? t.RollBack() : t.Commit();
            return Push;
        }
    }
}
