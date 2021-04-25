using System;

namespace HelperLicense
{
    public static class Test
    {
        public static void Main()
        {
            Console.WriteLine("--------------------------");
            var info = GetMachineCode.GetCpuProcessorId();

            Console.WriteLine($"{info}");
            Console.WriteLine("--------------------------");

            string myKey2 = GetCrypt.GenerateKey();

            // H4sIAAAAAAAEALO3tw8P1rSvAgCdgtEeCAAAAA==
            // ???WS)?z
            Console.WriteLine($"原始的MD5key:{myKey2}");
            Console.WriteLine($"压缩后MD5key:{CompressString.Compress(myKey2)}");
            Console.WriteLine("--------------------------");

            // 读取注册列表中压缩后的 MD5key
            var r_key = "H4sIAAAAAAAEALO3V1bVcbOXBQD+O9ZKCAAAAA==";
            Console.WriteLine($"读取本机压缩的MD5key：{r_key}");

            // 解压 MD5key
            string myKey = CompressString.Decompress(r_key);
            Console.WriteLine($"解压本机压缩的MD5key：{myKey}");
            Console.WriteLine("--------------------------");

            // 压缩 MD5数据
            // 867E98085B1881B02CD3BA2D145923A8102A4648BCB0AC3E
            Console.WriteLine("压缩MD5数据：{0}", GetCrypt.Encrypt(info, myKey));

            // 解压 MD5数据
            // BFEBFBFF000806EC
            Console.WriteLine("解压MD5数据：{0}", GetCrypt.Decrypt(GetCrypt.Encrypt(info, myKey), myKey));
            Console.WriteLine("--------------------------");
        }
    }
}
