using Autodesk.Revit.UI;

namespace AppRevitEventArgs
{
    /// <summary>
    /// 保存文件时自动运行导出族文件事件（break 每次一个）
    /// </summary>
    class AppEventArgs : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            // 事件方法
            RevitSavingArgs revitSaving = new RevitSavingArgs();

            // 订阅事件
            application.Idling 
                += revitSaving.Application_Idling;
            // 中心文件
            application.ControlledApplication.DocumentSynchronizingWithCentral
                += revitSaving.DocCentralEventArgs;
            // 保存文件
            application.ControlledApplication.DocumentSaving 
                += revitSaving.DocSavingEventArgs;

            return Result.Succeeded;
        }

    }
}
