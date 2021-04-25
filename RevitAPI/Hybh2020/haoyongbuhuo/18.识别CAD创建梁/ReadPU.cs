using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using RvtTxt;
using System.Linq;
using Autodesk.Revit.Attributes;


namespace hybh
{
    [Transaction(TransactionMode.Manual)]
    class AAAAAA: IExternalCommand
    {
        private const string familyName = "ColumnBH";
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // 定义变量
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;


            XYZ loctionPont = null;
            double b = double.NaN;
            double h = double.NaN;
            XYZ midPoint = null;

            try
            {
                SelPick.SelRef = sel.PickObject(ObjectType.PointOnElement, "请点选cad轮廓线:");
            }
            catch { }

            if (SelPick.SelRef == null)
            {
                return Result.Failed;
            }
            ImportInstance dwg = doc.GetElement(SelPick.SelRef) as ImportInstance;
            var geoObj = dwg.GetGeometryObjectFromReference(SelPick.SelRef);
            Transform transform = dwg.GetTransform();

            Line locationCurve = null;

            if (geoObj is PolyLine)
            {
                var polyLine = geoObj as PolyLine;
                var coordinates = polyLine.GetCoordinates().ToList();

                for (int i = 0; i < coordinates.Count - 1; i++)
                {
                    if (i < coordinates.Count - 2)
                    {
                        Line line = Line.CreateBound(transform.OfPoint(coordinates[i]), transform.OfPoint(coordinates[i + 1]));
                        // 判断点再线段上
                        if (true)
                        {
                            locationCurve = line;
                        }
                    }
                    else
                    {
                        Line line = Line.CreateBound(transform.OfPoint(coordinates[i]), transform.OfPoint(coordinates[0]));
                        // 判断点再线段上
                        if (true)
                        {
                            locationCurve = line;
                        }
                    }
                }

                TaskDialog.Show("0", "gg");

            }

            return Result.Succeeded;
        }
        /// <summary>
        /// 获取XYZ(0,1,0)与element向量的旋转弧度
        /// </summary>
        /// <param name="line"> </param>
        /// <returns></returns>
        public double ToRotaLineAngle(Line line)
        {
            var lineDirection = line.Direction;
            // 套管初始方向 Y--(1,0,0) X--(0,1,0)
            var familyDirection = new XYZ(1, 0, 0);
            var angle = familyDirection.AngleTo(lineDirection);
            return angle;
        }

        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="xYZ"></param>
        /// <param name="familySymbol"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        private FamilyInstance CreateNewInstance(Document doc, XYZ xYZ, FamilySymbol familySymbol, Level level)
        {

            using (Transaction transaction = new Transaction(doc))
            {
                transaction.Start("创建-结构柱");
                if (!familySymbol.IsActive)
                {
                    familySymbol.Activate();
                }
                var familyInstance = doc.Create.NewFamilyInstance(xYZ, familySymbol, level, Autodesk.Revit.DB.Structure.StructuralType.Column);
                transaction.Commit();
                return familyInstance;
            }
        }

        private void IsHaveFamily(Document doc)
        {
            #region 判断族是否存在
            bool ishave = false;
            var els = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralColumns).WhereElementIsElementType().ToElements();
            foreach (var _family in els)
            {
                var Familytype = _family as ElementType;
                if (familyName == Familytype.FamilyName)
                {
                    ishave = true;
                }
            }
            #endregion

            #region 载入族文件
            if (!ishave)
            {
                Transaction t = new Transaction(doc);
                t.Start("加载族文件");
                var familyPath = @"C:\ProgramData\Autodesk\ApplicationPlugins\hybh2020.bundle\Contents\2018\Family\ColumnBH.rfa";
                doc.LoadFamily(familyPath);
                t.Commit();
            }
            #endregion
        }

        private FamilySymbol GetElement(Document doc, string name)
        {
            IsHaveFamily(doc);

            FamilySymbol familySymbol = null;

            var elsType = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralColumns).WhereElementIsElementType().ToElements();
            //TaskDialog.Show("0", elsType.Count.ToString());

            List<ElementType> elementType = new List<ElementType>();
            foreach (var _family in elsType)
            {
                var Familytype = _family as ElementType;

                if (Familytype.Name == name)
                {
                    familySymbol = _family as FamilySymbol;
                }
                //TaskDialog.Show("name", Familytype.FamilyName);

                if (Familytype.FamilyName == familyName)
                {
                    elementType.Add(Familytype);
                }
            }

            if (familySymbol != null)
            {
                return familySymbol;
            }
            else
            {
                var elType = elementType.First();
                Transaction t = new Transaction(doc);
                t.Start("创建类型");
                familySymbol = elType.Duplicate(name) as FamilySymbol;
                t.Commit();
                return familySymbol;
            }

        }
    }
}
