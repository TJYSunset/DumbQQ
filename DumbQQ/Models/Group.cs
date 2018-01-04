using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using DumbQQ.Constants;
using DumbQQ.Models.Abstract;
using DumbQQ.Models.Receipts;
using DumbQQ.Utils;
using RestSharp.Deserializers;
using SimpleJson;

namespace DumbQQ.Models
{
    public class Group : UserCollection<Group.Member>, IUseLazyProperty, IMessageTarget
    {
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

            [DeserializeAs(Name = @"uin")]
            public override ulong Id { get; internal set; }

            [DeserializeAs(Name = @"nick")]
            public override string Name { get; internal set; }

            [DeserializeAs(Name = @"card")]
            public override string NameAlias { get; internal set; }
        }

        protected enum LazyProperty
        {
            Members,
            OwnerId,
            Created,
            PinnedAnnouncement
        }

        internal Group()
        {
            Properties = new LazyProperties(() =>
            {
                var response =
                    Client.RestClient.Get<GroupPropertiesReceipt>(Api.GetDiscussInfo, PropertiesCode,
                        Client.Session.tokens.vfwebqq);
                if (!response.IsSuccessful)
                    throw new HttpRequestException($"HTTP request unsuccessful: status code {response.StatusCode}");

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

        [DeserializeAs(Name = @"id")]
        public override ulong Id { get; internal set; }

        [DeserializeAs(Name = @"code")]
        public ulong PropertiesCode { get; internal set; }

        [DeserializeAs(Name = @"name")]
        public override string Name { get; internal set; }

        public override ReadOnlyDictionary<long, Member> Members => Properties[(int) LazyProperty.Members];
        public string PinnedAnnouncement => Properties[(int) LazyProperty.PinnedAnnouncement];
        public override IEnumerator<Member> GetEnumerator() => Members.Values.GetEnumerator();
        public void LoadLazyProperties() => Properties.Load();

        #endregion

        public void Message(string content)
        {
            var response = Client.RestClient.Post<Receipt>(Api.SendMessageToGroup,
                new JsonObject
                {
                    {@"group_uin", Id},
                    {@"content", new JsonArray {content, new JsonArray {@"font", Miscellaneous.Font}}.ToString()},
                    {@"face", 573},
                    {@"client_id", Miscellaneous.ClientId},
                    {@"msg_id", Miscellaneous.MessageId},
                    {@"psessionid", Client.Session.tokens.psessionid}
                });
            if (!response.IsSuccessful)
                throw new HttpRequestException($"HTTP request unsuccessful: status code {response.StatusCode}");
            if (response.Data.Code is int code && code != 0)
                throw new ApplicationException($"Request unsuccessful: returned {response.Data.Code}");
        }
    }
}