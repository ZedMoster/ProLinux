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
            if(doc.ActiveView is View3D)
            {
                $"{tranName}-debug-0001-".ToDebug();
                MessageBox.Show("不支持在三维视图创建", "警告");
                return;
            }

            // 获取定位边线
            CADElement CADModel = new CADElement();
            var models = CADModel.GetLayerCurves(doc, ToDoData.ReferenceCurves);
            $"{tranName}-debug-0002-{models.Count}".ToDebug();
            var group = models.ToCurves().GroupCurves();
            $"{tranName}-debug-0003-{group.Count}".ToDebug();
            if(group.Count == 0)
            {
                MessageBox.Show("图纸图层无可创建的闭合区域", "提示");
                return;
            }

            // 依据名称创建楼板
            GetFloorTypeByName floorTypeByName = new GetFloorTypeByName();
            TransactionGroup T = new TransactionGroup(doc, "识别楼板");
            T.Start();
            FloorType floorType = floorTypeByName.Get(doc, ToDoData.SelectElement);
            $"{tranName}-debug-0004-{floorType.Name}".ToDebug();
            var Push = CreateFloor(doc, models, floorType);
            // 处理事务组
            _ = Push.Count == 0 ? T.RollBack() : T.Assimilate();
            $"{tranName}-debug-0005-{Push.Count}".ToDebug();
            MessageBox.Show($"总计创建：{Push.Count}");
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
            foreach(var model in curveModels)
            {
                CurveArray curveArray = model.Curves.ToCurveArray();
                // 闭合定位线
                if(curveArray.IsEmpty)
                {
                    continue;
                }
                 
                var floor = doc.Create.NewFloor(curveArray, floorType, doc.ActiveView.GenLevel, isStructure);
                // 设置标高偏移值
                floor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).Set(elva.MillimeterToFeet());
                Push.Add(true);
            }
            _ = Push.Count == 0 ? t.RollBack() : t.Commit();
            return Push;
        }
    }
}
