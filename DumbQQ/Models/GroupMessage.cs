using DumbQQ.Client;
using DumbQQ.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DumbQQ.Models
{
    /// <summary>
    ///     群消息。
    /// </summary>
    public class GroupMessage : IMessage
    {
        [JsonIgnore] private readonly LazyHelper<Group> _group = new LazyHelper<Group>();

        [JsonIgnore] private readonly LazyHelper<bool> _mentionedMe = new LazyHelper<bool>();

        [JsonIgnore] private readonly LazyHelper<GroupMember> _sender = new LazyHelper<GroupMember>();

        [JsonIgnore] private readonly LazyHelper<bool> _strictlyMentionedMe = new LazyHelper<bool>();
        [JsonIgnore] internal DumbQQClient Client;

        /// <summary>
        ///     群ID。
        /// </summary>
        [JsonProperty("group_code")]
        internal long GroupId { get; set; }

        /// <summary>
        ///     消息来源群。
        /// </summary>
        [JsonIgnore]
        public Group Group => _group.GetValue(() => Client.Groups.Find(_ => _.Id == GroupId));

        /// <summary>
        ///     字体。
        /// </summary>
        [JsonIgnore]
        internal Font Font { get; set; }

        /// <summary>
        ///     用于parse消息和字体的对象。
        /// </summary>
        [JsonProperty("content")]
        internal JArray ContentAndFont
        {
            set
            {
                Font = ((JArray) value.First).Last.ToObject<Font>();
                value.RemoveAt(0);
                foreach (var shit in value)
                    Content += StringHelper.ParseEmoticons(shit);
            }
        }

        /// <summary>
        ///     发送者ID。
        /// </summary>
        [JsonProperty("send_uin")]
        internal long SenderId { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public GroupMember Sender => _sender.GetValue(() => Group.Members.Find(_ => _.Id == SenderId));

        /// <summary>
        ///     指示本账户是否被提到。
        /// </summary>
        [JsonIgnore]
        public bool MentionedMe => _mentionedMe.GetValue(() =>
            Group.MyAlias != null && Content.Contains(Group.MyAlias) ||
            Client.Nickname != null && Content.Contains(Client.Nickname));

        /// <summary>
        ///     指示本账户是否被@。
        /// </summary>
        /// <remarks>
        ///     此属性无法区分真正的@与内容相同的纯文本。
        /// </remarks>
        [JsonIgnore]
        public bool StrictlyMentionedMe => _strictlyMentionedMe.GetValue(() =>
            (Group.MyAlias != null || Client.Nickname != null) &&
            Content.Contains("@" + (Group.MyAlias ?? Client.Nickname)));

        [JsonIgnore]
        User IMessage.Sender => Sender;

        /// <inheritdoc />
        [JsonProperty("time")]
        public long Timestamp { get; internal set; }

        /// <inheritdoc />
        [JsonIgnore]
        public string Content { get; internal set; }

        /// <inheritdoc />
        /// <param name="content">回复内容。</param>
        public void Reply(string content)
        {
            Client.Message(DumbQQClient.TargetType.Group, GroupId, content);
        }

        /// <inheritdoc />
        IMessageable IMessage.RepliableTarget => Group;
    }
}