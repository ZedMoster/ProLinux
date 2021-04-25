using System.Xml;

namespace HelperXml.Extensions
{
    public static class XmlExtensions
    {
        #region 属性名称
        static readonly string ElementName = "dictionary";
        static readonly string AttributeName = "category";
        static readonly string ElementNameEn = "key";
        static readonly string ElementNameZh = "value";
        #endregion

        /// <summary>
        /// 创建节点
        /// </summary>
        /// <param name="doc"> xml doc</param>
        /// <param name="sheet"> 分类名称</param>
        /// <param name="valueEn"> 英文</param>
        /// <param name="valueZh"> 中文</param>
        /// <returns></returns>
        public static XmlElement NewXmlElement(this XmlDocument doc, string attributeName, string key, string value)
        {
            //再创建根节点下的子节点
            XmlElement student = doc.CreateElement(ElementName);

            //设置子节点属性
            student.SetAttribute(AttributeName, attributeName);

            // 添加节点内容-1
            XmlElement one_X = doc.CreateElement(ElementNameEn);
            XmlText xmlText1 = doc.CreateTextNode(key);
            one_X.AppendChild(xmlText1);
            student.AppendChild(one_X);

            // 添加节点内容-2
            XmlElement one_Y = doc.CreateElement(ElementNameZh);
            XmlText xmlText2 = doc.CreateTextNode(value);
            one_Y.AppendChild(xmlText2);
            student.AppendChild(one_Y);

            return student;
        }

        /// <summary>
        /// 获取xml文档指定名称下的值
        /// </summary>
        /// <param name="doc"> xml文档</param>
        /// <param name="sheet"> 分类名称</param>
        /// <param name="en"> 英文</param>
        /// <returns> 获取zh翻译</returns>
        public static string GetXmlElement(this XmlDocument doc, string sheet, string key)
        {
            string value = "";
            //获取根节点
            XmlElement root = doc.DocumentElement;
            //获取子节点
            XmlNodeList pnodes = root.GetElementsByTagName(ElementName);
            // 遍历集合数据
            foreach(XmlNode node in pnodes)
            {
                string name = ((XmlElement)node).GetAttribute(AttributeName);
                if(name != sheet)
                {
                    continue;
                }
                string x = ((XmlElement)node).GetElementsByTagName(ElementNameEn)[0].InnerText;
                if(x != key)
                {
                    continue;
                }
                value = ((XmlElement)node).GetElementsByTagName(ElementNameZh)[0].InnerText;
                break;
            }

            return value;
        }
    }
}
