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
    /// wpfFloor.xaml 的交互逻辑
    /// </summary>
    public partial class WPFFloor : Window
    {
        public WPFFloor(Document doc)
        {
            InitializeComponent();
            // 选择列表
            ElementList.ItemsSource = GetSelectedValues(doc, BuiltInCategory.OST_Floors);
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
            ElementsType = isElementType ? new FilteredElementCollector(doc).OfCategory(category).WhereElementIsElementType().ToElements()
                : new FilteredElementCollector(doc).OfCategory(category).WhereElementIsNotElementType().ToElements();
            var SelectedValue = new List<SelectElement>();
            // 获取所有的类型名称 
            foreach (Element el in ElementsType)
            {
                SelectedValue.Add(new SelectElement { Element = el, ElementName = el.Name, Checked = checking });
            }
            return SelectedValue;
        }

        /// <summary>
        /// 全选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            var levelList = LevelList.ItemsSource as List<SelectElement>;
            var SelectedValue = new List<SelectElement>();
            // 更新绑定数据
            foreach (var item in levelList)
            {
                if (!item.Checked)
                    item.Checked = !item.Checked;
                SelectedValue.Add(item);
            }
            LevelList.ItemsSource = SelectedValue;
        }

        /// <summary>
        /// 反选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 创建
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            #region 未指定楼板类型
            if (ElementList.SelectedIndex == -1)
            {
                MessageBox.Show("需要指定楼板类型名称", "警告");
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

        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closed(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 取消创建
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void No_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 输入数字
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9.-]+");
            e.Handled = re.IsMatch(e.Text);
        }
    }
}
