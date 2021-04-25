using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace Xml
{
    public static class HybhTransaction
    {
        /// <summary>
        /// 忽略警告弹窗提示
        /// </summary>
        /// <param name="t"></param>
        public static void NoFailure(this Transaction t)
        {
            FailureHandlingOptions fho = t.GetFailureHandlingOptions().SetFailuresPreprocessor(new FailuresPreprocessor());
            t.SetFailureHandlingOptions(fho);
        }
    }

    /// <summary>
    /// 跳过警告提示
    /// </summary>
    public class FailuresPreprocessor : IFailuresPreprocessor
    {
        /// <summary>
        /// 忽略警告弹窗
        /// </summary>
        /// <param name="failuresAccessor"></param>
        /// <returns></returns>
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            IList<FailureMessageAccessor> listFma = failuresAccessor.GetFailureMessages();
            if (listFma.Count == 0)
                return FailureProcessingResult.Continue;
            foreach (FailureMessageAccessor fma in listFma)
            {
                if (fma.GetSeverity() == FailureSeverity.Error)
                {
                    if (fma.HasResolutions())
                        failuresAccessor.ResolveFailure(fma);
                }
                if (fma.GetSeverity() == FailureSeverity.Warning)
                {
                    failuresAccessor.DeleteWarning(fma);
                }
            }
            return FailureProcessingResult.ProceedWithCommit;
        }
    }
}
