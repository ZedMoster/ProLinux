using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using HybhCADModel.Lib;

namespace HybhCADModel.Extensions
{
    public static class TransactionExcetion
    {
        /// <summary>
        /// 忽略警告弹窗提示
        /// </summary>
        /// <param name="t"></param>
        public static void NoFailure(this Transaction t)
        {
            FailureHandlingOptions fho = t.GetFailureHandlingOptions().SetFailuresPreprocessor(new FailureDelete());
            t.SetFailureHandlingOptions(fho);
        }
    }
}
