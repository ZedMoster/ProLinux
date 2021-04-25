using System.Collections.Generic;
using System.Xml;

using HelperXml.Extensions;

namespace HelperXml
{
    public class XmlTemp
    {
        /// <summary>
        /// 环境变量 key
        /// </summary>
        private string Variable { get; set; }

        /// <summary>
        /// 参数设置:
        ///     ALLUSERSPROFILE=C:\ProgramData
        ///     APPDATA = C:\Users\user\AppData\Roaming
        ///     HOMEPATH = \Users\user
        ///     LOCALAPPDATA=C:\Users\user\AppData\Local
        ///     PROGRAMDATA = C:\ProgramData
        ///     PUBLIC = C:\Users\Public
        ///     TEMP = C:\Users\user\AppData\Local\Temp
        ///     TMP = C:\Users\user\AppData\Loca
        /// </summary>
        /// <param name="variable"></param>
        public XmlTemp(string variable = "TEMP")
        {
            Variable = variable;
        }

        /// <summary>
        /// 创建xml文件（每页数据）
        /// </summary>
        /// <param name="path"></param>
        public bool Create(List<Dictionary<string, string>> dict, string attributeName = "test", string fileName = "_infomation")
        {
            var path = GetFilePath(fileName);

            // 创建一个 xml 文档对象
            XmlDocument doc = new XmlDocument();
            // 声明XML头部信息
            XmlDeclaration declaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            // 添加进 doc 对象子节点
            doc.AppendChild(declaration);
            // 创建根节点
            XmlElement root = doc.CreateElement("root");
            doc.AppendChild(root);
#if false
            // 创建标记节点
            var student = doc.NewXmlElement("test", "1", "02");
            var student2 = doc.NewXmlElement("test", "5", "20");
            var student3 = doc.NewXmlElement("test", "9", "30");
            // 标记节点添加到根节点下
            root.AppendChild(student);
            root.AppendChild(student2);
            root.AppendChild(student3);
#else
            // 添加 xml 文件节点
            foreach(var item in dict)
            {
                foreach(var key in item.Keys)
                {
                    item.TryGetValue(key, out string value);
                    var student = doc.NewXmlElement(attributeName, key, value);
                    root.AppendChild(student);
                }
            }
#endif
            try
            {
                // 保存 xml 缓存文件
                doc.Save(path);
                return true;
            }
            catch(System.Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 读取xml文件（每页数据）
        /// </summary>
        /// <param name="attributeName"> 属性名称</param>
        /// <param name="valueEn"> 键</param>
        /// <param name="path"> 文件路径</param>
        /// <returns> 值</returns>
        public string Read(string attributeName, string key, string fileName = "_infomation")
        {
            var path = GetFilePath(fileName);
            //将XML文件加载进来
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            return doc.GetXmlElement(attributeName, key);
        }

        /// <summary>
        /// 缓存文件路径
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private string GetFilePath(string fileName, string type = ".xml")
        {
            // 检索环境变量的值  C:\Users\{UserName}\AppData\Local\Temp
            var path = System.Environment.GetEnvironmentVariable(Variable);
            if(path == null)
            {
                throw new System.Exception("Can't find file path in this PC!");
            }
            var _filename = fileName.Contains(type) ? fileName : $"{fileName}{type}";
            return System.IO.Path.Combine(path, _filename);
        }
    }
}
