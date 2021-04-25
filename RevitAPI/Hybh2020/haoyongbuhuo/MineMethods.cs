using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using Gma.System.MouseKeyHook;

using Microsoft.Win32;

using MongoDB.Bson;
using MongoDB.Driver;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using RvtTxt;

namespace hybh
{
    /// <summary>
    /// 获取电脑信息
    /// </summary>
    class MachineCode
    #region 获取电脑硬件信息
    {
        ///   <summary> 
        ///   获取cpu序列号     
        ///   </summary> 
        ///   <returns> string </returns> 
        public static string CpuInfo { get; set; }
        public static string GetCpuInfo()
        {
            try
            {
                using (ManagementClass cimobject = new ManagementClass("Win32_Processor"))
                {
                    ManagementObjectCollection moc = cimobject.GetInstances();

                    foreach (ManagementObject mo in moc)
                    {
                        CpuInfo = mo.Properties["ProcessorId"].Value.ToString();
                        mo.Dispose();
                    }
                }
            }
            catch { }

            return CpuInfo.ToString();
        }
    }
    #endregion

    /// <summary>
    /// 判断是否登录 运行程序与否
    /// </summary>
    static class Run
    #region 判断用户是否已经注册账户
    {
        /// <summary>
        /// 无需密码自动验证成功
        /// </summary>
        /// <returns></returns>
        public static bool Running(string _key)
        {
            _ = _key;
            return true;
        }

        /// <summary>
        /// 判断用户是否登录账户
        /// </summary>
        /// <returns>登录：true  注销：flase</returns>
        public static bool Running(/*string _key*/)
        {
            var _key = Strings.key;

            bool start = true;
            MyGroup myGroup = new MyGroup();
            RegistryStorage registryStorage = new RegistryStorage();
            // 链接数据库判断用户是否注册
            var thisTime = myGroup.GetKeytoStart(_key);
            // 判断是否登录成功
            var local = registryStorage.OpenAfterStart(_key);

            if (!thisTime && local == Strings.value)
            {
                // 数据库用户验证失败 且 本地未找到用户
                TaskDialog.Show("提示", "未注册登录的用户不能使用该功能！");
                start = false;
            }
            else
            {
                // 用户使用次数计数
                string userName = registryStorage.OpenAfterStart("name");
                string userPassword = registryStorage.OpenAfterStart("password");
                var collection = ClientMongoDB.GetClient("userInfo");
                var filter = Builders<BsonDocument>.Filter.Eq("password", userPassword) & Builders<BsonDocument>.Filter.Eq("name", userName);
                var result = collection.Find(filter).ToList();
                if (result.Count == 1)
                {
                    try
                    {
                        // 次数 +1
                        var count = result[0]["count"].ToInt32();
                        count++;
                        collection.UpdateOne(filter, Builders<BsonDocument>.Update.Set("count", count));
                    }
                    catch
                    {
                        // 第一次使用更新参数 count +1
                        collection.UpdateOne(filter, Builders<BsonDocument>.Update.Set("count", 1));
                    }
                    //MessageBox.Show("次数 +1");
                }
                else
                {
                    start = false;
                }
            }
            return start;
        }

        /// <summary>
        /// 查看当前视图是不是 3D视图
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static bool Is3DViewCanNotWork(Document doc)
        {
            var view = doc.ActiveView;
            if (view.ViewType == ViewType.ThreeD)
            {
                TaskDialog.Show("提示", "不支持在三维视图使用");
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    #endregion

    /// <summary>
    /// 查看当前用户是否已经注册
    /// </summary>
    class MyGroup
    #region 判断用户是否已经注册
    {
        /// <summary>
        /// 获取 键 所对应的值，进行判断
        /// </summary>
        /// <param name="_key"> 键</param>
        /// <returns> 本地账户是否同注册列表</returns>

        public bool GetKeytoStart(string _key)
        {
            bool start = false;
            try
            {
                // 注册列表文件读写方法
                RegistryStorage registryStorage = new RegistryStorage();
                var local = registryStorage.OpenAfterStart(_key);

                // 登录服务器方法
                var collection = ClientMongoDB.GetClient("userInfo");
                var result = collection.Find(Builders<BsonDocument>.Filter.Empty).ToList();

                foreach (var item in result)
                {
                    var _id = item["_id"].ToString();
                    if (local == _id)
                    {
                        start = true;
                    }
                }
            }
            catch
            {
                //TaskDialog.Show("error0", e.Message);
                TaskDialog.Show("提示", "网络链接失败，请稍后重试！");
            }
            return start;
        }
    }
    #endregion

    /// <summary>
    /// 创建注册列表缓存数据
    /// </summary>
    class RegistryStorage
    #region 设置注册列表保存与读取数据
    {
        public string GetValue { get; set; }
        RegistryKey registryKey;
        /// <summary>
        /// 注册列表数据读取（key）
        /// </summary>
        /// <param name="_key"> 键</param>
        /// <returns> 获取本地注册列表键值对信息\n指定路径下键所对应的值</returns>
        public string OpenAfterStart(string _key)
        {
            try
            {
                // HKCU\Software\RegeditStorage
                registryKey = Registry.CurrentUser.OpenSubKey(@"Software\RegistryInfor");
                // 注册列表文件名称 计算机\HKEY_CURRENT_USER\Software\RegistryInfor
                GetValue = registryKey.GetValue(_key).ToString();
                registryKey.Close();
            }
            catch { }

            return GetValue;
        }

        /// <summary>
        /// 写入本地注册列表键值对信息
        /// </summary>
        /// <param name="_key"> 键</param>
        /// <param name="_value"> 值</param>
        public void SaveBeforeExit(string _key, string _value)
        {
            try
            {
                // 计算机\HKEY_CURRENT_USER\Software\RegistryInfor
                registryKey = Registry.CurrentUser.CreateSubKey(@"Software\RegistryInfor");
                registryKey.SetValue(_key, _value);
                registryKey.Close();
            }
            catch { }
        }
    }
    #endregion

    /// <summary>
    /// 连接数据库-MongoDB
    /// </summary>
    class ClientMongoDB
    #region 链接到数据库 MongoDB
    {
        /// <summary>
        /// 链接到数据库
        /// </summary>
        /// <param name="basename"> 表名称</param>
        /// <returns> 返回MongoDB数据库的 表名称basename的链接 Col</returns>
        public static IMongoCollection<BsonDocument> GetClient(string basename)
        {
            //var Local = "mongodb://revit:revit.123@111.229.98.184:27017/RevitUser";
            var Local = "mongodb://" + Strings.Revit + ":" + Strings.Revit + Strings.Dot + "123@"
                + "111" + Strings.Dot + "229" + Strings.Dot + "98" + Strings.Dot + "184" + ":" + "27017/" + Strings.User;
            var client = new MongoClient(Local);
            var database = client.GetDatabase("RevitUser");
            var collection = database.GetCollection<BsonDocument>(basename);

            return collection;
        }
    }
    #endregion

    /// <summary>
    /// 获取与给定模型相碰撞的模型类别
    /// </summary>
    class GetInstersectsElements
    #region 获取与模型element相交的 指定类别模型 返回Ilist列表
    {
        /// <summary>
        /// 获取与模型element相交的 类别模型
        /// </summary>
        /// <param name="doc"> 文档</param>
        /// <param name="element"> 主类别</param>
        /// <param name="builtInCategory"> 与element相交的模型elements</param>
        /// <returns> 返回与element相交的所有Category类别的elements</returns>
        public IList<Element> ElementByType(Document doc, Element element, BuiltInCategory builtInCategory)
        {
            // element 相交模型过滤器
            var elementIntersects = new ElementIntersectsElementFilter(element);
            // 获取指定类别的模型
            var instersectsElements = new FilteredElementCollector(doc).OfCategory(builtInCategory).WherePasses(elementIntersects).ToElements();
            return instersectsElements;
        }
    }
    #endregion

    /// <summary>
    /// 获取基于定位线模型的两个定位点
    /// </summary>
    class GetElementLocationCurvePoints
    #region 获取基于定位线的模型的两个定位点
    {
        /// <summary>
        /// 获取element curve 两个定位点的XYZ
        /// </summary>
        /// <param name="el"> el(需要location为定位线)</param>
        /// <returns> 返回模型的定位线的起点和终点</returns>
        public List<XYZ> CurvePoints(Element el)
        {
            List<XYZ> twoPoints = new List<XYZ>();
            // Location
            if (el.Location is LocationCurve location)
            {
                var line = location.Curve;
                var p0 = line.GetEndPoint(0);
                var p1 = line.GetEndPoint(1);
                twoPoints.Add(p0);
                twoPoints.Add(p1);
            }

            return twoPoints;
        }

        public XYZ LocalPoint(Element el)
        {
            // Location
            if (el.Location is LocationPoint location)
            {
                var pot = location.Point;
                return pot;
            }
            else
            {
                return null;
            }
        }
    }
    #endregion

    /// <summary>
    /// 创建用于wpf下拉选择列表的属性类
    /// </summary>
    public class SelectElementByName
    #region 设置wpf 窗口下拉界面的键值对数据
    {
        /// <summary>
        /// WPF下拉列表
        ///     ElName--> name
        ///     Element--> elemnt
        ///     BuiltInC--> BuiltInCategory
        /// </summary>
        public string HybhElName { get; set; }
        public string HybhKey { get; set; }
        public string HybhValue { get; set; }
        public Element HybhElement { get; set; }
        public BuiltInCategory HybhBuiltInCategory { get; set; }
    }
    #endregion

    /// <summary>
    /// 选择CAD图纸模型类别图元
    /// </summary>
    class PickCADFilter : ISelectionFilter
    #region 选择图纸对象
    {
        public bool AllowElement(Element el)
        {
            if (el.Category.Name.EndsWith(".dwg"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }
    #endregion

    /// <summary>
    /// 获取链接的CAD文件的元素过滤器
    /// </summary>
    class SelectionlinkFilter : ISelectionFilter
    #region 手动选择链接的RVT文件（指定 ListCategoryName）
    {
        public RevitLinkInstance instance = null;
        public bool AllowElement(Element elem)
        {
            instance = elem as RevitLinkInstance;
            if (instance != null)
            {
                return true;
            }
            return false;
        }
        public bool AllowReference(Reference reference, XYZ position)
        {
            if (instance == null)
            {
                return false;
            }
            else
            {
                Document linkDocument = instance.GetLinkDocument();
                if (ListCategoryName.Contains(linkDocument.GetElement(reference.LinkedElementId).Category.Name))
                {
                    return true;
                }
                return false;
            }
        }

        private List<string> listCategoryName;
        public List<string> ListCategoryName
        {
            get { return listCategoryName; }
            set { listCategoryName = value; }
        }
    }
    #endregion

    /// <summary>
    /// 选择指定类型名称的图元
    /// </summary>
    class PickByCategorySelectionFilter : ISelectionFilter
    #region 指定手动选择指定类型的过滤器
    {
        /// <summary>
        /// 选择指定单一类型的模型
        /// </summary>
        /// <param name="el">获取element的类别</param>
        /// <returns>是否可以选择Pick</returns>
        public bool AllowElement(Element el)
        {
            if (el.Category.Name == CategoryName)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }

        public string CategoryName { get; set; }

    }
    #endregion

    /// <summary>
    /// 选择定位为LocationPoint的构件
    /// </summary>
    class PickByLocationPointSelectionFilter : ISelectionFilter
    #region 指定手动选择指定类型的过滤器
    {
        /// <summary>
        /// 模型基于点定位的构件不包含：结构柱
        /// </summary>
        /// <param name="el">获取element的类别</param>
        /// <returns>是否可以选择Pick</returns>
        public bool AllowElement(Element el)
        {
            if (el.Location is LocationPoint && el.Category.Name != "结构柱" && el.Category.Name != "柱")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
        public string CategoryName { get; set; }
    }
    #endregion

    /// <summary>
    /// 选择定位为LocationPoint的构件
    /// </summary>
    class PickByLocationCurveSelectionFilter : ISelectionFilter
    #region 指定手动选择指定类型的过滤器 Location is LocationCurve 排除：墙 结构框架
    {
        /// <summary>
        /// 模型基于点定位的构件不包含：结构柱
        /// </summary>
        /// <param name="el">获取element的类别</param>
        /// <returns>是否可以选择Pick</returns>
        public bool AllowElement(Element el)
        {
            if (el.Location is LocationCurve && el.Category.Name != "墙" && el.Category.Name != "结构框架")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
        public string CategoryName { get; set; }
        public List<string> NoCategoryName { get; set; }
    }
    #endregion

    class PickFilterLocationCurve : ISelectionFilter
    #region 指定手动选择指定类型的过滤器  Location is LocationCurve
    {
        /// <summary>
        /// 模型基于点定位的构件
        /// </summary>
        /// <param name="el">获取element的类别</param>
        /// <returns>是否可以选择Pick</returns>
        public bool AllowElement(Element el)
        {
            if (el.Location is LocationCurve)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }

    }

    #endregion

    /// <summary>
    /// 选择标记注释类别图元
    /// </summary>
    class PickDetailSelectionFilter : ISelectionFilter
    #region 手动选择注释标记等类别构件
    {
        public bool AllowElement(Element el)
        {
            if (el.Category.Name.Contains("标记") || el.Category.Name.Contains("注释"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }
    #endregion

    /// <summary>
    /// 选择模型类别图元
    /// </summary>
    class PickModelSelectionFilter : ISelectionFilter
    #region 手动选择模型类别构件
    {
        public bool AllowElement(Element el)
        {
            if (!el.Category.Name.Contains("标记") && !el.Category.Name.Contains("注释"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }
    #endregion

    /// <summary>
    /// 手动选择指定列表类型
    /// </summary>
    class PickByListCategorySelectionFilter : ISelectionFilter
    #region 指定手动选择多类型列表的过滤器
    {
        /// <summary>
        /// 选择列表形式多类型的模型
        /// </summary>
        /// <param name="el">获取element的类别</param>
        /// <returns>是否可以选择Pick</returns>
        public bool AllowElement(Element el)
        {
            // 判断el是否是指定列表的类型文件 el.Category.Name
            if (ListCategoryName.Contains(el.Category.Name))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }

        private List<string> listCategoryName;
        public List<string> ListCategoryName
        {
            get { return listCategoryName; }
            set { listCategoryName = value; }
        }
    }
    #endregion

    /// <summary>
    /// 加载指定族文件
    /// </summary>
    class DoLoadFamily
    #region 为当前项目加载指定的族文件
    {
        // 项目中加载套管族文件
        public bool isTrue { get; set; }
        public bool isHave { get; set; }
        /// <summary>
        /// 是否已经载入族文件
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="familyNames"></param>
        /// <returns></returns>
        public bool IsHaveFamily(Document doc, BuiltInCategory category, string familyName)
        {
            var els = new FilteredElementCollector(doc).OfCategory(category).WhereElementIsElementType().ToElements();
            foreach (var _family in els)
            {
                var Familytype = _family as ElementType;
                if (familyName == Familytype.FamilyName)
                {
                    isHave = true;
                }
            }
            return isHave;
        }
        /// <summary>
        /// 载入族文件
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="familyName"></param>
        /// <returns></returns>
        public bool LoadFamily(Document doc, string fileName, string familyName)
        {
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            string familyNamePath = string.Format("\\Family\\{0}", fileName);
            string familyPath = Path.GetDirectoryName(thisAssemblyPath) + familyNamePath;

            using (Transaction t = new Transaction(doc))
                if (t.Start("载入族") == TransactionStatus.Started)
                {
                    Family family = null;
                    try
                    {
                        var loadfamily = doc.LoadFamily(familyPath, out family);
                        family.Name = familyName;
                        t.Commit();
                        isTrue = true;
                    }
                    catch (Exception)
                    {
                        t.RollBack();
                        isTrue = false;
                    }
                }

            return isTrue;
        }
    }
    #endregion

    /// <summary>
    /// 创建 视图过滤器
    /// </summary>
    class ViewFilterRule
    #region 创建视图过滤器
    {
        public ParameterFilterElement FilterElement { get; set; }         // 过滤器 ParameterFilterElement
        public bool IsHave { get; set; }         // 过滤规则是否存在

        public int Transparency { get; set; }         // 颜色透明度 %
        /// <summary>
        /// 创建视图--过滤器规则
        /// </summary>
        /// <param name="doc"> </param>
        /// <param name="view"> 视图element</param>
        /// <param name="categories"> 指定模型类别</param>
        /// <param name="filterKey"> name：key</param>
        /// <param name="filterValue"> 注释：value</param>
        public void EqualsRule(Document doc, Autodesk.Revit.DB.View view, List<ElementId> categories, string filterKey, string filterValue, Color color)
        {
            // 创建过滤器名称
            var Rulesname = filterKey + "-" + filterValue;
            var elementsType = new FilteredElementCollector(doc).WhereElementIsNotElementType().ToElements();

            #region 判断过滤器规则是否存在
            IsHave = true;
            foreach (var item in elementsType)
            {
                if (item.Name == Rulesname)
                {
                    IsHave = false;
                    FilterElement = item as ParameterFilterElement;
                }
            }
            #endregion

            #region 创建过滤器
            using (Transaction t = new Transaction(doc, "创建审查过滤器"))
            {
                t.Start();
                // 如果 没有 则创建规则并创建过滤器
                if (IsHave)
                {
                    FilterElement = ParameterFilterElement.Create(doc, Rulesname, categories);
                    SetFilter(doc, filterValue, FilterElement, view, color);
                }
                // 如果 有 判断是否已创建
                else
                {
                    // 获取当前视图所有过滤器
                    var colFilter = view.GetFilters();
                    for (int i = 0; i < colFilter.Count; i++)
                    {
                        FilterElement filter = doc.GetElement(colFilter.ElementAt(i)) as FilterElement;
                        if (filter.Name == Rulesname)
                        {
                            IsHave = true;
                        }
                    }
                    // 如果当前视图不存在则创建
                    if (!IsHave)
                    {
                        SetFilter(doc, filterValue, FilterElement, view, color);
                    }
                }
                t.Commit();
            }
            #endregion
        }

        /// <summary>
        /// 删除审查过滤器
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="keyName"> 指定过滤器包含的特殊字符</param>
        /// <param name="DelAllFilter"> 删除当前视图中所有的过滤器</param>
        public void DelFilter(Document doc, string keyName, bool DelAllFilter = false)
        {
            // 获取当前视图所有的过滤器
            var collection = doc.ActiveView.GetFilters();
            if (collection.Count != 0)
            #region 删除存在的指定过滤器
            {
                var t = new Transaction(doc);
                t.Start("删除审查过滤器");
                try
                {
                    foreach (var item in collection)
                    {
                        var filter = doc.GetElement(item) as FilterElement;
                        if (DelAllFilter)
                        {
                            // 删除当前视图中所有的过滤器
                            doc.ActiveView.RemoveFilter(filter.Id);
                        }
                        else
                        {
                            // 删除包含特定字符的过滤器
                            if (filter.Name.Contains(keyName))
                            {
                                doc.ActiveView.RemoveFilter(filter.Id);
                            }
                        }

                    }
                    t.Commit();
                }
                catch (Exception e)
                {
                    t.RollBack();
                    TaskDialog.Show("提示", e.Message + Strings.error);
                }
            }
            #endregion
        }

        /// <summary>
        /// 设置视图过滤器--显示规则
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="filterValue"> 注释的值 value</param>
        /// <param name="filterElement"> 过滤器element</param>
        /// <param name="view"> 设置视图</param>
        /// <param name="color"> 颜色样式</param>
        public void SetFilter(Document doc, string filterValue, ParameterFilterElement filterElement, Autodesk.Revit.DB.View view, Color color)
        {
            var filterId = filterElement.Id;
            // 注释参数
            ElementId exteriorParamId = new ElementId(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);
            List<FilterRule> rules = new List<FilterRule>
            {
                ParameterFilterRuleFactory.CreateEqualsRule(exteriorParamId, filterValue, true)
            };

            filterElement.SetRules(rules);

            // 添加过滤器到视图 
            view.AddFilter(filterId);
            doc.Regenerate();
            // 设置 显示状态
            OverrideGraphicSettings ogs = view.GetFilterOverrides(filterId);
            var CutPattern = FillPatternElement.GetFillPatternElementByName(doc, FillPatternTarget.Drafting, "实体填充");

            ogs.SetCutFillColor(color);
            ogs.SetCutFillPatternId(CutPattern.Id);
            // 设置表面填充 实体填充
            ogs.SetProjectionFillColor(color);
            ogs.SetProjectionFillPatternId(CutPattern.Id);
            if (filterValue == "OK")
            {
                Transparency = 35;
            }
            else
            {
                Transparency = 0;
            }
            ogs.SetSurfaceTransparency(Transparency);
            // 设置截面线颜色
            ogs.SetCutLineColor(color);
            // 设置投影线颜色
            ogs.SetProjectionLineColor(color);
            // 设置显示状态
            view.SetFilterOverrides(filterId, ogs);
        }
    }
    #endregion

    /// <summary>
    /// 替换视图中的图形
    /// </summary>
    class ColorWithModel
    #region 给模型当前视图替换颜色设置
    {
        /// <summary>
        /// 恢复图形替换
        /// </summary>
        /// <param name="uidoc"></param>
        /// <param name="doc"></param>
        /// <param name="el"></param>
        public void BackOverride(UIDocument uidoc, Document doc, Element el)
        {
            Transaction trans = new Transaction(doc);
            trans.Start("更新错误");
            try
            {
                // 恢复图形当前视图替换
                var overrideGraphicSettings = new OverrideGraphicSettings();
                var activeView = uidoc.ActiveView;
                // 图形替换恢复
                activeView.SetElementOverrides(el.Id, overrideGraphicSettings);
                trans.Commit();
            }
            catch (Exception)
            {
                trans.RollBack();
            }
        }

        /// <summary>
        /// 设定图形当前视图替换
        /// </summary>
        /// <param name="uidoc"></param>
        /// <param name="doc"></param>
        /// <param name="sel"></param>
        /// <param name="color"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void ElementOverrideGraphicSetting(UIDocument uidoc, Document doc, Selection sel, Color color, string key, string value = null)
        {
            try
            {
                var ogs = new OverrideGraphicSettings();
                var activeView = uidoc.ActiveView;

                // 中文样板 2018 api
                var CutPattern = FillPatternElement.GetFillPatternElementByName(doc, FillPatternTarget.Drafting, "实体填充");
                // 设置截面填充 实体填充
                ogs.SetCutFillColor(color);
                ogs.SetCutFillPatternId(CutPattern.Id);
                // 设置表面填充 实体填充
                ogs.SetProjectionFillColor(color);
                ogs.SetProjectionFillPatternId(CutPattern.Id);
                // 设置截面线颜色
                ogs.SetCutLineColor(color);
                // 设置投影线颜色
                ogs.SetProjectionLineColor(color);

                var selIDs = sel.GetElementIds();
                if (selIDs.Count() > 0)
                {
                    Transaction trans = new Transaction(doc);
                    trans.Start("模型审查");

                    // 关闭警告
                    FailureHandlingOptions fho = trans.GetFailureHandlingOptions();
                    fho.SetFailuresPreprocessor(new FailuresPreprocessor());
                    trans.SetFailureHandlingOptions(fho);

                    foreach (var selID in selIDs)
                    {
                        var el = doc.GetElement(selID);
                        // 设置当前视图颜色
                        activeView.SetElementOverrides(el.Id, ogs);

                        // 设置注释 ALL_MODEL_INSTANCE_COMMENTS
                        el.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set(key);
                        if (value == null)
                        {
                            var nowDate = DateTime.Now.ToString(); //获取当前时间
                            el.get_Parameter(BuiltInParameter.DOOR_NUMBER).Set(nowDate);
                        }
                        else
                        {
                            el.get_Parameter(BuiltInParameter.DOOR_NUMBER).Set(value);
                        }
                    }
                    trans.Commit();
                }
            }
            catch
            {
                TaskDialog.Show("提示", "2020api已修改，未作更新");
            }
        }

        /// <summary>
        /// 设置错误模型显示
        /// </summary>
        /// <param name="uidoc"></param>
        /// <param name="doc"></param>
        /// <param name="el"> element 模型</param>
        /// <param name="color"> 设置颜色</param>
        /// <param name="key"> 设置注释内容</param>
        public void ElementSetError(UIDocument uidoc, Document doc, Element el, Color color, string key = null)
        {
            var version = doc.Application.VersionNumber;
            try
            {
                var ogs = new OverrideGraphicSettings();
                var activeView = uidoc.ActiveView;

                if (version == "2020")
                {
                    // 中文样板 2018 api
                    var CutPattern = FillPatternElement.GetFillPatternElementByName(doc, FillPatternTarget.Drafting, "<实体填充>");
                    //// 设置截面填充 实体填充
                    //ogs.SetCutBackgroundPatternColor(color);
                    //ogs.SetCutBackgroundPatternId(CutPattern.Id);
                    //ogs.SetCutForegroundPatternColor(color);
                    //ogs.SetCutForegroundPatternId(CutPattern.Id);
                    //// 设置表面填充 实体填充
                    //ogs.SetSurfaceBackgroundPatternColor(color);
                    //ogs.SetSurfaceBackgroundPatternId(CutPattern.Id);
                    //ogs.SetSurfaceForegroundPatternColor(color);
                    //ogs.SetSurfaceForegroundPatternId(CutPattern.Id);
                    // 设置截面线颜色
                    ogs.SetCutLineColor(color);
                    // 设置投影线颜色
                    ogs.SetProjectionLineColor(color);

                    Transaction trans = new Transaction(doc);
                    trans.Start("更新错误");
                    try
                    {
                        // 关闭警告
                        FailureHandlingOptions fho = trans.GetFailureHandlingOptions();
                        fho.SetFailuresPreprocessor(new FailuresPreprocessor());
                        trans.SetFailureHandlingOptions(fho);
                        // 设置当前视图颜色
                        activeView.SetElementOverrides(el.Id, ogs);
                        // 设置注释 ALL_MODEL_INSTANCE_COMMENTS
                        el.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set(key);
                        trans.Commit();
                    }
                    catch
                    {
                        trans.RollBack();
                    }
                }
                else
                {
                    // 中文样板 2018 api
                    var CutPattern = FillPatternElement.GetFillPatternElementByName(doc, FillPatternTarget.Drafting, "实体填充");
                    // 设置截面填充 实体填充
                    ogs.SetCutFillColor(color);
                    ogs.SetCutFillPatternId(CutPattern.Id);
                    // 设置表面填充 实体填充
                    ogs.SetProjectionFillColor(color);
                    ogs.SetProjectionFillPatternId(CutPattern.Id);
                    // 设置截面线颜色
                    ogs.SetCutLineColor(color);
                    // 设置投影线颜色
                    ogs.SetProjectionLineColor(color);

                    Transaction trans = new Transaction(doc);
                    trans.Start("更新错误");
                    try
                    {
                        // 关闭警告
                        FailureHandlingOptions fho = trans.GetFailureHandlingOptions();
                        fho.SetFailuresPreprocessor(new FailuresPreprocessor());
                        trans.SetFailureHandlingOptions(fho);
                        // 设置当前视图颜色
                        activeView.SetElementOverrides(el.Id, ogs);
                        // 设置注释 ALL_MODEL_INSTANCE_COMMENTS
                        el.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set(key);

                        trans.Commit();
                    }
                    catch
                    {
                        trans.RollBack();
                    }
                }
            }
            catch
            {
                TaskDialog.Show("error-api", "api已更新\n");
            }
        }

        public OverrideGraphicSettings ogs { get; set; }
    }
    #endregion

    /// <summary>
    /// 关闭警告弹窗
    /// </summary>
    class FailuresPreprocessor : IFailuresPreprocessor
    #region 关闭弹出的警告窗口
    {
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            IList<FailureMessageAccessor> listFma = failuresAccessor.GetFailureMessages();
            if (listFma.Count == 0)
                return FailureProcessingResult.Continue;
            foreach (FailureMessageAccessor fma in listFma)
            {
                if (fma.GetSeverity() == FailureSeverity.Error)
                {
                    if (fma.HasResolutions())
                        failuresAccessor.ResolveFailure(fma);
                }
                if (fma.GetSeverity() == FailureSeverity.Warning)
                {
                    failuresAccessor.DeleteWarning(fma);
                }
            }
            return FailureProcessingResult.ProceedWithCommit;
        }
    }
    #endregion

    /// <summary>
    /// 获取给定标高底部或顶部的level -element
    /// </summary>
    class GetLevel
    #region 获取给定基准标高: true 获取顶部的标高  false 获取底部的标高
    {
        /// <summary>
        /// level: 基准标高
        ///     true 获取顶部的标高
        ///     false 获取底部的标高
        ///     项目仅一个标高返回当前标高
        /// </summary>
        /// <param name="level"></param>
        /// <param name="First">  true false</param>
        /// <returns> Level 标高</returns>
        public Level NearLevel(Level level, bool First)
        {
            var L = level.ProjectElevation;
            // 获取所有高度小于给定的标高
            var eles = new FilteredElementCollector(level.Document).OfCategory(BuiltInCategory.OST_Levels).WhereElementIsNotElementType();
            List<MineParaters> mineParaters = new List<MineParaters>();
            foreach (var item in eles)
            {
                var el = item as Level;
                var elevation = el.ProjectElevation;
                mineParaters.Add(new MineParaters { MineLevel = el, MineProjectElevation = elevation });
            }

            if (mineParaters.Count > 0)
            {
                if (First)
                {
                    try
                    {
                        var res = mineParaters.Where(i => i.MineProjectElevation > L).OrderBy(i => i.MineProjectElevation).First();
                        return res.MineLevel;
                    }
                    catch { return level; }

                }
                else
                {
                    try
                    {
                        var res = mineParaters.Where(i => i.MineProjectElevation < L).OrderBy(i => i.MineProjectElevation).Last();
                        return res.MineLevel;
                    }
                    catch { return level; }
                }
            }
            else
            {
                return level;
            }
        }
    }
    #endregion

    /// <summary>
    /// 获取连接CAD图纸中的信息
    /// </summary>
    class CAD
    #region 获取链接CAD图纸的图形线或文字内容
    {
        /// <summary>
        /// 获取 CAD图纸 PolyLine
        /// </summary>
        /// <param name="coordinates"></param>
        /// <param name="dwg"></param>
        /// <returns> GCurveArray</returns>
        public CurveArray GetCurveArray(IList<XYZ> coordinates, ImportInstance dwg)
        {
            try
            {
                CurveArray curveArray = new CurveArray();
                Transform transform = dwg.GetTransform();

                for (int i = 0; i < coordinates.Count - 1; i++)
                {
                    if (i < coordinates.Count - 2)
                    {
                        Line line = Line.CreateBound(transform.OfPoint(coordinates[i]), transform.OfPoint(coordinates[i + 1]));
                        curveArray.Append(line);
                    }
                    else
                    {
                        Line line2 = Line.CreateBound(transform.OfPoint(coordinates[i]), transform.OfPoint(coordinates[0]));
                        curveArray.Append(line2);
                    }
                }
                return curveArray;
            }
            catch (Exception e)
            {
                TaskDialog.Show("提示", e.Message);
                return null;
            }
        }

        /// <summary>
        /// 获得CAD文字
        /// </summary>
        /// <param name="reference"> 选择图纸对应的对象</param>
        /// <param name="doc"> Document</param>
        /// <returns> string：文字内容 或者 空字符</returns>
        public string GetCADText(Reference reference, Document doc)
        {
            try
            {
                var selPo = reference.GlobalPoint;
                var CADLinkInstance = doc.GetElement(reference) as Instance;
                var textType = new FilteredElementCollector(doc).OfClass(typeof(TextNoteType)).Cast<TextNoteType>().ToList().FirstOrDefault();
                var styleId = -1;
                try
                {
                    styleId = int.Parse((doc.GetElement(CADLinkInstance.GetGeometryObjectFromReference(reference).GraphicsStyleId) as GraphicsStyle).Id.ToString());
                }
                catch { }
                var geoNode = new CGeoNode();
                var element = doc.GetElement(reference);
                var geo = element.GetGeometryObjectFromReference(new Reference(element));
                var textNodes = new List<CTextNode>();
                geoNode.ParaseGeoText(geo, textNodes);
                var type = textNodes.FirstOrDefault().GetType();
                XYZ po = new XYZ();
                string str = "--";
                double num = double.MaxValue;
                foreach (var textNode in textNodes)
                {
                    try
                    {
                        if (textNode.m_idStyle != styleId)
                            continue;

                        var cPoint = textNode.GetType().GetField("m_pt1").GetValue(textNode) as CPointMen;
                        var point = new XYZ(cPoint.m_dx, cPoint.m_dy, cPoint.m_dz);
                        var num1 = point.DistanceTo(selPo);
                        if (num1 < num)
                        {
                            num = num1;
                            str = textNode.m_sValue;
                            po = point;
                        }
                    }
                    catch { continue; }
                }
                return str;
            }
            catch
            {
                return "--";
            }
        }

        /// <summary>
        /// 隐藏所选CAD图层
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="geometryObject"></param>
        /// <returns></returns>
        public bool HideCADGraphicsStyleCategory(Document doc, GeometryObject geometryObject)
        {
            bool push = false;
            if (doc is null)
                throw new ArgumentNullException(nameof(doc));

            if (geometryObject is null)
                throw new ArgumentNullException(nameof(geometryObject));

            var result = doc.GetElement(geometryObject.GraphicsStyleId);
            Element res = result is GraphicsStyle ? result : null;

            if (res != null)
            {
                using (var transaction = new Transaction(doc, "隐藏图层"))
                {
                    transaction.Start();
                    doc.ActiveView.SetCategoryHidden(res.Id, true);
                    transaction.Commit();
                    push = true;
                }
            }

            return push;
        }
    }
    #endregion

    /// <summary>
    /// 获取族类型
    /// </summary>
    class GetFamilySymbolType
    #region 获取特定名称的族类型，不存在则自动复制类型 返回FamilySymbol
    {
        /// <summary>
        /// 获取特定名称的族类型，不存在则自动复制类型
        /// </summary>
        /// <param name="doc"> 文档</param>
        /// <param name="builtInCategory"> 过滤器 OfCategory类别</param>
        /// <param name="familyname"> 族类型完整名称</param>
        /// <returns></returns>
        public FamilySymbol GetFamilySymbol(Document doc, BuiltInCategory builtInCategory, string familyname)
        {
            // 获取 梁类型
            var BeamType = new FilteredElementCollector(doc).OfCategory(builtInCategory).WhereElementIsElementType().ToElements();
            // 获取所有的类型名称 
            foreach (Element el in BeamType)
            {
                var Familytype = el as ElementType;
                // 类型名称 是否等于 familyname
                var _FamilytypeName = Familytype.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString();
                if (_FamilytypeName == familyname)
                {
                    Symbol = Familytype as FamilySymbol;
                }
            }
            if (Symbol != null)
            {
                return Symbol;
            }
            else
            {
                var elementType = BeamType.First() as ElementType;
                Transaction t = new Transaction(doc);
                t.Start("创建类型");
                Symbol = elementType.Duplicate(familyname) as FamilySymbol;
                t.Commit();
                return Symbol;
            }
        }

        private FamilySymbol symbol;
        public FamilySymbol Symbol
        {
            get { return symbol; }
            set { symbol = value; }
        }
    }
    #endregion

    /// <summary> 
    /// 颜色选择可视化
    /// </summary>
    class ColorDialog
    #region 设置颜色选择可视化工具
    {

        public Color SelectColorDialog()
        {
            var ColorDLG = new ColorSelectionDialog();
            var result = ColorDLG.Show();
            // 选择了颜色
            if (result.ToString() == "Confirmed")
            {
                var color = ColorDLG.SelectedColor;
                return color;
            }
            // 取消选择颜色
            else
            {
                TaskDialog.Show("警告", "已取消操作！");
                return null;
            }
        }

    }
    #endregion

    class GetRegistryBackgroundColor
    #region 读取保存的颜色RGB值 返回 color
    {
        readonly RegistryStorage Registry = new RegistryStorage();

        public Color BackgroundColor(string r, string g, string b)
        {
            var a0 = int.TryParse(Registry.OpenAfterStart(r), out int B0_r);
            var b0 = int.TryParse(Registry.OpenAfterStart(g), out int B0_g);
            var c0 = int.TryParse(Registry.OpenAfterStart(b), out int B0_b);

            if (a0 && b0 && c0)
            {
                return new Color((byte)B0_r, (byte)B0_g, (byte)B0_b);
            }
            else
            {
                return new Color((byte)_r, (byte)_g, (byte)_b);
            }
        }
        public int _r { get; set; }
        public int _g { get; set; }
        public int _b { get; set; }
    }
    #endregion

    /// <summary>
    /// 空格 完成多选
    /// </summary>
    class ToFinish
    #region 使用空格代替确认完成按键
    {
        private IKeyboardMouseEvents m_GlobalHook;
        /// <summary>
        /// 空格即完成
        /// </summary>
        public void Subscribe()
        {
            // Note: for the application hook, use the Hook.AppEvents() instead
            m_GlobalHook = Hook.GlobalEvents();
            m_GlobalHook.KeyUp += GlobalHookKeySpace;
            //m_GlobalHook.MouseUpExt += GlobalHookMouseUpExt;
        }

        /// <summary>
        /// 监控 键盘空格键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GlobalHookKeySpace(object sender, KeyEventArgs e)
        {
            // 32 represent Space
            if (e.KeyValue == 32) { CompleteMultiSelection(); }

        }

        /// <summary>
        /// 卸载任务
        /// </summary>
        public void Unsubscribe()
        {
            //m_GlobalHook.MouseUpExt -= GlobalHookMouseUpExt;
            m_GlobalHook.KeyUp -= GlobalHookKeySpace;
            //It is recommened to dispose it
            m_GlobalHook.Dispose();
        }

        /// <summary>
        /// 触发按键后调用程序
        /// </summary>
        private void CompleteMultiSelection()
        {
            var rvtWindow = Autodesk.Windows.ComponentManager.ApplicationWindow;
            var list = new List<IntPtr>();
            var flag = WindowsHelper.EnumChildWindows(rvtWindow,
                       (hwnd, l) =>
                       {
                           StringBuilder windowText = new StringBuilder(200);
                           WindowsHelper.GetWindowText(hwnd, windowText, windowText.Capacity);
                           StringBuilder className = new StringBuilder(200);
                           WindowsHelper.GetClassName(hwnd, className, className.Capacity);
                           if ((windowText.ToString().Equals("完成", StringComparison.Ordinal) ||
                          windowText.ToString().Equals("Finish", StringComparison.Ordinal)) &&
                          className.ToString().Contains("Button"))
                           {
                               list.Add(hwnd);
                               return false;
                           }
                           return true;
                       }, new IntPtr(0));

            var complete = list.FirstOrDefault();
            WindowsHelper.SendMessage(complete, 245, 0, 0);
        }
    }

    /// <summary>
    /// 引用库
    /// </summary>
    public static class WindowsHelper
    {
        [DllImport("user32.dll", CharSet = CharSet.None, ExactSpelling = false)]
        public static extern bool EnumChildWindows(IntPtr hwndParent, CallBack lpEnumFunc, IntPtr lParam);
        public delegate bool CallBack(IntPtr hwnd, int lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpText, int nCount);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
        [DllImport("user32.dll", EntryPoint = "SendMessageA")]
        public static extern int SendMessage(IntPtr hwnd, uint wMsg, int wParam, int lParam);
    }
    #endregion

    /// <summary>
    /// 合并 solids-solid
    /// </summary>
    class SolidByUnion
    #region 合并 solids --> solid
    {
        // 过程合并 solid
        private Solid SolidUnion { get; set; }

        public Solid ByUnion(IList<Element> allElements)
        {
            List<Solid> solids = new List<Solid>();
            if (allElements.Count != 0)
            {
                Options options = new Options();

                // 提取element solid
                foreach (var item in allElements)
                {
                    var geometry = item.get_Geometry(options);
                    foreach (var _solid in geometry)
                    {
                        //Solid solid = _solid as Solid;
                        //// 判断是否是实体
                        //if (solid != null && solid.Volume > 0)
                        //{
                        //    solids.Add(solid);
                        //}

                        #region 判断是不是族实例在获取实体
                        try
                        {
                            Solid solid = _solid as Solid;
                            // 判断是否是实体
                            if (solid != null && solid.Volume > 0)
                            {
                                solids.Add(solid);
                            }
                        }
                        catch
                        {
                            // el 为 FamilyInstance 类别
                            if (item is FamilyInstance)
                            {
                                var ins = _solid as GeometryInstance;
                                foreach (GeometryObject solidIns in ins.GetInstanceGeometry())
                                {
                                    Solid solid = solidIns as Solid;
                                    // 判断是否是实体
                                    if (solid != null && solid.Volume > 0)
                                    {
                                        solids.Add(solid);
                                    }
                                }
                            }
                            else
                            {
                                Solid solid = _solid as Solid;
                                // 判断是否是实体
                                if (solid != null && solid.Volume > 0)
                                {
                                    solids.Add(solid);
                                }
                            }
                        }
                        #endregion
                    }
                }

                // 合并solids
                if (solids.Count > 1)
                {
                    SolidUnion = solids[0];
                    for (int i = 1; i < solids.Count; i++)
                    {
                        SolidUnion = BooleanOperationsUtils.ExecuteBooleanOperation(SolidUnion, solids[i], BooleanOperationsType.Union);
                    }
                }
                return SolidUnion;
            }
            else
            {
                TaskDialog.Show("提示", "所选择的构件不存在实体");
                return null;
            }

        }

    }
    #endregion

    /// <summary>
    /// 单位转换类
    /// </summary>
    static class UUtools
    #region 单位转换
    {
        /// <summary>
        /// 毫米 转 英尺
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double MillimetersToUnits(double value)
        {
            return UnitUtils.Convert(value, DisplayUnitType.DUT_MILLIMETERS, DisplayUnitType.DUT_DECIMAL_FEET);
        }

        /// <summary>
        /// 英尺 转 毫米
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double UnitsToMillimeters(double value)
        {
            return UnitUtils.Convert(value, DisplayUnitType.DUT_DECIMAL_FEET, DisplayUnitType.DUT_MILLIMETERS);
        }
    }
    #endregion

    /// <summary>
    /// 读取json文件
    /// </summary>
    class ReadJson
    #region 依据给定的文件地址读取json文件key对应的值
    {
        public static string ByKey(string key)
        {
            try
            {
                using (StreamReader file = File.OpenText(JsonFilePath))
                {
                    try
                    {
                        using (JsonTextReader reader = new JsonTextReader(file))
                        {
                            JObject o = (JObject)JToken.ReadFrom(reader);
                            var value = o[key].ToString();
                            return value;
                        }
                    }
                    catch
                    {
                        //TaskDialog.Show("提示", "为找到相应的key参数");
                        return null;
                    }

                }
            }
            catch
            {
                TaskDialog.Show("提示", "为找到相应的json文件");
                return null;
            }
        }

        /// <summary>
        /// json文件位置
        /// </summary>
        public static string JsonFilePath { get; set; }
    }
    #endregion

    /// <summary>
    /// 列表数据 --> 缓存文件
    /// </summary>
    static class TempFile
    #region 缓存数据保存到文件
    {
        // 新增本地存储数据且存在则跳过
        public static void WriteSelIdsToFile(List<ElementId> list, string title)
        #region 保存selIds list 为本地的缓存文件
        {
            // 查看已保存的 ids
            var ids = ReadSelIdsFileToList(title);
            var Intids = ReadSelIdsFileToListInt(title);

            var filePath = @"C:\ProgramData\Autodesk\ApplicationPlugins\hybh2020.bundle\Contents\2018\data\SelID_" + title + ".hybh";
            //创建一个文件流，用以写入或者创建一个StreamWriter 
            FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.Flush();
            // 使用StreamWriter来往文件中写入内容 
            sw.BaseStream.Seek(0, SeekOrigin.Begin);

            foreach (var id in list)
            {
                if (ids.Count == 0)
                {
                    sw.WriteLine(id);
                }
                else if (!Intids.Contains(id.IntegerValue))
                {
                    sw.WriteLine(id);
                }
            }

            // 保存历史数据
            if (ids.Count != 0)
            {
                foreach (var id in ids)
                {
                    sw.WriteLine(id);
                }
            }

            //关闭此文件t 
            sw.Flush();
            sw.Close();
            fs.Close();
        }
        #endregion

        // 删除本次选择的构件 id
        public static void DelSelIdsToFile(List<ElementId> list, string title)
        #region 删除selIds 文件 本次选择的ids
        {
            // 查看已保存的 ids
            var ids = ReadSelIdsFileToList(title);
            var Intids = ReadSelIdsFileToListInt(title);

            var filePath = @"C:\ProgramData\Autodesk\ApplicationPlugins\hybh2020.bundle\Contents\2018\data\SelID_" + title + ".hybh";
            //创建一个文件流，用以写入或者创建一个StreamWriter 
            FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.Flush();
            // 使用StreamWriter来往文件中写入内容 
            sw.BaseStream.Seek(0, SeekOrigin.Begin);

            foreach (var id in list)
            {
                if (ids.Count != 0 && Intids.Contains(id.IntegerValue))
                {
                    ids.Remove(id);
                }
            }

            //关闭此文件t 
            sw.Flush();
            sw.Close();
            fs.Close();

            File.WriteAllText(filePath, string.Empty);

            // 重新保存文件
            WriteSelIdsToFile(ids, title);
        }
        #endregion

        // 获取本地保存文件的 ids
        public static List<ElementId> ReadSelIdsFileToList(string title)
        #region 读取selIds 文件存储的数据 返回 list<ElementId> 格式
        {
            List<ElementId> list = new List<ElementId>();
            try
            {
                // 读取文本文件转换为List 

                var filePath = @"C:\ProgramData\Autodesk\ApplicationPlugins\hybh2020.bundle\Contents\2018\data\SelID_" + title + ".hybh";
                FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);
                //使用StreamReader类来读取文件 
                sr.BaseStream.Seek(0, SeekOrigin.Begin);
                // 从数据流中读取每一行，直到文件的最后一行
                string tmp = sr.ReadLine();
                while (tmp != null)
                {
                    ElementId _id = new ElementId(Convert.ToInt32(tmp));
                    list.Add(_id);
                    tmp = sr.ReadLine();
                }
                //关闭此StreamReader对象 
                sr.Close();
                fs.Close();
                return list;
            }
            catch
            {
                // 未保存 ids
                return list;
            }

        }
        #endregion

        // 获取本地保存文件的 ids
        public static List<int> ReadSelIdsFileToListInt(string title)
        #region 读取selIds 文件存储的数据 返回 list<int> 格式
        {
            List<int> list = new List<int>();
            try
            {
                // 读取文本文件转换为List 

                var filePath = @"C:\ProgramData\Autodesk\ApplicationPlugins\hybh2020.bundle\Contents\2018\data\SelID_" + title + ".hybh";
                FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);
                //使用StreamReader类来读取文件 
                sr.BaseStream.Seek(0, SeekOrigin.Begin);
                // 从数据流中读取每一行，直到文件的最后一行
                string tmp = sr.ReadLine();
                while (tmp != null)
                {
                    list.Add(Convert.ToInt32(tmp));
                    tmp = sr.ReadLine();
                }
                //关闭此StreamReader对象 
                sr.Close();
                fs.Close();
                return list;
            }
            catch
            {
                // 未保存 ids
                return list;
            }
        }
        #endregion
    }
    #endregion

    /// <summary>
    /// PickObject 接收参数名定义
    /// </summary>
    static class SelPick
    #region PickObject 接收参数名定义
    {
        public static Reference SelRef { get; set; }
        public static List<Reference> SelRefsList { get; set; }

        public static Reference SelRef_two { get; set; }
        public static List<Reference> SelRefsList_two { get; set; }

        public static Reference SelRef_three { get; set; }
        public static List<Reference> SelRefsList_three { get; set; }
    }
    #endregion

    /// <summary>
    /// 获取定位线curve与模型实体相交点的中心点（两个交点）
    /// </summary>
    static class GetInsertSolidPoint
    #region 定位线curve与模型实体交点的中心点
    {
        /// <summary>
        /// 中点
        /// </summary>
        public static XYZ MidPoint { get; set; }
        /// <summary>
        /// 获取穿过实体的管线与实体两个面相交的中点
        /// </summary>
        /// <param name="Insertelment"> 实体模型</param>
        /// <param name="curve"> 定位线 curve</param>
        /// <returns></returns>
        public static XYZ GetMidPointsWithCurve(Element Insertelment, Curve curve)
        {
            // 设置定位点列表
            List<XYZ> Points = new List<XYZ>();
            // 创建几何选项
            var options = new Options();
            var geometry = Insertelment.get_Geometry(options);
            foreach (GeometryObject obj in geometry)
            {
                // el 为 FamilyInstance 类别
                if (Insertelment is FamilyInstance)
                {
                    var ins = obj as GeometryInstance;
                    foreach (GeometryObject _solid in ins.GetInstanceGeometry())
                    {
                        List<XYZ> xYZs = SolidIntersectCurve(_solid, curve);
                        if (xYZs.Count > 0)
                        {
                            Points = xYZs;
                        }
                    }
                }
                // el 为 楼板 墙 等类别
                else
                {
                    List<XYZ> xYZs = SolidIntersectCurve(obj, curve);
                    if (xYZs.Count > 0)
                    {
                        Points = xYZs;
                    }
                }

                // 两个交点
            }
            if (Points.Count == 2)
            {
                MidPoint = (Points[0] + Points[1]) / 2;
            }
            return MidPoint;
        }

        /// <summary>
        /// 获取solid 与 curve 相交的点 
        /// </summary>
        /// <param name="_solid"></param>
        /// <param name="curve"></param>
        /// <returns> 返回 List<XYZ></returns>
        public static List<XYZ> SolidIntersectCurve(GeometryObject _solid, Curve curve)
        {
            // 设置定位点列表
            List<XYZ> Points = new List<XYZ>();

            Solid solid = _solid as Solid;
            if (solid == null || solid.Faces.Size == 0 || solid.Edges.Size == 0)
            {
                return Points;
            }
            FaceArray faceArray = solid.Faces;
            if (faceArray.Size == 0 || faceArray == null)
            {
                return Points;
            }

            // 相交OUT结果
            IntersectionResultArray Result = new IntersectionResultArray();
            foreach (var faceI in faceArray)
            {
                var face = faceI as Face;

                var intersect = face.Intersect(curve, out Result);
                if (intersect == SetComparisonResult.Overlap)
                {
                    var intersectPoint = Result.get_Item(0).XYZPoint;
                    Points.Add(intersectPoint);
                }
            }
            return Points;
        }
    }
    #endregion

    /// <summary>
    /// 获取点到模型实体面的距离
    /// </summary>
    class PointDistanceSolidFace
    #region 获取点到模型实体面的指定方向的距离
    {
        public double Distance { get; set; }
        public double PointZ { get; set; }
        public Face Face { get; set; }
        public XYZ IntersectPoint { get; set; }

        /// <summary>
        /// 获取点到模型实体面的指定方向的距离
        /// </summary>
        /// <param name="el"> 计算的模型实体</param>
        /// <param name="xYZ"> 定位点</param>
        /// <param name="Direction"> 方向</param>
        /// <param name="First"> true：底面 flase：顶面</param>
        /// <returns> 点到模型Z轴的距离</returns>
        public PointDistanceSolidFace GetPointDistanceFace(Element el, XYZ xYZ, XYZ Direction, bool First)
        {
            Line line = Line.CreateUnbound(xYZ, Direction);
            List<PointDistanceSolidFace> dictDoubleFaces = new List<PointDistanceSolidFace>();
            // 创建几何选项
            var options = new Options() { DetailLevel = ViewDetailLevel.Fine };
            GeometryElement geometry = el.get_Geometry(options);
            try
            {
                foreach (GeometryObject obj in geometry)
                {
                    // el 为 FamilyInstance 类别
                    if (el is FamilyInstance)
                    {
                        var ins = obj as GeometryInstance;
                        foreach (GeometryObject _solid in ins.GetInstanceGeometry())
                        {
                            dictDoubleFaces = SolidIntersectCurve(_solid, xYZ, Direction);
                        }
                    }
                    else
                    {
                        dictDoubleFaces = SolidIntersectCurve(obj, xYZ, Direction);
                    }
                }
            }

            catch (Exception e)
            {
                TaskDialog.Show("提示", e.Message + Strings.error);
            }

            //foreach (var item in dictDoubleFaces)
            //{
            //    TaskDialog.Show("---", (item.Distance * 304.8).ToString());
            //}

            if (dictDoubleFaces.Count > 0)
            {
                if (First)
                {
                    var res = dictDoubleFaces.OrderBy(i => i.PointZ).First();
                    return res;
                }
                else
                {
                    // List<DictDoubleFaces> 按distance 值进行排序 并获取第一个 DictDoubleFaces
                    var res = dictDoubleFaces.OrderBy(i => i.PointZ).Last();
                    return res;
                }
            }
            else
            {
                return null;
            }
        }

        public List<PointDistanceSolidFace> SolidIntersectCurve(GeometryObject _solid, XYZ xYZ, XYZ Direction)
        {
            // 设置定位点列表
            List<PointDistanceSolidFace> dictDoubleFaces = new List<PointDistanceSolidFace>();
            // 创建辅助线
            Line line = Line.CreateUnbound(xYZ, Direction);
            // 判断实体是否存在
            Solid solid = _solid as Solid;
            if (solid == null || solid.Faces.Size == 0 || solid.Edges.Size == 0)
            {
                return dictDoubleFaces;
            }
            FaceArray faceArray = solid.Faces;
            if (faceArray.Size == 0 || faceArray == null)
            {
                return dictDoubleFaces;
            }
            // 相交OUT结果
            IntersectionResultArray result = new IntersectionResultArray();
            foreach (var faceI in faceArray)
            {
                var face = faceI as Face;
                var intersect = face.Intersect(line, out result);
                if (intersect == SetComparisonResult.Overlap)
                {
                    var intersectPoint = result.get_Item(0).XYZPoint;
                    var distanceP = intersectPoint.DistanceTo(xYZ);
                    dictDoubleFaces.Add(new PointDistanceSolidFace
                    {
                        Face = face,
                        IntersectPoint = intersectPoint,
                        Distance = distanceP,
                        PointZ = intersectPoint.Z
                    });
                }
            }
            // 返回 List<PointDistanceSolidFace>
            return dictDoubleFaces;
        }
    }
    #endregion

    /// <summary>
    /// 获取分析模型的模型线的坐标点
    /// </summary>
    static class GetAnalyCurve
    #region 获取分析模型的坐标点
    {
        /// <summary>
        /// 获取结构柱定位点
        /// 通过结构柱 结构分析 定位顶点和低点
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="el"> 结构模型</param>
        /// <returns></returns>
        public static List<XYZ> GetCurveTessellate(Document doc, Element el)
        {
            var instance = el as FamilyInstance;
            // 获取结构分析 参数
            var isS = instance.get_Parameter(BuiltInParameter.STRUCTURAL_ANALYTICAL_MODEL);
            if (isS.AsInteger() != 1)
            {
                Transaction t = new Transaction(doc);
                t.Start("启用结构分析");
                try
                {
                    isS.Set(1);
                    t.Commit();
                }
                catch (Exception e)
                {
                    t.RollBack();
                    TaskDialog.Show("提示", e.Message + Strings.error);
                }
            }

            List<XYZ> TwoPoints = new List<XYZ>();
            try
            {
                // 获取分析模型的模型线
                var analyticalModel = instance.GetAnalyticalModel();
                var analyLine = analyticalModel.GetCurve() as Line;
                TwoPoints = analyLine.Tessellate().ToList();
                // List < XYZ >
                return TwoPoints;
            }
            catch
            {
                return TwoPoints;
            }
        }
    }
    #endregion

    static class MakeElement
    #region 修改element图元的属性
    {
        /// <summary>
        /// 旋转构件（放在事务过程中）
        /// </summary>
        /// <param name="el"> 需要旋转的构件</param>
        /// <param name="element"> 获取构件的角度值</param>
        /// <returns> 返回 bool</returns>
        public static bool ToRotaPointElement(Element el, Element element)
        {
            if (el.Location is LocationPoint && element.Location is LocationPoint)
            {
                var locationPoint = (el.Location as LocationPoint).Point;
                var el_Rotation = (el.Location as LocationPoint).Rotation;
                var element_Rotation = (element.Location as LocationPoint).Rotation;
                // 创建旋转轴
                var axis = Line.CreateBound(locationPoint, new XYZ(locationPoint.X, locationPoint.Y, locationPoint.Z + 10));
                // 计算旋转的弧度
                var angle = element_Rotation - el_Rotation;
                return el.Location.Rotate(axis, angle);
            }
            else
            {
                return false;
            }

        }
    }
    #endregion

    /// <summary>
    /// 创建revit内部拓展数据储存
    /// </summary>
    class StoreDataCreate
    #region 拓展数据储存-double类型数据
    {
        /// <summary>
        /// 构件绑定double类型的数据
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="entity"></param>
        /// <param name="el"></param>
        /// <param name="key"></param>
        /// <param name="height"></param>
        /// <returns> 成功返回true 失败false</returns>
        public bool StoreDataWriteDouble(Schema schema, Entity entity, Element el, string key, double height)
        {
            using (Transaction createSchemaAndStoreData = new Transaction(el.Document, "write"))
            {
                createSchemaAndStoreData.Start();
                try
                {
                    Field fieldSpliceLocation = schema.GetField(key); // get the field from the schema
                    entity.Set<double>(fieldSpliceLocation, height, DisplayUnitType.DUT_DECIMAL_FEET); // set the value for this entity
                    el.SetEntity(entity); // store the entity in the element

                    createSchemaAndStoreData.Commit();
                    return true;
                }
                catch (Exception)
                {
                    createSchemaAndStoreData.RollBack();
                    return false;
                }
            }
        }

        /// <summary>
        /// 读取拓展存储的数据并返回值
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="el"></param>
        /// <param name="key"></param>
        /// <returns> double 类型的值</returns>
        public double StoreDataReadDouble(Schema schema, Element el, string key)
        {
            try
            {
                Entity retrievedEntity = el.GetEntity(schema);
                double retrievedData = retrievedEntity.Get<double>(schema.GetField(key), DisplayUnitType.DUT_METERS);
                //TaskDialog.Show("单位：m", retrievedData.ToString());
                return retrievedData;
            }
            catch
            {
                return 0.0;
            }
        }
    }
    #endregion


    #region 定义参照的列表
    class MineParaters
    {
        public double MineProjectElevation { get; set; }
        public Level MineLevel { get; set; }
    }
    #endregion

    #region 一键创建CAD图纸模型
    /// <summary>
    /// 创建模型信息
    /// </summary>
    class AutoCADModel
    {
        public XYZ LocationPoint { get; set; }  // 定位点
        public XYZ MidPoint { get; set; }       // 中点
        public double B { get; set; }           // 宽度
        public double H { get; set; }           // 高度
        public double Rotation { get; set; }    // 旋转角度
    }

    /// <summary>
    /// 识别CAD文字
    /// </summary>
    class AutoCADTextDate
    {
        public string TextNote { get; set; }
        public XYZ LocationPoint { get; set; }
        public double Rotation { get; set; }
    }

    /// <summary>
    /// 构件定位点及旋转角度
    /// </summary>
    class AutoCADInstanceData
    {
        public XYZ LocationPoint { get; set; }
        public double Angle { get; set; }
    }
    #endregion

    /// <summary>
    /// 固定字符串 -- 糖
    /// </summary>
    class Strings
    #region 设置 const 固定字符串内容
    {
        // 固定字符串 "mongodb://revit:revit.123@111.229.98.184:27017/RevitUser"
        public const string Lower = "abcdefghijklmnopqrstuvwxyz";
        public const string Upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public const string Number = "0123456789";
        public const string error = "\n\n\n----使用问题\n----请添加QQ加群：17075104\n----进行反馈及咨询";
        public const string key = "robot";
        public const string value = "000000x0";
        public const string hybhSpace = "hybh.";
        public const string Revit = "revit";
        public const string Dot = ".";
        public const string User = "RevitUser";
        public const string Background = "#FFC8FFFF";

        //////////////////////////////  语法糖 无用处部分 //////////////////////////////////////////////
        #region None Data
        public string Password { get; set; }
        public string Username { get; set; }

        private int keyNumber;

        public int KeyNumber
        {
            get { return keyNumber; }
            set { keyNumber = value; }
        }

        private string data;

        public string Data
        {
            get { return data; }
            set
            {
                if (Strings.Number != Strings.Lower)
                {
                    data = value;
                }
                if (Strings.Number != Strings.Lower)
                {
                    KeyNumber = 0;
                }
            }
        }
        #endregion
    }
    #endregion
}

