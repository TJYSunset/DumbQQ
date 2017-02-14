using Newtonsoft.Json;

namespace DumbQQ.Models
{
    /// <summary>
    /// 讨论组成员。
    /// </summary>
    public class DiscussionMember : IUser
    {
        /// <summary>
        /// 可用于发送消息的编号，不等于QQ号。
        /// </summary>
        [JsonProperty("uin")]
        public long Id { get; set; }

        /// <summary>
        /// 昵称。
        /// </summary>
        [JsonProperty("nick")]
        public string Nickname { get; set; }

        /// <summary>
        /// 客户端类型。
        /// </summary>
        [JsonProperty("clientType")]
        public int ClientType { get; set; }

        /// <summary>
        /// 当前状态。
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; }
    }
}