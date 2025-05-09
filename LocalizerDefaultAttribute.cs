﻿namespace AspNetCore.Grpc.LocalizerStore
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

        /// <summary>
        /// 语言翻译文
        /// </summary>
        /// <param name="value"></param>
        /// <param name="code">语言代码</param>
        /// <param name="tid">默认的类别ID</param>
        public LocalizerDefaultAttribute(string value, string code = "zh-CN", int tid = 0)
        {
            langValue = value;
            langCode = code;
            langTid = tid;
        }

        public string Value => langValue;

        public string Code => langCode;
        /// <summary>
        /// 指定的类别ID
        /// </summary>
        public int Tid => langTid;
    }
}
