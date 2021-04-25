using System.Collections.Generic;

namespace CADReader.Model
{
    public class GlobaData
    {
        private GlobaData() { }
        private static GlobaData _GlobaData = new GlobaData();

        public static GlobaData GlobaDataDic
        {
            get { return _GlobaData; }
        }

        public Dictionary<string, string> ColorDic { get; set; } = new Dictionary<string, string>
        {
            {"居住用地","#ffff00" },
            {"行政办公用地","#ff7f9f" },
            {"文化设施用地","#ff9f7f" },
            {"教育科研用地","#ff7fbf" },
            {"体育用地","#00a552" },
            {"医疗卫生用地","#ff7f7f" },
            {"公共设施用地","#005f7f" },
            {"公园绿地","#00ff3f" },
            {"防护绿地","#007ff0" },

            {"社会福利用地","#a55267" },
            {"文物古迹用地","#9152a5" },
            {"宗教用地","#a55267" },
            {"商业用地","#ff003f" },
            {"娱乐康体用地","#ffbf7f" },
            {"公共设施营业网点用地","#ff9f7f" },
            {"特殊用地","#2f4c26" },
            {"水域","#7fbfff" },
            {"农林用地","#29a500" },

            {"其它服务设施用地","#ff9f7f" },
            {"一类工业用地","#ffbf7f" },
            {"二类工业用地","#4c3926" },
            {"物流仓储用地","#9f7fff" },
            {"交通枢纽用地","#808080" },
            {"其他交通设施用地","#3f6f7f" },
            {"广场用地","#808080" },
            {"村庄建设用地","#a5a552" },
            {"规划区范围","#212830" },
        };
    }
}
