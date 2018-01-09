using System.Collections.Generic;
using System.Collections.ObjectModel;
using DumbQQ.Constants;
using DumbQQ.Helpers;
using DumbQQ.Models.Abstract;
using DumbQQ.Models.Utilities;
using RestSharp.Deserializers;
using SimpleJson;

namespace DumbQQ.Models
{
    public class Group : UserCollection<Group.Member>, IUseLazyProperty, IMessageTarget
    {
        public void Message(string content)
        {
            Client.RestClient.Post<MessageResponse>(Api.SendMessageToGroup,
                new JsonObject
                {
                    {@"group_uin", Id},
                    {@"content", new JsonArray {content, new JsonArray {@"font", Miscellaneous.Font}}.ToString()},
                    {@"face", 573},
                    {@"client_id", Miscellaneous.ClientId},
                    {@"msg_id", Miscellaneous.MessageId},
                    {@"psessionid", Client.Session.tokens.psessionid}
                });
        }

        #region properties

        public class Member : User
        {
            [DeserializeAs(Name = @"muin")]
            private ulong IdCards
            {
                set => Id = value;
            }

            [DeserializeAs(Name = @"u")]
            private ulong IdVipinfo
            {
                set => Id = value;
            }

            [DeserializeAs(Name = @"uin")] public override ulong Id { get; internal set; }

            [DeserializeAs(Name = @"nick")] public override string Name { get; internal set; }

            [DeserializeAs(Name = @"card")] public override string NameAlias { get; internal set; }
        }

        protected enum LazyProperty
        {
            Members,
            OwnerId,
            Created,
            PinnedAnnouncement
        }

        public Group()
        {
            Properties = new LazyProperties(() =>
            {
                var response =
                    Client.RestClient.Get<GroupPropertiesResponse>(Api.GetGroupInfo, PropertiesCode,
                        Client.Session.tokens.vfwebqq);

                return new Dictionary<int, object>
                {
                    {
                        (int) LazyProperty.Members,
                        new ReadOnlyDictionary<ulong, Member>(response.Data.Result.MemberList.Reassemble(x => x.Id,
                            Client,
                            response.Data.Result.MemberNameAliasList,
                            response.Data.Result.MemberStatusList))
                    },
                    {(int) LazyProperty.PinnedAnnouncement, response.Data.Result.Miscellaneous.PinnedAnnouncement}
                };
            });
        }

        protected readonly LazyProperties Properties;

        [DeserializeAs(Name = @"gid")] public override ulong Id { get; internal set; }

        [DeserializeAs(Name = @"code")] public ulong PropertiesCode { get; internal set; }

        [DeserializeAs(Name = @"name")] public override string Name { get; internal set; }

        [LazyProperty]
        public override ReadOnlyDictionary<ulong, Member> Members => Properties[(int) LazyProperty.Members];

        [LazyProperty] public string PinnedAnnouncement => Properties[(int) LazyProperty.PinnedAnnouncement];

        public override IEnumerator<Member> GetEnumerator()
        {
            return Members.Values.GetEnumerator();
        }

        public void LoadLazyProperties()
        {
            Properties.Load();
        }

        #endregion
    }
}