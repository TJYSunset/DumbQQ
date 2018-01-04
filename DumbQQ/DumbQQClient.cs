using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DumbQQ.Constants;
using DumbQQ.Models;
using DumbQQ.Models.Abstract;
using DumbQQ.Models.Receipts;
using DumbQQ.Utils;
using RestSharp;
using RestSharp.Deserializers;
using SimpleJson;

namespace DumbQQ
{
    public class DumbQQClient : UserCollection<Friend>, IUseLazyProperty
    {
        #region session related

        public struct TokenContainer
        {
            public string Ptwebqq;
            public string Vfwebqq;
            public ulong Uin;
            public string Psessionid;
        }

        public RestClient RestClient { get; } =
            new RestClient {UserAgent = Miscellaneous.DefaultUserAgent, Encoding = Encoding.UTF8};

        private TokenContainer _tokenContainer;

        public (TokenContainer tokens, CookieContainer cookies) Session
        {
            get => (_tokenContainer, RestClient.CookieContainer);
            set
            {
                _tokenContainer = value.tokens;
                RestClient.CookieContainer = value.cookies;

                // some magical extra step
                RestClient.Get(Api.GetFriendStatus.Get(_tokenContainer.Vfwebqq, _tokenContainer.Psessionid));
            }
        }

        public static (TokenContainer tokens, CookieContainer cookies) QrAuthenticate(Action<byte[]> imageCallback,
            uint maxAttempts = 10)
        {
            var tokens = new TokenContainer();
            var client = new RestClient();

            // download QR code and get the temporary token "qrsig"
            var qrResponse = client.Get(Api.GetQrCode.Get());
            var qrsig = qrResponse.Cookies.First(x => x.Name == @"qrsig").Value;
            imageCallback(qrResponse.RawBytes);

            // wait for manual authentication and get the url to next step
            int Hash33(string s)
            {
                int e = 0, i = 0, n = s.Length;
                for (; n > i; ++i)
                    e += (e << 5) + s[i];
                return 2147483647 & e;
            }

            string ptwebqqUrl;
            while (true)
            {
                Thread.Sleep(1);
                var message = client.Get(Api.VerifyQrCode.Get(Hash33(qrsig))).Content;
                if (message.Contains(@"失效")) throw new TimeoutException("QR authentication timed out.");
                if (!message.Contains(@"成功")) continue;
                ptwebqqUrl = Api.GetPtwebqqPattern.Match(message).Value;
                break;
            }

            // get ptwebqq
            tokens.Ptwebqq = client.Get(Api.GetPtwebqq.Get(ptwebqqUrl))
                .Cookies
                .First(x => x.Name == @"ptwebqq")
                .Value;

            // get vfwebqq
            VfwebqqReceipt? vfwebqqReceipt = null;
            while (maxAttempts > 0)
            {
                var response = client.Get<VfwebqqReceipt>(Api.GetVfwebqq.Get(tokens.Ptwebqq));
                if (response.IsSuccessful)
                {
                    vfwebqqReceipt = response.Data;
                    break;
                }
                maxAttempts--;
            }
            if (vfwebqqReceipt == null)
                throw new HttpRequestException(
                    "QR authentication unsuccessful: maximum attempts reached getting vfwebqq.");
            tokens.Vfwebqq = vfwebqqReceipt.Value.Result.Vfwebqq;

            // get uin & psessionid
            var uinPsessionidReceipt = client.Post<UinPsessionidReceipt>(Api.GetUinAndPsessionid.Post(
                new JsonObject
                {
                    {@"ptwebqq", tokens.Ptwebqq},
                    {@"clientid", Miscellaneous.ClientId},
                    {@"psessionid", @""},
                    {@"status", @"online"}
                })).Data;
            tokens.Uin = uinPsessionidReceipt.Result.Uin;
            tokens.Psessionid = uinPsessionidReceipt.Result.Psessionid;

            return (tokens, client.CookieContainer);
        }

        #endregion

        #region properties

        protected enum LazyProperty
        {
            FriendCategories,
            Friends,
            Groups,
            Discussions
        }

        protected readonly LazyProperties Properties;

        public DumbQQClient()
        {
            Properties = new LazyProperties(() =>
            {
                string Hash(long uin, string ptwebqq)
                {
                    var n = new int[4];
                    for (var T = 0; T < ptwebqq.Length; T++)
                        n[T % 4] ^= ptwebqq[T];
                    string[] u = {"EC", "OK"};
                    var v = new long[4];
                    v[0] = ((uin >> 24) & 255) ^ u[0][0];
                    v[1] = ((uin >> 16) & 255) ^ u[0][1];
                    v[2] = ((uin >> 8) & 255) ^ u[1][0];
                    v[3] = (uin & 255) ^ u[1][1];

                    var u1 = new long[8];

                    for (var t = 0; t < 8; t++)
                        u1[t] = t % 2 == 0 ? n[t >> 1] : v[t >> 1];

                    string[] n1 = {"0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F"};
                    var v1 = "";
                    foreach (var aU1 in u1)
                    {
                        v1 += n1[(int) ((aU1 >> 4) & 15)];
                        v1 += n1[(int) (aU1 & 15)];
                    }
                    return v1;
                }

                var hash = Hash((long) Session.tokens.Uin, Session.tokens.Ptwebqq);

                // load friends and their categories
                var friendsReceipt = RestClient.Post<FriendsReceipt>(Api.GetFriendList.Post(new JsonObject
                {
                    {@"vfwebqq", Session.tokens.Vfwebqq},
                    {@"hash", hash}
                })).Data.Result;

                // load groups
                var groupsReceipt = RestClient.Post<GroupsReceipt>(Api.GetGroupList.Post(new JsonObject
                {
                    {@"vfwebqq", Session.tokens.Vfwebqq},
                    {@"hash", hash}
                })).Data.Result;

                // load discussions
                var discussionsReceipt =
                    RestClient.Get<DiscussionsReceipt>(Api.GetDiscussList.Get(Session.tokens.Psessionid,
                        Session.tokens.Vfwebqq)).Data.Result;

                return new Dictionary<int, object>
                {
                    {
                        (int) LazyProperty.FriendCategories,
                        friendsReceipt.CategoryNameList.Prepend(null).ToList().AsReadOnly()
                    },
                    {
                        (int) LazyProperty.Friends,
                        new ReadOnlyDictionary<ulong, Friend>(friendsReceipt.FriendList.Reassemble(x => x.Id, this,
                            friendsReceipt.FriendNameAliasList,
                            friendsReceipt.FriendCategoryIndexList))
                    },
                    {
                        (int) LazyProperty.Groups,
                        new ReadOnlyDictionary<ulong, Group>(groupsReceipt.GroupList.ToDictionary(x => x.Id))
                    },
                    {
                        (int) LazyProperty.Discussions,
                        new ReadOnlyDictionary<ulong, Discussion>(
                            discussionsReceipt.DiscussionList.ToDictionary(x => x.Id))
                    }
                };
            });
        }

        public ReadOnlyCollection<string> FriendCategories => Properties[(int) LazyProperty.FriendCategories];
        public ReadOnlyDictionary<ulong, Friend> Friends => Properties[(int) LazyProperty.Friends];
        public ReadOnlyDictionary<ulong, Group> Groups => Properties[(int) LazyProperty.Groups];
        public ReadOnlyDictionary<ulong, Discussion> Discussions => Properties[(int) LazyProperty.Discussions];

        public void LoadLazyProperties() => Properties.Load();

        public override IEnumerator<Friend> GetEnumerator() => Friends.Values.GetEnumerator();

        #endregion

        #region polling

        public IEnumerable<Message> Poll()
        {
            var response =
                Client.RestClient.Post<PollingReceipt>(Api.GetDiscussInfo.Post(new JsonObject
                {
                    {@"ptwebqq", Session.tokens.Ptwebqq},
                    {@"clientid", Miscellaneous.ClientId},
                    {@"psessionid", Session.tokens.Psessionid},
                    {@"key", @""}
                }));
            if (!response.IsSuccessful)
                throw new HttpRequestException($"HTTP request unsuccessful: status code {response.StatusCode}");

            return response.Data.MessageList;
        }

        #endregion
    }
}