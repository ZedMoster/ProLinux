using System.Management;

namespace ConsoleApp1
{
    /// <summary>
    /// 获取电脑信息
    /// </summary>
    class HelperMachineCode
    {
        /// <summary>
        /// 获取 cpu序列号
        /// </summary>
        /// <returns></returns>
        public static string GetCpuProcessorId()
        {
            string CpuInfo = string.Empty;
            // 获取信息
            using(ManagementClass cimobject = new ManagementClass("Win32_Processor"))
            {
                ManagementObjectCollection moc = cimobject.GetInstances();

                foreach(ManagementObject mo in moc)
                {
                    CpuInfo = mo.Properties["ProcessorId"].Value.ToString();
                    mo.Dispose();
                }
            }

            return CpuInfo;
        }

        /// <summary>
        /// 获取 硬盘ID
        /// </summary>
        /// <returns></returns>
        public static string GetHDid()
        {
            string HDid = "";

            using(ManagementClass cimobject1 = new ManagementClass("Win32_DiskDrive"))
            {
                ManagementObjectCollection moc1 = cimobject1.GetInstances();
                foreach(ManagementObject mo in moc1)
                {
                    HDid = (string)mo.Properties["Model"].Value;
                    mo.Dispose();
                }
            }

            return HDid.ToString();
        }

        /// <summary>
        /// 获取 网卡地址
        /// </summary>
        /// <returns></returns>
        public static string GetMoAddress()
        {
            string MoAddress = "";

            using(ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration"))
            {
                ManagementObjectCollection moc2 = mc.GetInstances();
                foreach(ManagementObject mo in moc2)
                {
                    if((bool)mo["IPEnabled"] == true)
                        MoAddress = mo["MacAddress"].ToString();
                    mo.Dispose();
                }
            }
            return MoAddress.ToString();
        }
    }
}