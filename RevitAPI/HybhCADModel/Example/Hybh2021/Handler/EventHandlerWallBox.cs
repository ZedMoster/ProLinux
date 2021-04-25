using System.Collections.Generic;
using System.Linq;
using System.Windows;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace CADReader.Handler
{
    class EventHandlerWallBox : IExternalEventHandler
    {
        /// <summary>
        /// 参数设置
        /// </summary>
        public ToDo ToDoData { get; set; }
        // 事务名称
        private readonly string tranName = "主体建筑";

        /// <summary>
        /// 创建主体建筑
        /// </summary>
        /// <param name="app"></param>
        public void Execute(UIApplication app)
        {
            // 全局参数
            Document doc = app.ActiveUIDocument.Document;
            CADElement CADModel = new CADElement();
            CADText CADtext = new CADText();

            if (doc.ActiveView is View3D)
            {
                MessageBox.Show("不支持在3D视图创建!", "警告");
                return;
            }

            // 数据
            var models = CADModel.GetLayerPolyCurves(doc, ToDoData.ReferenceCurves);
            var names = CADtext.GetLayer(doc, ToDoData.ReferenceCategory);
            var schedule = ToDoData.ScheduleDatas;
            var n = ToDoData.Index;
            // 获取默认类型
            var wallTypes = doc.TCollector<WallType>(true);
            var floorTypes = doc.TCollector<FloorType>(true);

            var START = System.DateTime.Now;
            // 获取数据
            var data = GetCreateData(models, names);

            #region 创建模型
            List<bool> Push = new List<bool>();
            TransactionGroup T = new TransactionGroup(doc, "识别墙体");
            T.Start();
            foreach (var model in data)
            {
                string name = model.CategoryName ?? string.Empty;
                List<Curve> curves = model.Curves;
                // 复制类型
                var _walltype = ThisType<WallType>(doc, name);
                var _floortype = ThisType<FloorType>(doc, name);
                try
                {
                    var strH = schedule.FirstOrDefault(i => i.Index.ToString().Contains(name.Replace("#", "").Replace("楼", "")));  // 设置第二列名称
                    var _Hight = GetKey(strH, n).Parse(1000);
                    // 创建外墙
                    var push_wall = CreateNewWallExterior(doc, curves, _walltype, _Hight);
                    // 创建顶板
                    CurveArray curveArray = ListCurveToArray(curves);
                    var push_floor = CreateFloor(doc, curveArray, _floortype, _Hight);
                    if (push_floor && push_wall)
                    {
                        Push.Add(true);
                    }
                }
                catch { }
            }
            _ = Push.Count == 0 ? T.RollBack() : T.Assimilate();
            #endregion

            var END = System.DateTime.Now;
            MessageBox.Show($"创建个数:{Push.Count} \n\nrunning time: {(int)(END - START).TotalSeconds} s.");

        }

        /// <summary>
        /// name
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            return tranName;
        }

        /// <summary>
        /// 建筑高度默认值27m
        /// </summary>
        private string GetKey(ScheduleDatas datas, int index)
        {
            string h = "27";
            switch (index)
            {
                case 1:
                    h = datas.Key_1;
                    break;
                case 2:
                    h = datas.Key_2;
                    break;
                case 3:
                    h = datas.Key_3;
                    break;
                case 4:
                    h = datas.Key_4;
                    break;
                case 5:
                    h = datas.Key_5;
                    break;
                case 6:
                    h = datas.Key_6;
                    break;
                case 7:
                    h = datas.Key_7;
                    break;
                case 8:
                    h = datas.Key_8;
                    break;
                case 9:
                    h = datas.Key_9;
                    break;
                default:
                    break;
            }
            return h;
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
            // 是否提交事务
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
        /// 获取类型
        /// </summary>
        private ElementType ThisType<T>(Document doc, string name) where T : ElementType
        {
            var allTypes = doc.TCollector<T>(true);
            if (name == string.Empty)
            {
                return allTypes.FirstOrDefault(i => i.FamilyName == "基本墙" || i.FamilyName == "楼板");
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
                var Ctype = allTypes.FirstOrDefault(i => i.FamilyName == "基本墙" || i.FamilyName == "楼板")?.Duplicate(name);
                t.Commit();
                return Ctype;
            }
        }

        /// <summary>
        /// 获取标签及定位边线数据
        /// </summary>
        /// <param name="models"></param>
        /// <param name="names"></param>
        /// <returns></returns>
        public List<CreateNamePolyLine> GetCreateData(List<CADCurveModel> models, List<CADTextModel> names)
        {
            List<CreateNamePolyLine> namePolyLines = new List<CreateNamePolyLine>();
            List<int> _index = new List<int>();
            foreach (var item in models)
            {
                CreateNamePolyLine data = new CreateNamePolyLine();
                var curves = item.Curves;
                data.Curves = curves;
                CurveLoop baseLoop = CurveLoop.Create(curves);
                var solid = baseLoop.CurvesToSolid();
                for (int i = 0; i < names.Count; i++)
                {
                    if (_index.Contains(i))
                    {
                        continue;
                    }
                    var categorymodel = names[i];
                    if (solid != null)
                    {
                        if (solid.IsPointInSolid(categorymodel.Location))
                        {
                            data.CategoryName = categorymodel.Text;
                            _index.Add(i);
                            break;
                        }
                    }
                    else
                    {
                        if (curves.IsPointInCurves(categorymodel.Location))
                        {
                            data.CategoryName = categorymodel.Text;
                            _index.Add(i);
                            break;
                        }
                    }
                }
                if (data.Curves.Count > 0 && data.CategoryName != null)
                {
                    namePolyLines.Add(data);
                }
            }
            return namePolyLines;
        }
    }
}
