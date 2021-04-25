using Autodesk.Revit.UI;

namespace HybhCADModel.Lib
{
    class ShowResult
    {
        /// <summary>
        /// 没有确认选择-1
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message1"></param>
        /// <returns></returns>
        public static bool Print<T>(T message1)
        {
            TaskDialog mainDialog = new TaskDialog("结果")
            {
                //MainInstruction = "确认",
                //MainContent = "本次选择的是否正确",
                FooterText = "确认数据是否符合需求"
            };
            mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, $"{message1}");
            mainDialog.CommonButtons = TaskDialogCommonButtons.No;
            TaskDialogResult tResult = mainDialog.Show();
            return TaskDialogResult.CommandLink1 != tResult;
        }
    }
}
