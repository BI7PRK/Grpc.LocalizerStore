namespace AspNetCore.Grpc.LocalizerStore
{
    /// <summary>
    /// 用于语言翻译的属性标识
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class LocalizerDefaultAttribute : Attribute
    {
        private readonly string langValue;
        private readonly string langCode;
        private readonly int langTid;
        private readonly string? _category;

        /// <summary>
        /// 语言翻译文
        /// </summary>
        /// <param name="value"></param>
        /// <param name="code">语言代码</param>
        /// <param name="tid">默认的类别ID</param>
        /// <param name="category">默认的类别ID</param>
        public LocalizerDefaultAttribute(string value, string code = "zh-CN",  int tid = 0, string? category = null)
        {
            langValue = value;
            langCode = code;
            langTid = tid;
            _category = category;
        }
        /// <summary>
        /// 指定的语言翻译文本
        /// </summary>
        public string Value => langValue;
        /// <summary>
        /// 指定的语言代码
        /// </summary>
        public string Code => langCode;
        /// <summary>
        /// 指定的类别ID
        /// </summary>
        public int Tid => langTid;

        /// <summary>
        /// 指定的类别
        /// </summary>
        public string? Category => _category;
    }
}
