using System.Collections.Generic;
using DumbQQ.Client;
using DumbQQ.Constants;
using DumbQQ.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DumbQQ.Models
{
    /// <summary>
    /// 讨论组（不含详细信息）。
    /// </summary>
    public class Discussion : IListable, IMessageable
    {
        /// <summary>
        /// 可用于发送消息的编号。
        /// </summary>
        [JsonProperty("did")]
        public long Id { get; set; }

        /// <summary>
        /// 名称。
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        internal static List<Discussion> GetList(DumbQQClient client)
        {
            DumbQQClient.Logger.Debug("开始获取讨论组列表");
            var response = client.Client.Get(ApiUrl.GetDiscussionList, client.Psessionid, client.Vfwebqq,
                RandomHelper.GetRandomDouble());
            return
                ((JArray) ((JObject) client.GetResponseJson(response)["result"])["dnamelist"])
                .ToObject<List<Discussion>>();
        }

        internal static DiscussionInfo GetInfo(DumbQQClient client, long id)
        {
            DumbQQClient.Logger.Debug("开始获取讨论组信息");

            var response = client.Client.Get(ApiUrl.GetDiscussionInfo, id, client.Vfwebqq, client.Psessionid);
            var result = (JObject) client.GetResponseJson(response)["result"];
            var info = result["info"].ToObject<DiscussionInfo>();
            // 获得讨论组成员信息
            var members = new Dictionary<long, DiscussionMember>();
            var minfo = (JArray) result["mem_info"];
            for (var i = 0; minfo != null && i < minfo.Count; i++)
            {
                var member = minfo[i].ToObject<DiscussionMember>();
                members.Add(member.Id, member);
                info.Members.Add(member);
            }
            var stats = (JArray) result["mem_status"];
            for (var i = 0; stats != null && i < stats.Count; i++)
            {
                var item = (JObject) stats[i];
                var member = members[item["uin"].Value<long>()];
                member.ClientType = item["client_type"].Value<int>();
                member.Status = item["status"].Value<string>();
            }
            return info;
        }

        DumbQQClient.TargetType IMessageable.TargetType => DumbQQClient.TargetType.Discussion;
    }
}