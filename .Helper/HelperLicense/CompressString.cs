using System.IO;
using System.IO.Compression;

namespace HelperLicense
{
    /// <summary>
    /// 字符串 压缩解压
    /// </summary>
    sealed class CompressString
    {
        /// <summary>
        /// 压缩字符串
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Compress(string input)
        {
            byte[] inputBytes = System.Text.Encoding.Default.GetBytes(input);
            byte[] result = Compress(inputBytes);
            return System.Convert.ToBase64String(result);
        }

        /// <summary>
        /// 解压缩字符串
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Decompress(string input)
        {
            byte[] inputBytes = System.Convert.FromBase64String(input);
            byte[] depressBytes = Decompress(inputBytes);
            return System.Text.Encoding.Default.GetString(depressBytes);
        }

        /// <summary>
        /// 压缩字节
        /// </summary>
        /// <param name="inputBytes"></param>
        /// <returns></returns>
        private static byte[] Compress(byte[] inputBytes)
        {
            using(MemoryStream outStream = new MemoryStream())
            {
                using(GZipStream zipStream = new GZipStream(outStream, CompressionMode.Compress, true))
                {
                    zipStream.Write(inputBytes, 0, inputBytes.Length);
                    zipStream.Close();
                    return outStream.ToArray();
                }
            }
        }

        /// <summary>
        /// 解压字节
        /// </summary>
        /// <param name="inputBytes"></param>
        /// <returns></returns>
        private static byte[] Decompress(byte[] inputBytes)
        {
            using(MemoryStream inputStream = new MemoryStream(inputBytes))
            {
                using(MemoryStream outStream = new MemoryStream())
                {
                    using(GZipStream zipStream = new GZipStream(inputStream, CompressionMode.Decompress))
                    {
                        zipStream.CopyTo(outStream);
                        zipStream.Close();
                        return outStream.ToArray();
                    }
                }
            }
        }
    }
}
