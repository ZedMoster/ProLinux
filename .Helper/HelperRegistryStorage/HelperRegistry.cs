using Microsoft.Win32;

namespace HelperRegistryStorage
{
    /// <summary>
    /// 帮助：注册列表
    /// </summary>
    public class HelperRegistry
    {
        /// <summary>
        /// 注册列表分类文件夹
        /// </summary>
        private string Category { get; set; }

        /// <summary>
        /// 注册列表二级文件夹
        /// </summary>
        private string Level { get; set; }

        /// <summary>
        /// 设置文件夹名称
        /// </summary>
        /// <param name="level"> 二级名称</param>
        /// <param name="category"> 一级名称</param>
        public HelperRegistry(string level = "", string category = "DTreeInfor")
        {
            Category = category;
            Level = level;
        }

        /// <summary>
        /// 读取注册列表数据（key）
        /// 计算机\HKEY_CURRENT_USER\Software\{DTreeInfor}\{level}
        /// </summary>
        /// <param name="key"> 键</param>
        /// <returns> 获取本地注册列表键值对信息\n指定路径下键所对应的值</returns>
        public string Get(string key)
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(GetPath());
            var value = "";
            try
            {
                value = registryKey.GetValue(key).ToString();
                registryKey.Close();
            }
            catch
            {
            }
            return value;
        }

        /// <summary>
        /// 写入本地注册列表键值对信息
        /// 计算机\HKEY_CURRENT_USER\Software\{input}
        /// </summary>
        /// <param name="key"> 键</param>
        /// <param name="value"> 值</param>
        public bool Add(string key, string value)
        {
            // 计算机\HKEY_CURRENT_USER\Software\{input}
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(GetPath());
                registryKey.SetValue(key, value);
                registryKey.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 注册地址
        /// </summary>
        /// <returns></returns>
        private string GetPath()
        {
            var address = string.IsNullOrWhiteSpace(Level)
                ? System.IO.Path.Combine("Software", Category)
                : System.IO.Path.Combine("Software", Category, Level);
            return address;
        }
    }
}
