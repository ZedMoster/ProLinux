using System.Collections.Generic;
using System.Linq;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using CADReader.WPF;

namespace CADReader.Command
{
    [Transaction(TransactionMode.Manual)]
    class NewWallsExterior : IExternalCommand
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
            // 文字信息
            pickdwg.Refer(sel, out Reference ReferenceText);
            if (ReferenceText == null)
            {
                MessageBox.Show("已取消");
                return Result.Cancelled;
            }
            #endregion

            // 获取图形
            CADElement CADModel = new CADElement();
            // 获取编号
            CADText CADtext = new CADText();

            var models = CADModel.GetLayerCurves(doc, referenceModel);
            var texts = CADtext.GetLayer(doc, ReferenceText);
            // 边界线识别错误
            if (models.Count == 0)
            {
                MessageBox.Show("需要先选择外墙定位线图层图元\n再选择编号图层图元");
                return Result.Failed;
            }

            WPFWall wpfWall = new WPFWall(doc);
            wpfWall.ShowDialog();
            // 是否点击取消创建
            if (wpfWall.IsHitTestVisible)
                return Result.Cancelled;

            #region 处理创建参数
            WallType wallType = wpfWall.ElementList.SelectedValue as WallType;
            List<SelectElement> levelList = wpfWall.LevelList.ItemsSource as List<SelectElement>;
            List<SelectElement> levels = levelList.Where(x => x.Checked == true).ToList();
            bool isStructure = wpfWall.IsStruct.IsChecked.Value;
            double.TryParse(wpfWall.height.Text, out double topHeight);
            #endregion

            #region 创建多标高外墙
            List<bool> Push = new List<bool>();
            TransactionGroup T = new TransactionGroup(doc, "识别墙体");
            T.Start();
            foreach (var curveModel in models)
            {
                string text = texts.InslideText(curveModel.Curves);
                foreach (var item in levels)
                {
                    Level level = item.Element as Level;
                    var push = CreateNewWallExterior(doc, curveModel.Curves, wallType, level, topHeight, false, text);
                    if (push)
                        Push.Add(push);
                }
            }
            // 处理事务组
            _ = Push.Count == 0 ? T.RollBack() : T.Assimilate();
            #endregion

            MessageBox.Show($"总计外墙组数：{Push.Count}");
            return Result.Succeeded;
        }

        /// <summary>
        /// 创建墙体
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="curves"></param>
        /// <param name="wallType"></param>
        /// <param name="level"></param>
        /// <param name="isStructure"></param>
        /// <param name="texts"></param>
        /// <returns></returns>
        private bool CreateNewWallExterior(Document doc, List<Curve> curves, WallType wallType, Level level, double topHeight, bool isStructure, string text)
        {
            // 获取参照标高
            var topLevel = level.NearLevel(true);
            if (topHeight == 0 && topLevel.Id == level.Id)
                return false;
            Transaction t = new Transaction(doc, "创建墙体");
            t.Start();
            t.NoFailure();
            List<bool> Push = new List<bool>();
            foreach (var curve in curves)
            {
                try
                {
                    // 不创建椭圆定位线的墙体
                    if (curve is Ellipse)
                        continue;
                    Wall wall = Wall.Create(doc, curve, level.Id, isStructure);
                    // 设置注释 - 楼栋号
                    wall.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set(text);
                    // 更新墙体类型
                    wall.ChangeTypeId(wallType.Id);

                    #region 墙体是否为女儿墙
                    if (topLevel.Id == level.Id)
                        wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).Set(topHeight.MillimeterToFeet());
                    else
                        wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(topLevel.Id);
                    #endregion
                    Push.Add(true);
                }
                catch { }
            }
            _ = Push.Count > 0 ? t.Commit() : t.RollBack();

            return Push.Count > 0;
        }
    }
}
