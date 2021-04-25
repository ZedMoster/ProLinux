using Autodesk.Revit.DB;

namespace HybhCADModel.Model
{
    public class CAD_Text
    {
        /// <summary>
        /// 文字内容
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// 文字定位点
        /// </summary>
        public XYZ Location { get; set; }
        /// <summary>
        /// 文字图块中心点
        /// </summary>
        public XYZ MidPoint { get; set; }
        /// <summary>
        /// 文字图层名称
        /// </summary>
        public string LayerName { get; set; }
        /// <summary>
        /// 文字旋转角度
        /// </summary>
        public double Rotation { get; set; }
    }
}
