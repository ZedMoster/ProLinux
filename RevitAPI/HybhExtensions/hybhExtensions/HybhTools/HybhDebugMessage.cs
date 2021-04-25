/// <summary>
/// 缓存记录
/// </summary>

namespace Xml.HybhTools
{
    public static class HybhDebugMessage
    {
        /// <summary>
        /// 缓存记录
        /// C:\Users\{UserName}\AppData\Local\Temp
        /// </summary>
        /// <param name="message"> 缓存消息</param>
        public static void ToDebug(this string message, string filename = "~tmp")
        {
            var path = System.Environment.GetEnvironmentVariable("TEMP");
            if(path == null)
            {
                System.Windows.MessageBox.Show("缓存保存失败");
                return;
            }
            var filePath = System.IO.Path.Combine(path, filename);

            using(System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write))
            {
                using(System.IO.StreamWriter sw = new System.IO.StreamWriter(fs))
                {
                    sw.BaseStream.Seek(0, System.IO.SeekOrigin.End);
                    sw.WriteLine("{0}:{1}", System.DateTime.Now.ToString("O"), message, System.DateTime.Now);
                    sw.Flush();
                }
            }
        }
    }
}
