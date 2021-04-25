using System;
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
    class NewBoxExterior : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // 定义变量
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            // 选择链接图纸信息
            Pickdwg pickdwg = new Pickdwg();

            #region 1.框选建筑明细表\n2.选择建筑明细表文字图层\n3.选择墙体轮廓定位线\n4.选择建筑编号
            // 框选明细表
            var box = sel.PickBox(PickBoxStyle.Crossing, "准确框选建筑明细表中的建筑型号及高度数据列");
            // 明细表文字
            pickdwg.Refer(sel, out Reference ReferenceText);
            if (ReferenceText == null)
            {
                MessageBox.Show("已取消");
                return Result.Cancelled;
            }
            // 定位线
            pickdwg.Refer(sel, out Reference referenceModel);
            if (referenceModel == null)
            {
                MessageBox.Show("已取消");
                return Result.Cancelled;
            }
            // 编号
            pickdwg.Refer(sel, out Reference nameText);
            if (nameText == null)
            {
                MessageBox.Show("已取消");
                return Result.Cancelled;
            }
            #endregion

            #region 获取图形 polyline
            // 获取图形
            CADElement CADModel = new CADElement();
            CADText CADtext = new CADText();

            var texts = CADtext.GetBoxText(doc, ReferenceText).Where(i => box.InBox(i.Location));
            var models = CADModel.GetLayerPolyCurves(doc, referenceModel);
            var names = CADtext.GetLayer(doc, nameText);
            // 边界线识别错误
            if (models.Count == 0)
            {
                MessageBox.Show("需选择外墙定位线图层图元,类型应为多段线");
                return Result.Failed;
            }
            #endregion
            // 获取明细表数据
            var schedule = ScheduleBox(texts);

            // 获取默认类型
            var wallTypes = doc.TCollector<WallType>(true);
            var floorTypes = doc.TCollector<FloorType>(true);
            int n = 6;

            if (doc.ActiveView is ViewPlan)
            {
                #region 创建模型
                List<bool> Push = new List<bool>();
                TransactionGroup T = new TransactionGroup(doc, "识别墙体");
                T.Start();
                foreach (var curveModel in models)
                {
                    string name = names.InslideText(curveModel.Curves).Replace("楼", "");
                    // 复制类型
                    var _walltype = ThisType<WallType>(doc, name);
                    var _floortype = ThisType<FloorType>(doc, name);
                    if (_walltype == null || _floortype == null)
                    {
                        MessageBox.Show("请使用指定项目样板文件", "警告");
                        break;
                    }
                    try
                    {
                        var _Hight = schedule[name][n - 1].Text.Parse(1000);
                        // 创建外墙
                        var push_wall = CreateNewWallExterior(doc, curveModel.Curves, _walltype, _Hight);
                        // 创建顶板
                        CurveArray curveArray = ListCurveToArray(curveModel.Curves);
                        var push_floor = CreateFloor(doc, curveArray, _floortype, _Hight);
                        if (push_floor && push_wall) Push.Add(true);
                    }
                    catch (Exception)
                    {
                        // No key
                    }
                }
                _ = Push.Count == 0 ? T.RollBack() : T.Assimilate();
                #endregion
            }

            return Result.Succeeded;
        }

        /// <summary>
        /// 创建墙体
        /// </summary>
        private bool CreateNewWallExterior(Document doc, List<Curve> curves, ElementType wallType, double wallHight, bool isStructure = false)
        {
            Level level = doc.ActiveView.GenLevel;
            Transaction t = new Transaction(doc, "创建墙体");
            t.Start();
            t.NoFailure();
            List<bool> Push = new List<bool>();
            // 创建墙
            foreach (var curve in curves)
            {
                if (curve is Ellipse)
                    continue;
                Wall wall = Wall.Create(doc, curve, level.Id, isStructure);
                wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).Set(wallHight.MillimeterToFeet());
                // 更新墙体类型
                wall.ChangeTypeId(wallType.Id);
                Push.Add(true);
            }

            _ = Push.Count > 0 ? t.Commit() : t.RollBack();

            return Push.Count > 0;
        }

        /// <summary>
        /// 创建楼板
        /// </summary>
        private bool CreateFloor(Document doc, CurveArray curveArray, ElementType floorType, double elva = 0.0, bool isStructure = false)
        {
            Level level = doc.ActiveView.GenLevel;
            bool push = true;
            Transaction t = new Transaction(doc);
            t.Start("创建楼板");
            t.NoFailure();
            try
            {
                var floor = doc.Create.NewFloor(curveArray, (FloorType)floorType, level, isStructure);
                // 设置标高偏移值
                floor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).Set(elva.MillimeterToFeet());
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
        private CurveArray ListCurveToArray(List<Curve> curves)
        {
            CurveArray curveArray = new CurveArray();
            foreach (var curve in curves)
            {
                curveArray.Append(curve);
            }
            return curveArray;
        }

        /// <summary>
        /// 数据分组
        /// </summary>
        private Dictionary<string, List<CADTextModel>> ScheduleBox(IEnumerable<CADTextModel> texts)
        {
            // 分组数据
            var groupInfo = texts.GroupBy(m => Math.Round(m.Location.Y.FeetToMillimeter() / 4500)).ToList();
            // 建筑明细表
            Dictionary<string, List<CADTextModel>> ArSchedule = new Dictionary<string, List<CADTextModel>>();
            foreach (var gp in groupInfo)
            {
                var gpSort = gp.ToList().OrderBy(i => i.Location.X).ToList();
                var key = gpSort.Where(i => i.Text.Contains("#")).FirstOrDefault();
                if (key != null && !ArSchedule.Keys.Contains(key.Text.Replace("楼", "")))
                {
                    var keystr = key.Text.Replace("楼", "");
                    if (ArSchedule.Keys.Contains(keystr))
                    {
                        continue;
                    }
                    ArSchedule.Add(keystr, gpSort);
                }
            }
            return ArSchedule;
        }

        /// <summary>
        /// 获取类型
        /// </summary>
        private ElementType ThisType<T>(Document doc, string name) where T : ElementType
        {
            var allTypes = doc.TCollector<T>(true);
            if (name == string.Empty)
            {
                return allTypes.FirstOrDefault(i => i.Name.Contains("#"));
            }
            var _type = allTypes.FirstOrDefault(i => i.Name == name);
            if (_type != null)
            {
                return _type;
            }
            else
            {
                Transaction t = new Transaction(doc, "Duplicate");
                t.Start();
                var Ctype = allTypes.FirstOrDefault(i => i.Name.Contains("#"))?.Duplicate(name);
                t.Commit();
                return Ctype;
            }
        }
    }
}
