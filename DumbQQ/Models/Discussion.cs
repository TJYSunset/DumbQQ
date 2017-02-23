using System.Collections.Generic;
using DumbQQ.Client;
using DumbQQ.Constants;
using DumbQQ.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DumbQQ.Models
{
    /// <summary>
    ///     讨论组（不含详细信息）。
    /// </summary>
    public class Discussion : IListable, IMessageable
    {
        [JsonIgnore] private DiscussionInfo _info;

        [JsonIgnore] internal DumbQQClient Client;

        [JsonIgnore]
        private DiscussionInfo Info
        {
            get
            {
                if (_info != null) return _info;
                DumbQQClient.Logger.Debug("开始获取讨论组信息");

                var response = Client.Client.Get(ApiUrl.GetDiscussionInfo, Id, Client.Vfwebqq, Client.Psessionid);
                var result = (JObject) Client.GetResponseJson(response)["result"];
                _info = result["info"].ToObject<DiscussionInfo>();
                // 获得讨论组成员信息
                var members = new Dictionary<long, DiscussionMember>();
                var minfo = (JArray) result["mem_info"];
                for (var i = 0; minfo != null && i < minfo.Count; i++)
                {
                    var member = minfo[i].ToObject<DiscussionMember>();
                    members.Add(member.Id, member);
                    _info.Members.Add(member);
                }
                var stats = (JArray) result["mem_status"];
                for (var i = 0; stats != null && i < stats.Count; i++)
                {
                    var item = (JObject) stats[i];
                    var member = members[item["uin"].Value<long>()];
                    member.ClientType = item["client_type"].Value<int>();
                    member.Status = item["status"].Value<string>();
                }
                _info.Members.ForEach(_ => _.Client = Client);
                return _info;
            }
        }

        /// <summary>
        ///     成员。
        /// </summary>
        [JsonIgnore]
        public List<DiscussionMember> Members => Info.Members;

        /// <summary>
        ///     名称。
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }

        /// <summary>
        ///     可用于发送消息的编号。
        /// </summary>
        [JsonProperty("did")]
        public long Id { get; internal set; }

        /// <summary>
        ///     发送消息。
        /// </summary>
        /// <param name="content">消息内容。</param>
        public void Message(string content)
        {
            Client.Message(DumbQQClient.TargetType.Discussion, Id, content);
        }

        protected bool Equals(Discussion other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Discussion) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        internal static List<Discussion> GetList(DumbQQClient client)
        {
            DumbQQClient.Logger.Debug("开始获取讨论组列表");
            var response = client.Client.Get(ApiUrl.GetDiscussionList, client.Psessionid, client.Vfwebqq,
                RandomHelper.GetRandomDouble());
            var result =
                ((JArray) ((JObject) client.GetResponseJson(response)["result"])["dnamelist"])
                .ToObject<List<Discussion>>();
            result.ForEach(_ => _.Client = client);
            return result;
        }

        public static bool operator ==(Discussion left, Discussion right) => left?.Id == right?.Id;
        public static bool operator !=(Discussion left, Discussion right) => !(left == right);
    }
}