using DumbQQ.Client;
using Newtonsoft.Json;

namespace DumbQQ.Models
{
    /// <summary>
    ///     讨论组成员。
    /// </summary>
    public class DiscussionMember : User
    {
        [JsonIgnore] internal DumbQQClient Client;

        /// <summary>
        ///     QQ号。
        /// </summary>
        [JsonIgnore]
        public override long QQNumber => Client.GetQQNumberOf(Id);

        /// <summary>
        ///     可用于发送消息的编号，不等于QQ号。
        /// </summary>
        [JsonProperty("uin")]
        public override long Id { get; internal set; }

        /// <summary>
        ///     昵称。
        /// </summary>
        [JsonProperty("nick")]
        public override string Nickname { get; internal set; }

        /// <summary>
        ///     客户端类型。
        /// </summary>
        [JsonProperty("clientType")]
        public int ClientType { get; set; }

        /// <summary>
        ///     当前状态。
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; }
    }
}