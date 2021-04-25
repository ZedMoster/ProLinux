using System.Collections.Generic;
using System.IO;
using System.Text;

using Autodesk.Revit.DB;

using HybhCADModel.Model;

using Teigha.DatabaseServices;
using Teigha.Runtime;

namespace HybhCADModel.Lib
{
    class ReadCADUtils
    {
        /// <summary>
        /// 取得链接cad的路径
        /// </summary>
        /// <param name="cadLinkTypeID"></param>
        /// <param name="revitDoc"></param>
        /// <returns></returns>
        public string GetCADPath(ElementId cadLinkTypeID, Document revitDoc)
        {
            CADLinkType cadLinkType = revitDoc.GetElement(cadLinkTypeID) as CADLinkType;
            return ModelPathUtils.ConvertModelPathToUserVisiblePath(cadLinkType.GetExternalFileReference().GetAbsolutePath());
        }

        /// <summary>
        /// 取得CAD的文字信息
        /// </summary>
        /// <param name="dwgFile"></param>
        /// <returns></returns>
        public List<CAD_Text> GetCADTextInfo(string dwgFile)
        {
            List<CAD_Text> listCADModels = new List<CAD_Text>();
            using (new Services())
            {
                using (Database database = new Database(false, false))
                {
                    database.ReadDwgFile(dwgFile, FileShare.Read, true, "");
                    using (var trans = database.TransactionManager.StartTransaction())
                    {
                        using (BlockTable table = (BlockTable)database.BlockTableId.GetObject(OpenMode.ForRead))
                        {
                            using (SymbolTableEnumerator enumerator = table.GetEnumerator())
                            {
                                StringBuilder sb = new StringBuilder();
                                while (enumerator.MoveNext())
                                {
                                    using (BlockTableRecord record = (BlockTableRecord)enumerator.Current.GetObject(OpenMode.ForRead))
                                    {

                                        foreach (ObjectId id in record)
                                        {
                                            Entity entity = (Entity)id.GetObject(OpenMode.ForRead, false, false);
                                            CAD_Text model = new CAD_Text();
                                            switch (entity.GetRXClass().Name)
                                            {
                                                case "AcDbText":
                                                    Teigha.DatabaseServices.DBText text = (Teigha.DatabaseServices.DBText)entity;
                                                    model.Location = new XYZ(text.Position.X, text.Position.Y, text.Position.Z);
                                                    model.Text = text.TextString;
                                                    model.Rotation = text.Rotation;
                                                    listCADModels.Add(model);
                                                    break;
                                                case "AcDbMText":
                                                    Teigha.DatabaseServices.MText mText = (Teigha.DatabaseServices.MText)entity;
                                                    model.Location = new XYZ(mText.Location.X, mText.Location.Y, mText.Location.Z);
                                                    model.Text = mText.Text;
                                                    model.Rotation = mText.Rotation;
                                                    listCADModels.Add(model);
                                                    break;
                                            }
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
            }
            return listCADModels;
        }

        /// <summary>
        /// 取得cad的图层名称
        /// </summary>
        /// <param name="dwgFile"></param>
        /// <returns></returns>
        public IList<string> GetLayerName(string dwgFile)
        {
            IList<string> cadLayerNames = new List<string>();
            using (new Services())
            {
                using (Database database = new Database(false, false))
                {
                    database.ReadDwgFile(dwgFile, FileShare.Read, true, "");
                    using (var trans = database.TransactionManager.StartTransaction())
                    {
                        using (LayerTable lt = (LayerTable)trans.GetObject(database.LayerTableId, OpenMode.ForRead))
                        {
                            foreach (ObjectId id in lt)
                            {
                                LayerTableRecord ltr = (LayerTableRecord)trans.GetObject(id, OpenMode.ForRead);
                                cadLayerNames.Add(ltr.Name);
                            }
                        }
                        trans.Commit();
                    }
                }
            }
            return cadLayerNames;
        }
    }
}
