using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

using Autodesk.Revit.DB;

namespace CADReader.WPF
{
    /// <summary>
    /// WPFFamilySymbol.xaml 的交互逻辑
    /// </summary>
    public partial class WPFFamilySymbol : Window
    {
        public Document Doc { get; set; }
        public BuiltInCategory BuiltInCategory { get; set; }

        public WPFFamilySymbol(Document doc, BuiltInCategory inCategory, List<CADTextModel> textModels)
        {
            InitializeComponent();
            this.Doc = doc;
            this.BuiltInCategory = inCategory;

            // 数据绑定族名称
            FamilyList.ItemsSource = GetSelectedValues(doc, textModels, doc.CategoryName(BuiltInCategory));
        }

        /// <summary>
        /// 确认创建
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Create_Click(object sender, RoutedEventArgs e)
        {
            var DataList = FamilyList.ItemsSource as List<CADModel>;
            var AllFamilyNameNotNull = DataList.All(i => i.SelectFamilyName != null);
            if (!AllFamilyNameNotNull)
            {
                return;
            }

            // 传递窗口输入参数
            this.Close();
            this.IsHitTestVisible = false;
        }

        /// <summary>
        /// 关闭创建
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 获取指定类别的族
        /// </summary>
        /// <returns></returns>
        private List<CADModel> GetSelectedValues(Document doc, List<CADTextModel> textModels, string category)
        {
            // 获取所有注释类型
            List<CADModel> SymbolList = new List<CADModel>();
            // 所有族类型
            List<string> familyNames = new List<string>();
            var symbolTypes = SymbolTypeNames(doc, out familyNames);

            foreach (var item in textModels)
            {
                if (!SymbolList.Any(i => i.SymbolName == item.Text))
                {
                    if (item.Text.Contains(category.FristChar()))
                    {
                        var para = GetParameter(item, category.FristChar(), out bool push);
                        SymbolList.Add(new CADModel()
                        {
                            SymbolName = item.Text,
                            Lhight = item.Text.Contains("C") ? 900 : 0,
                            Width = para[0],
                            Higth = para[1],
                            FamilyNameList = familyNames,
                            FamilyList = symbolTypes,
                            SelectFamilyName = familyNames.FirstOrDefault(),
                        });
                    }
                }
            }
            return SymbolList;
        }

        /// <summary>
        /// 获取指定类别的族
        /// </summary>
        /// <returns></returns>
        private List<SelectFamily> SymbolTypeNames(Document doc, out List<string> familyNames)
        {
            List<SelectFamily> selectFamilies = new List<SelectFamily>();
            familyNames = new List<string>();
            // 获取所有的类型
            var elsType = new FilteredElementCollector(doc).OfCategory(BuiltInCategory).WhereElementIsElementType().ToList();
            if (elsType.Count == 0)
            {
                return selectFamilies;
            }
            foreach (var item in elsType)
            {
                var _symbol = item as FamilySymbol;
                var _family = _symbol.Family;
                var _familyName = _symbol.FamilyName;
                if (!familyNames.Contains(_familyName))
                {
                    selectFamilies.Add(new SelectFamily() { Family = _family, FamilyName = _familyName });
                    familyNames.Add(_familyName);
                }
            }
            return selectFamilies;
        }

        /// <summary>
        /// 获取参数尺寸
        /// </summary>
        /// <param name="textModel"></param>
        /// <param name="nameContains"></param>
        /// <param name="push"></param>
        /// <returns></returns>
        private List<double> GetParameter(CADTextModel textModel, char nameContains, out bool push)
        {
            push = false;
            List<double> res = new List<double> { 0, 0 };
            var parameter = textModel.Text.Split(nameContains).LastOrDefault();
            string match = Regex.Replace(parameter, @"[^0-9]+", "");

            if (match.Length == 4)
            {
                try
                {
                    var width = match.Substring(0, 2).Parse(100);
                    var hight = match.Substring(2).Parse(100);
                    res[0] = width;
                    res[1] = hight;
                }
                catch (Exception) { }
                push = true;
            }
            return res;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
