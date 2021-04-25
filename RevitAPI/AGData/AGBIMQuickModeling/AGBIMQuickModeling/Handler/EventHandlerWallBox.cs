using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        public void Execute(UIApplication uiapp)
        {
            // 全局参数
            var app = uiapp.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            CADElement CADModel = new CADElement();
            CADText CADtext = new CADText();

            if(doc.ActiveView is View3D)
            {
                $"{tranName}:debug-0001{doc.ActiveView.Id}".ToDebug();
                MessageBox.Show("不支持在3D视图创建!", "警告");
                return;
            }

            // 数据
            var models = CADModel.GetLayerCurves(doc, ToDoData.ReferenceCurves);
            $"{tranName}-debug-0002-{models.Count}".ToDebug();
            var group = models.ToCurves().GroupCurves();
            $"{tranName}-debug-0003-{group.Count}".ToDebug();
            var names = CADtext.GetLayer(doc, ToDoData.ReferenceCategory);
            $"{tranName}-debug-0004-{names.Count}".ToDebug();
            var schedule = ToDoData.ScheduleDatas;
            $"{tranName}-debug-0005-{schedule.Count}".ToDebug();
            var n = ToDoData.Index;
            $"{tranName}-debug-0006-{n}".ToDebug();
            // 获取默认类型
            var wallTypes = doc.TCollector<WallType>(true);
            var floorTypes = doc.TCollector<FloorType>(true);

            var START = System.DateTime.Now;
            // 获取数据
            var data = GetCreateData(group, names);
            $"{tranName}:debug-0007{data.Count}".ToDebug();
            #region 创建模型
            List<bool> Push = new List<bool>();
            TransactionGroup T = new TransactionGroup(doc, "识别墙体");
            T.Start();

            // 添加共享参数名称
            var titles = schedule[0];
            // 参数名称
            var keys = GetKeys(titles, "AG_");
            $"{tranName}-debug-0008-{keys.Count}".ToDebug();
            var parameter = AddParatemerByName(uiapp, keys);
            $"{tranName}-debug-0009-{parameter}".ToDebug();
            foreach(var model in data)
            {
                string name = model.CategoryName ?? string.Empty;
                List<Curve> curves = model.Curves;
                // 复制类型
                var _walltype = ThisType<WallType>(doc, name);
                var _floortype = ThisType<FloorType>(doc, name);
                try
                {
                    var strH = schedule.FirstOrDefault(i => i.Index.ToString().Contains(name.ToNumberStr()));
                    var values = GetKeys(strH);
                    // 关联参数
                    UpdataParatemer(doc, _floortype, keys, values);
                    UpdataParatemer(doc, _walltype, keys, values);
                    var _Hight = GetKey(strH, n).Parse(1000);
                    // 创建 外墙
                    var push_wall = CreateNewWallExterior(doc, curves, _walltype, _Hight);
                    // 创建 顶板
                    CurveArray curveArray = ListCurveToArray(curves);
                    var push_floor = CreateFloor(doc, curveArray, _floortype, _Hight);
                    if(push_floor && push_wall)
                    {
                        Push.Add(true);
                    }
                }
                catch(Exception e)
                {
                    _ = e.Message;
                }
            }
            _ = Push.Count == 0 ? T.RollBack() : T.Assimilate();
            #endregion

            var END = System.DateTime.Now;
            $"{tranName}:debug-0010{Push.Count}".ToDebug();
            MessageBox.Show($"总计创建：{Push.Count}\n运行时间： {(int)(END - START).TotalSeconds} s.");
        }

        /// <summary>
        /// 事务名称
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            return tranName;
        }

        /// <summary>
        /// 建筑高度默认值27m
        /// </summary>
        private string GetKey(ScheduleDatas datas, int index, string str = "27")
        {
            string h = str;
            switch(index)
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
            UpdateStructure(doc, (WallType)wallType, 100, "建筑材质-外墙");

            Level level = doc.ActiveView.GenLevel;
            Transaction t = new Transaction(doc, "创建墙体");
            t.Start();
            t.NoFailure();
            List<bool> Push = new List<bool>();
            // 创建墙
            foreach(var curve in curves)
            {
                if(curve is Ellipse)
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
            // 更新参数
            UpdateStructure(doc, (FloorType)floorType, elva);
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
        /// 更新类型厚度及材质参数
        /// </summary>
        private void UpdateStructure(Document doc, ElementType elementType, double width, string meterialName = "建筑材质-主体")
        {
            // 创建材质
            List<Material> materials = doc.TCollector<Material>();
            Material material = materials.FirstOrDefault(i => i.Name == meterialName);
            if(material == null)
            {
                var t = new Transaction(doc, "newMatrial");
                t.Start();
                material = Material.Create(doc, meterialName).ToElement(doc) as Material;
                t.Commit();
            }
            // 处理 Material 颜色
            var t_color = new Transaction(doc, "updateColor");
            t_color.Start();
            material.Color = new Color(192, 192, 192);
            t_color.Commit();
            // 更新类型材质
            Transaction tranColor = new Transaction(doc, "updataMatrial");
            tranColor.Start();
            var layer = CompoundStructure.CreateSingleLayerCompoundStructure(
                MaterialFunctionAssignment.Structure, width.MillimeterToFeet(), material.Id);
            layer.EndCap = EndCapCondition.NoEndCap;
            if(elementType is FloorType floorType)
            {

                floorType.SetCompoundStructure(layer);
            }
            if(elementType is WallType wallType)
            {
                wallType.SetCompoundStructure(layer);
            }
            tranColor.Commit();
        }


        /// <summary>
        /// 获取 CurveArray
        /// <summary>
        private CurveArray ListCurveToArray(List<Curve> curves)
        {
            CurveArray curveArray = new CurveArray();
            foreach(var curve in curves)
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
            if(name == string.Empty)
            {
                return allTypes.FirstOrDefault(i => i.FamilyName == "基本墙" || i.FamilyName == "楼板");
            }
            var _type = allTypes.FirstOrDefault(i => i.Name == name);
            if(_type != null)
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
            foreach(var item in models)
            {
                CreateNamePolyLine data = new CreateNamePolyLine();
                var curves = item.Curves;
                data.Curves = curves;
                CurveLoop baseLoop = CurveLoop.Create(curves);
                var solid = baseLoop.CurvesToSolid();
                for(int i = 0; i < names.Count; i++)
                {
                    if(_index.Contains(i))
                    {
                        continue;
                    }
                    var categorymodel = names[i];
                    if(solid != null)
                    {
                        if(solid.IsPointInSolid(categorymodel.Location))
                        {
                            data.CategoryName = categorymodel.Text;
                            _index.Add(i);
                            break;
                        }
                    }
                    else
                    {
                        if(curves.IsPointInCurves(categorymodel.Location))
                        {
                            data.CategoryName = categorymodel.Text;
                            _index.Add(i);
                            break;
                        }
                    }
                }
                if(data.Curves.Count > 0 && data.CategoryName != null)
                {
                    namePolyLines.Add(data);
                }
            }
            return namePolyLines;
        }

        /// <summary>
        /// 创建参数
        /// </summary>
        /// <param name="uiapp"></param>
        /// <param name="keys"></param>
        private bool AddParatemerByName(UIApplication uiapp, List<string> keys)
        {
            // 设置参数文件
            ShareParatemer shareParatemer = new ShareParatemer();
            // 添加参数
            List<bool> Push = new List<bool>();
            Transaction tranpara = new Transaction(uiapp.ActiveUIDocument.Document, "添加参数");
            tranpara.Start();

            foreach(var key in keys)
            {
                var isSuccess = shareParatemer.Create(uiapp, key, ParameterType.Text, false);
                if(isSuccess)
                {
                    Push.Add(isSuccess);
                }
            }
            // 处理事务
            _ = Push.Count > 0 ? tranpara.Commit() : tranpara.RollBack();
            return Push.Count > 0;
        }

        /// <summary>
        /// 获取参数键名
        /// </summary>
        /// <param name="scheduleData"></param>
        /// <param name="head"></param>
        /// <returns></returns>
        private List<string> GetKeys(ScheduleDatas scheduleData, string head = "")
        {
            var keys = new List<string>() {
                scheduleData.Key_1.AddHead(head),
                scheduleData.Key_2.AddHead(head),
                scheduleData.Key_3.AddHead(head),
                scheduleData.Key_4.AddHead(head),
                scheduleData.Key_5.AddHead(head),
                scheduleData.Key_6.AddHead(head),
                scheduleData.Key_7.AddHead(head),
                scheduleData.Key_8.AddHead(head),
                scheduleData.Key_9.AddHead(head),
                scheduleData.Key_10.AddHead(head),
            };

            #region 删除空行
            for(int i = keys.Count - 1; i >= 0; i--)
            {
                if(keys[i] == "N/A".AddHead(head))
                {
                    keys.Remove(keys[i]);
                }
            }
            #endregion
            return keys;
        }

        /// <summary>
        /// 更新参数值
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="elementType"></param>
        /// <param name="keys"></param>
        /// <param name="values"></param>
        private void UpdataParatemer(Document doc, ElementType elementType, List<string> keys, List<string> values)
        {
            if(keys.Count == values.Count && keys.Count > 0)
            {
                Transaction tranUp = new Transaction(doc, "更新参数");
                tranUp.Start();
                for(int i = 0; i < keys.Count; i++)
                {
                    var key = keys[i];
                    var value = values[i];
                    elementType.LookupParameter(key).Set(value);
                }
                tranUp.Commit();
            }
        }
    }
}
