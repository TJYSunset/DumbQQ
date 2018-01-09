using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using DumbQQ.Constants;
using DumbQQ.Helpers;
using DumbQQ.Models;
using DumbQQ.Models.Abstract;
using DumbQQ.Models.Utilities;
using RestSharp;
using RestSharp.Deserializers;
using SimpleJson;

[assembly: InternalsVisibleTo(@"DumbQQ.Test")]

namespace DumbQQ
{
    public class DumbQQClient : IEnumerable<Friend>, IUseLazyProperty
    {
        #region polling

        public IEnumerable<Message> Poll()
        {
            var response =
                RestClient.Post<PollingResponse>(Api.PollMessage, new JsonObject
                {
                    {@"ptwebqq", Session.tokens.ptwebqq},
                    {@"clientid", Miscellaneous.ClientId},
                    {@"psessionid", Session.tokens.psessionid},
                    {@"key", @""}
                });

            return response.Data.MessageList.Select(x => new Message
            {
                Client = this,
                Type = x.Type,
                Content = string.Join(string.Empty, x.Data.Content.Skip(1)),
                SenderId = x.Data.SenderId,
                SourceId = x.Data.SourceId,
                Timestamp = x.Data.Timestamp
            });
        }

        #endregion

        #region session related

        public RestClient RestClient { get; } =
            new RestClient {UserAgent = Miscellaneous.DefaultUserAgent, CookieContainer = new CookieContainer()};
        // TODO BaseUrl problem

        private (string ptwebqq, string vfwebqq, ulong uin, string psessionid) _tokens;

        public ((string ptwebqq, string vfwebqq, ulong uin, string psessionid) tokens, CookieContainer cookies) Session
        {
            get => (_tokens, RestClient.CookieContainer);
            set
            {
                _tokens = value.tokens;
                RestClient.CookieContainer = value.cookies;

                // some magical extra step
                RestClient.Get(Api.GetFriendStatus, value.tokens.vfwebqq, value.tokens.psessionid);
            }
        }

        public static ((string ptwebqq, string vfwebqq, ulong uin, string psessionid) tokens, CookieContainer cookies)
            QrAuthenticate(Action<byte[]> imageCallback,
                uint maxAttempts = 10)
        {
            int Hash33(string s)
            {
                var e = 0;
                foreach (var t in s)
                    e += (e << 5) + t;
                return int.MaxValue & e;
            }

            var client = new RestClient
            {
                UserAgent = Miscellaneous.DefaultUserAgent,
                CookieContainer = new CookieContainer()
            };
            client.AddHandler(@"text/plain", new JsonDeserializer());

            // download QR code and get the temporary token "qrsig"
            var qrResponse = client.Get(Api.GetQrCode);
            var ptqrtoken = Hash33(qrResponse.Cookies.First(x => x.Name == @"qrsig").Value);
            imageCallback(qrResponse.RawBytes);

            // wait for manual authentication and get the url to next step

            string ptwebqqUrl, ptwebqq;
            while (true)
            {
                Thread.Sleep(1000);
                var message = client.Get(Api.VerifyQrCode, ptqrtoken);
                if (message.Content.Contains(@"已失效")) throw new TimeoutException("QR authentication timed out.");
                if (!message.Content.Contains(@"成功")) continue;
                ptwebqqUrl = Api.GetPtwebqqPattern.Match(message.Content).Value;
                ptwebqq = message.Cookies
                    .First(x => x.Name == @"ptwebqq")
                    .Value;
                break;
            }

            // get ptwebqq (not really)
            client.Get(Api.GetPtwebqq, ptwebqqUrl);

            // get vfwebqq
            VfwebqqResponse vfwebqqResponse = null;
            while (maxAttempts > 0)
            {
                var response = client.Get<VfwebqqResponse>(Api.GetVfwebqq, ptwebqq);
                if (response.IsSuccessful)
                {
                    vfwebqqResponse = response.Data;
                    break;
                }

                maxAttempts--;
            }

            if (vfwebqqResponse == null)
                throw new HttpRequestException(
                    "QR authentication unsuccessful: maximum attempts reached getting vfwebqq.");
            var vfwebqq = vfwebqqResponse.Result.Vfwebqq;

            // get uin & psessionid
            var uinPsessionidResponse = client.Post<UinPsessionidResponse>(Api.GetUinAndPsessionid,
                new JsonObject
                {
                    {@"ptwebqq", ptwebqq},
                    {@"clientid", Miscellaneous.ClientId},
                    {@"psessionid", @""},
                    {@"status", @"online"}
                }).Data;
            var uin = uinPsessionidResponse.Result.Uin;
            var psessionid = uinPsessionidResponse.Result.Psessionid;

            return ((ptwebqq, vfwebqq, uin, psessionid), client.CookieContainer);
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
            RestClient.AddHandler(@"text/plain", new JsonDeserializer());
            Properties = new LazyProperties(() =>
            {
                string Hash(long uin, string ptwebqq)
                {
                    var n = new int[4];
                    for (var T = 0; T < ptwebqq.Length; T++)
                        n[T % 4] ^= ptwebqq[T];
                    string[] u = {@"EC", @"OK"};
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

                var hash = Hash((long) Session.tokens.uin, Session.tokens.ptwebqq);

                // load friends and their categories
                var friendsResponse = RestClient.Post<FriendsResponse>(Api.GetFriendList, new JsonObject
                {
                    {@"vfwebqq", Session.tokens.vfwebqq},
                    {@"hash", hash}
                }).Data.Result;

                // load groups
                var groupsResponse = RestClient.Post<GroupsResponse>(Api.GetGroupList, new JsonObject
                {
                    {@"vfwebqq", Session.tokens.vfwebqq},
                    {@"hash", hash}
                }).Data.Result;

                // load discussions
                var discussionsResponse =
                    RestClient.Get<DiscussionsResponse>(Api.GetDiscussList, Session.tokens.psessionid,
                        Session.tokens.vfwebqq).Data.Result;

                return new Dictionary<int, object>
                {
                    {
                        (int) LazyProperty.FriendCategories,
                        new ReadOnlyDictionary<ulong, FriendCategory>(
                            friendsResponse.CategoryList.Reassemble(x => x.Index, this))
                    },
                    {
                        (int) LazyProperty.Friends,
                        new ReadOnlyDictionary<ulong, Friend>(friendsResponse.FriendList.Reassemble(x => x.Id, this,
                            friendsResponse.FriendNameAliasList,
                            friendsResponse.FriendCategoryIndexList))
                    },
                    {
                        (int) LazyProperty.Groups,
                        new ReadOnlyDictionary<ulong, Group>(groupsResponse.GroupList.Reassemble(x => x.Id, this))
                    },
                    {
                        (int) LazyProperty.Discussions,
                        new ReadOnlyDictionary<ulong, Discussion>(
                            discussionsResponse.DiscussionList.Reassemble(x => x.Id, this))
                    }
                };
            });
        }

        [LazyProperty]
        public ReadOnlyDictionary<ulong, FriendCategory> FriendCategories =>
            Properties[(int) LazyProperty.FriendCategories];

        [LazyProperty] public ReadOnlyDictionary<ulong, Friend> Friends => Properties[(int) LazyProperty.Friends];

        [LazyProperty] public ReadOnlyDictionary<ulong, Group> Groups => Properties[(int) LazyProperty.Groups];

        [LazyProperty]
        public ReadOnlyDictionary<ulong, Discussion> Discussions => Properties[(int) LazyProperty.Discussions];

        public void LoadLazyProperties()
        {
            Properties.Load();
        }

        public IEnumerator<Friend> GetEnumerator()
        {
            return Friends.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}