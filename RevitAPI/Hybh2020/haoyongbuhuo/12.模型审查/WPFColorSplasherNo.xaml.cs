using System;
using System.Windows;

namespace hybh
{
    /// <summary>
    /// WPFColorSplasherNo.xaml 的交互逻辑
    /// </summary>
    public partial class WPFColorSplasherNo : Window
    {
        readonly RegistryStorage RegistryStorage = new RegistryStorage();
        public WPFColorSplasherNo()
        {
            InitializeComponent();
            // 更新文字的内容
            this.text.Text = RegistryStorage.OpenAfterStart("text_SplasherNo") ?? "问题：";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RegistryStorage.SaveBeforeExit("text_SplasherNo", this.text.Text);
            this.Close();
            this.IsHitTestVisible = true;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.IsHitTestVisible = false;
            this.Close();
        }
    }
}
