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

namespace AGBIMMunicipalPipeline
{
    /// <summary>
    /// Main.xaml 的交互逻辑
    /// </summary>
    public partial class Main : Window
    {
        public OriginPoint OriginPoint { get; set; }
        public Main()
        {
            InitializeComponent();
            Init();
        }

        public void Init()
        {
            _X.Text = "0";
            _Y.Text = "0";
            _Z.Text = "0";

            _FilePath.Text = @"D:\缓存文件\临时\万环西路调查表(合并).xlsx";

            OriginPoint = new OriginPoint();
        }

        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            _FilePath.Text = ToSelectFileGetPath();
        }

        private string ToSelectFileGetPath()
        {
            string path = string.Empty;
            var openFileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Excel格式文件 (*.xlsx)|*.xlsx"
            };
            var result = openFileDialog.ShowDialog();
            if(result == true)
            {
                path = openFileDialog.FileName;
            }
            return path;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            OriginPoint.X = _X.Text;
            OriginPoint.Y = _Y.Text;
            OriginPoint.Z = _Z.Text;
            OriginPoint.FilePath = _FilePath.Text;
            Close();
            IsHitTestVisible = false;
        }

        private void NO_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }


    public class OriginPoint
    {
        public string X { get; set; }
        public string Y { get; set; }
        public string Z { get; set; }

        public string FilePath { get; set; }
    }
}
