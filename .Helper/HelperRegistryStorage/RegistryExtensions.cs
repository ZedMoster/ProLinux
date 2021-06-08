namespace HelperRegistryStorage
{
    public static class RegistryExtensions
    {
        /// <summary>
        /// 判断值是否为空
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this HelperRegistry registry, string key)
        {
            // 空值
            return string.IsNullOrWhiteSpace(registry.Get(key));
        }
    }
}
