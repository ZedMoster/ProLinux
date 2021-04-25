using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace Xml
{
    public class CADModel
    {
        /// <summary>
        /// 定位线 Curves
        /// </summary>
        public List<Curve> Curves { get; set; }
        /// <summary>
        /// 定位点 XYZs
        /// </summary>
        public List<XYZ> XYZs { get; set; }
        /// <summary>
        /// 定位点 XYZ
        /// </summary>
        public XYZ LocationPoint { get; set; }
        /// <summary>
        /// 图形旋转角度
        /// </summary>
        public double Rotation { get; set; }
        /// <summary>
        /// 数字字符串
        /// </summary>
        public string ValueString { get; set; }
        /// <summary>
        /// 宽度
        /// </summary>
        public double Width { get; set; }
        /// <summary>
        /// 长度
        /// </summary>
        public double Length { get; set; }
        /// <summary>
        /// 定位中线
        /// </summary>
        public Curve MidCurve { get; set; }
    }

    public class CADText
    {
        /// <summary>
        /// 文字内容
        /// </summary>
        public string TextNote { get; set; }
        /// <summary>
        /// 文字定位点
        /// </summary>
        public XYZ LocationPoint { get; set; }
        /// <summary>
        /// 文字旋转角度
        /// </summary>
        public double Rotation { get; set; }
    }
}
