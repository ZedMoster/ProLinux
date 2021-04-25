using System.Windows;

namespace HelperRevitVersion
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Pick_Click(object sender, RoutedEventArgs e)
        {
            GetRevitInfo getRfaInfo = new GetRevitInfo();
            var path = SelectFile();
            if(path == "")
            {
                return;
            }

            FilePath.Text = path;
            // 显示 revit 版本
            ShowVersion.Text = getRfaInfo.Version(path);
        }

        /// <summary>
        /// 选择文件窗口
        /// </summary>
        /// <returns></returns>
        private string SelectFile(string typeFilter = "Revit格式文件 (*.rvt)|*.rvt")
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = typeFilter
            };
            var result = openFileDialog.ShowDialog();
            // 获取文件地址
            return result == true ? openFileDialog.FileName : "";
        }
    }
}
