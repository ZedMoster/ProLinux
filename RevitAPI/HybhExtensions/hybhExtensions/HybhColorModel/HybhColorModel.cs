using Autodesk.Revit.DB;

namespace Xml
{
    public static class ColorWithModelExtensions
    {
        /// <summary>
        /// 设置错误模型显示
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="el"> element 模型</param>
        public static bool SetColor(this Document doc, Element el)
        {
            MakeBackgroundColor _color = new MakeBackgroundColor(255, 0, 0);
            Color color = _color.BackgroundColor();
            var ogs = new OverrideGraphicSettings();
#if RVT2020
            var CutPattern = FillPatternElement.GetFillPatternElementByName(doc, FillPatternTarget.Drafting, "<实体填充>");
            ogs.SetCutBackgroundPatternColor(color);
            ogs.SetCutBackgroundPatternId(CutPattern.Id);
            ogs.SetSurfaceBackgroundPatternColor(color);
            ogs.SetSurfaceBackgroundPatternId(CutPattern.Id);
            ogs.SetCutForegroundPatternColor(color);
            ogs.SetCutForegroundPatternId(CutPattern.Id);
            ogs.SetCutLineColor(color);
            ogs.SetProjectionLineColor(color);
#else
            var CutPattern = FillPatternElement.GetFillPatternElementByName(doc, FillPatternTarget.Drafting, "实体填充");
            ogs.SetCutFillColor(color);
            ogs.SetCutFillPatternId(CutPattern.Id);
            ogs.SetProjectionFillColor(color);
            ogs.SetProjectionFillPatternId(CutPattern.Id);
            ogs.SetCutLineColor(color);
            ogs.SetProjectionLineColor(color);
#endif
            Transaction trans = new Transaction(doc);
            trans.Start("更新颜色");
            try
            {
                // 关闭警告
                FailureHandlingOptions fho = trans.GetFailureHandlingOptions();
                fho.SetFailuresPreprocessor(new FailuresPreprocessor());
                trans.SetFailureHandlingOptions(fho);
                // 设置当前视图颜色
                doc.ActiveView.SetElementOverrides(el.Id, ogs);
                trans.Commit();
                return true;
            }
            catch
            {
                trans.RollBack();
                return false;
            }
        }
    }


    /// <summary>
    /// 创建 color 颜色
    /// </summary>
    public class MakeBackgroundColor
    {
        public MakeBackgroundColor(int r, int g, int b)
        {
            _r = r;
            _g = g;
            _b = b;
        }
        public Color BackgroundColor()
        {
            return new Color((byte)_r, (byte)_g, (byte)_b);
        }
        public int _r { get; set; }
        public int _g { get; set; }
        public int _b { get; set; }
    }
}
