using System.Collections.Generic;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using CADReader.WPF;

namespace CADReader.Command
{
    [Transaction(TransactionMode.Manual)]
    class NewTopography : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // 定义变量
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            // 选择链接图纸信息
            Pickdwg pickdwg = new Pickdwg();

            #region 选择CAD图纸信息
            // 获取定位线
            pickdwg.Refer(sel, out Reference referenceModel);
            if (referenceModel == null)
            {
                MessageBox.Show("已取消");
                return Result.Cancelled;
            }
            // 获取图形
            CADElement CADModel = new CADElement();
            var models = CADModel.GetLayerCurves(doc, referenceModel);
            if (models.Count == 0)
            {
                return Result.Failed;
            }
            #endregion

            WPFTopography wPFTopography = new WPFTopography(doc);
            wPFTopography.ShowDialog();
            if (wPFTopography.IsHitTestVisible)
            {
                MessageBox.Show("已取消");
                return Result.Cancelled;
            }

            TopographyManage topographyManage = new TopographyManage();
            #region 处理创建参数
            FloorType floorType;
            double.TryParse(wPFTopography.elva.Text, out double elva);
            if (wPFTopography.NewType.IsChecked.Value)
            {
                Material material = wPFTopography.MaterialList.SelectedValue as Material;
                string typeName = wPFTopography.NewTypeName.Text;
                double.TryParse(wPFTopography.NewTypeWidth.Text, out double width);
                floorType = topographyManage.CreateNewFloorType(doc, typeName, width, material);
            }
            else
            {
                floorType = wPFTopography.ElementList.SelectedValue as FloorType;
            }
            #endregion

            #region 创建地形（楼板）
            List<bool> Push = new List<bool>();
            TransactionGroup T = new TransactionGroup(doc, "创建地形");
            T.Start();
            foreach (var item in models)
            {
                CurveArray curveArray = topographyManage.ListCurveToArray(item.Curves);
                var push = topographyManage.CreateFloor(doc, curveArray, floorType, elva);
                if (push)
                    Push.Add(push);
            }
            // 处理事务组
            _ = Push.Count == 0 ? T.RollBack() : T.Assimilate();
            #endregion

            MessageBox.Show($"总计地形组数：{Push.Count}");

            return Result.Succeeded;
        }
    }
}
