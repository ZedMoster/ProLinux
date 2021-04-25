using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace CADReader
{
    /// <summary>
    /// 获取文字数据
    /// </summary>
    public class CADTextModel
    {
        /// <summary>
        /// 文本内容
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// 图层名称
        /// </summary>
        public string LayerName { get; set; }
        /// <summary>
        /// 定位点
        /// </summary>
        public XYZ Location { get; set; }
        /// <summary>
        /// 旋转弧度
        /// </summary>
        public double Rotation { get; set; }
    }

    /// <summary>
    /// 创建模型信息
    /// </summary>
    public class CADCurveModel
    {
        /// <summary>
        /// 定位线
        /// </summary>
        public List<Curve> Curves { get; set; }
        /// <summary>
        /// 定位点列表
        /// </summary>
        public List<XYZ> XYZs { get; set; }
        /// <summary>
        /// 数字字符串
        /// </summary>
        public string ValueString { get; set; }
        /// <summary>
        /// 宽度
        /// </summary>
        public double Width { get; set; }
        /// <summary>
        /// 定位中线
        /// </summary>
        public Curve MidCurve { get; set; }
    }

    /// <summary>
    /// 标高高程
    /// </summary>
    class LevelParamter
    {
        /// <summary>
        /// 标高
        /// </summary>
        public Level MLevel { get; set; }
        /// <summary>
        /// 项目高程值
        /// </summary>
        public double MProjectElevation { get; set; }
    }

    /// <summary>
    /// 墙定位线
    /// </summary>
    class WallCurve
    {
        /// <summary>
        /// 定位线
        /// </summary>
        public Curve Curve { get; set; }
        /// <summary>
        /// 线长度
        /// </summary>
        public double Length { get; set; }
    }
}
