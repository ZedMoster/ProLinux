using System.Collections.Generic;
using System.ComponentModel;

using Autodesk.Revit.DB;

namespace Xml.HybhModel
{
    public class CADSelectModel : HybhNotifyPropertyChanged
    {
        public string FamilyName { get; set; }
        public Family Family { get; set; }
        public string ElementName { get; set; }
        public Element Element { get; set; }
        public double Lhight { get; set; }
        public FamilySymbol Symbol { get; set; }
        public string SymbolName { get; set; }
        public List<Family> FamilyList { get; set; }
        public List<string> FamilyNameList { get; set; }
        public bool Checked { get; set; }

        private string selectFamilyName;
        public string SelectFamilyName
        {
            get { return selectFamilyName; }
            set
            {
                if (value != selectFamilyName)
                {
                    selectFamilyName = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}
