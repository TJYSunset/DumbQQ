using DumbQQ.Client;
using Newtonsoft.Json;

namespace DumbQQ.Models
{
    /// <summary>
    ///     群成员。
    /// </summary>
    public class GroupMember : User
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
        ///     群名片。
        /// </summary>
        [JsonProperty("card")]
        public string Alias { get; set; }

        /// <summary>
        ///     客户端类型。
        /// </summary>
        [JsonProperty("clientType")]
        public int ClientType { get; set; }

        /// <summary>
        ///     当前状态。
        /// </summary>
        [JsonProperty("status")]
        public int Status { get; set; }

        /// <summary>
        ///     国家。
        /// </summary>
        [JsonProperty("country")]
        public string Country { get; set; }

        /// <summary>
        ///     省份。
        /// </summary>
        [JsonProperty("province")]
        public string Province { get; set; }

        /// <summary>
        ///     城市。
        /// </summary>
        [JsonProperty("city")]
        public string City { get; set; }

        /// <summary>
        ///     性别。
        /// </summary>
        [JsonProperty("gender")]
        public string Gender { get; set; }

        /// <summary>
        ///     QQ会员状态。
        /// </summary>
        [JsonProperty("vip")]
        public bool IsVip { get; set; }

        /// <summary>
        ///     会员等级。
        /// </summary>
        [JsonProperty("vipLevel")]
        public int VipLevel { get; set; }
    }
}