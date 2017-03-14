using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using DumbQQ.Constants;
using DumbQQ.Models;
using DumbQQ.Utils;
using EasyHttp.Http;
using EasyHttp.Infrastructure;
using log4net;
using log4net.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HttpClient = EasyHttp.Http.HttpClient;

namespace DumbQQ.Client
{
    /// <summary>
    ///     用于连接到SmartQQ的客户端。
    /// </summary>
    public class DumbQQClient
    {
        /// <summary>
        ///     表示客户端状态的枚举。
        /// </summary>
        public enum ClientStatus
        {
            /// <summary>
            ///     客户端并没有连接到SmartQQ。
            /// </summary>
            Idle,

            /// <summary>
            ///     客户端正在登录。
            /// </summary>
            LoggingIn,

            /// <summary>
            ///     客户端已登录到SmartQQ。
            /// </summary>
            Active
        }

        /// <summary>
        ///     登录结果。
        /// </summary>
        public enum LoginResult
        {
            /// <summary>
            ///     登录成功。
            /// </summary>
            Succeeded,

            /// <summary>
            ///     二维码失效。登录失败。
            /// </summary>
            QrCodeExpired,

            /// <summary>
            ///     cookie失效。登录失败。
            /// </summary>
            CookieExpired,

            /// <summary>
            ///     发生了二维码失效和cookie失效以外的错误。
            /// </summary>
            Failed
        }

        /// <summary>
        ///     发送消息的目标类型。
        /// </summary>
        public enum TargetType
        {
            /// <summary>
            ///     好友。
            /// </summary>
            Friend,

            /// <summary>
            ///     群。
            /// </summary>
            Group,

            /// <summary>
            ///     讨论组。
            /// </summary>
            Discussion
        }

        internal const long ClientId = 53999199;
        internal static readonly ILog Logger = LogManager.GetLogger(typeof(DumbQQClient));
        private static long _messageId = 43690001;

        // 数据缓存
        private readonly CacheDepot _cache;
        private readonly Cache<FriendInfo> _myInfoCache;
        private readonly CacheDictionary<long, long> _qqNumberCache;

        internal readonly HttpClient Client = new HttpClient();

        private TimeSpan _cacheTimeout = TimeSpan.FromHours(2);

        // 线程开关
        private volatile bool _pollStarted;

        // 二维码验证参数
        private string _qrsig;
        internal string Hash;
        internal string Psessionid;

        // 鉴权参数
        internal string Ptwebqq;
        internal long Uin;
        internal string Vfwebqq;

        /// <summary>
        ///     初始化一个DumbQQClient。
        /// </summary>
        public DumbQQClient()
        {
            XmlConfigurator.Configure();
            Client.Request.UserAgent = ApiUrl.UserAgent;
            Client.Request.PersistCookies = true;
            Client.Request.KeepAlive = true;
            Client.Request.Accept = null;
            Client.ThrowExceptionOnHttpError = false;
            _cache = new CacheDepot(CacheTimeout);
            _myInfoCache = new Cache<FriendInfo>(CacheTimeout);
            _qqNumberCache = new CacheDictionary<long, long>(CacheTimeout);
        }

        /// <summary>
        ///     已登录账户的最近会话。
        /// </summary>
        public List<ChatHistory> RecentConversations => GetListOf<ChatHistory>();

        /// <summary>
        ///     已登录账户加入的讨论组。
        /// </summary>
        public List<Discussion> Discussions => GetListOf<Discussion>();

        /// <summary>
        ///     已登录账户的好友。
        /// </summary>
        public List<Friend> Friends => GetListOf<Friend>();

        /// <summary>
        ///     已登录账户的好友分组。
        /// </summary>
        public List<FriendCategory> Categories => GetListOf<FriendCategory>();

        /// <summary>
        ///     已登录账户加入的群。
        /// </summary>
        public List<Group> Groups => GetListOf<Group>();

        /// <summary>
        ///     缓存的超时时间。
        /// </summary>
        public TimeSpan CacheTimeout
        {
            get { return _cacheTimeout; }
            set
            {
                _cacheTimeout = value;
                _cache.Timeout = value;
                _myInfoCache.Timeout = value;
                _qqNumberCache.Timeout = value;
            }
        }

        /// <summary>
        ///     发送消息的重试次数。
        /// </summary>
        public int RetryTimes { get; set; } = 5;

        /// <summary>
        ///     客户端当前状态。
        /// </summary>
        public ClientStatus Status { get; private set; } = ClientStatus.Idle;

        private FriendInfo MyInfo
        {
            get
            {
                if (Status != ClientStatus.Active)
                    throw new InvalidOperationException("尚未登录，无法进行该操作");
                FriendInfo cachedInfo;
                if (_myInfoCache.TryGetValue(out cachedInfo))
                    return cachedInfo;
                Logger.Debug("开始获取登录账户信息");

                var response = Client.Get(ApiUrl.GetAccountInfo);
                var info = ((JObject) GetResponseJson(response)["result"]).ToObject<FriendInfo>();
                _myInfoCache.SetValue(info);
                return info;
            }
        }

        /// <summary>
        ///     已登录账户的编号。
        /// </summary>
        public long Id => MyInfo.Id;

        /// <summary>
        ///     已登录账户的QQ号。
        /// </summary>
        public long QQNumber => GetQQNumberOf(Id);

        /// <summary>
        ///     已登录账户的昵称。
        /// </summary>
        public string Nickname => MyInfo.Nickname;

        /// <summary>
        ///     已登录账户的个性签名。
        /// </summary>
        public string Bio => MyInfo.Bio;

        /// <summary>
        ///     已登录账户的生日。
        /// </summary>
        public Birthday Birthday => MyInfo.Birthday;

        /// <summary>
        ///     已登录账户的座机号码。
        /// </summary>
        public string Phone => MyInfo.Phone;

        /// <summary>
        ///     已登录账户的手机号码。
        /// </summary>
        public string Cellphone => MyInfo.Cellphone;

        /// <summary>
        ///     已登录账户的邮箱地址。
        /// </summary>
        public string Email => MyInfo.Email;

        /// <summary>
        ///     已登录账户的职业。
        /// </summary>
        public string Job => MyInfo.Job;

        /// <summary>
        ///     已登录账户的个人主页。
        /// </summary>
        public string Homepage => MyInfo.Homepage;

        /// <summary>
        ///     已登录账户的学校。
        /// </summary>
        public string School => MyInfo.School;

        /// <summary>
        ///     已登录账户的国家。
        /// </summary>
        public string Country => MyInfo.Country;

        /// <summary>
        ///     已登录账户的省份。
        /// </summary>
        public string Province => MyInfo.Province;

        /// <summary>
        ///     已登录账户的城市。
        /// </summary>
        public string City => MyInfo.City;

        /// <summary>
        ///     已登录账户的性别。
        /// </summary>
        public string Gender => MyInfo.Gender;

        /// <summary>
        ///     已登录账户的生肖。
        /// </summary>
        public int Shengxiao => MyInfo.Shengxiao;

        /// <summary>
        ///     已登录账户的某信息字段。意义暂不明确。
        /// </summary>
        public string Personal => MyInfo.Personal;

        /// <summary>
        ///     已登录账户的某信息字段。意义暂不明确。
        /// </summary>
        public int VipInfo => MyInfo.VipInfo;

        /// <summary>
        ///     当需要在浏览器中手动登录时被引发。参数为登录网址。
        /// </summary>
        [Obsolete]
        public event EventHandler<string> ExtraLoginNeeded;

        /// <summary>
        ///     当掉线时被引发。
        /// </summary>
        public event EventHandler ConnectionLost;

        /// <summary>
        ///     接收到好友消息时被引发。
        /// </summary>
        public event EventHandler<FriendMessage> FriendMessageReceived;

        /// <summary>
        ///     接收到群消息时被引发。
        /// </summary>
        public event EventHandler<GroupMessage> GroupMessageReceived;

        /// <summary>
        ///     接收到讨论组消息时被引发。
        /// </summary>
        public event EventHandler<DiscussionMessage> DiscussionMessageReceived;

        /// <summary>
        ///     发出消息时被引发。适合用于在控制台打印发送消息的回显。
        /// </summary>
        public event EventHandler<MessageEchoEventArgs> MessageEcho;

        /// <summary>
        ///     查询列表。
        /// </summary>
        /// <returns></returns>
        internal List<T> GetListOf<T>() where T : class, IListable
        {
            if (Status != ClientStatus.Active)
                throw new InvalidOperationException("尚未登录，无法进行该操作");
            List<T> tempData;
            if (_cache.GetCache<List<T>>().TryGetValue(out tempData))
            {
                Logger.Debug("加载了缓存的" + typeof(T).Name + "列表");
                return tempData;
            }
            // 为了性能所以不使用reflection而采用硬编码
            List<T> result;
            switch (typeof(T).Name)
            {
                case "ChatHistory":
                    result = (List<T>) (object) ChatHistory.GetList(this);
                    break;
                case "Discussion":
                    result = (List<T>) (object) Discussion.GetList(this);
                    break;
                case "Friend":
                    result = (List<T>) (object) Friend.GetList(this);
                    break;
                case "FriendCategory":
                    result = (List<T>) (object) FriendCategory.GetList(this);
                    break;
                case "FriendStatus":
                    result = (List<T>) (object) FriendStatus.GetList(this);
                    break;
                case "Group":
                    result = (List<T>) (object) Group.GetList(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            _cache.GetCache<List<T>>().SetValue(result);
            return result;
        }

        /// <summary>
        ///     根据ID获取QQ号。
        /// </summary>
        /// <param name="userId">用户ID。</param>
        /// <returns>QQ号。</returns>
        public long GetQQNumberOf(long userId)
        {
            if (Status != ClientStatus.Active)
                throw new InvalidOperationException("尚未登录，无法进行该操作");
            Logger.Debug("开始获取QQ号");

            if (_qqNumberCache.ContainsKey(userId))
            {
                Logger.Debug("加载了缓存的QQ号");
                return _qqNumberCache[userId];
            }

            var qq =
            ((JObject)
                JObject.Parse(Client.Get(ApiUrl.GetQQById, userId, Vfwebqq, RandomHelper.GetRandomDouble()).RawText)[
                    "result"])["account"].Value<long>();
            _qqNumberCache.Put(userId, qq);
            return qq;
        }

        /// <summary>
        ///     发送消息。
        /// </summary>
        /// <param name="type">目标类型。</param>
        /// <param name="id">用于发送的ID。</param>
        /// <param name="content">消息内容。</param>
        public void Message(TargetType type, long id, string content)
        {
            if (Status != ClientStatus.Active)
                throw new InvalidOperationException("尚未登录，无法进行该操作");
            Logger.Debug("开始发送消息，对象类型：" + type);

            string paramName;
            ApiUrl url;

            switch (type)
            {
                case TargetType.Friend:
                    paramName = "to";
                    url = ApiUrl.SendMessageToFriend;
                    break;
                case TargetType.Group:
                    paramName = "group_uin";
                    url = ApiUrl.SendMessageToGroup;
                    break;
                case TargetType.Discussion:
                    paramName = "did";
                    url = ApiUrl.SendMessageToDiscussion;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            var response = Client.PostWithRetry(url, new JObject
            {
                {paramName, id},
                {
                    "content",
                    new JArray
                        {
                            StringHelper.TranslateEmoticons(content),
                            new JArray {"font", JObject.FromObject(Font.DefaultFont)}
                        }
                        .ToString(Formatting.None)
                },
                {"face", 573},
                {"clientid", ClientId},
                {"msg_id", _messageId++},
                {"psessionid", Psessionid}
            }, RetryTimes);

            if (response.StatusCode != HttpStatusCode.OK)
                Logger.Error("消息发送失败，HTTP返回码" + (int) response.StatusCode);

            var status = JObject.Parse(response.RawText)["retcode"].ToObject<int?>();
            if (status != null && (status == 0 || status == 100100))
            {
                Logger.Debug("消息发送成功");
                if (MessageEcho == null) return;
                MessageEchoEventArgs args;
                switch (type)
                {
                    case TargetType.Friend:
                    {
                        args = new MessageEchoEventArgs(Friends.Find(_ => _.Id == id), content);
                        break;
                    }
                    case TargetType.Group:
                    {
                        args = new MessageEchoEventArgs(Groups.Find(_ => _.Id == id), content);
                        break;
                    }
                    case TargetType.Discussion:
                    {
                        args = new MessageEchoEventArgs(Discussions.Find(_ => _.Id == id), content);
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
                MessageEcho(this, args);
            }
            else
            {
                Logger.Error("消息发送失败，API返回码" + status);
            }
        }

        /// <summary>
        ///     导出当前cookie集合。
        /// </summary>
        /// <returns>当前cookie集合的JSON字符串。</returns>
        public string DumpCookies()
        {
            if (Status != ClientStatus.Active)
                throw new InvalidOperationException("仅在登录后才能导出cookie");
            var cookieContainer = Client.Request.GetType()
                .GetField(@"cookieContainer", BindingFlags.NonPublic | BindingFlags.Instance);
            if (cookieContainer == null)
                throw new NotImplementedException("无法读取Cookie，可能是因为EasyHttp更改了HttpRequest类的内部结构；请Issue汇报该问题。");
            return new JObject
            {
                {"hash", Hash},
                {"psessionid", Psessionid},
                {"ptwebqq", Ptwebqq},
                {"uin", Uin},
                {"vfwebqq", Vfwebqq},
                {
                    "cookies",
                    JArray.FromObject(((CookieContainer) cookieContainer.GetValue(Client.Request)).GetAllCookies())
                }
            }.ToString(Formatting.None);
        }

        /// <summary>
        ///     使用cookie连接到SmartQQ。
        /// </summary>
        /// <param name="json">由DumpCookies()导出的JSON字符串。</param>
        public LoginResult Start(string json)
        {
            if (Status != ClientStatus.Idle)
                throw new InvalidOperationException("已在登录或者已经登录，不能重复进行登录操作");
            try
            {
                Logger.Debug("开始通过cookie登录");
                Status = ClientStatus.LoggingIn;
                var dump = JObject.Parse(json);
                Hash = dump["hash"].Value<string>();
                Psessionid = dump["psessionid"].Value<string>();
                Ptwebqq = dump["ptwebqq"].Value<string>();
                Uin = dump["uin"].Value<long>();
                Vfwebqq = dump["vfwebqq"].Value<string>();
                var cookieContainerField = Client.Request.GetType()
                    .GetField(@"cookieContainer", BindingFlags.NonPublic | BindingFlags.Instance);
                if (cookieContainerField == null)
                    throw new NotImplementedException("无法写入Cookie，可能是因为EasyHttp更改了HttpRequest类的内部结构；请Issue汇报该问题。");
                var cookies = new CookieContainer();
                foreach (var cookie in dump["cookies"].Value<JArray>().ToObject<List<Cookie>>())
                    cookies.Add(cookie);
                cookieContainerField.SetValue(Client.Request, cookies);

                if (TestLogin())
                {
                    Status = ClientStatus.Active;
                    StartMessageLoop();
                    return LoginResult.Succeeded;
                }
                Status = ClientStatus.Idle;
                return LoginResult.CookieExpired;
            }
            catch (Exception ex)
            {
                Status = ClientStatus.Idle;
                Logger.Error("登录失败，抛出异常：" + ex);
                return LoginResult.Failed;
            }
        }

        /// <summary>
        ///     连接到SmartQQ。
        /// </summary>
        /// <param name="qrCodeDownloadedCallback">二维码已下载时的回调函数。回调函数的参数为二维码图像的字节数组。</param>
        public LoginResult Start(Action<byte[]> qrCodeDownloadedCallback)
        {
            if (Status != ClientStatus.Idle)
                throw new InvalidOperationException("已在登录或者已经登录，不能重复进行登录操作");
            var result = Login(qrCodeDownloadedCallback);
            if (result != LoginResult.Succeeded)
            {
                Status = ClientStatus.Idle;
                return result;
            }
            Status = ClientStatus.Active;
            StartMessageLoop();
            return result;
        }

        /// <summary>
        ///     连接到SmartQQ。
        /// </summary>
        /// <param name="qrCodeDownloadedCallback">二维码已下载时的回调函数。回调函数的参数为已下载的二维码的绝对路径。</param>
        [Obsolete("此方法已不赞成使用，并可能在未来版本中移除。请考虑改为使用Start(Action<byte[]>)。")]
        public LoginResult Start(Action<string> qrCodeDownloadedCallback) => Start(_ =>
        {
            var filePath = Path.GetFullPath("qrcode" + RandomHelper.GetRandomInt() + ".png");
            File.WriteAllBytes(filePath, _);
            qrCodeDownloadedCallback(filePath);
        });

        // 登录
        private LoginResult Login(Action<byte[]> qrCodeDownloadedCallback)
        {
            try
            {
                Status = ClientStatus.LoggingIn;
                GetQrCode(qrCodeDownloadedCallback);
                var url = VerifyQrCode();
                GetPtwebqq(url);
                GetVfwebqq();
                GetUinAndPsessionid();
                if (!TestLogin())
#pragma warning disable 612
                    ExtraLoginNeeded?.Invoke(this, @"http://w.qq.com");
#pragma warning restore 612
                Hash = StringHelper.SomewhatHash(Uin, Ptwebqq);
                return LoginResult.Succeeded;
            }
            catch (TimeoutException)
            {
                return LoginResult.QrCodeExpired;
            }
            catch (Exception ex)
            {
                Logger.Error("登录失败，抛出异常：" + ex);
                return LoginResult.Failed;
            }
        }

        // 获取二维码
        private void GetQrCode(Action<byte[]> qrCodeDownloadedCallback)
        {
            Logger.Debug("开始获取二维码");

            Client.StreamResponse = true;
            var response = Client.Get(ApiUrl.GetQrCode.Url);
            foreach (Cookie cookie in response.Cookies)
            {
                if (cookie.Name != "qrsig") continue;
                _qrsig = cookie.Value;
                break;
            }
            Client.StreamResponse = false;
            Logger.Info("二维码已获取");

            qrCodeDownloadedCallback.Invoke(response.ResponseStream.ToBytes());
        }

        private static int Hash33(string s)
        {
            int e = 0, i = 0, n = s.Length;
            for (; n > i; ++i)
                e += (e << 5) + s[i];
            return 2147483647 & e;
        }

        // 校验二维码
        private string VerifyQrCode()
        {
            Logger.Debug("等待扫描二维码");

            // 阻塞直到确认二维码认证成功
            while (true)
            {
                Thread.Sleep(1000);
                var response = Client.Get(ApiUrl.VerifyQrCode, Hash33(_qrsig));
                var result = response.RawText;
                if (result.Contains("成功"))
                {
                    var cookie = response.Cookies["ptwebqq"];
                    if (cookie != null) Ptwebqq = cookie.Value;
                    else throw new InvalidOperationException();
                    foreach (var content in result.Split(new[] {"','"}, StringSplitOptions.None))
                    {
                        if (!content.StartsWith("http")) continue;
                        Logger.Info("正在登录，请稍后");
                        return content;
                    }
                }
                else if (result.Contains("已失效"))
                {
                    Logger.Warn("二维码已失效，终止登录流程");
                    throw new TimeoutException();
                }
            }
        }

        // 获取ptwebqq
        private void GetPtwebqq(string url)
        {
            Logger.Debug("开始获取ptwebqq");
            Client.Get(ApiUrl.GetPtwebqq, url);
        }

        // 获取vfwebqq
        private void GetVfwebqq()
        {
            Logger.Debug("开始获取vfwebqq");

            var response = Client.Get(ApiUrl.GetVfwebqq, Ptwebqq);
            Vfwebqq = ((JObject) GetResponseJson(response)["result"])["vfwebqq"].Value<string>();
        }

        // 获取uin和psessionid
        private void GetUinAndPsessionid()
        {
            Logger.Debug("开始获取uin和psessionid");

            var r = new JObject
            {
                {"ptwebqq", Ptwebqq},
                {"clientid", ClientId},
                {"psessionid", ""},
                {"status", "online"}
            };

            var response = Client.Post(ApiUrl.GetUinAndPsessionid, r);
            var result = (JObject) GetResponseJson(response)["result"];
            Psessionid = result["psessionid"].Value<string>();
            Uin = result["uin"].Value<long>();
        }

        // 解决103错误码
        private bool TestLogin()
        {
            Logger.Debug("开始向服务器发送测试连接请求");

            var result = Client.Get(ApiUrl.TestLogin, Vfwebqq, ClientId, Psessionid, RandomHelper.GetRandomDouble());
            return result.StatusCode == HttpStatusCode.OK &&
                   JObject.Parse(result.RawText)["retcode"].Value<int?>() == 0;
        }

        // 开始消息轮询
        private void StartMessageLoop()
        {
            _pollStarted = true;
            new Thread(() =>
            {
                while (true)
                {
                    if (!_pollStarted) return;
                    if (
                        FriendMessageReceived == null && GroupMessageReceived == null &&
                        DiscussionMessageReceived == null) continue;
                    try
                    {
                        PollMessage();
                    }
                    catch (Exception ex)
                    {
                        if (!(ex is HttpRequestException) || !(ex.InnerException is HttpException) ||
                            ((HttpException) ex.InnerException).StatusCode != HttpStatusCode.GatewayTimeout)
                            Logger.Error(ex);
                        // 自动掉线
                        if (TestLogin()) continue;
                        Close();
                        ConnectionLost?.Invoke(this, EventArgs.Empty);
                    }
                }
            }) {IsBackground = true}.Start();
        }

        // 拉取消息
        private void PollMessage()
        {
            Logger.Debug(DateTime.Now.ToLongTimeString() + " 开始接收消息");

            var r = new JObject
            {
                {"ptwebqq", Ptwebqq},
                {"clientid", ClientId},
                {"psessionid", Psessionid},
                {"key", ""}
            };

            var response = Client.Post(ApiUrl.PollMessage, r, 120000);
            var array = GetResponseJson(response)["result"] as JArray;
            for (var i = 0; array != null && i < array.Count; i++)
            {
                var message = (JObject) array[i];
                var type = message["poll_type"].Value<string>();
                switch (type)
                {
                    case "message":
                        var fmsg = message["value"].ToObject<FriendMessage>();
                        fmsg.Client = this;
                        FriendMessageReceived?.Invoke(this, fmsg);
                        break;
                    case "group_message":
                        var gmsg = message["value"].ToObject<GroupMessage>();
                        gmsg.Client = this;
                        GroupMessageReceived?.Invoke(this, gmsg);
                        break;
                    case "discu_message":
                        var dmsg = message["value"].ToObject<DiscussionMessage>();
                        dmsg.Client = this;
                        DiscussionMessageReceived?.Invoke(this, dmsg);
                        break;
                    default:
                        Logger.Warn("意外的消息类型：" + type);
                        break;
                }
            }
        }

        /// <summary>
        ///     停止通讯。
        /// </summary>
        public void Close()
        {
            if (Status == ClientStatus.Idle)
                throw new InvalidOperationException("尚未登录，无法进行该操作");
            _pollStarted = false;
            // 清除缓存
            _cache.Clear();
            _myInfoCache.Clear();
            _qqNumberCache.Clear();
        }

        internal JObject GetResponseJson(HttpResponse response)
        {
            if (response.StatusCode != HttpStatusCode.OK)
                throw new HttpRequestException("请求失败，Http返回码" + (int) response.StatusCode + "(" + response.StatusCode +
                                               ")", new HttpException(HttpStatusCode.GatewayTimeout, "Gateway Timeout"));
            var json = JObject.Parse(response.RawText);
            var retCode = json["retcode"].Value<int?>();
            switch (retCode)
            {
                case 0:
                    return json;
                case null:
                    throw new HttpRequestException("请求失败，API未返回状态码");
                case 103:
                    Logger.Error("请求失败，API返回码103；可能需要进一步登录。");
                    break;
                default:
                    throw new HttpRequestException("请求失败，API返回码" + retCode, new ApiException((int) retCode));
            }
            return json;
        }

        internal static Dictionary<long, Friend> ParseFriendDictionary(JObject result)
        {
            var friends = new Dictionary<long, Friend>();
            var info = result["info"] as JArray;
            for (var i = 0; info != null && i < info.Count; i++)
            {
                var x = (JObject) info[i];
                var friend = new Friend
                {
                    Id = x["uin"].Value<long>(),
                    Nickname = x["nick"].Value<string>()
                };
                friends.Add(friend.Id, friend);
            }
            var marknames = result["marknames"] as JArray;
            for (var i = 0; marknames != null && i < marknames.Count; i++)
            {
                var item = (JObject) marknames[i];
                friends[item["uin"].ToObject<long>()].Alias = item["markname"].ToObject<string>();
            }
            var vipinfo = result["vipinfo"] as JArray;
            for (var i = 0; vipinfo != null && i < vipinfo.Count; i++)
            {
                var item = (JObject) vipinfo[i];
                var friend = friends[item["u"].Value<long>()];
                friend.IsVip = item["is_vip"].Value<int>() == 1;
                friend.VipLevel = item["vip_level"].Value<int>();
            }
            return friends;
        }

        /// <summary>
        ///     消息回显事件参数。
        /// </summary>
        public class MessageEchoEventArgs : EventArgs
        {
            internal MessageEchoEventArgs(IMessageable target, string content)
            {
                Target = target;
                Content = content;
            }

            /// <summary>
            ///     消息目标。
            /// </summary>
            public IMessageable Target { get; }

            /// <summary>
            ///     消息内容。
            /// </summary>
            public string Content { get; }
        }
    }

    internal abstract class Cache
    {
        protected readonly Timer Timer;
        protected bool IsValid;

        /// <summary>
        ///     初始化一个缓存对象。
        /// </summary>
        /// <param name="timeout">表示缓存的超时时间。</param>
        protected Cache(TimeSpan timeout)
        {
            Timeout = timeout;
            Timer = new Timer(_ => Clear(), null, Timeout, System.Threading.Timeout.InfiniteTimeSpan);
        }

        public TimeSpan Timeout { get; set; }

        protected object Value { get; set; }

        /// <summary>
        ///     尝试取得缓存的值。
        /// </summary>
        /// <param name="target">值的赋值目标。</param>
        /// <returns>值是否有效。</returns>
        public bool TryGetValue<T>(out T target)
        {
            target = (T) Value;
            return IsValid;
        }

        /// <summary>
        ///     设置缓存的值并重置过期计时器。
        /// </summary>
        /// <param name="target">值</param>
        public void SetValue(object target)
        {
            Value = target;
            IsValid = true;
            Timer.Change(Timeout, System.Threading.Timeout.InfiniteTimeSpan);
        }

        public void Clear()
        {
            IsValid = false;
            Value = null;
        }
    }

    /// <summary>
    ///     缓存数据。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class Cache<T> : Cache where T : class
    {
        /// <summary>
        ///     初始化一个缓存对象。
        /// </summary>
        /// <param name="timeout">表示缓存的超时时间。</param>
        public Cache(TimeSpan timeout) : base(timeout)
        {
        }

        /// <summary>
        ///     尝试取得缓存的值。
        /// </summary>
        /// <param name="target">值的赋值目标。</param>
        /// <returns>值是否有效。</returns>
        public bool TryGetValue(out T target)
        {
            target = Value as T;
            return IsValid;
        }

        /// <summary>
        ///     设置缓存的值并重置过期计时器。
        /// </summary>
        /// <param name="target">值</param>
        public void SetValue(T target)
        {
            Value = target;
            IsValid = true;
            Timer.Change(Timeout, System.Threading.Timeout.InfiniteTimeSpan);
        }
    }

    /// <summary>
    ///     放一堆不同类型的东西的缓存的字典。
    /// </summary>
    internal class CacheDepot
    {
        private readonly Dictionary<string, Cache> _dic = new Dictionary<string, Cache>();

        public CacheDepot(TimeSpan timeout)
        {
            Timeout = timeout;
        }

        public TimeSpan Timeout { get; set; }

        public Cache GetCache<T>() where T : class
        {
            if (!_dic.ContainsKey(typeof(T).FullName))
                _dic.Add(typeof(T).FullName, new Cache<T>(Timeout));
            return _dic[typeof(T).FullName];
        }

        public void Clear()
        {
            _dic.Clear();
        }
    }

    /// <summary>
    ///     缓存词典（会定时清空内容）。
    /// </summary>
    /// <typeparam name="TKey">键的类型。</typeparam>
    /// <typeparam name="TValue">值的类型。</typeparam>
    internal class CacheDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        // ReSharper disable once NotAccessedField.Local
        private readonly Timer _timer;

        private TimeSpan _timeout;

        public CacheDictionary(TimeSpan timeout)
        {
            _timeout = timeout;
            _timer = new Timer(_ => Clear(), null, timeout, timeout);
        }

        public TimeSpan Timeout
        {
            get { return _timeout; }
            set
            {
                _timeout = value;
                _timer.Change(value, value);
            }
        }
    }

    /// <summary>
    ///     因API错误产生的异常。
    /// </summary>
    public class ApiException : Exception
    {
        /// <summary>
        ///     声明一个API异常。
        /// </summary>
        /// <param name="errorCode"></param>
        public ApiException(int errorCode)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        ///     返回的错误码。
        /// </summary>
        public int ErrorCode { get; }

        /// <inheritdoc />
        public override string Message => "API错误，返回码" + ErrorCode;
    }
}