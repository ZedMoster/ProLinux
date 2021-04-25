using System.Collections.Generic;
using System.ComponentModel;

using Autodesk.Revit.DB;

namespace CADReader.WPF
{
    /// <summary>
    /// 选择族类型
    /// </summary>
    public class SelectElement
    {
        public string ElementName { get; set; }
        public Element Element { get; set; }
        public bool Checked { get; set; }
    }

    /// <summary>
    /// 选择族类别
    /// </summary>
    public class SelectFamily
    {
        public string FamilyName { get; set; }
        public string SymbolName { get; set; }
        public Family Family { get; set; }
        public FamilySymbol Symbol { get; set; }
    }

    /// <summary>
    /// CAD图形类
    /// </summary>
    public class CADModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 低高度
        /// </summary>
        public double Lhight { get; set; }
        /// <summary>
        /// 类型名称
        /// </summary>
        public string SymbolName { get; set; }
        /// <summary>
        /// 族列表
        /// </summary>
        public List<SelectFamily> FamilyList { get; set; }
        /// <summary>
        /// 族名称列表
        /// </summary>
        public List<string> FamilyNameList { get; set; }
        /// <summary>
        /// 宽度
        /// </summary>
        private double width;
        public double Width
        {
            get { return width; }
            set
            {
                if (value != width)
                {
                    width = value;
                    NotifyPropertyChanged();
                }
            }
        }
        /// <summary>
        /// 高度
        /// </summary>
        private double hight;
        public double Higth
        {
            get { return hight; }
            set
            {
                if (value != hight)
                {
                    hight = value;
                    NotifyPropertyChanged();
                }
            }
        }
        /// <summary>
        /// 选择的族名称
        /// </summary>
        private string myFamilyName;
        public string SelectFamilyName
        {
            get { return myFamilyName; }
            set
            {
                if (value != myFamilyName)
                {
                    myFamilyName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string value = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(value));
        }
    }
}
