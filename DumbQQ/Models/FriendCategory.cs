using System.Collections.Generic;
using System.Linq;
using DumbQQ.Client;
using DumbQQ.Constants;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DumbQQ.Models
{
    /// <summary>
    ///     好友分组。
    /// </summary>
    public class FriendCategory : IListable
    {
        [JsonIgnore] internal DumbQQClient Client;

        /// <summary>
        ///     序号。
        /// </summary>
        [JsonProperty("index")]
        public int Index { get; set; }

        /// <summary>
        ///     意义暂不明确。
        /// </summary>
        [JsonProperty("sort")]
        public int Sort { get; set; }

        /// <summary>
        ///     名称。
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        ///     成员。
        /// </summary>
        [JsonIgnore]
        public List<Friend> Members => Client.Friends.FindAll(_ => _.CategoryIndex == Index);

        /// <summary>
        ///     用于初始化默认分组。
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
            //获得分组
            var categories = result["categories"] as JArray;
            var categoryDictionary = new Dictionary<int, FriendCategory> {{0, DefaultCategory()}};
            for (var i = 0; categories != null && i < categories.Count; i++)
            {
                var category = categories[i].ToObject<FriendCategory>();
                categoryDictionary.Add(category.Index, category);
            }
            foreach (var category in categoryDictionary.Values)
                category.Client = client;
            return categoryDictionary.Select(_ => _.Value).ToList();
        }
    }
}