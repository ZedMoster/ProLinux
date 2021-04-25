using System;
using System.Collections.Generic;
using System.Windows;

using Autodesk.Revit.DB;

namespace CADReader.View
{
    /// <summary>
    /// WPFFamilySymbol.xaml 的交互逻辑
    /// </summary>
    public partial class WPFFamilySymbol : Window
    {
        public WPFFamilySymbol(Document doc, BuiltInCategory inCategory)
        {
            InitializeComponent();

            // 数据绑定族名称
            ElementList.ItemsSource = GetSelectedValues(doc, inCategory);
            ElementList.SelectedIndex = 0;
        }

        /// <summary>
        /// 获取楼板类型
        /// </summary>
        private List<SelectElement> GetSelectedValues(Document doc, BuiltInCategory category)
        {
            List<SelectElement> SelectedValue = new List<SelectElement>();
            // 获取所有 category 的类型
            IList<Element> ElementsType = new FilteredElementCollector(doc).OfCategory(category).WhereElementIsElementType().ToElements();
            if (ElementsType.Count != 0)
            {
                // 获取所有的类型名称 
                foreach (Element el in ElementsType)
                {
                    SelectedValue.Add(new SelectElement { Element = el, ElementName = el.Name });
                }
            }
            return SelectedValue;
        }

        /// <summary>
        /// 确认创建
        /// </summary>
        private void Create_Click(object sender, RoutedEventArgs e)
        {
            #region 未指定类型
            if (ElementList.SelectedIndex == -1)
            {
                MessageBox.Show("需要选定创建的类型名称", "警告");
                return;
            }
            #endregion
            Close();
            IsHitTestVisible = false;
        }

        /// <summary>
        /// 关闭创建
        /// </summary>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        private void Window_Closed(object sender, EventArgs e)
        {
            Close();
        }
    }
}
