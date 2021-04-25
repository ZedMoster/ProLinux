using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace CADReader.Command
{
    [Transaction(TransactionMode.Manual)]
    class NewTopography_Copy : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // 定义变量
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            // 选择链接图纸信息
            Pickdwg pickdwg = new Pickdwg();

            #region 选择CAD图纸信息
            // 获取定位线
            pickdwg.Refer(sel, out Reference referenceModel);
            if (referenceModel == null)
            {
                MessageBox.Show("已取消");
                return Result.Cancelled;
            }
            #endregion

            // 获取图形
            CADElement CADModel = new CADElement();
            var models = CADModel.GetPickGetCoordinates(doc, referenceModel);
            CreateTopography(doc, models.FirstOrDefault().XYZs);

            return Result.Succeeded;
        }

        /// <summary>
        /// 通过点位创建地形
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="xyzs"></param>
        /// <returns></returns>
        private bool CreateTopography(Document doc, List<XYZ> xyzs)
        {
            if (xyzs.FirstOrDefault().IsAlmostEqualTo(xyzs.LastOrDefault()))
            {
                xyzs.RemoveAt(xyzs.Count - 1);
            }

            Transaction t = new Transaction(doc, "创建地形");
            t.Start();
            try
            {
                Autodesk.Revit.DB.Architecture.TopographySurface.Create(doc, xyzs);
                t.Commit();
                return true;
            }
            catch (Exception)
            {
                t.RollBack();
                return false;
            }
        }
    }
}
