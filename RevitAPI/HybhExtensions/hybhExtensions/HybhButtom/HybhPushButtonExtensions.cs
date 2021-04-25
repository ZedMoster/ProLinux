﻿using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace Xml.HybhButtom
{
    public static class HybhPushButtonExtensions
    {
        /// <summary>
        /// 创建模板
        /// </summary>
        /// <param name="ButtonParamter"></param>
        /// <returns></returns>
        public static Autodesk.Revit.UI.PushButtonData PushData(this PushButtonData ButtonParamter)
        {
            // 按键集合 - 系统名称，面板名称，dll文件位置，命名空间引用
            Autodesk.Revit.UI.PushButtonData ButtonData = new Autodesk.Revit.UI.PushButtonData(
                ButtonParamter.InName,
                ButtonParamter.ButtonName,
                ButtonParamter.AssemblyName,
                ButtonParamter.NameSpace)
            {
                // 提示信息
                ToolTip = ButtonParamter.Tooltip
            };
            try
            {
                // 设置图标 32*32 大图
                string imageName = string.Format("\\Resources\\{0}", ButtonParamter.ImageName);
                string imagePath = Path.GetDirectoryName(ButtonParamter.AssemblyName) + imageName;
                Uri uri = new Uri(imagePath, UriKind.RelativeOrAbsolute);
                ButtonData.LargeImage = new BitmapImage(uri);
            }
            catch(Exception e)
            {
                _ = e.Message;
                throw new Exception($"未找到图片资源图片(LargeImage):{ButtonParamter.ImageName}");
            }
            try
            {
                // 设置图标 16*16 小图
                string imageName = string.Format("\\Resources\\{0}", ButtonParamter.StackedImageName);
                string imagePath = Path.GetDirectoryName(ButtonParamter.AssemblyName) + imageName;
                Uri uri = new Uri(imagePath, UriKind.RelativeOrAbsolute);
                ButtonData.Image = new BitmapImage(uri);
            }
            catch(Exception)
            {
                throw new Exception($"未找到图片资源图片(StackedImageName):{ButtonParamter.StackedImageName}");
            }

            return ButtonData;
        }
    }
}
