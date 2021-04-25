using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

using Autodesk.Revit.DB;

namespace CADReader.View
{
    /// <summary>
    /// WPFViewSchedule.xaml 的交互逻辑
    /// </summary>
    public partial class WPFViewSchedule : Window
    {
        public WPFViewSchedule(List<CADTextModel> texts, List<Line> lines)
        {
            InitializeComponent();

            ScheduleViewData.ItemsSource = SchedulesToGruop(texts, lines);
        }

        /// <summary>
        /// 数据分组
        /// </summary>
        private List<ScheduleDatas> SchedulesToGruop(List<CADTextModel> texts, List<Line> lines)
        {
            List<ScheduleDatas> scheduleDatas = new List<ScheduleDatas>();
            List<Line> models_Y = lines.Where(i => i.Direction.IsAlmostEqualTo(XYZ.BasisY) ||
                i.Direction.IsAlmostEqualTo(XYZ.BasisY.Negate())).ToList()
                .OrderBy(i => i.Origin.X).ToList();
            // 表格线 X
            List<Line> models_X = lines.Where(i => i.Direction.IsAlmostEqualTo(XYZ.BasisX) ||
                i.Direction.IsAlmostEqualTo(XYZ.BasisX.Negate())).ToList()
                .OrderBy(i => i.Origin.Y).ToList();

            // 按Y表格线分组 -> 再按X表格线分组
            var scheduleLinesY = models_Y.Select(i => i.Origin.X);
            var scheduleLinesX = models_X.Select(i => i.Origin.Y);

            List<ScheduleLine> schedulesGruop = new List<ScheduleLine>();
            foreach (var item in texts)
            {
                int index = TextInPart(item.Location, scheduleLinesX, false);
                if (index == -1)
                {
                    continue;
                }
                schedulesGruop.Add(new ScheduleLine()
                {
                    Index = index,
                    Text = item.Text,
                    Location = item.Location,
                });
            }
            List<IGrouping<int, ScheduleLine>> groupInfo = schedulesGruop.GroupBy(i => i.Index).ToList();
            List<Dictionary<int, string>> keyValuePairs = new List<Dictionary<int, string>>();
            foreach (var gp in groupInfo)
            {
                Dictionary<int, string> keyValues = new Dictionary<int, string>();
                var gpSort = gp.OrderBy(i => i.Location.X).ToList();
                var key = gpSort.FirstOrDefault(i => i.Text.Contains("#"))?.Text ?? "0";
                var keystr = key.Replace("楼", "").Replace("#", "");
                var result = int.TryParse(keystr, out int index);
                if (!result)
                {
                    continue;
                }
                foreach (var item in gpSort)
                {
                    int _index = TextInPart(item.Location, scheduleLinesY);
                    if (_index == -1)
                    {
                        continue;
                    }
                    if (keyValues.ContainsKey(_index))
                    {
                        keyValues[_index] = keyValues[_index] + item.Text;
                    }
                    else
                    {
                        keyValues[_index] = item.Text;
                    }
                }

                var values = keyValues.Values.ToList();
                try
                {
                    scheduleDatas.Add(new ScheduleDatas()
                    {
                        Index = index,
                        Key_1 = gpSort[0].Text ?? "N/A",
                        Key_2 = gpSort[1].Text ?? "N/A",
                        Key_3 = gpSort[2].Text ?? "N/A",
                        Key_4 = gpSort[3].Text ?? "N/A",
                        Key_5 = gpSort[4].Text ?? "N/A",
                        Key_6 = gpSort[5].Text ?? "N/A",
                        Key_7 = gpSort[6].Text ?? "N/A",
                        Key_8 = gpSort[7].Text ?? "N/A",
                        Key_9 = gpSort[8].Text ?? "N/A",
                        Key_10 = gpSort[9].Text ?? "N/A",
                    });
                }
                catch { };
            }

            return scheduleDatas.OrderBy(i => i.Index).ToList();
        }

        /// <summary>
        /// 获取parts 分段
        /// </summary>
        /// <param name="location"></param>
        /// <param name="parts"></param>
        /// <returns></returns>
        private int TextInPart(XYZ location, IEnumerable<double> parts, bool isBasisX = true)
        {
            for (int i = 0; i < parts.Count() - 1; i++)
            {
                var _left = parts.ElementAt(i);
                var _right = parts.ElementAt(i + 1);
                if (isBasisX)
                {
                    if (location.X > _left && location.X < _right)
                    {
                        return i;
                    }
                }
                else
                {
                    if (location.Y > _left && location.Y < _right)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// 确认创建
        /// </summary>
        private void Create_Click(object sender, RoutedEventArgs e)
        {
            var toindex = int.TryParse(indexRow.Text, out int n);
            if (!toindex)
            {
                MessageBox.Show("再左下角输入高度列定位的编号", "警告");
            }
            // 传递窗口输入参数
            this.Close();
            this.IsHitTestVisible = false;
        }

        /// <summary>
        /// 关闭创建
        /// </summary>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 关闭创建
        /// </summary>
        private void Window_Closed(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
