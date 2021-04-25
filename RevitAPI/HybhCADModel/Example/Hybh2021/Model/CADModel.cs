using System.Collections.Generic;
using System.ComponentModel;

using Autodesk.Revit.DB;

using CADReader.Model;

namespace CADReader
{
    /// <summary>
    /// 定位线及编号
    /// </summary>
    public class CreateNamePolyLine
    {
        /// <summary>
        /// 定位线
        /// </summary>
        public List<Curve> Curves { get; set; }
        /// <summary>
        /// 分类名称
        /// </summary>
        public string CategoryName { get; set; }
    }

    /// <summary>
    /// 用户交互选择图层
    /// </summary>
    public class ToDo
    {
        /// <summary>
        /// 范围框
        /// </summary>
        public Autodesk.Revit.UI.Selection.PickedBox PickedBox { get; set; }
        /// <summary>
        /// 文字
        /// </summary>
        public Reference ReferenceText { get; set; }
        /// <summary>
        /// 分割线
        /// </summary>
        public Reference ReferenceLine { get; set; }
        /// <summary>
        /// 定位线
        /// </summary>
        public Reference ReferenceCurves { get; set; }
        /// <summary>
        /// 类别
        /// </summary>
        public Reference ReferenceCategory { get; set; }
        /// <summary>
        /// 明细表
        /// </summary>
        public List<ScheduleDatas> ScheduleDatas { get; set; }
        /// <summary>
        /// 序号
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// 选择的对象
        /// </summary>
        public SelectElement SelectElement { get; set; }
        /// <summary>
        /// 车位宽
        /// </summary>
        public double Width { get; set; }
        /// <summary>
        /// 车位长
        /// </summary>
        public double Lengh { get; set; }
    }

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
        /// 定位点
        /// </summary>
        public List<XYZ> XYZs { get; set; }
    }

    /// <summary>
    /// 数据分桶
    /// </summary>
    public class ScheduleLine
    {
        /// <summary>
        /// 序号
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// 文本内容
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// 定位点
        /// </summary>
        public XYZ Location { get; set; }
    }

    /// <summary>
    /// 选择族类型
    /// </summary>
    public class SelectElement
    {
        private string elementName;
        /// <summary>
        /// 类型名称
        /// </summary>
        public string ElementName
        {
            get { return elementName; }
            set
            {
                elementName = value;
                if (!string.IsNullOrEmpty(elementName) && GlobaData.GlobaDataDic.ColorDic.TryGetValue(elementName, out string c))
                {
                    Color = c;
                }
            }
        }
        /// <summary>
        /// 着色
        /// </summary>
        public string Color { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public Element Element { get; set; }
    }

    /// <summary>
    /// 明细表数据
    /// </summary>
    public class ScheduleDatas : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string value = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(value));
        }

        private int index;
        private string key_1;
        private string key_2;
        private string key_3;
        private string key_4;
        private string key_5;
        private string key_6;
        private string key_7;
        private string key_8;
        private string key_9;
        private string key_10;

        public int Index
        {
            get { return index; }
            set
            {
                if (value != index)
                {
                    index = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string Key_1
        {
            get { return key_1; }
            set
            {
                if (value != key_1)
                {
                    key_1 = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string Key_2
        {
            get { return key_2; }
            set
            {
                if (value != key_2)
                {
                    key_2 = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string Key_3
        {
            get { return key_3; }
            set
            {
                if (value != key_3)
                {
                    key_3 = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string Key_4
        {
            get { return key_4; }
            set
            {
                if (value != key_4)
                {
                    key_4 = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string Key_5
        {
            get { return key_5; }
            set
            {
                if (value != key_5)
                {
                    key_5 = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string Key_6
        {
            get { return key_6; }
            set
            {
                if (value != key_6)
                {
                    key_6 = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string Key_7
        {
            get { return key_7; }
            set
            {
                if (value != key_7)
                {
                    key_7 = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string Key_8
        {
            get { return key_8; }
            set
            {
                if (value != key_8)
                {
                    key_8 = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string Key_9
        {
            get { return key_9; }
            set
            {
                if (value != key_9)
                {
                    key_9 = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string Key_10
        {
            get { return key_10; }
            set
            {
                if (value != key_10)
                {
                    key_10 = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}
