using System.Windows;

using Autodesk.Revit.UI;

namespace hybh
{
    /// <summary>
    /// WPFCreateGrids.xaml 的交互逻辑
    /// </summary>
    public partial class WPFCreateGrids : Window
    {
        RegistryStorage registryStorage = new RegistryStorage();
        public WPFCreateGrids()
        {

            InitializeComponent();

            // 读取记录的数据
            var kj = registryStorage.OpenAfterStart("WPFCreateGrids_kj");
            var js = registryStorage.OpenAfterStart("WPFCreateGrids_js");
            var kj_num = registryStorage.OpenAfterStart("WPFCreateGrids_kj_num");
            var js_num = registryStorage.OpenAfterStart("WPFCreateGrids_js_num");

            try
            {
                KJ.Text = kj;
                JS.Text = js;
                KJ_Copy.Text = kj_num;
                JS_Copy.Text = js_num;
            }
            catch
            {
                KJ_Copy.Text = "1";
                JS_Copy.Text = "A";
                TaskDialog.Show("提示", Strings.error);
            }
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            registryStorage.SaveBeforeExit("WPFCreateGrids_kj", this.KJ.Text);
            registryStorage.SaveBeforeExit("WPFCreateGrids_js", this.JS.Text);
            registryStorage.SaveBeforeExit("WPFCreateGrids_kj_num", this.KJ_Copy.Text);
            registryStorage.SaveBeforeExit("WPFCreateGrids_js_num", this.JS_Copy.Text);
            this.Close();
            // 运行
            this.IsHitTestVisible = true;
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            // 不运行
            this.IsHitTestVisible = false;
            this.Close();
        }
    }
}
