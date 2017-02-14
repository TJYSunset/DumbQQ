using System.Collections.Generic;

namespace DumbQQ.Utils
{
    public static class DictionaryHelper
    {
        /// <summary>
        ///     在键已存在时覆盖内容，反之添加新键值对。
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="key">键。</param>
        /// <param name="value">值。</param>
        public static void Put<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, TValue value)
        {
            if (dic.ContainsKey(key))
                dic[key] = value;
            else
                dic.Add(key, value);
        }
    }
}