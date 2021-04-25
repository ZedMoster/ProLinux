using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace FamilyLoadCreate
{
    [Transaction(TransactionMode.Manual)]
    class GetVersion : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //int num = 0;

            //string[] lines = File.ReadAllLines(@"C:\Users\xml\Desktop\paths.txt", Encoding.Default);

            //foreach (var filePath in lines.ToList())
            //{
            //    string version = GetFileInfo.Version(filePath);
            //    if (version == null)
            //    {
            //        num++;
            //    }
            //}

            //if (num == 0)
            //{
            //    MessageBox.Show("读取到所有文件的版本");
            //}
            //else
            //{
            //    MessageBox.Show("未解析到文件版本个数：" + num.ToString());
            //}


            // 文件位置
            string filePath = @"D:\缓存文件\钉钉\现代柱1.rfa";
            //string filePath = @"C:\Users\xml\Desktop\族库\族库\场地\园林景观水域\植物\2D\树篱平面-基于线 2D.rfa";
            // 获取revit文档版本信息
            string version = GetFileInfo.Version(filePath);
            var result = string.Format("文件：{0}\nRevit版本：{1}", filePath, version);
            TaskDialog.Show("结果", result);

            return Result.Succeeded;
        }
    }

    public class GetFileInfo
    {
        /// <summary>
        /// 获取文件的版创建版本
        /// </summary>
        /// <param name="RevitFilePath"> RVT RFA 文件完整路径</param>
        /// <param name="SaveTemp"> 缓存读取到的文件信息</param>
        /// <returns> (string) 创建版本</returns>
        public static string Version(string RevitFilePath)
        {
            #region 读取文件流并转换未字符串形式数据
            // 判断文件是否存在 不存在输出异常
            if (!StructuredStorageUtils.IsFileStucturedStorage(RevitFilePath))
            {
                throw new NotSupportedException("未找到相应的文件:" + RevitFilePath);
            }
            // byte 读取文档数据流
            var rawData = GetRawBasicFileInfo(RevitFilePath);
            // 编码获取string
            var rawString = Encoding.UTF8.GetString(rawData);

            // 获取信息字符串序列
            var fileInfoData = rawString.Split(new string[] { "\0", /*"\r\n"*/ }, StringSplitOptions.RemoveEmptyEntries);
            // 转换成字符串形式
            string result = string.Join("", fileInfoData);
            #endregion

            #region 获取创建的版本信息 通过正则匹配数据
            string year;
            // 通过正则表达式获取年份
            // 获取 Revit Build 版本
            Regex buildInfoRegex = new Regex(@"Build:.(?<Year>\d{4}).");
            var _build = buildInfoRegex.Match(result).Groups["Year"].Value;
            var res = int.TryParse(_build, out int Year);
            if (res)
            {
                year = (Year + 1).ToString();
            }
            else
            {
                Regex buildInfoRegex_Architecture = new Regex(@"Architecture.(?<Year>\d{4}).");
                var _build_Architecture = buildInfoRegex_Architecture.Match(result).Groups["Year"].Value;
                var res_Architecture = int.TryParse(_build_Architecture, out int Year_Architecture);
                if (res_Architecture)
                {
                    year = Year_Architecture.ToString();
                }
                else
                {
                    Regex buildInfoRegex_Autodesk = new Regex(@"Autodesk Revit (?<Year>\d{4}).");
                    var _build_Autodesk = buildInfoRegex_Autodesk.Match(result).Groups["Year"].Value;
                    var res_Autodesk = int.TryParse(_build_Autodesk, out int Year_Autodesk);
                    if (res_Autodesk)
                    {
                        year = Year_Autodesk.ToString();
                    }
                    else
                    {
                        year = "--";
                    }
                }
            }
            #endregion

            // 缓存读取的文件信息
            bool SaveTemp = true;

            #region 是否保持缓存文件
            if (SaveTemp)
            {
                string Path = @"C:\temp\";
                if (!Directory.Exists(Path))
                {
                    Directory.CreateDirectory(Path);
                }
                StreamWriter sw = File.CreateText(Path + "temp.tmp");
                sw.Write(result);
                sw.Flush();
                sw.Close();
            }
            #endregion

            return year;
        }

        /// <summary>
        /// Stream读取Revit文件
        /// </summary>
        /// <param name="revitFileName"> 文件完整路径</param>
        /// <returns> 数据类型byte[]</returns>
        private static byte[] GetRawBasicFileInfo(string revitFileName)
        {
            if (!StructuredStorageUtils.IsFileStucturedStorage(revitFileName))
            {
                throw new NotSupportedException("File is not a structured storage file");
            }

            #region 读取文件以byte形式
            using (StructuredStorageRoot ssRoot = new StructuredStorageRoot(revitFileName))
            {
                if (!ssRoot.BaseRoot.StreamExists(StreamName))
                {
                    throw new NotSupportedException(string.Format("File doesn't contain {0} stream", StreamName));
                }

                StreamInfo imageStreamInfo = ssRoot.BaseRoot.GetStreamInfo(StreamName);
                using (Stream stream = imageStreamInfo.GetStream(FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);
                    return buffer;
                }
            }
            #endregion
        }
        // 设置 StreamName
        private const string StreamName = "BasicFileInfo";
    }

    #region -*- 勿动 -*-
    class StructuredStorageUtils
    {
        [DllImport("ole32.dll")]
        static extern int StgIsStorageFile([MarshalAs(UnmanagedType.LPWStr)] string pwcsName);

        public static bool IsFileStucturedStorage(string fileName)
        {
            int res = StgIsStorageFile(fileName);

            if (res == 0)
                return true;

            if (res == 1)
                return false;

            throw new FileNotFoundException(
              "未找到文件", fileName);
        }
    }

    class StructuredStorageException : Exception
    {
        public StructuredStorageException()
        {
        }

        public StructuredStorageException(string message) : base(message)
        {
        }

        public StructuredStorageException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    class StructuredStorageRoot : IDisposable
    {
        private readonly StorageInfo _storageRoot;

        public StructuredStorageRoot(Stream stream)
        {
            try
            {
                _storageRoot = (StorageInfo)InvokeStorageRootMethod(null, "CreateOnStream", stream);
            }
            catch (Exception ex)
            {
                throw new StructuredStorageException("Cannot get StructuredStorageRoot", ex);
            }
        }

        public StructuredStorageRoot(string fileName)
        {
            try
            {
                _storageRoot = (StorageInfo)InvokeStorageRootMethod(null, "Open", fileName,
                    FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception ex)
            {
                throw new StructuredStorageException("Cannot get StructuredStorageRoot", ex);
            }
        }

        private static object InvokeStorageRootMethod(StorageInfo storageRoot, string methodName, params object[] methodArgs)
        {
            Type storageRootType = typeof(StorageInfo).Assembly.GetType("System.IO.Packaging.StorageRoot", true, false);
            object result = storageRootType.InvokeMember(methodName,
                BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod,
                null, storageRoot, methodArgs);
            return result;
        }

        private void CloseStorageRoot()
        {
            InvokeStorageRootMethod(_storageRoot, "Close");
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            CloseStorageRoot();
        }

        #endregion

        public StorageInfo BaseRoot
        {
            get { return _storageRoot; }
        }
    }
    #endregion
}
