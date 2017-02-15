using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DumbQQ.Utils
{
    internal static class StringHelper
    {
        /// <summary>
        ///     将一个消息JSON中的节点转换为表情的文字描述或文字本身。
        /// </summary>
        /// <param name="token">JSON节点。</param>
        /// <returns>文字。</returns>
        public static string ParseEmoticons(JToken token)
        {
            if (token is JArray)
                return token.ToString(Formatting.None);
            return token.Value<string>();
        }

        /// <summary>
        ///     将消息中的表情文字（e.g. "/微笑"）转换为节点内容以实现发送内置表情。
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static JToken[] TranslateEmoticons(string message)
        {
            // TODO: 实现将文本中的表情转换为JSON节点
            return new[]
            {
                JToken.FromObject(message)
            };
        }

        internal static string SomewhatHash(long uin, string ptwebqq)
        {
            var n = new int[4];
            for (var T = 0; T < ptwebqq.Length; T++)
                n[T % 4] ^= ptwebqq[T];
            string[] u = {"EC", "OK"};
            var v = new long[4];
            v[0] = ((uin >> 24) & 255) ^ u[0][0];
            v[1] = ((uin >> 16) & 255) ^ u[0][1];
            v[2] = ((uin >> 8) & 255) ^ u[1][0];
            v[3] = (uin & 255) ^ u[1][1];

            var u1 = new long[8];

            for (var t = 0; t < 8; t++)
                u1[t] = t % 2 == 0 ? n[t >> 1] : v[t >> 1];

            string[] n1 = {"0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F"};
            var v1 = "";
            foreach (var aU1 in u1)
            {
                v1 += n1[(int) ((aU1 >> 4) & 15)];
                v1 += n1[(int) (aU1 & 15)];
            }
            return v1;
        }
    }
}