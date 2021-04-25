using System.Collections.Generic;
using System.Windows;

using Autodesk.Revit.UI;

namespace Xml
{
    /// <summary>
    /// 取消按键
    /// </summary>
    public static class ShowResult
    {
        /// <summary>
        /// 没有选择待确认的项
        /// </summary>
        public static bool Print<T>(T message)
        {
            TaskDialog mainDialog = new TaskDialog("结果")
            {
                //MainInstruction = "确认",
                //MainContent = "本次选择的是否正确",
                FooterText = "确认数据:"
            };
            mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, $"{message}");
            mainDialog.CommonButtons = TaskDialogCommonButtons.No;
            TaskDialogResult tResult = mainDialog.Show();
            return TaskDialogResult.CommandLink1 != tResult;
        }

        /// <summary>
        /// 取消
        /// </summary>
        public static void Cancelled()
        {
            MessageBox.Show("取消运行", "提示", MessageBoxButton.OK);
        }

        /// <summary>
        /// 成功
        /// </summary>
        public static void Succeed()
        {
            MessageBox.Show("运行成功", "结果", MessageBoxButton.OK);
        }
    }
}
