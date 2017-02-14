using DumbQQ.Client;
using DumbQQ.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DumbQQ.Models
{
    /// <summary>
    ///     好友消息。
    /// </summary>
    public class FriendMessage : IMessage
    {
        /// <summary>
        ///     字体。
        /// </summary>
        [JsonProperty("content_font")]
        public Font Font { get; set; }

        /// <summary>
        ///     用于parse消息和字体的对象。
        /// </summary>
        [JsonProperty("content")]
        internal JArray ContentAndFont
        {
            set
            {
                Font = value.First.ToObject<Font>();
                value.RemoveAt(0);
                foreach (var shit in value)
                    Content += StringHelper.ParseEmoticons(shit);
            }
        }

        /// <summary>
        ///     发送者ID。
        /// </summary>
        [JsonProperty("send_uin")]
        public long UserId { get; set; }

        /// <summary>
        ///     消息时间戳。
        /// </summary>
        [JsonProperty("time")]
        public long Timestamp { get; set; }

        /// <summary>
        ///     消息文字内容。
        /// </summary>
        [JsonProperty("content_text")]
        public string Content { get; set; }

        long IMessage.RepliableId
        {
            get { return UserId; }
            set { UserId = value; }
        }

        DumbQQClient.TargetType IMessage.Type => DumbQQClient.TargetType.Friend;
    }
}