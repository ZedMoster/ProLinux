using System.Collections.Generic;
using System.Linq;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace CADReader.Command
{
    [Transaction(TransactionMode.Manual)]
    class NewRoads : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // 定义变量
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            // 选择链接图纸信息
            Pickdwg pickdwg = new Pickdwg();

            #region 获取定位线
            pickdwg.Refer(sel, out Reference referenceModel);
            if (referenceModel == null)
            {
                MessageBox.Show("已取消");
                return Result.Cancelled;
            }
            CADElement CADModel = new CADElement();
            // 获取选择图形的定位点
            var models = CADModel.GetLayerCurves(doc, referenceModel);
            if (models.Count == 0)
            {
                return Result.Failed;
            }
            #endregion

            // 获取类型
            var eltypes = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralFraming)
                .WhereElementIsElementType().ToElements();
            var _thisType = eltypes.FirstOrDefault(i => (i as ElementType).FamilyName == "道路基于中心线") as FamilySymbol;

            TransactionGroup T = new TransactionGroup(doc, "创建道路");
            T.Start();
            foreach (var cad in models)
            {
                var curves = cad.Curves;
                NewRoad(doc, _thisType, curves);
            }
            T.Assimilate();


            MessageBox.Show($"总计墙段：{models.Count}");
            return Result.Succeeded;
        }

        private void NewRoad(Document doc, FamilySymbol symbol, List<Curve> curves)
        {
            if (!symbol.IsActive)
            {
                symbol.Activate();
            }
            foreach (var curve in curves)
            {
                #region 创建实例并更新参数
                Transaction transaction = new Transaction(doc);
                transaction.Start("创建实例");
                transaction.NoFailure();
                try
                {
                    // 创建
                    FamilyInstance instance = doc.Create.NewFamilyInstance(curve, symbol, doc.ActiveView.GenLevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                    // 初始化参数    起点标高偏移  终点标高偏移
                    instance.get_Parameter(BuiltInParameter.INSTANCE_REFERENCE_LEVEL_PARAM).Set(doc.ActiveView.GenLevel.Id);
                    instance.get_Parameter(BuiltInParameter.STRUCTURAL_BEAM_END0_ELEVATION).Set(0);
                    instance.get_Parameter(BuiltInParameter.STRUCTURAL_BEAM_END1_ELEVATION).Set(0);

                    transaction.Commit();
                }
                catch
                {
                    transaction.RollBack();
                }
                #endregion
            }
        }
    }
}
