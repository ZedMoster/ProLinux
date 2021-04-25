using Autodesk.Revit.UI.Events;

namespace AppRevitEventArgs
{

    public class RevitSavingArgs
    {

        /// <summary>
        /// 关闭软件事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void Close(object sender, ApplicationClosingEventArgs args)
        {

        }

        /// <summary>
        /// 启动软件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void Start(object sender, DocumentOpened args)
        {

        }
    }
}
