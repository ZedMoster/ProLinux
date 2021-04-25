using System.Collections.Generic;

using Autodesk.Revit.DB;

using HybhCADModel.Model;

using RvtTxt;

namespace HybhCADModel.Units
{
    class CADText
    {
        /// <summary>
        /// 获得选择的CAD文字数据集
        /// </summary>
        public List<CAD_Text> GetPick(Document doc, Reference refer)
        {
            // 定义变量
            List<CAD_Text> textModel = new List<CAD_Text>();

            // 链接CAD图纸
            var CADLinkInstance = doc.GetElement(refer) as Instance;
            // 获取选择的图元对象
            var _geo = CADLinkInstance.GetGeometryObjectFromReference(refer);
            // 获取选择的图层类型
            var graphicsStyle = doc.GetElement(_geo.GraphicsStyleId) as GraphicsStyle;
            // 获取选择的图纸对象
            var geo = CADLinkInstance.GetGeometryObjectFromReference(new Reference(CADLinkInstance));
            // 处理文字
            var textNodes = new List<CTextNode>();
            var geoNode = new CGeoNode();
            // 获取文字
            geoNode.ParaseGeoText(geo, textNodes);

            // 获取文字内容
            var selPo = refer.GlobalPoint;
            foreach (var textNode in textNodes)
            {
                if (textNode.m_idStyle != graphicsStyle.Id.IntegerValue)
                {
                    continue;
                }
                var cPoint_0 = textNode.GetType().GetField("m_pt1").GetValue(textNode) as CPointMen;
                var cPoint_2 = textNode.GetType().GetField("m_pt3").GetValue(textNode) as CPointMen;
                var point_0 = new XYZ(cPoint_0.m_dx, cPoint_0.m_dy, cPoint_0.m_dz);
                var point_2 = new XYZ(cPoint_2.m_dx, cPoint_2.m_dy, cPoint_2.m_dz);
                var mid_point = (point_0 + point_2) * 0.5;
                if (selPo.IsAlmostEqualTo(point_0, 0.0001))
                {
                    textModel.Add(new CAD_Text
                    {
                        Text = textNode.m_sValue,
                        Location = point_0,
                        MidPoint = mid_point,
                        Rotation = textNode.m_dRot,
                        LayerName = graphicsStyle.GraphicsStyleCategory.Name,
                    });
                    break;
                }
            }

            return textModel;
        }

        /// <summary>
        /// 获取选中图层的CAD文字数据集
        /// </summary>
        public List<CAD_Text> GetLayer(Document doc, Reference refer)
        {
            // 定义变量
            List<CAD_Text> textModel = new List<CAD_Text>();

            // 链接CAD图纸
            var CADLinkInstance = doc.GetElement(refer) as Instance;
            // 获取选择的图元对象
            var _geo = CADLinkInstance.GetGeometryObjectFromReference(refer);
            // 获取选择的图层类型
            var graphicsStyle = doc.GetElement(_geo.GraphicsStyleId) as GraphicsStyle;
            // 获取选择的图纸对象
            var geo = CADLinkInstance.GetGeometryObjectFromReference(new Reference(CADLinkInstance));
            // 处理文字
            var textNodes = new List<CTextNode>();
            var geoNode = new CGeoNode();
            // 获取文字
            geoNode.ParaseGeoText(geo, textNodes);

            // 获取文字内容
            foreach (var textNode in textNodes)
            {
                if (textNode.m_idStyle != graphicsStyle.Id.IntegerValue)
                {
                    continue;
                }
                var cPoint_0 = textNode.GetType().GetField("m_pt1").GetValue(textNode) as CPointMen;
                var cPoint_2 = textNode.GetType().GetField("m_pt3").GetValue(textNode) as CPointMen;
                var point_0 = new XYZ(cPoint_0.m_dx, cPoint_0.m_dy, cPoint_0.m_dz);
                var point_2 = new XYZ(cPoint_2.m_dx, cPoint_2.m_dy, cPoint_2.m_dz);
                var mid_point = (point_0 + point_2) * 0.5;
                textModel.Add(new CAD_Text()
                {
                    Text = textNode.m_sValue,
                    Location = point_0,
                    MidPoint = mid_point,
                    Rotation = textNode.m_dRot,
                    LayerName = graphicsStyle.GraphicsStyleCategory.Name,
                });
            }

            return textModel;
        }
    }
}
