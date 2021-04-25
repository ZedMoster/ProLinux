using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace CADReader
{
    /// <summary>
    /// 选择图纸
    /// </summary>
    class PickCADFilter : ISelectionFilter
    {
        public bool AllowElement(Element el)
        {
            return el.Category.Name.EndsWith(".dwg") || el.Category.Name.EndsWith(".dwg2");
        }
        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }

    /// <summary>
    /// 指定类别名称
    /// </summary>
    class PickFilterCategory : ISelectionFilter
    {
        public string CategoryName { get; set; }
        public PickFilterCategory(string cate)
        {
            CategoryName = cate;
        }
        public bool AllowElement(Element elem)
        {
            return elem.Category.Name == CategoryName;
        }
        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }

    /// <summary>
    /// 确认信息
    /// </summary>
    class Task
    {
        public static bool Show(string message)
        {
            TaskDialog mainDialog = new TaskDialog("图层类型")
            {
                MainInstruction = "确认类型",
                MainContent = "本次选择的是否正确",
                FooterText = "重试：重新选择  /  确认：确认选择"
            };
            mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, $"名称：{message}");
            mainDialog.CommonButtons = TaskDialogCommonButtons.Retry;
            TaskDialogResult tResult = mainDialog.Show();
            return TaskDialogResult.CommandLink1 != tResult;
        }
    }

    /// <summary>
    /// 选择图纸
    /// </summary>
    public class Pickdwg
    {
        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="sel"></param>
        /// <param name="reference"></param>
        /// <returns></returns>
        public bool Refer(Selection sel, out Reference reference, string status = "点选CAD图元获取相应图层信息")
        {
            PickCADFilter pickCAD = new PickCADFilter();
            try
            {
                reference = sel.PickObject(ObjectType.PointOnElement, pickCAD, status);
                return true;
            }
            catch
            {
                reference = null;
                return false;
            }
        }
    }

    /// <summary>
    /// 忽略警告
    /// </summary>
    public class FailuresPreprocessor : IFailuresPreprocessor
    {
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
