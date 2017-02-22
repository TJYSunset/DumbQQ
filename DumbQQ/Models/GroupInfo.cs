using System.Collections.Generic;
using Newtonsoft.Json;

namespace DumbQQ.Models
{
    /// <summary>
    ///     群详细信息。
    /// </summary>
    internal class GroupInfo
    {
        /// <summary>
        ///     用于发送信息的编号。不等于群号。
        /// </summary>
        [JsonProperty("gid")]
        public long Id { get; set; }

        /// <summary>
        ///     创建时间。
        /// </summary>
        [JsonProperty("createtime")]
        public long CreateTime { get; set; }

        /// <summary>
        ///     「本群须知」公告。(大概……）
        /// </summary>
        [JsonProperty("memo")]
        public string Announcement { get; set; }

        /// <summary>
        ///     群名。
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        ///     备注名称。
        /// </summary>
        [JsonProperty("markname")]
        public string Alias { get; set; }

        /// <summary>
        ///     群主ID。
        /// </summary>
        [JsonProperty("owner")]
        public long OwnerId { get; set; }

        /// <summary>
        ///     成员。
        /// </summary>
        [JsonProperty("users")]
        public List<GroupMember> Members { get; set; } = new List<GroupMember>();
    }
}