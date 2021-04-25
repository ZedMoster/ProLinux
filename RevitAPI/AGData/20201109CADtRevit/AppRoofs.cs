using System;
using System.Linq;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using CADReader.WPF;

namespace CADReader.Command
{
    [Transaction(TransactionMode.Manual)]
    class AppRoofs : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;

            RoofManage roofManage = new RoofManage();
            // 获取边界线及设置参数
            var walls = roofManage.GetCurveArrayInWalls(uidoc, out CurveArray curveArray);
            if (walls.Count == 0 || curveArray.Size == 0)
                return Result.Failed;

            // 屋顶参数设置
            WPFRoof wPFRoof = new WPFRoof(doc);
            wPFRoof.ShowDialog();
            if (wPFRoof.IsHitTestVisible)
            {
                MessageBox.Show("已取消");
                return Result.Cancelled;
            }

            #region 处理创建参数
            RoofType roofType;
            double.TryParse(wPFRoof.elva.Text, out double elva);
            if (wPFRoof.NewType.IsChecked.Value)
            {
                Material material = wPFRoof.MaterialList.SelectedValue as Material;
                string typeName = wPFRoof.NewTypeName.Text;
                double.TryParse(wPFRoof.NewTypeWidth.Text, out double width);
                roofType = roofManage.CreateNewType(doc, typeName, width, material);
            }
            else
                roofType = wPFRoof.ElementList.SelectedValue as RoofType;
            // 获取参照标高
            var level = doc.ActiveView.GenLevel?.NearLevel(true) ?? (doc.GetElement(walls.FirstOrDefault()
                .get_Parameter(BuiltInParameter.WALL_BASE_CONSTRAINT).AsElementId()) as Level).NearLevel(true);
            if (level == null)
                throw new Exception("No level.");
            #endregion

            try
            {
                // 创建屋顶
                roofManage.CreateRoof(doc, curveArray, level, roofType, elva);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return Result.Succeeded;
        }
    }
}