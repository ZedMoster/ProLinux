using Autodesk.Revit.UI;
using System;
using System.Windows;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;

namespace hybh
{
    /// <summary>
    /// WPFLogin.xaml 的交互逻辑
    /// </summary>
    public partial class WPFLogin : Window
    {
        public WPFLogin()
        {
            InitializeComponent();
        }

        private void Signin_Click(object sender, RoutedEventArgs e)
        {
            // 注册窗口
            Window windowsigin = new WPFSignin();
            windowsigin.ShowDialog();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            // 注册列表文件读写方法
            RegistryStorage registryStorage = new RegistryStorage();

            //1. 获取数据
            //从TextBox中获取用户输入信息
            string userName = this.name.Text;
            string userPassword = this.password.Password;

            //2. 验证数据
            // 验证用户输入是否为空，若为空，提示用户信息
            if (userName.Equals("") || userPassword.Equals(""))
            {
                MessageBox.Show("用户名或密码不能为空！", "提示");
            }
            // 若不为空，验证用户名和密码是否与数据库匹配
            // 这里只做字符串对比验证
            else
            {
                var collection = ClientMongoDB.GetClient("userInfo");
                var filter = Builders<BsonDocument>.Filter.Eq("password", userPassword) & Builders<BsonDocument>.Filter.Eq("name", userName);
                var result = collection.Find(filter).ToList();

                //获取数据
                if (result.Count == 1)
                {
                    //用户名和密码验证正确，提示成功，并执行跳转界面。
                    var value = result[0]["_id"].ToString();
                    var keysValue = registryStorage.OpenAfterStart(Strings.key);

                    var tMax = result[0]["max"].ToInt32();
                    if (keysValue != value)
                    {
                        if (tMax > 0)
                        {
                            tMax--;
                            collection.UpdateOne(filter, Builders<BsonDocument>.Update.Set("max", tMax));
                            // 存本地数据
                            registryStorage.SaveBeforeExit(Strings.key, value);
                            registryStorage.SaveBeforeExit("name", userName);
                            registryStorage.SaveBeforeExit("password", userPassword);
                            MessageBox.Show("登录成功！", "提示");
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("最大登录次数已为0\n需要注销释放登录接口(默认：2)", "提示");
                            this.Close();
                        }
                    }
                    else
                    {
                        MessageBox.Show("未注销用户且已成功登录！", "提示");
                        this.Close();
                    }
                }
                else
                {
                    MessageBox.Show("用户名或密码错误！\n若没注册过请先注册才可以正常使用！" , "提示");
                }
            }
        }
    }
}
