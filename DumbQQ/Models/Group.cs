using System;
using System.Collections.Generic;
using DumbQQ.Client;
using DumbQQ.Constants;
using DumbQQ.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DumbQQ.Models
{
    /// <summary>
    ///     群（不含详细信息）。
    /// </summary>
    public class Group : IListable, IMessageable
    {
        [JsonIgnore] private readonly LazyHelper<GroupInfo> _info = new LazyHelper<GroupInfo>();

        [JsonIgnore] internal DumbQQClient Client;

        /// <summary>
        ///     意义尚不明确。
        /// </summary>
        [JsonProperty("flag")]
        public long Flag { get; internal set; }

        /// <summary>
        ///     用于查询详细信息信息的编号。
        /// </summary>
        [JsonProperty("code")]
        internal long Code { get; set; }

        [JsonIgnore]
        private GroupInfo Info => _info.GetValue(() =>
        {
            DumbQQClient.Logger.Debug("开始获取群资料");

            var response = Client.Client.Get(ApiUrl.GetGroupInfo, Code, Client.Vfwebqq);
            var result = (JObject) Client.GetResponseJson(response)["result"];
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
                if (item["muin"].Value<long>() == Client.Id)
                    info.MyAlias = item["card"].Value<string>();
            }
            var vipinfo = (JArray) result["vipinfo"];
            for (var i = 0; vipinfo != null && i < vipinfo.Count; i++)
            {
                var item = (JObject) vipinfo[i];
                var member = members[item["u"].Value<long>()];
                member.IsVip = item["is_vip"].Value<int>() == 1;
                member.VipLevel = item["vip_level"].Value<int>();
            }
            info.Members.ForEach(_ => _.Client = Client);
            return info;
        });

        /// <summary>
        ///     创建时间。
        /// </summary>
        [JsonIgnore]
        public long CreateTime => Info.CreateTime;

        /// <summary>
        ///     「本群须知」公告。(大概……）
        /// </summary>
        [JsonIgnore]
        public string Announcement => Info.Announcement;

        /// <summary>
        ///     备注名称。
        /// </summary>
        [JsonIgnore]
        [Obsolete("此属性没有用处。")]
        public string Alias => Info.Alias;

        /// <summary>
        ///     群主。
        /// </summary>
        [JsonIgnore]
        public GroupMember Owner => _owner.GetValue(() => Members.Find(_ => _.Id == Info.OwnerId));

        [JsonIgnore] private readonly LazyHelper<GroupMember> _owner = new LazyHelper<GroupMember>();

        /// <summary>
        ///     成员。
        /// </summary>
        [JsonIgnore]
        public List<GroupMember> Members => Info.Members;

        /// <summary>
        ///     已登录账户在此群的群名片。
        /// </summary>
        [JsonIgnore]
        public string MyAlias => Info.MyAlias;

        /// <inheritdoc />
        [JsonProperty("name")]
        public string Name { get; internal set; }

        /// <inheritdoc />
        [JsonProperty("gid")]
        public long Id { get; internal set; }

        /// <inheritdoc />
        /// <param name="content">消息内容。</param>
        public void Message(string content)
        {
            Client.Message(DumbQQClient.TargetType.Group, Id, content);
        }

        /// <inheritdoc />
        protected bool Equals(Group other)
        {
            return Id == other.Id;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Group) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        internal static List<Group> GetList(DumbQQClient client)
        {
            DumbQQClient.Logger.Debug("开始获取群列表");
            var response = client.Client.Post(ApiUrl.GetGroupList,
                new JObject {{"vfwebqq", client.Vfwebqq}, {"hash", client.Hash}});
            var result =
                ((JArray) ((JObject) client.GetResponseJson(response)["result"])["gnamelist"])
                .ToObject<List<Group>>();
            result.ForEach(_ => _.Client = client);
            return result;
        }

        /// <inheritdoc />
        public static bool operator ==(Group left, Group right) => left?.Id == right?.Id;

        /// <inheritdoc />
        public static bool operator !=(Group left, Group right) => !(left == right);
    }
}