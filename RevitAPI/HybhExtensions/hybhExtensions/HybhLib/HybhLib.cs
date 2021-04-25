using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace Xml
{
    /// <summary> 
    /// 颜色选择可视化工具
    /// </summary>
    public static class ColorDialog
    {
        /// <summary>
        /// 选择颜色
        /// </summary>
        /// <returns> Autodesk.Revit.DB.Color </returns>
        public static Color Pick()
        {
            var ColorDLG = new ColorSelectionDialog();
            var result = ColorDLG.Show();

            if (result.ToString() != "Confirmed")
            {
                MessageBox.Show("已取消颜色选择");
                return null;
            }
            return ColorDLG.SelectedColor;
        }
    }

    /// <summary>
    /// 提交事务组
    /// </summary>
    public static class ToPush
    {
        /// <summary>
        /// 存在正确
        /// </summary>
        /// <param name="Pushs"></param>
        /// <param name="any"></param>
        /// <returns></returns>
        public static bool Any(List<bool> Pushs, bool any = true)
        {
            return Pushs.Any(i => i == any);
        }
    }



    /// <summary>
    /// 注册列表键值读写
    /// </summary>
    public static class RegistryStorage
    {
        /// <summary>
        /// 注册列表通过名称读取值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool OpenDict(string key, out string value)
        {
            // 定义变量
            bool push;
            try
            {
                // HKCU\Software\RegistryInfor
                Microsoft.Win32.RegistryKey registryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\RegistryInfor");
                value = registryKey.GetValue(key).ToString();
                registryKey.Close();
                push = true;
            }
            catch (Exception)
            {
                MessageBox.Show("错误代码 701");
                value = null;
                push = false;
            }
            return push;
        }

        /// <summary>
        /// 注册列表保存名称和值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool SaveDict(string key, string value)
        {
            bool push;
            try
            {
                // HKCU\Software\RegistryInfor
                Microsoft.Win32.RegistryKey registryKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\RegistryInfor");
                registryKey.SetValue(key, value);
                registryKey.Close();
                push = true;
            }
            catch (Exception)
            {
                MessageBox.Show("错误代码 702");
                push = false;
            }
            return push;
        }
    }


}
