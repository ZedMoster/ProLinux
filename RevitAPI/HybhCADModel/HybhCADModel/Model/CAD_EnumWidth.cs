using System;
using System.Collections.Generic;

namespace HybhCADModel.Model
{
    /// <summary>
    /// 宽度
    /// </summary>
    public class EnumWidth
    {
        public EnumWidth()
        {
            List<int> Evalues = new List<int>();
            foreach (var val in Enum.GetValues(typeof(Elementwidth)))
            {
                Evalues.Add((int)(val));
            }
            EnumValues = Evalues;
        }

        ~EnumWidth() { }

        public enum Elementwidth
        {
            a = 100,
            b = 150,
            c = 200,
            d = 250,
            e = 300,
            f = 350,
            g = 400,
            h = 450,
            i = 500,
            j = 550,
            k = 600,
            l = 650,
            m = 700,
            //n = 750,
        };

        public List<int> EnumValues { get; set; }
    }
}
