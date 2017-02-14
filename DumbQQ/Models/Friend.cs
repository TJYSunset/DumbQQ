using System.Collections.Generic;
using System.Linq;
using DumbQQ.Client;
using DumbQQ.Constants;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DumbQQ.Models
{
    /// <summary>
    ///     好友（不含详细信息）。
    /// </summary>
    public class Friend : IListable, IUser, IMessageable
    {
        /// <summary>
        ///     备注姓名。
        /// </summary>
        [JsonProperty("markname")]
        public string Alias { get; set; }

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

        DumbQQClient.TargetType IMessageable.TargetType => DumbQQClient.TargetType.Friend;

        /// <summary>
        ///     用于发送信息的编号。不等于QQ号。
        /// </summary>
        [JsonProperty("userId")]
        public long Id { get; set; }

        /// <summary>
        ///     昵称。
        /// </summary>
        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        internal static List<Friend> GetList(DumbQQClient client)
        {
            DumbQQClient.Logger.Debug("开始获取好友列表");
            var response = client.Client.Post(ApiUrl.GetFriendList,
                new JObject {{"vfwebqq", client.Vfwebqq}, {"hash", client.Hash}});
            return
                DumbQQClient.ParseFriendDictionary(client.GetResponseJson(response)["result"] as JObject)
                    .Select(_ => _.Value)
                    .ToList();
        }

        internal static FriendInfo GetInfo(DumbQQClient client, long id)
        {
            DumbQQClient.Logger.Debug("开始获取好友信息");

            var response = client.Client.Get(ApiUrl.GetFriendInfo, id, client.Vfwebqq, client.Psessionid);
            return ((JObject) client.GetResponseJson(response)["result"]).ToObject<FriendInfo>();
        }
    }
}