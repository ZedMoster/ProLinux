using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace HybhCADModel.Lib
{
    public class FailureDelete : IFailuresPreprocessor
    {
        /// <summary>
        /// 忽略警告
        /// </summary>
        /// <param name="failuresAccessor"></param>
        /// <returns></returns>
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            IList<FailureMessageAccessor> failureMessages = failuresAccessor.GetFailureMessages();
            // 无警告
            if(failureMessages.Count == 0)
            {
                return FailureProcessingResult.Continue;
            }
            // 删除警告
            foreach(FailureMessageAccessor failure in failureMessages)
            {
                // 判断错误（Error）类型
                if(failure.GetSeverity() == FailureSeverity.Error)
                {
                    // 删除 HasResolutions 的警告
                    if(failure.HasResolutions())
                    {
                        failuresAccessor.ResolveFailure(failure);
                    }
                }
                // 删除 Warning
                if(failure.GetSeverity() == FailureSeverity.Warning)
                {
                    failuresAccessor.DeleteWarning(failure);
                }
            }
            return FailureProcessingResult.ProceedWithCommit;
        }
    }
}
