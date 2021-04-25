/// <summary>
/// 日志管理
/// </summary>
namespace HelperLog
{
    public static class LoggingExtensions
    {
        /// <summary>
        /// 缓存日志文件
        /// </summary>
        /// <param name="message"> 日志消息</param>
        /// <param name="filename"> 日志文件名称</param>
        /// <param name="category"> 类别</param>
        /// <param name="variable"></param>
        public static void Tolog(this string message, string filename = "log_temp", string filetype = ".log", string category = "INFO", string variable = "USERPROFILE")
        {
            // 缓存文件位置
            var filePath = GetFilePath(filename, variable, filetype);
            // 写入数据
            using(System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write))
            {
                using(System.IO.StreamWriter sw = new System.IO.StreamWriter(fs))
                {
                    sw.BaseStream.Seek(0, System.IO.SeekOrigin.End);
                    sw.WriteLine("{0}--{1}--{2}", category, System.DateTime.Now.ToString("s"), message);
                    sw.Flush();
                }
            }
        }

        /// <summary>
        /// 获取缓存文件路径
        /// </summary>
        /// <param name="fileName"> 文件名称</param>
        /// <param name="variable"> 环境变量</param>
        /// <param name="filetype"> 文件类型</param>
        /// <returns> 文件路径</returns>
        private static string GetFilePath(string fileName, string variable, string filetype)
        {
            // 缓存文件夹
            var path = GetPathByVariable(variable);
            // 文件类型
            var _type = filetype.Contains(".") ? filetype : $".{filetype}";
            // 文件名称
            var filename = fileName.Contains(filetype) ? fileName : $"{fileName}{_type}";
            // 返回文件地址
            return System.IO.Path.Combine(path, filename);
        }

        /// <summary>
        /// 获取缓存文件路径
        /// 
        /// 参数设置:
        ///     ALLUSERSPROFILE=C:\ProgramData
        ///     APPDATA = C:\Users\user\AppData\Roaming
        ///     HOMEPATH = \Users\user
        ///     LOCALAPPDATA=C:\Users\user\AppData\Local
        ///     PROGRAMDATA = C:\ProgramData
        ///     PUBLIC = C:\Users\Public
        ///     TEMP = C:\Users\user\AppData\Local\Temp
        ///     TMP = C:\Users\user\AppData\Local\Temp
        ///     USERPROFILE = C:\Users\user
        /// </summary>
        /// <param name="variable"> 系统环境变量</param>
        /// <returns> 环境变量所在目录</returns>
        private static string GetPathByVariable(string variable)
        {
            var path = System.Environment.GetEnvironmentVariable(variable);
            if(path == null)
            {
                throw new System.Exception("Can't find file path in this PC!");
            }
            return path;
        }
    }
}
