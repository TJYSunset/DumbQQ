using Newtonsoft.Json;

namespace DumbQQ.Models
{
    /// <summary>
    ///     好友详细信息。
    /// </summary>
    internal class FriendInfo
    {
        /// <summary>
        ///     可用于发送消息的编号。
        /// </summary>
        [JsonProperty("uin")]
        public long Id { get; set; }

        /// <summary>
        ///     昵称。
        /// </summary>
        [JsonProperty("nick")]
        public string Nickname { get; set; }

        /// <summary>
        ///     个性签名。
        /// </summary>
        [JsonProperty("lnick")]
        public string Bio { get; set; }

        /// <summary>
        ///     生日。
        /// </summary>
        [JsonProperty("birthday")]
        public Birthday Birthday { get; set; }

        /// <summary>
        ///     座机号码。
        /// </summary>
        [JsonProperty("phone")]
        public string Phone { get; set; }

        /// <summary>
        ///     手机号码。
        /// </summary>
        [JsonProperty("mobile")]
        public string Cellphone { get; set; }

        /// <summary>
        ///     邮箱地址。
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        ///     职业。
        /// </summary>
        [JsonProperty("occupation")]
        public string Job { get; set; }

        /// <summary>
        ///     个人主页。
        /// </summary>
        [JsonProperty("homepage")]
        public string Homepage { get; set; }

        /// <summary>
        ///     学校。
        /// </summary>
        [JsonProperty("college")]
        public string School { get; set; }

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
        ///     生肖。
        /// </summary>
        [JsonProperty("shengxiao")]
        public int Shengxiao { get; set; }

        /// <summary>
        ///     意义暂不明确。
        /// </summary>
        [JsonProperty("personal")]
        public string Personal { get; set; }

        /// <summary>
        ///     意义暂不明确。
        /// </summary>
        [JsonProperty("vip_info")]
        public int VipInfo { get; set; }
    }

    /// <summary>
    ///     生日。
    /// </summary>
    public class Birthday
    {
        /// <summary>
        ///     年。
        /// </summary>
        [JsonProperty("year")]
        public int Year { get; set; }

        /// <summary>
        ///     月。
        /// </summary>
        [JsonProperty("month")]
        public int Month { get; set; }

        /// <summary>
        ///     日。
        /// </summary>
        [JsonProperty("day")]
        public int Day { get; set; }
    }
}