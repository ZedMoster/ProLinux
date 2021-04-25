using System.Reflection;

namespace CADReader
{
    /// <summary>
    /// 定义按键参数
    /// </summary>
    public class PushButtonParamter
    {
        /// <summary>
        /// 内部名称
        /// </summary>
        public string InName
        {
            get { return inName; }
            set { inName = value; }
        }
        private string inName;

        /// <summary>
        /// 功能名称
        /// </summary>
        public string ButtonName
        {
            get { return buttonName; }
            set { buttonName = value; }
        }
        private string buttonName;

        /// <summary>
        /// 命名空间
        /// </summary>
        public string NameSpace
        {
            get { return nameSpace; }
            set
            {
                nameSpace = value.Contains(".") ? value : "CADReader.Command." + value;
            }
        }
        private string nameSpace;

        /// <summary>
        /// 程序集路径
        /// </summary>
        public string AssemblyName
        {
            get { return assemblyName; }
            set { assemblyName = value; }
        }
        private string assemblyName = Assembly.GetExecutingAssembly().Location;

        /// <summary>
        /// 提示图片
        /// </summary>
        public string TooltipImage
        {
            get { return tooltipImage; }
            set { tooltipImage = value; }
        }
        private string tooltipImage;

        /// <summary>
        /// 提示文字
        /// </summary>
        public string Tooltip
        {
            get { return tooltip; }
            set { tooltip = value; }
        }
        private string tooltip;

        /// <summary>
        /// 大图名称
        /// </summary>
        public string ImageName
        {
            get { return imageName; }
            set { imageName = value; }
        }
        private string imageName;

        /// <summary>
        /// 小图名称
        /// </summary>
        public string StackedImageName
        {
            get { return stackedImageName; }
            set { stackedImageName = value; }
        }
        private string stackedImageName;
    }
}
