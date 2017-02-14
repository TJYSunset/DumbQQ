using System.Collections.Generic;
using DumbQQ.Client;
using DumbQQ.Constants;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DumbQQ.Models
{
    /// <summary>
    ///     群（不含详细信息）。
    /// </summary>
    public class Group : IListable, IMessageable
    {
        /// <summary>
        ///     名称。
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        ///     意义尚不明确。
        /// </summary>
        [JsonProperty("flag")]
        public long Flag { get; set; }

        /// <summary>
        ///     用于查询详细信息信息的编号。
        /// </summary>
        [JsonProperty("code")]
        public long Code { get; set; }

        /// <summary>
        ///     用于发送信息的编号。不等于群号。
        /// </summary>
        [JsonProperty("gid")]
        public long Id { get; set; }

        DumbQQClient.TargetType IMessageable.TargetType => DumbQQClient.TargetType.Group;

        internal static List<Group> GetList(DumbQQClient client)
        {
            DumbQQClient.Logger.Debug("开始获取群列表");
            var response = client.Client.Post(ApiUrl.GetGroupList,
                new JObject {{"vfwebqq", client.Vfwebqq}, {"hash", client.Hash}});
            return
                ((JArray) ((JObject) client.GetResponseJson(response)["result"])["gnamelist"])
                .ToObject<List<Group>>();
        }

        internal static GroupInfo GetInfo(DumbQQClient client, long id)
        {
            DumbQQClient.Logger.Debug("开始获取群资料");

            var response = client.Client.Get(ApiUrl.GetGroupInfo, id, client.Vfwebqq);
            var result = (JObject) client.GetResponseJson(response)["result"];
            var info = result["ginfo"].ToObject<GroupInfo>();
            // 获得群成员信息
            var members = new Dictionary<long, GroupMember>();
            var minfo = (JArray) result["minfo"];
            for (var i = 0; minfo != null && i < minfo.Count; i++)
            {
                var member = minfo[i].ToObject<GroupMember>();
                members.Add(member.Id, member);
                info.Members.Add(member);
            }
            var stats = (JArray) result["stats"];
            for (var i = 0; stats != null && i < stats.Count; i++)
            {
                var item = (JObject) stats[i];
                var member = members[item["uin"].Value<long>()];
                member.ClientType = item["client_type"].Value<int>();
                member.Status = item["stat"].Value<int>();
            }
            var cards = (JArray) result["cards"];
            for (var i = 0; cards != null && i < cards.Count; i++)
            {
                var item = (JObject) cards[i];
                members[item["muin"].Value<long>()].Alias = item["card"].Value<string>();
            }
            var vipinfo = (JArray) result["vipinfo"];
            for (var i = 0; vipinfo != null && i < vipinfo.Count; i++)
            {
                var item = (JObject) vipinfo[i];
                var member = members[item["u"].Value<long>()];
                member.IsVip = item["is_vip"].Value<int>() == 1;
                member.VipLevel = item["vip_level"].Value<int>();
            }
            return info;
        }
    }
}