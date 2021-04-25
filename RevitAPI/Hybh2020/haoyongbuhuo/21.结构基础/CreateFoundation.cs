using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RvtTxt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class CreateFoundation : IExternalCommand
    {
        private const string familyName = "Foundation";
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // 定义变量
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            // 程序是否注册可运行
            if (Run.Running(Strings.key))
            {
                try
                {
                    // 获取图形定位点
                    SelPick.SelRef = sel.PickObject(ObjectType.PointOnElement, "<请点选矩形基础轮廓线>");
                }
                catch { }

                if (SelPick.SelRef == null)
                {
                    return Result.Cancelled;
                }

                ImportInstance dwg = doc.GetElement(SelPick.SelRef) as ImportInstance;
                var geoObj = (dwg as Element).GetGeometryObjectFromReference(SelPick.SelRef);
                Transform transform = dwg.GetTransform();
                if (geoObj is PolyLine)
                {
                    var polyLine = geoObj as PolyLine;

                    if (polyLine.NumberOfCoordinates == 5)
                    {
                        var p0 = transform.OfPoint(polyLine.GetCoordinate(0));
                        var p1 = transform.OfPoint(polyLine.GetCoordinate(1));
                        var p2 = transform.OfPoint(polyLine.GetCoordinate(2));

                        var localPoint = (p0 + p2) / 2;

                        var b = p1.DistanceTo(p2);
                        var h = p0.DistanceTo(p1);

                        CAD cad = new CAD();
                        try
                        {
                            // 获取选中 梁 标注的信息
                            SelPick.SelRef_two = sel.PickObject(ObjectType.PointOnElement, "<请选择矩形基础标注名称>");
                        }
                        catch { }

                        if (SelPick.SelRef_two == null)
                        {
                            return Result.Cancelled;
                        }

                        var cadText = cad.GetCADText(SelPick.SelRef_two, doc);
                        if (cadText == "--")
                        {
                            TaskDialog.Show("提示", "CAD图纸文字识别错误");
                            return Result.Failed;
                        }

                        TransactionGroup T = new TransactionGroup(doc);
                        T.Start("创建结构基础");

                        // 获取族类型
                        var familySymbol = GetElement(doc, cadText);
                        // 获取标高
                        var level = doc.ActiveView.GenLevel;

                        #region 创建实例
                        var instance = CreateNewInstance(doc, localPoint, familySymbol, level);

                        Transaction t2 = new Transaction(doc);
                        t2.Start("更新参数");
                        instance.Symbol.LookupParameter("宽度").Set(b);
                        instance.Symbol.LookupParameter("长度").Set(h);
                        instance.Symbol.LookupParameter("基础厚度").Set(ReadData(cadText));
                        t2.Commit();
                        #endregion

                        try
                        {
                            #region 旋转构件角度
                            Transaction t3 = new Transaction(doc);
                            t3.Start("旋转构件");
                            Line line = Line.CreateBound((p0 + p1) / 2, localPoint);
                            var angle = toRotaLineAngle(line);
                            // 选择角度
                            var axis = Line.CreateBound(localPoint, new XYZ(localPoint.X, localPoint.Y, localPoint.Z + 1));
                            var rotated = instance.Location.Rotate(axis, -angle);
                            t3.Commit();
                            #endregion

                            #region ui 选中构件
                            //List<ElementId> elementIds = new List<ElementId>();
                            //elementIds.Add(instance.Id);
                            //sel.SetElementIds(elementIds);
                            #endregion

                            T.Assimilate();
                        }
                        catch { T.RollBack(); }

                    }
                    else
                    {
                        TaskDialog.Show("提示", "不是矩形轮廓结构柱！");
                        return Result.Failed;
                    }
                }
                else
                {
                    TaskDialog.Show("提示", "请选择结构基础的轮廓的边界线！");
                    return Result.Failed;
                }


            }

            return Result.Succeeded;
        }

        /// <summary>
        /// 读取本地json文件获取参数值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public double ReadData(string key)
        {
            ReadJson.JsonFilePath = @"C:\ProgramData\Autodesk\ApplicationPlugins\hybh2020.bundle\Contents\2018\data\data22.json";
            var res = ReadJson.ByKey(key) ?? "1000";
            double.TryParse(res, out double result);
            return UUtools.MillimetersToUnits(result);
        }

        /// <summary>
        /// 获取XYZ(0,1,0)与element向量的旋转弧度
        /// </summary>
        /// <param name="line"> </param>
        /// <returns></returns>
        public double toRotaLineAngle(Line line)
        {
            var lineDirection = line.Direction;
            // 套管初始方向 Y--(1,0,0) X--(0,1,0)
            var familyDirection = new XYZ(1, 0, 0);
            var angle = familyDirection.AngleTo(lineDirection);
            return angle;
        }

        private FamilyInstance CreateNewInstance(Document doc, XYZ xYZ, FamilySymbol familySymbol, Level level)
        {

            using (Transaction transaction = new Transaction(doc))
            {
                transaction.Start("创建结构基础实例");
                if (!familySymbol.IsActive)
                {
                    familySymbol.Activate();
                }
                var familyInstance = doc.Create.NewFamilyInstance(xYZ, familySymbol, level, Autodesk.Revit.DB.Structure.StructuralType.Footing);
                transaction.Commit();
                return familyInstance;
            }
        }

        private void IsHaveFamily(Document doc)
        {
            #region 判断族是否存在
            bool ishave = false;
            var els = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralColumns).WhereElementIsElementType().ToElements();
            foreach (var _family in els)
            {
                var Familytype = _family as ElementType;
                if (familyName == Familytype.FamilyName)
                {
                    ishave = true;
                }
            }
            #endregion

            #region 载入族文件
            if (!ishave)
            {
                Transaction t = new Transaction(doc);
                t.Start("加载族文件");
                var familyPath = @"C:\ProgramData\Autodesk\ApplicationPlugins\hybh2020.bundle\Contents\2018\Family\Foundation.rfa";
                doc.LoadFamily(familyPath, out Family family);
                family.Name = "hybh混凝土_结构基础";
                t.Commit();
            }
            #endregion
        }

        private FamilySymbol GetElement(Document doc, string name)
        {
            IsHaveFamily(doc);

            FamilySymbol familySymbol = null;

            var elsType = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralFoundation).WhereElementIsElementType().ToElements();
            //TaskDialog.Show("0", elsType.Count.ToString());

            List<ElementType> elementType = new List<ElementType>();
            foreach (var _family in elsType)
            {
                var Familytype = _family as ElementType;

                if (Familytype.Name == name)
                {
                    familySymbol = _family as FamilySymbol;
                }
                //TaskDialog.Show("name", Familytype.FamilyName);

                if (Familytype.FamilyName == familyName)
                {
                    elementType.Add(Familytype);
                }
            }

            if (familySymbol != null)
            {
                return familySymbol;
            }
            else
            {
                //TaskDialog.Show("1", elementType.Count.ToString());
                var elType = elementType.First();
                Transaction t = new Transaction(doc);
                t.Start("创建类型");
                familySymbol = elType.Duplicate(name) as FamilySymbol;
                t.Commit();
                return familySymbol;
            }

        }
    }
}
