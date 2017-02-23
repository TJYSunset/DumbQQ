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
        [JsonIgnore] internal DumbQQClient Client;

        /// <summary>
        ///     字体。
        /// </summary>
        [JsonIgnore]
        internal Font Font { get; set; }

        /// <summary>
        ///     用于parse消息和字体的对象。
        /// </summary>
        [JsonProperty("content")]
        internal JArray ContentAndFont
        {
            set
            {
                Font = ((JArray) value.First).Last.ToObject<Font>();
                value.RemoveAt(0);
                foreach (var shit in value)
                    Content += StringHelper.ParseEmoticons(shit);
            }
        }

        /// <summary>
        ///     发送者ID。
        /// </summary>
        [JsonProperty("from_uin")]
        internal long SenderId { get; set; }

        /// <summary>
        ///     发送者。
        /// </summary>
        [JsonIgnore]
        public Friend Sender => Client.Friends.Find(_ => _.Id == SenderId);

        [JsonIgnore]
        User IMessage.Sender => Sender;

        /// <summary>
        ///     消息时间戳。
        /// </summary>
        [JsonProperty("time")]
        public long Timestamp { get; internal set; }

        /// <summary>
        ///     消息文字内容。
        /// </summary>
        [JsonIgnore]
        public string Content { get; internal set; }

        /// <summary>
        ///     回复该消息。
        /// </summary>
        /// <param name="content">回复内容。</param>
        public void Reply(string content)
        {
            Client.Message(DumbQQClient.TargetType.Friend, SenderId, content);
        }
    }
}