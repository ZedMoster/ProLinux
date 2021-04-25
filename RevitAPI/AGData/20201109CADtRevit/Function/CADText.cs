using System.Collections.Generic;

using Autodesk.Revit.DB;

using RvtTxt;

namespace CADReader
{
    /// <summary>
    /// CAD文字类
    /// </summary>
    public class CADText
    {
        /// <summary>
        /// 获得选择的CAD文字数据集
        /// </summary>
        /// <param name="refer"> 选择图纸对应的对象</param>
        /// <param name="doc"> Document</param>
        /// <returns> string：文字内容 或者 空字符</returns>
        public List<CADTextModel> GetPick(Document doc, Reference refer)
        {
            // 定义变量
            List<CADTextModel> textModel = new List<CADTextModel>();
            CADTextModel text = new CADTextModel();

            #region 获取链接的图纸信息
            var CADLinkInstance = doc.GetElement(refer) as Instance;
            var styleId = int.Parse((doc.GetElement(CADLinkInstance.GetGeometryObjectFromReference(refer).GraphicsStyleId) as GraphicsStyle).Id.ToString());
            CGeoNode geoNode = new CGeoNode();
            var element = doc.GetElement(refer);
            var geo = element.GetGeometryObjectFromReference(new Reference(element));
            var textNodes = new List<CTextNode>();
            geoNode.ParaseGeoText(geo, textNodes);
            // 获取图层
            GraphicsStyle graphicsStyle = doc.GetElement(CADLinkInstance.GetGeometryObjectFromReference(refer).GraphicsStyleId) as GraphicsStyle;
            #endregion

            #region 获取文字内容
            var selPo = refer.GlobalPoint;
            double numMin = double.MaxValue;
            foreach (var textNode in textNodes)
            {
                if (textNode.m_idStyle != styleId)
                    continue;
                var cPoint_1 = textNode.GetType().GetField("m_pt1").GetValue(textNode) as CPointMen;
                var cPoint_3 = textNode.GetType().GetField("m_pt3").GetValue(textNode) as CPointMen;
                var point_1 = new XYZ(cPoint_1.m_dx, cPoint_1.m_dy, cPoint_1.m_dz);
                var point_3 = new XYZ(cPoint_3.m_dx, cPoint_3.m_dy, cPoint_3.m_dz);
                var point = (point_1 + point_3) / 2;
                var _num = point.DistanceTo(selPo);
                if (_num < numMin)
                {
                    numMin = _num;
                    text.Text = textNode.m_sValue;
                    text.Location = point;
                    text.Rotation = textNode.m_dRot;
                    text.LayerName = graphicsStyle.GraphicsStyleCategory.Name;
                }
            }
            #endregion

            if (text != null)
            {
                textModel.Add(text);
            }

            return textModel;
        }

        /// <summary>
        /// 获取选中图层的CAD文字数据集
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="refer"></param>
        /// <returns></returns>
        public List<CADTextModel> GetLayer(Document doc, Reference refer)
        {
            // 定义变量
            List<CADTextModel> textModel = new List<CADTextModel>();

            #region 获取链接的图纸信息
            var CADLinkInstance = doc.GetElement(refer) as Instance;
            var styleId = int.Parse((doc.GetElement(CADLinkInstance.GetGeometryObjectFromReference(refer).GraphicsStyleId) as GraphicsStyle).Id.ToString());
            var geoNode = new CGeoNode();
            var element = doc.GetElement(refer);
            var geo = element.GetGeometryObjectFromReference(new Reference(element));
            var textNodes = new List<CTextNode>();
            geoNode.ParaseGeoText(geo, textNodes);
            // 获取图层
            GraphicsStyle graphicsStyle = doc.GetElement(CADLinkInstance.GetGeometryObjectFromReference(refer).GraphicsStyleId) as GraphicsStyle;
            #endregion

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
                    Location = point,
                    Rotation = textNode.m_dRot,
                    LayerName = graphicsStyle.GraphicsStyleCategory.Name,
                });
            }

            return textModel;
        }


        public List<CADTextModel> GetBoxText(Document doc, Reference refer)
        {
            // 定义变量
            List<CADTextModel> textModel = new List<CADTextModel>();
            #region 获取链接的图纸信息
            var CADLinkInstance = doc.GetElement(refer) as Instance;
            var geoNode = new CGeoNode();
            var element = doc.GetElement(refer);
            var geo = element.GetGeometryObjectFromReference(new Reference(element));
            var textNodes = new List<CTextNode>();
            geoNode.ParaseGeoText(geo, textNodes);
            // 获取图层
            GraphicsStyle graphicsStyle = doc.GetElement(CADLinkInstance.GetGeometryObjectFromReference(refer).GraphicsStyleId) as GraphicsStyle;
            #endregion
            foreach (var textNode in textNodes)
            {
                var cPoint_1 = textNode.GetType().GetField("m_pt1").GetValue(textNode) as CPointMen;
                var cPoint_3 = textNode.GetType().GetField("m_pt3").GetValue(textNode) as CPointMen;
                var point_1 = new XYZ(cPoint_1.m_dx, cPoint_1.m_dy, cPoint_1.m_dz);
                var point_3 = new XYZ(cPoint_3.m_dx, cPoint_3.m_dy, cPoint_3.m_dz);
                var point = (point_1 + point_3) / 2;
                textModel.Add(new CADTextModel()
                {
                    Text = textNode.m_sValue,
                    Location = point,
                    Rotation = textNode.m_dRot,
                    LayerName = graphicsStyle.GraphicsStyleCategory.Name,
                });
            }

            return textModel;
        }
    }
}
