using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace hybh
{
    /// <summary>
    /// WPFSelectKeyWordElements.xaml 的交互逻辑
    /// </summary>
    public partial class WPFSelectKeyWordElements : Window
    {
        public WPFSelectKeyWordElements(IList<Element> elements)
        {
            InitializeComponent();

            List<SelectElementByName> listelement = new List<SelectElementByName>();
            List<string> words = new List<string>();
            foreach (Element el in elements)
            {
                var s = el.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString();
                if (s!="")
                {
                    if (!words.Contains(s))
                    {
                        listelement.Add(new SelectElementByName { HybhKey = s, HybhValue = s });
                        words.Add(s);
                    }
                }
            }
            KeyWord.ItemsSource = listelement;
            KeyWord.SelectedIndex = 0;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
