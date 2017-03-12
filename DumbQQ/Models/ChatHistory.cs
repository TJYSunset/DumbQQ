using System.Collections.Generic;
using DumbQQ.Client;
using DumbQQ.Constants;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DumbQQ.Models
{
    /// <summary>
    ///     最近的会话记录。
    /// </summary>
    public class ChatHistory : IListable
    {
        /// <summary>
        ///     会话记录类型。
        /// </summary>
        public enum HistoryType
        {
            /// <summary>
            ///     好友会话。
            /// </summary>
            Friend = 0,

            /// <summary>
            ///     群会话。
            /// </summary>
            Group = 1,

            /// <summary>
            ///     讨论组会话。
            /// </summary>
            Discussion = 2
        }

        /// <summary>
        ///     历史记录类型。
        /// </summary>
        [JsonProperty("type")]
        public HistoryType Type { get; set; }

        /// <summary>
        ///     来源ID。
        /// </summary>
        [JsonProperty("uin")]
        public long Id { get; set; }

        internal static List<ChatHistory> GetList(DumbQQClient client)
        {
            DumbQQClient.Logger.Debug("开始获取最近聊天记录列表");
            var response = client.Client.Post(ApiUrl.GetChatHistoryList,
                new JObject {{"vfwebqq", client.Vfwebqq}, {"clientid", DumbQQClient.ClientId}, {"psessionid", ""}});
            return
                ((JArray) client.GetResponseJson(response)["result"])
                .ToObject<List<ChatHistory>>();
        }
    }
}