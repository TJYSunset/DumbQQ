using System.Collections.Generic;
using System.Linq;
using DumbQQ.Client;
using DumbQQ.Constants;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DumbQQ.Models
{
    /// <summary>
    /// 好友分组。
    /// </summary>
    public class FriendCategory : IListable
    {
        /// <summary>
        /// 序号。
        /// </summary>
        [JsonProperty("index")]
        public int Index { get; set; }

        /// <summary>
        /// 意义暂不明确。
        /// </summary>
        [JsonProperty("sort")]
        public int Sort { get; set; }

        /// <summary>
        /// 名称。
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// 成员。
        /// </summary>
        [JsonProperty("friends")]
        public List<Friend> Members { get; set; } = new List<Friend>();

        /// <summary>
        /// 用于初始化默认分组。
        /// </summary>
        public static FriendCategory DefaultCategory()
        {
            return new FriendCategory
            {
                Index = 0,
                Sort = 0,
                Name = "我的好友"
            };
        }

        internal static List<FriendCategory> GetList(DumbQQClient client)
        {
            DumbQQClient.Logger.Debug("开始获取好友列表");
            var response = client.Client.Post(ApiUrl.GetFriendList,
                new JObject {{"vfwebqq", client.Vfwebqq}, {"hash", client.Hash}});
            var result = (JObject) client.GetResponseJson(response)["result"];
            //获得好友信息
            var friendDictionary = DumbQQClient.ParseFriendDictionary(result);
            //获得分组
            var categories = result["categories"] as JArray;
            var categoryDictionary = new Dictionary<int, FriendCategory> {{0, DefaultCategory()}};
            for (var i = 0; categories != null && i < categories.Count; i++)
            {
                var category = categories[i].ToObject<FriendCategory>();
                categoryDictionary.Add(category.Index, category);
            }
            var friends = result["friends"] as JArray;
            for (var i = 0; friends != null && i < friends.Count; i++)
            {
                var item = (JObject) friends[i];
                var friend = friendDictionary[item["uin"].Value<long>()];
                categoryDictionary[item["categories"].Value<int>()].Members.Add(friend);
            }
            return categoryDictionary.Select(_ => _.Value).ToList();
        }
    }
}