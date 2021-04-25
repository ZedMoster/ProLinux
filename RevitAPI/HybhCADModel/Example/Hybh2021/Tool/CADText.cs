using System.Collections.Generic;

using Autodesk.Revit.DB;

using RvtTxt;

namespace CADReader
{
    public class CADText
    {
        /// <summary>
        /// 点选图层文字
        /// </summary>
        public List<CADTextModel> GetLayer(Document doc, Reference refer)
        {
            // 定义变量
            List<CADTextModel> textModel = new List<CADTextModel>();
            var CADLinkInstance = doc.GetElement(refer) as Instance;
            var styleId = int.Parse((doc.GetElement(CADLinkInstance.GetGeometryObjectFromReference(refer).GraphicsStyleId) as GraphicsStyle).Id.ToString());
            var geoNode = new CGeoNode();
            var element = doc.GetElement(refer);
            var geo = element.GetGeometryObjectFromReference(new Reference(element));
            var textNodes = new List<CTextNode>();
            geoNode.ParaseGeoText(geo, textNodes);
            // 获取图层
            GraphicsStyle graphicsStyle = doc.GetElement(CADLinkInstance.GetGeometryObjectFromReference(refer).GraphicsStyleId) as GraphicsStyle;
            foreach (var textNode in textNodes)
            {
                if (textNode.m_idStyle != styleId)
                    continue;
                var cPoint_1 = textNode.GetType().GetField("m_pt1").GetValue(textNode) as CPointMen;
                var cPoint_3 = textNode.GetType().GetField("m_pt3").GetValue(textNode) as CPointMen;
                var point_1 = new XYZ(cPoint_1.m_dx, cPoint_1.m_dy, cPoint_1.m_dz);
                var point_3 = new XYZ(cPoint_3.m_dx, cPoint_3.m_dy, cPoint_3.m_dz);
                var point = (point_1 + point_3) / 2;
                textModel.Add(new CADTextModel()
                {
                    Text = textNode.m_sValue,
                    Location = point.Flatten(),
                    Rotation = textNode.m_dRot,
                });
            }

            return textModel;
        }

        /// <summary>
        /// 框选文字
        /// </summary>
        public List<CADTextModel> GetBoxText(Document doc, Reference refer)
        {
            // 定义变量
            List<CADTextModel> textModel = new List<CADTextModel>();
            var CADLinkInstance = doc.GetElement(refer) as Instance;
            var geoNode = new CGeoNode();
            var element = doc.GetElement(refer);
            var geo = element.GetGeometryObjectFromReference(new Reference(element));
            var textNodes = new List<CTextNode>();
            geoNode.ParaseGeoText(geo, textNodes);
            // 获取图层
            GraphicsStyle graphicsStyle = doc.GetElement(CADLinkInstance.GetGeometryObjectFromReference(refer).GraphicsStyleId) as GraphicsStyle;
            foreach (var textNode in textNodes)
            {
                var cPoint_1 = textNode.GetType().GetField("m_pt1").GetValue(textNode) as CPointMen;
                var cPoint_3 = textNode.GetType().GetField("m_pt3").GetValue(textNode) as CPointMen;
                try
                {
                    var point_1 = new XYZ(cPoint_1.m_dx, cPoint_1.m_dy, cPoint_1.m_dz);
                    var point_3 = new XYZ(cPoint_3.m_dx, cPoint_3.m_dy, cPoint_3.m_dz);
                    var point = (point_1 + point_3) / 2;
                    textModel.Add(new CADTextModel()
                    {
                        Text = textNode.m_sValue,
                        Location = point,
                        Rotation = textNode.m_dRot,
                    });
                }
                catch { }
            }

            return textModel;
        }
    }
}
