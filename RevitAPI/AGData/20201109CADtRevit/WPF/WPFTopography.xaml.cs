using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Autodesk.Revit.DB;

namespace CADReader.WPF
{
    /// <summary>
    /// WPFTopography.xaml 的交互逻辑
    /// </summary>
    public partial class WPFTopography : Window
    {
        public WPFTopography(Document doc, BuiltInCategory Category = BuiltInCategory.OST_Floors)
        {
            InitializeComponent();
            // 选择列表
            ElementList.ItemsSource = GetSelectedValues(doc, Category);
            MaterialList.ItemsSource = GetSelectedValues(doc, BuiltInCategory.OST_Materials, isElementType: false);
        }

        /// <summary>
        /// 获取楼板类型
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private List<SelectElement> GetSelectedValues(Document doc, BuiltInCategory category, bool checking = false, bool isElementType = true)
        {
            IList<Element> ElementsType;
            if (isElementType)
                ElementsType = new FilteredElementCollector(doc).OfCategory(category).WhereElementIsElementType().ToElements();
            else
                ElementsType = new FilteredElementCollector(doc).OfCategory(category).WhereElementIsNotElementType().ToElements();

            // 获取所有的类型名称 
            var SelectedValue = new List<SelectElement>();
            foreach (Element el in ElementsType)
                SelectedValue.Add(new SelectElement { Element = el, ElementName = el.Name, Checked = checking });

            return SelectedValue;
        }

        // 创建
        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            #region 新建类型
            if (NewType.IsChecked.Value)
            {
                if (MaterialList.SelectedIndex == -1)
                {
                    MessageBox.Show("需要指定一个材质名称", "警告");
                    return;
                }
                if (NewTypeName.Text == "")
                {
                    MessageBox.Show("需要设置新类型的名称", "警告");
                    return;
                }
                if (NewTypeWidth.Text == "")
                {
                    MessageBox.Show("需要设置新类型的厚度", "警告");
                    return;
                }
            }
            else
            {
                #region 未指定类型
                if (ElementList.SelectedIndex == -1)
                {
                    MessageBox.Show("需要指定类型名称", "警告");
                    return;
                }
                #endregion
            }
            #endregion
            this.Close();
            this.IsHitTestVisible = false;
        }

        // 关闭窗口
        private void Window_Closed(object sender, EventArgs e)
        {
            this.Close();
        }

        // 名称厚度 参数
        private void NewType_TextChanged(object sender, TextChangedEventArgs e)
        {
            NewType.IsChecked = NewTypeName.Text != "";
        }

        // 输入数字
        private void Text_Input(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9.-]+");
            e.Handled = re.IsMatch(e.Text);
        }
    }
}
