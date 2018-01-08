using System.Collections.Generic;
using DumbQQ.Constants;
using DumbQQ.Helpers;
using DumbQQ.Models.Abstract;
using DumbQQ.Models.Utilities;
using RestSharp.Deserializers;
using SimpleJson;

namespace DumbQQ.Models
{
    public class Friend : User, IUseLazyProperty, IMessageTarget
    {
        public void Message(string content)
        {
            Client.RestClient.Post<MessageResponse>(Api.SendMessageToFriend,
                new JsonObject
                {
                    {@"to", Id},
                    {@"content", new JsonArray {content, new JsonArray {@"font", Miscellaneous.Font}}.ToString()},
                    {@"face", 573},
                    {@"client_id", Miscellaneous.ClientId},
                    {@"msg_id", Miscellaneous.MessageId},
                    {@"psessionid", Client.Session.tokens.psessionid}
                });
        }

        #region properties

        protected enum LazyProperty
        {
            Bio
        }

        public Friend()
        {
            Properties = new LazyProperties(() =>
            {
                var response =
                    Client.RestClient.Get<FriendPropertiesResponse>(Api.GetFriendInfo, Id,
                        Client.Session.tokens.vfwebqq, Client.Session.tokens.psessionid);

                return new Dictionary<int, object>
                {
                    {(int) LazyProperty.Bio, response.Data.Result.Bio}
                };
            });
        }

        protected readonly LazyProperties Properties;

        [DeserializeAs(Name = @"u")]
        private ulong IdVipinfo
        {
            set => Id = value;
        }

        [DeserializeAs(Name = @"uin")] public override ulong Id { get; internal set; }

        [DeserializeAs(Name = @"nick")] public override string Name { get; internal set; }

        [DeserializeAs(Name = @"markname")] public override string NameAlias { get; internal set; }

        [DeserializeAs(Name = @"categories")] public ulong CategoryIndex { get; internal set; }

        [LazyProperty] public string Bio => Properties[(int) LazyProperty.Bio];

        public void LoadLazyProperties()
        {
            Properties.Load();
        }

        #endregion
    }
}