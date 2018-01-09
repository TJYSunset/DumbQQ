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
    public class Discussion : UserCollection<Discussion.Member>, IUseLazyProperty, IMessageTarget
    {
        public void Message(string content)
        {
            Client.RestClient.Post<MessageResponse>(Api.SendMessageToDiscuss,
                new JsonObject
                {
                    {@"did", Id},
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
            [DeserializeAs(Name = @"uin")] public override ulong Id { get; internal set; }

            [DeserializeAs(Name = @"nick")] public override string Name { get; internal set; }

            public override string NameAlias => null;
        }

        protected enum LazyProperty
        {
            Members
        }

        public Discussion()
        {
            Properties = new LazyProperties(() =>
            {
                var response =
                    Client.RestClient.Get<DiscussionPropertiesResponse>(Api.GetDiscussInfo, Id,
                        Client.Session.tokens.vfwebqq,
                        Client.Session.tokens.psessionid);

                return new Dictionary<int, object>
                {
                    {
                        (int) LazyProperty.Members,
                        new ReadOnlyDictionary<ulong, Member>(response.Data.Result.MemberList.Reassemble(x => x.Id,
                            Client,
                            response.Data.Result.MemberStatusList))
                    }
                };
            });
        }

        protected readonly LazyProperties Properties;

        [DeserializeAs(Name = @"did")] public override ulong Id { get; internal set; }

        [DeserializeAs(Name = @"name")] public override string Name { get; internal set; }

        [LazyProperty]
        public override ReadOnlyDictionary<ulong, Member> Members => Properties[(int) LazyProperty.Members];

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