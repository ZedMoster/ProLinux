using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows;
using MongoDB.Bson;
using MongoDB.Driver;

namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class LogoutUser : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // 注册列表文件读写方法
            RegistryStorage registryStorage = new RegistryStorage();
            // 存本地数据

            string userName = registryStorage.OpenAfterStart("name");
            string userPassword = registryStorage.OpenAfterStart("password");
            var keysValue = registryStorage.OpenAfterStart(Strings.key);
            if (keysValue != Strings.value)
            {
                var collection = ClientMongoDB.GetClient("userInfo");
                var filter = Builders<BsonDocument>.Filter.Eq("password", userPassword) & Builders<BsonDocument>.Filter.Eq("name", userName);
                var result = collection.Find(filter).ToList();

                // 存本地数据
                registryStorage.SaveBeforeExit(Strings.key, Strings.value);
                // 登录最大次数 +1
                var tMax = result[0]["max"].ToInt32();
                tMax++;
                collection.UpdateOne(filter, Builders<BsonDocument>.Update.Set("max", tMax));
                // 提示
                MessageBox.Show("注销成功！", "提示");
            }
            else
            {
                MessageBox.Show("未登录无需注销！", "提示");
            }
            return Result.Succeeded;
        }
    }
}
