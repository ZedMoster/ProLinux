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
    class NewFloor : IExternalCommand
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
            // 获取内容
            var models = CADModel.GetLayerCurves(doc, referenceModel);
            var texts = CADtext.GetLayer(doc, ReferenceText);
            // 边界线识别错误
            if (models.Count == 0)
                return Result.Failed;

            // 用户输入
            WPFFloor wpfFloor = new WPFFloor(doc);
            wpfFloor.ShowDialog();
            // 是否点击取消创建
            if (wpfFloor.IsHitTestVisible)
            {
                MessageBox.Show("已取消!");
                return Result.Cancelled;
            }

            #region 处理创建参数
            FloorType floorType = wpfFloor.ElementList.SelectedValue as FloorType;
            List<SelectElement> levelList = wpfFloor.LevelList.ItemsSource as List<SelectElement>;
            List<SelectElement> levels = levelList.Where(x => x.Checked == true).ToList();
            bool isStructure = wpfFloor.IsStruct.IsChecked.Value;
            double.TryParse(wpfFloor.elva.Text, out double elva);
            
            List<bool> Push = new List<bool>();
            TransactionGroup T = new TransactionGroup(doc, "识别楼板");
            T.Start();
            foreach (var curveModel in models)
            {
                #region 创建多标高楼层楼板
                CurveArray curveArray = ListCurveToArray(curveModel.Curves);
                string text = texts.InslideText(curveModel.Curves);
                foreach (var item in levels)
                {
                    Level level = item.Element as Level;
                    var push = CreateFloor(doc, curveArray, floorType, level, isStructure, text, elva);
                    if (push)
                        Push.Add(push);
                }
                #endregion
            }
            // 处理事务组
            _ = Push.Count == 0 ? T.RollBack() : T.Assimilate();

            MessageBox.Show($"总计楼板组数：{Push.Count}");
            #endregion

            return Result.Succeeded;
        }

        /// <summary>
        /// 创建楼板
        /// </summary>
        /// <param name="doc"> 当前文档</param>
        /// <param name="curveArray"> 楼板边界线</param>
        /// <param name="floorType"> 楼板类型</param>
        /// <param name="level"> 创建的标高</param>
        /// <param name="isStructure"> 是否为结构</param>
        /// <param name="texts"> 楼栋号</param>
        /// <param name="elva"> 偏移值 默认为0</param>
        /// <returns></returns>
        private bool CreateFloor(Document doc, CurveArray curveArray, FloorType floorType, Level level, bool isStructure, string text, double elva = 0.0)
        {
            bool push = true;
            Transaction t = new Transaction(doc);
            t.Start("创建楼板");
            t.NoFailure();
            try
            {
                var floor = doc.Create.NewFloor(curveArray, floorType, level, isStructure);
                // 设置标高偏移值
                floor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).Set(elva.MillimeterToFeet());
                // 设置注释 - 楼栋号
                floor.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set(text);
                t.Commit();
            }
            catch
            {
                t.RollBack();
                push = false;
            }
            return push;
        }

        /// <summary>
        /// 获取 CurveArray
        /// </summary>
        /// <param name="curves"> Curve 列表</param>
        /// <returns></returns>
        private CurveArray ListCurveToArray(List<Curve> curves)
        {
            CurveArray curveArray = new CurveArray();
            foreach (var curve in curves)
            {
                curveArray.Append(curve);
            }
            return curveArray;
        }
    }
}
