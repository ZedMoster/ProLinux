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
    class NewWalls : IExternalCommand
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
            #endregion

            // 获取图形
            CADElement CADModel = new CADElement();
            // 获取选择图形的定位点
            var models = CADModel.GetLayerMidCurves(doc, referenceModel);
            if (models.Count == 0)
            {
                return Result.Failed;
            }
            // 顶部标高
            var topLevel = doc.ActiveView.GenLevel.NearLevel(true);
            // 获取墙类型
            var walltypes = doc.TCollector<WallType>(true);
            // 创建墙
            List<bool> Push = new List<bool>();
            Transaction t = new Transaction(doc, "创建墙");
            t.Start();
            t.NoFailure();
            foreach (var item in models)
            {
                // 墙长度小于墙宽度时不创建墙体
                if (item.MidCurve.Length < item.Width && item.ValueString != "100")
                    continue;
                var wall = Wall.Create(doc, item.MidCurve, doc.ActiveView.GenLevel.Id, false);
                wall.get_Parameter(BuiltInParameter.WALL_KEY_REF_PARAM).Set(0);
                if (topLevel.Id != doc.ActiveView.GenLevel.Id)
                {
                    wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(topLevel.Id);
                }
                // 获取类型
                var type = walltypes.FirstOrDefault(x => x.Width.FeetToMillimeter().ToString() == item.ValueString);
                if (type != null)
                {
                    wall.ChangeTypeId(type.Id);
                    Push.Add(true);
                }
            }
            t.Commit();

            MessageBox.Show($"总计墙段：{Push.Count}");
            return Result.Succeeded;
        }
    }
}
