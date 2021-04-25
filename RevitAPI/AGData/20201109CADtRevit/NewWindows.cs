using System.Collections.Generic;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using CADReader.WPF;

namespace CADReader.Command
{
    [Transaction(TransactionMode.Manual)]
    class NewWindows : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // 定义变量
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            #region 选择CAD图纸信息
            Pickdwg pickdwg = new Pickdwg();
            // 获取定位线
            pickdwg.Refer(sel, out Reference referenceModel);
            if (referenceModel == null)
            {
                MessageBox.Show("已取消");
                return Result.Cancelled;
            }
            #endregion
            CADText CADtext = new CADText();
            var texts = CADtext.GetLayer(doc, referenceModel);

            #region 设置门编号对应的族 Family
            WPFFamilySymbol wpfFamilySymbol = new WPFFamilySymbol(doc, BuiltInCategory.OST_Windows, texts);
            wpfFamilySymbol.ShowDialog();
            if (wpfFamilySymbol.IsHitTestVisible)
            {
                MessageBox.Show("取消创建窗");
                return Result.Cancelled;
            }
            var selectFamily = wpfFamilySymbol.FamilyList.ItemsSource as List<WPF.CADModel>;
            #endregion

            #region 创建模型
            WallManage wallManage = new WallManage('C');
            List<bool> Push = new List<bool>();
            TransactionGroup T = new TransactionGroup(doc, "识别窗");
            T.Start();
            foreach (var text in texts)
            {
                var work = wallManage.CreateFamilySymbol(doc, selectFamily, text);
                if (work)
                    Push.Add(work);
            }
            _ = Push.Count == 0 ? T.RollBack() : T.Assimilate();
            #endregion


            MessageBox.Show($"总计窗组：{Push.Count}");
            return Result.Succeeded;
        }
    }



}
