using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

using Autodesk.Revit.DB;

namespace CADReader.WPF
{
    /// <summary>
    /// wpfWall.xaml 的交互逻辑
    /// </summary>
    public partial class WPFWall : Window
    {
        public WPFWall(Document doc)
        {
            InitializeComponent();

            // 选择列表
            ElementList.ItemsSource = GetSelectedValues(doc, BuiltInCategory.OST_Walls);
            LevelList.ItemsSource = GetSelectedValues(doc, BuiltInCategory.OST_Levels, isElementType: false);
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
            {
                ElementsType = new FilteredElementCollector(doc).OfCategory(category).WhereElementIsElementType().ToElements();
            }
            else
            {
                ElementsType = new FilteredElementCollector(doc).OfCategory(category).WhereElementIsNotElementType().ToElements();
            }

            var SelectedValue = new List<SelectElement>();
            // 获取所有的类型名称 
            foreach (Element el in ElementsType)
            {
                SelectedValue.Add(new SelectElement { Element = el, ElementName = el.Name, Checked = checking });
            }
            return SelectedValue;
        }

        // 全选
        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            var levelList = LevelList.ItemsSource as List<SelectElement>;
            var SelectedValue = new List<SelectElement>();
            // 更新绑定数据
            foreach (var item in levelList)
            {
                if (!item.Checked)
                {
                    item.Checked = !item.Checked;
                }
                SelectedValue.Add(item);
            }
            LevelList.ItemsSource = SelectedValue;
        }

        // 反选
        private void SelectTran_Click(object sender, RoutedEventArgs e)
        {
            var levelList = LevelList.ItemsSource as List<SelectElement>;
            var SelectedValue = new List<SelectElement>();
            // 更新绑定数据
            foreach (var item in levelList)
            {
                item.Checked = !item.Checked;
                SelectedValue.Add(item);
            }
            LevelList.ItemsSource = SelectedValue;
        }

        // 创建
        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            #region 未指定类型
            if (ElementList.SelectedIndex == -1)
            {
                MessageBox.Show("需要指定类型名称", "警告");
                return;
            }
            #endregion

            #region 未选中创建标高
            var levelList = LevelList.ItemsSource as List<SelectElement>;
            var levels = levelList.Where(x => x.Checked == true).ToList();
            if (levels.Count == 0)
            {
                MessageBox.Show("至少应选中一个标高", "警告");
                return;
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

        // 取消创建
        private void No_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // 输入数字
        private void ToTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9.]+");
            e.Handled = re.IsMatch(e.Text);
        }
    }
}
