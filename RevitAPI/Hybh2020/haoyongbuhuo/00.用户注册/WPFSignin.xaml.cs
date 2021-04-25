using Autodesk.Revit.UI;
using System;
using System.Windows;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;

namespace hybh
{
    /// <summary>
    /// WPFSignin.xaml 的交互逻辑
    /// </summary>
    public partial class WPFSignin : Window
    {
        public WPFSignin()
        {
            InitializeComponent();
        }

        private void SignTrue_Click(object sender, RoutedEventArgs e)
        {
            var userText = this.userText.Text;
            var passwordText = this.passwordText.Text;
            var captchaText = this.captchaText.Text;

            var _collection = ClientMongoDB.GetClient("userInfo");
            var filter = Builders<BsonDocument>.Filter.Eq("captcha", captchaText) & Builders<BsonDocument>.Filter.Eq("name", userText);
            var result = _collection.Find(filter).ToList();

            // 设置验证码
            if (result.Count != 0)
            {
                var update = Builders<BsonDocument>.Update.Set("password", passwordText);
                _collection.UpdateMany(filter, update);
                MessageBox.Show("用户注册成功！", "提示");
                this.Close();
            }
            else
            {
                MessageBox.Show("用户注册失败！\n验证码错误", "提示");
            }
        }

        private void SignFlase_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
