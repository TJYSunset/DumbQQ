using System.Collections.Generic;
using Newtonsoft.Json;

namespace DumbQQ.Models
{
    /// <summary>
    ///     讨论组详细信息。
    /// </summary>
    internal class DiscussionInfo
    {
        /// <summary>
        ///     可用于发送消息的编号。
        /// </summary>
        [JsonProperty("did")]
        public long Id { get; set; }

        /// <summary>
        ///     名称。
        /// </summary>
        [JsonProperty("discu_name")]
        public string Name { get; set; }

        /// <summary>
        ///     成员。
        /// </summary>
        [JsonProperty("users")]
        public List<DiscussionMember> Members { get; set; } = new List<DiscussionMember>();
    }
}