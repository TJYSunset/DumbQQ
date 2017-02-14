using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly CacheDictionary<long, DiscussionInfo> _discussionInfoCache;
        private readonly CacheDictionary<long, FriendInfo> _friendInfoCache;
        private readonly CacheDictionary<long, GroupInfo> _groupInfoCache;
        private readonly Cache<FriendInfo> _myInfoCache;
        private readonly CacheDictionary<long, long> _qqNumberCache;

        internal readonly HttpClient Client = new HttpClient();

        private bool _extraLoginNeededRaised;

        // 临时变量
        private string _lastQrCodePath;

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
            _friendInfoCache = new CacheDictionary<long, FriendInfo>(CacheTimeout);
            _groupInfoCache = new CacheDictionary<long, GroupInfo>(CacheTimeout);
            _discussionInfoCache = new CacheDictionary<long, DiscussionInfo>(CacheTimeout);
        }

        /// <summary>
        ///     缓存的超时时间。
        /// </summary>
        public TimeSpan CacheTimeout { get; set; } = TimeSpan.FromHours(2);

        /// <summary>
        ///     发送消息的重试次数。
        /// </summary>
        public int RetryTimes { get; set; } = 5;

        /// <summary>
        ///     客户端当前状态。
        /// </summary>
        public ClientStatus Status { get; private set; } = ClientStatus.Idle;

        /// <summary>
        ///     当二维码下载完毕时被引发。参数为二维码的绝对路径。
        /// </summary>
        public event EventHandler<string> QrCodeDownloaded;

        /// <summary>
        ///     当二维码失效时被引发。此时需要重新调用Start()。参数为旧二维码的绝对路径。
        /// </summary>
        public event EventHandler<string> QrCodeExpired;

        /// <summary>
        ///     当登录失败时被引发。二维码失效的情况不包括在内。
        /// </summary>
        public event EventHandler<Exception> LoginFailed;

        /// <summary>
        ///     当需要在浏览器中手动登录时被引发。参数为登录网址。
        /// </summary>
        [Obsolete]
        public event EventHandler<string> ExtraLoginNeeded;

        /// <summary>
        ///     登录完成后被引发。
        /// </summary>
        public event EventHandler LoginCompleted;

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
        ///     获取详细信息。
        /// </summary>
        /// <typeparam name="TInfo">详细信息的类型。</typeparam>
        /// <param name="id">用于查询详细信息的编号。</param>
        /// <param name="forceRefresh">指定是否要强制重新获取而不使用缓存。</param>
        /// <returns>详细信息。</returns>
        public TInfo GetInfoOfType<TInfo>(long id, bool forceRefresh = false) where TInfo : class, IInfo
        {
            TInfo result = null;
            if (!forceRefresh)
            {
                switch (typeof(TInfo).Name)
                {
                    case "FriendInfo":
                        if (_friendInfoCache.ContainsKey(id))
                            result = (TInfo) (object) _friendInfoCache[id];
                        break;
                    case "GroupInfo":
                        if (_groupInfoCache.ContainsKey(id))
                            result = (TInfo) (object) _groupInfoCache[id];
                        break;
                    case "DiscussionInfo":
                        if (_discussionInfoCache.ContainsKey(id))
                            result = (TInfo) (object) _discussionInfoCache[id];
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                if (result != null) return result;
            }
            // 为了性能不使用reflection
            switch (typeof(TInfo).Name)
            {
                case "FriendInfo":
                    result = (TInfo) (object) Friend.GetInfo(this, id);
                    _friendInfoCache.Put(id, (FriendInfo) (object) result);
                    break;
                case "GroupInfo":
                    result = (TInfo) (object) Group.GetInfo(this, id);
                    _groupInfoCache.Put(id, (GroupInfo) (object) result);
                    break;
                case "DiscussionInfo":
                    result = (TInfo) (object) Discussion.GetInfo(this, id);
                    _discussionInfoCache.Put(id, (DiscussionInfo) (object) result);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return result;
        }

        /// <summary>
        ///     获取好友的详细信息。
        /// </summary>
        /// <param name="friend">好友。</param>
        /// <param name="forceRefresh">指定是否要强制重新获取而不使用缓存。</param>
        /// <returns>详细信息。</returns>
        public FriendInfo GetInfoAbout(Friend friend, bool forceRefresh = false)
            => GetInfoOfType<FriendInfo>(friend.Id, forceRefresh);

        /// <summary>
        ///     获取群的详细信息。
        /// </summary>
        /// <param name="group">群。</param>
        /// <param name="forceRefresh">指定是否要强制重新获取而不使用缓存。</param>
        /// <returns>详细信息。</returns>
        public GroupInfo GetInfoAbout(Group group, bool forceRefresh = false)
            => GetInfoOfType<GroupInfo>(group.Code, forceRefresh);

        /// <summary>
        ///     获取讨论组的详细信息。
        /// </summary>
        /// <param name="discussion">讨论组。</param>
        /// <param name="forceRefresh">指定是否要强制重新获取而不使用缓存。</param>
        /// <returns>详细信息。</returns>
        public DiscussionInfo GetInfoAbout(Discussion discussion, bool forceRefresh = false)
            => GetInfoOfType<DiscussionInfo>(discussion.Id, forceRefresh);

        /// <summary>
        ///     获取当前登录账户的详细信息。
        /// </summary>
        /// <param name="forceRefresh">指定是否要强制重新获取而不使用缓存。</param>
        /// <returns>详细信息。</returns>
        public FriendInfo GetInfoAboutMe(bool forceRefresh = false)
        {
            if (!forceRefresh)
            {
                FriendInfo cachedInfo;
                if (_myInfoCache.TryGetValue(out cachedInfo))
                    return cachedInfo;
            }
            Logger.Debug("开始获取登录账户信息");

            var response = Client.Get(ApiUrl.GetAccountInfo);
            var info = ((JObject) GetResponseJson(response)["result"]).ToObject<FriendInfo>();
            _myInfoCache.SetValue(info);
            return info;
        }

        /// <summary>
        ///     查询列表。
        /// </summary>
        /// <param name="forceRefresh">指定是否要强制重新获取而不使用缓存。</param>
        /// <returns></returns>
        public List<T> GetListOf<T>(bool forceRefresh = false) where T : class, IListable
        {
            if (!forceRefresh)
            {
                List<T> tempData;
                if (_cache.GetCache<List<T>>().TryGetValue(out tempData))
                {
                    Logger.Debug("加载了缓存的" + typeof(T).Name + "列表");
                    return tempData;
                }
            }
//            try
//            {
//                var result =
//                    (List<T>)
//                    typeof(T).GetMethod(@"GetList", BindingFlags.NonPublic | BindingFlags.Static)
//                        .Invoke(null, new object[] {this});
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
//            }
//            catch (TargetInvocationException ex)
//            {
//                if (ex.InnerException != null) throw ex.InnerException;
//                throw;
//            }
        }

        /// <summary>
        ///     根据ID获取QQ号。
        /// </summary>
        /// <param name="userId">用户ID。</param>
        /// <param name="forceRefresh">指定是否要强制重新获取而不使用缓存。</param>
        /// <returns>QQ号。</returns>
        public long GetQQNumberOf(long userId, bool forceRefresh = false)
        {
            Logger.Debug("开始获取QQ号");

            if (!forceRefresh)
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
        ///     根据消息获取发送者QQ号。
        /// </summary>
        /// <param name="message">消息。</param>
        /// <param name="forceRefresh">指定是否要强制重新获取而不使用缓存。</param>
        /// <returns>QQ号。</returns>
        public long GetQQNumberOf(IMessage message, bool forceRefresh = false)
            => GetQQNumberOf(message.UserId, forceRefresh);

        /// <summary>
        ///     获取用户QQ号。
        /// </summary>
        /// <param name="user">用户</param>
        /// <param name="forceRefresh">指定是否要强制重新获取而不使用缓存。</param>
        /// <returns>QQ号。</returns>
        public long GetQQNumberOf(IUser user, bool forceRefresh = false) => GetQQNumberOf(user.Id, forceRefresh);

        /// <summary>
        ///     发送消息。
        /// </summary>
        /// <param name="type">目标类型。</param>
        /// <param name="id">用于发送的ID。</param>
        /// <param name="content">消息内容。</param>
        public void Message(TargetType type, long id, string content)
        {
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
            if (status != null && status == 0)
            {
                Logger.Debug("消息发送成功");
            }
            else
            {
                if (status != 100100)
                    Logger.Error("消息发送失败，API返回码" + status);
            }
        }

        /// <summary>
        ///     发送消息。
        /// </summary>
        /// <param name="target">目标。</param>
        /// <param name="content">消息内容。</param>
        public void Message(IMessageable target, string content) => Message(target.TargetType, target.Id, content);

        /// <summary>
        ///     回复消息。
        /// </summary>
        /// <param name="message">原消息。</param>
        /// <param name="content">回复内容。</param>
        public void ReplyTo(IMessage message, string content) =>
            Message(message.Type, message.RepliableId, content);

        /// <summary>
        ///     异步地连接到SmartQQ。
        /// </summary>
        public async void StartAsync()
        {
            await Task.Run(() => Login());
            Status = ClientStatus.Active;
            LoginCompleted?.Invoke(this, EventArgs.Empty);
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
                    catch (HttpRequestException e)
                    {
                        if (!(e.InnerException is HttpException) ||
                            ((HttpException) e.InnerException).StatusCode != HttpStatusCode.GatewayTimeout)
                            Logger.Error(e);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                }
            }).Start();
        }

        /// <summary>
        ///     连接到SmartQQ。
        /// </summary>
        public void Start()
        {
            Login();
            Status = ClientStatus.Active;
            LoginCompleted?.Invoke(this, EventArgs.Empty);
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
                    catch (HttpRequestException e)
                    {
                        if (!(e.InnerException is HttpException) ||
                            ((HttpException) e.InnerException).StatusCode != HttpStatusCode.GatewayTimeout)
                            Logger.Error(e);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                }
            }).Start();
        }

        // 登录
        private void Login()
        {
            try
            {
                Status = ClientStatus.LoggingIn;
                GetQrCode();
                var url = VerifyQrCode();
                GetPtwebqq(url);
                GetVfwebqq();
                GetUinAndPsessionid();
                TestLogin();
                Hash = StringHelper.SomewhatHash(Uin, Ptwebqq);
            }
            catch (TimeoutException)
            {
                Status = ClientStatus.Idle;
                QrCodeExpired?.Invoke(this, _lastQrCodePath);
                throw;
            }
            catch (Exception ex)
            {
                Status = ClientStatus.Idle;
                Logger.Error("登录失败，抛出异常：" + ex);
                LoginFailed?.Invoke(this, ex);
                throw;
            }
        }

        // 获取二维码
        private void GetQrCode()
        {
            Logger.Debug("开始获取二维码");
            var filePath = Path.GetFullPath("qrcode" + RandomHelper.GetRandomInt() + ".png");

            var response = Client.GetAsFile(ApiUrl.GetQrCode.Url, filePath);
            foreach (Cookie cookie in response.Cookies)
            {
                if (cookie.Name != "qrsig") continue;
                _qrsig = cookie.Value;
                break;
            }
            Logger.Info("二维码已保存在 " + filePath + " 文件中，请打开手机QQ并扫描二维码");
            _lastQrCodePath = filePath;
            QrCodeDownloaded?.Invoke(this, filePath);
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

        private void TestLogin()
        {
            Logger.Debug("开始向服务器发送测试连接请求");

            Client.Get(ApiUrl.TestLogin, Vfwebqq, ClientId, Psessionid, RandomHelper.GetRandomDouble());
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
                var message = array[i] as JObject;
                // ReSharper disable once PossibleNullReferenceException
                var type = message["poll_type"].Value<string>();
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (type)
                {
                    case "message":
                        FriendMessageReceived?.Invoke(this, message["value"].ToObject<FriendMessage>());
                        break;
                    case "group_message":
                        GroupMessageReceived?.Invoke(this, message["value"].ToObject<GroupMessage>());
                        break;
                    case "discu_message":
                        DiscussionMessageReceived?.Invoke(this, message["value"].ToObject<DiscussionMessage>());
                        break;
                }
            }
        }

        /// <summary>
        ///     停止通讯。
        /// </summary>
        public void Close()
        {
            _pollStarted = false;
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
                    if (!_extraLoginNeededRaised)
                    {
                        _extraLoginNeededRaised = true;
#pragma warning disable 612
                        ExtraLoginNeeded?.Invoke(this, @"http://w.qq.com");
#pragma warning restore 612
                    }
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
    }

    public abstract class Cache
    {
        protected readonly TimeSpan Timeout;
        protected readonly Timer Timer;
        protected bool IsValid;

        /// <summary>
        ///     初始化一个缓存对象。
        /// </summary>
        /// <param name="timeout">表示缓存的超时时间。</param>
        protected Cache(TimeSpan timeout)
        {
            Timeout = timeout;
            Timer = new Timer(_ =>
            {
                IsValid = false;
                Value = null;
            }, null, Timeout, System.Threading.Timeout.InfiniteTimeSpan);
        }

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
            Timer.Change(Timeout, System.Threading.Timeout.InfiniteTimeSpan);
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
            Timer.Change(Timeout, System.Threading.Timeout.InfiniteTimeSpan);
        }
    }

    /// <summary>
    ///     放一堆不同类型的东西的缓存的字典。
    /// </summary>
    internal class CacheDepot
    {
        private readonly Dictionary<string, Cache> _dic = new Dictionary<string, Cache>();
        private readonly TimeSpan _timeout;

        public CacheDepot(TimeSpan timeout)
        {
            _timeout = timeout;
        }

        public Cache GetCache<T>() where T : class
        {
            if (!_dic.ContainsKey(typeof(T).FullName))
                _dic.Add(typeof(T).FullName, new Cache<T>(_timeout));
            return _dic[typeof(T).FullName];
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

        public CacheDictionary(TimeSpan timeout)
        {
            _timer = new Timer(_ => Clear(), null, timeout, timeout);
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

        public int ErrorCode { get; }
        public override string Message => "API错误，返回码" + ErrorCode;
    }
}