using System;
using System.Collections.Generic;
using System.Net.Http;
using DumbQQ.Constants;
using DumbQQ.Models.Abstract;
using DumbQQ.Models.Receipts;
using DumbQQ.Utils;
using RestSharp;
using RestSharp.Deserializers;
using SimpleJson;

namespace DumbQQ.Models
{
    public class Friend : User, IUseLazyProperty, IMessageTarget
    {
        #region properties

        protected enum LazyProperty
        {
            Bio
        }

        internal Friend()
        {
            Properties = new LazyProperties(() =>
            {
                var response =
                    Client.RestClient.Get<FriendPropertiesReceipt>(Api.GetFriendInfo.Get(Id,
                        Client.Session.tokens.Vfwebqq, Client.Session.tokens.Psessionid));
                if (!response.IsSuccessful)
                    throw new HttpRequestException($"HTTP request unsuccessful: status code {response.StatusCode}");

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

        [DeserializeAs(Name = @"uin")]
        public override ulong Id { get; internal set; }

        [DeserializeAs(Name = @"nick")]
        public override string Name { get; internal set; }

        [DeserializeAs(Name = @"markname")]
        public override string NameAlias { get; internal set; }

        [DeserializeAs(Name = @"categories")]
        public ulong CategoryIndex { get; internal set; }

        public string Bio => Properties[(int) LazyProperty.Bio];
        public void LoadLazyProperties() => Properties.Load();

        #endregion

        public void Message(string content)
        {
            var response = Client.RestClient.Post<MessageReceipt>(Api.SendMessageToFriend.Post(
                new JsonObject
                {
                    {@"to", Id},
                    {@"content", new JsonArray {content, new JsonArray {@"font", Miscellaneous.Font}}.ToString()},
                    {@"face", 573},
                    {@"client_id", Miscellaneous.ClientId},
                    {@"msg_id", Miscellaneous.MessageId},
                    {@"psessionid", Client.Session.tokens.Psessionid}
                }));
            if (!response.IsSuccessful)
                throw new HttpRequestException($"HTTP request unsuccessful: status code {response.StatusCode}");
            if (response.Data.Code != 0)
                throw new ApplicationException($"Request unsuccessful: returned {response.Data.Code}");
        }
    }
}