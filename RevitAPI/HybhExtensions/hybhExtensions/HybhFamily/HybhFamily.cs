using System.Collections.Generic;
using System.Linq;
using System.Windows;

using Autodesk.Revit.DB;

namespace Xml
{
    public static class FamilyExtensions
    {
        /// <summary>
        /// 加载族文件
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="RfaFilePath"></param>
        /// <param name="RfaFamilyName"></param>
        public static Family LoadRfaPath(this Document doc, string RfaFilePath, string RfaFamilyName)
        {
            Family family = null;
            Transaction tran = new Transaction(doc, "载入族");
            tran.Start();
            try
            {
                bool loadSuccess = doc.LoadFamily(RfaFilePath, out family);
                if (loadSuccess)
                {
                    family.Name = RfaFamilyName;
                }
                tran.Commit();
            }
            catch
            {
                tran.RollBack();
            }
            return family;
        }

        /// <summary>
        /// 获取指定名称的族类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="doc"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static T GetElementTypeByName<T>(this Document doc, string typeName) where T : ElementType
        {
            T type = default;
            // 获取所有的Family类型
            var elsType = doc.TCollector<T>();
            if (elsType.Count == 0)
            {
                MessageBox.Show($"{typeof(T)}类别个数为：0");
                return type;
            }
            else
            {
                type = elsType.FirstOrDefault(i => i.Name == typeName);
            }
            return type;
        }

        /// <summary>
        /// 获取指定名称的族类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="doc"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static T GetElementTypeByName<T>(this Document doc, string familyName, string typeName) where T : ElementType
        {
            T type = default;
            // 获取所有的Family类型
            var elsType = doc.TCollector<T>().Where(i => i.FamilyName == familyName);
            if (elsType.Count() == 0)
            {
                MessageBox.Show($"{typeof(T)}类别名称{familyName}个数为：0");
                return type;
            }
            else
            {
                type = elsType.FirstOrDefault(i => i.Name == typeName);
            }
            return type;
        }

        /// <summary>
        /// 获取Family指定族类型名称的族类型
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="family"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static FamilySymbol ByFamilyAndName(this Document doc, Family family, string name)
        {
            // 定义变量
            FamilySymbol symbol = null;
            var needId = family.GetFamilySymbolIds().FirstOrDefault(x => (doc.GetElement(x) as FamilySymbol).Name == name);
            if (needId != null)
            {
                symbol = doc.GetElement(needId) as FamilySymbol;
            }
            // 复制新类型
            if (symbol == null)
            {
                Transaction tcopy = new Transaction(doc);
                tcopy.Start("创建类型");
                var elType = doc.GetElement(family.GetFamilySymbolIds().FirstOrDefault()) as FamilySymbol;
                symbol = elType.Duplicate(name) as FamilySymbol;
                tcopy.Commit();
            }
            return symbol;
        }

        /// <summary>
        /// 获取族名称及类型名称获取族类型
        ///     若项目不存在则弹窗提示
        /// </summary>
        /// <param name="doc"> </param>
        /// <param name="familyName"> 族文件名称 FamilyName</param>
        /// <param name="typeName"> 族类型名称 FamilyName</param>
        /// <returns> 类型 FamilySymbol</returns>
        public static FamilySymbol ByFamilyNameAndTypeName(this Document doc, string familyName, string typeName)
        {
            // 定义变量
            FamilySymbol symbol = null;
            // 获取所有的Family类型
            var elsType = doc.TCollector<FamilySymbol>();
            // 判断是否需存在名称未 familyName 的族文件
            var elsType_FamilyName = elsType.Where(i => i.FamilyName == familyName);
            if (elsType_FamilyName.Count() > 0)
            {
                // 通过族类型及名称获取指定的类型
                symbol = elsType_FamilyName.FirstOrDefault(x => x.Name == typeName) as FamilySymbol;
                // 创建类型
                if (symbol == null)
                {
                    var elType = elsType_FamilyName.FirstOrDefault() as ElementType;
                    Transaction tcopy = new Transaction(doc);
                    tcopy.Start("创建类型");
                    symbol = elType.Duplicate(typeName) as FamilySymbol;
                    tcopy.Commit();
                }
            }
            else
            {
                MessageBox.Show($"项目中不包含指定族名称文件:{familyName}");
            }
            return symbol;
        }

        /// <summary>
        /// 获取族名称及类型名称获取族类型
        ///     若项目不存在族自动载入族文件
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="familyName"></param>
        /// <param name="typeName"></param>
        /// <param name="familyPath"></param>
        /// <returns></returns>
        public static FamilySymbol ByFamilyNameAndTypeName(this Document doc, string familyName, string typeName, string familyPath)
        {
            // 定义变量
            FamilySymbol symbol = null;
            // 载入的family
            Family family = null;
            // 获取所有的Family类型
            var elsType = doc.TCollector<FamilySymbol>();
            // 判断是否需要载入族文件
            var elsType_FamilyName = elsType.Where(i => i.FamilyName == familyName);
            if (elsType_FamilyName.Count() > 0)
            {
                // 通过族类型及名称获取指定的类型
                symbol = elsType_FamilyName.FirstOrDefault(x => x.Name == typeName) as FamilySymbol;
                // 创建类型
                if (symbol == null)
                {
                    var elType = elsType_FamilyName.FirstOrDefault() as ElementType;
                    Transaction tcopy = new Transaction(doc);
                    tcopy.Start("创建类型");
                    symbol = elType.Duplicate(typeName) as FamilySymbol;
                    tcopy.Commit();
                }
            }
            else
            {
                // 自动载入族文件 创建类型
                family = LoadRfaPath(doc, familyPath, familyName);
                var needId = family.GetFamilySymbolIds().FirstOrDefault(x => (doc.GetElement(x) as FamilySymbol).Name == typeName);
                if (needId != null)
                    symbol = doc.GetElement(needId) as FamilySymbol;
                // 复制新类型
                if (symbol == null)
                {
                    Transaction tcopy = new Transaction(doc);
                    tcopy.Start("创建类型");
                    var elType = doc.GetElement(family.GetFamilySymbolIds().FirstOrDefault()) as FamilySymbol;
                    symbol = elType.Duplicate(typeName) as FamilySymbol;
                    tcopy.Commit();
                }
            }
            return symbol;
        }


        /// <summary>
        /// 通过名称获取族类型
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="elsType"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static FamilySymbol GetFamilySymbolByName(this Document doc, List<FamilySymbol> elsType, string typeName)
        {
            // 定义变量
            FamilySymbol symbol = null;
            if (elsType.Count == 0)
            {
                ShowResult.Print($"请载入族文件");
                return symbol;
            }
            // 通过族类型及名称获取指定的类型
            symbol = elsType.FirstOrDefault(x => x.Name == typeName);
            // 创建类型
            if (symbol == null)
            {
                var elType = elsType.FirstOrDefault();
                Transaction tcopy = new Transaction(doc);
                tcopy.Start("创建类型");
                try
                {
                    symbol = elType.Duplicate(typeName) as FamilySymbol;
                    tcopy.Commit();
                }
                catch (System.Exception e)
                {
                    _ = e.Message;
                    tcopy.RollBack();
                }
            }

            return symbol;
        }
    }
}
