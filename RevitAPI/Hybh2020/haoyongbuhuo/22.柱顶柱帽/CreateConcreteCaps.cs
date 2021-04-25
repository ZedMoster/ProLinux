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
    class CreateConcreteCaps : IExternalCommand
    {
        private const string familyName = "ConcreteCap";
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // 定义变量
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            // 程序是否注册可运行
            if (Run.Running(Strings.key))
            {
                #region 选择构件
                // 选择结构柱
                var newfilter = new PickByCategorySelectionFilter
                {
                    CategoryName = "结构柱"
                };
                try
                {
                    ToFinish toFinish = new ToFinish();
                    toFinish.Subscribe();
                    SelPick.SelRefsList = sel.PickObjects(ObjectType.Element, newfilter, "<框选创建柱帽的结构柱>").ToList();
                    toFinish.Unsubscribe();
                }
                catch { SelPick.SelRefsList = new List<Reference>(); }

                if (SelPick.SelRefsList.Count == 0)
                {
                    return Result.Cancelled;
                }
                #endregion

                TransactionGroup T = new TransactionGroup(doc);
                T.Start("创建柱帽");

                #region 读取数据
                // 注册列表缓存数据
                RegistryStorage Registry = new RegistryStorage();
                var b_string = Registry.OpenAfterStart("ColumnB") ?? "b";
                var h_string = Registry.OpenAfterStart("ColumnH") ?? "h";
                // 柱帽族类型名称
                var cadText = Registry.OpenAfterStart("ConcreteName") ?? "ZM1";
                #endregion

                foreach (var item in SelPick.SelRefsList)
                {
                    var el = doc.GetElement(item);
                    // 查看参数
                    var boolB = el.LookupParameter(b_string);
                    var boolH = el.LookupParameter(h_string);
                    if (boolB != null && boolH != null)

                    #region 创建实例
                    {
                        // 获取柱顶部定位点
                        var localPoint = GetColumnAnalyTessellate(el);
                        // 获取族类型
                        var familySymbol = GetElement(doc, cadText);
                        // 获取标高
                        var level = doc.ActiveView.GenLevel;
                        // 创建族实例
                        var instance = CreateNewInstance(doc, localPoint, familySymbol, level);
                        // 更新族实例参数
                        Transaction t2 = new Transaction(doc);
                        t2.Start("更新参数");
                        instance.LookupParameter("柱宽").Set(boolB.AsDouble());
                        instance.LookupParameter("柱长").Set(boolH.AsDouble());
                        instance.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM).Set(0.0);
                        t2.Commit();

                        #region 旋转构件角度
                        Transaction t3 = new Transaction(doc);
                        t3.Start("旋转构件");
                        try
                        {
                            MakeElement.ToRotaPointElement(instance, el);
                            t3.Commit();
                        }
                        catch { }
                        #endregion
                    }
                    #endregion
                    else
                    {
                        TaskDialog.Show("提示", "在全局参数中设置结构柱尺寸参数名称");
                    }
                }
                T.Assimilate();
            }

            return Result.Succeeded;
        }

        /// <summary>
        /// 获取定位点中Z 轴偏移值更大的定位点
        /// </summary>
        /// <param name="el"></param>
        /// <returns></returns>
        public static XYZ GetColumnAnalyTessellate(Element el)
        {
            var tessellate = el.GetAnalyticalModel().GetCurve().Tessellate();
            var p0 = tessellate[0];
            var p1 = tessellate[1];
            if (p0.Z > p1.Z)
            {
                return p0;
            }
            else
            {
                return p1;
            }
        }


        /// <summary>
        /// 读取本地json文件获取参数值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public double ReadData(string key)
        {
            ReadJson.JsonFilePath = @"C:\ProgramData\Autodesk\ApplicationPlugins\hybh2020.bundle\Contents\2018\data\data23.json";
            // 如果没有读取到数据设置默认值为1000
            var res = ReadJson.ByKey(key) ?? "1000";
            // 将数字字符串转为double类型
            double.TryParse(res, out double result);
            // 返回值单位进行转换
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
                transaction.Start("创建柱帽实例");
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
                var familyPath = @"C:\ProgramData\Autodesk\ApplicationPlugins\hybh2020.bundle\Contents\2018\Family\ConcreteCap.rfa";
                doc.LoadFamily(familyPath, out Family family);
                family.Name = "hybh混凝土_矩形柱帽";
                t.Commit();
            }
            #endregion
        }

        private FamilySymbol GetElement(Document doc, string name)
        {
            IsHaveFamily(doc);

            FamilySymbol familySymbol = null;

            var elsType = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_GenericModel).WhereElementIsElementType().ToElements();
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

