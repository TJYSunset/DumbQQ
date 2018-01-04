using System.ComponentModel;
using DumbQQ.Models.Abstract;
using RestSharp.Deserializers;

namespace DumbQQ.Models
{
    public class Message : IClientExclusive
    {
        public DumbQQClient Client { get; set; }

        public enum SourceType
        {
            Friend,
            Group,
            Discussion
        }

        [DeserializeAs(Name = @"poll_type")]
        private string TypePrimitive
        {
            set
            {
                switch (value)
                {
                    case @"message":
                        Type = SourceType.Friend;
                        break;
                    case @"group_message":
                        Type = SourceType.Group;
                        break;
                    case @"discu_message":
                        Type = SourceType.Discussion;
                        break;
                    default:
                        throw new InvalidEnumArgumentException(
                            $"Error deserializing message: unexpected poll_type {value}");
                }
            }
        }

        [DeserializeAs(Name = @"send_uin")]
        private ulong SenderIdGroupDiscussion
        {
            set => SenderId = value;
        }

        [DeserializeAs(Name = @"did")]
        private ulong SourceIdDiscussion
        {
            set => SourceId = value;
        }

        public SourceType Type { get; internal set; }

        [DeserializeAs(Name = @"time")]
        public ulong? Timestamp { get; internal set; }

        [DeserializeAs(Name = @"content")]
        public string Content { get; internal set; }

        [DeserializeAs(Name = @"from_uin")]
        public ulong? SenderId { get; internal set; }

        [DeserializeAs(Name = @"group_code")]
        public ulong? SourceId { get; internal set; }
    }
}