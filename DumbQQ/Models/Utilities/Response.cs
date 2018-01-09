using System.Collections.Generic;
using System.ComponentModel;
using RestSharp.Deserializers;

namespace DumbQQ.Models.Utilities
{
    internal class Response
    {
        [DeserializeAs(Name = @"retcode")] public int? Code { get; set; }
    }

    internal class MessageResponse : Response
    {
    }

    internal class PollingResponse : Response
    {
        [DeserializeAs(Name = @"result")]
        public List<MessageWrapper> MessageList { get; set; } = new List<MessageWrapper>();

        public class MessageWrapper
        {
            [DeserializeAs(Name = @"poll_type")]
            private string TypePrimitive
            {
                set
                {
                    switch (value)
                    {
                        case @"message":
                            Type = Message.SourceType.Friend;
                            break;
                        case @"group_message":
                            Type = Message.SourceType.Group;
                            break;
                        case @"discu_message":
                            Type = Message.SourceType.Discussion;
                            break;
                        default:
                            throw new InvalidEnumArgumentException(
                                $"Error deserializing message: unexpected poll_type {value}");
                    }
                }
            }

            public Message.SourceType Type { get; internal set; }

            [DeserializeAs(Name = @"value")] public Wrapper Data { get; set; }

            public class Wrapper
            {
                [DeserializeAs(Name = @"send_uin")]
                public ulong SenderIdGroupDiscussion
                {
                    set => SenderId = value;
                }

                [DeserializeAs(Name = @"did")]
                public ulong SourceIdDiscussion
                {
                    set => SourceId = value;
                }

                [DeserializeAs(Name = @"time")] public ulong? Timestamp { get; internal set; }

                [DeserializeAs(Name = @"content")] public List<string> Content { get; internal set; }

                [DeserializeAs(Name = @"from_uin")] public ulong? SenderId { get; internal set; }

                [DeserializeAs(Name = @"group_code")] public ulong? SourceId { get; internal set; }
            }
        }
    }

    internal class VfwebqqResponse : Response
    {
        [DeserializeAs(Name = @"result")] public Wrapper Result { get; set; }

        public class Wrapper
        {
            [DeserializeAs(Name = @"vfwebqq")] public string Vfwebqq { get; set; }
        }
    }

    internal class UinPsessionidResponse : Response
    {
        [DeserializeAs(Name = @"result")] public Wrapper Result { get; set; }

        public class Wrapper
        {
            [DeserializeAs(Name = @"uin")] public ulong Uin { get; set; }

            [DeserializeAs(Name = @"psessionid")] public string Psessionid { get; set; }
        }
    }

    internal class FriendsResponse : Response
    {
        [DeserializeAs(Name = @"result")] public Wrapper Result { get; set; }

        public class Wrapper
        {
            [DeserializeAs(Name = @"categories")] public List<FriendCategory> CategoryList { get; set; }

            [DeserializeAs(Name = @"info")] public List<Friend> FriendList { get; set; }

            [DeserializeAs(Name = @"marknames")] public List<Friend> FriendNameAliasList { get; set; }

            [DeserializeAs(Name = @"friends")] public List<Friend> FriendCategoryIndexList { get; set; }
        }
    }

    internal class FriendPropertiesResponse : Response
    {
        [DeserializeAs(Name = @"result")] public Wrapper Result { get; set; }

        public class Wrapper
        {
            [DeserializeAs(Name = @"lnick")] public string Bio { get; set; }
        }
    }

    internal class GroupsResponse : Response
    {
        [DeserializeAs(Name = @"result")] public Wrapper Result { get; set; }

        public class Wrapper
        {
            [DeserializeAs(Name = @"gnamelist")] public List<Group> GroupList { get; set; }
        }
    }

    internal class GroupPropertiesResponse : Response
    {
        [DeserializeAs(Name = @"result")] public Wrapper Result { get; set; }

        public class Wrapper
        {
            [DeserializeAs(Name = @"ginfo")] public MiscellaneousWrapper Miscellaneous { get; set; }

            [DeserializeAs(Name = @"minfo")] public List<Group.Member> MemberList { get; set; }

            [DeserializeAs(Name = @"cards")] public List<Group.Member> MemberNameAliasList { get; set; }

            [DeserializeAs(Name = @"stats")] public List<Group.Member> MemberStatusList { get; set; }

            public class MiscellaneousWrapper
            {
                [DeserializeAs(Name = @"owner")] public ulong OwnerId { get; set; } // TODO find out what this one does

                [DeserializeAs(Name = @"createtime")]
                public ulong Created { get; set; } // TODO find out what this one does

                [DeserializeAs(Name = @"memo")] public string PinnedAnnouncement { get; set; }
            }
        }
    }

    internal class DiscussionsResponse : Response
    {
        [DeserializeAs(Name = @"result")] public Wrapper Result { get; set; }

        public class Wrapper
        {
            [DeserializeAs(Name = @"dnamelist")] public List<Discussion> DiscussionList { get; set; }
        }
    }

    internal class DiscussionPropertiesResponse : Response
    {
        [DeserializeAs(Name = @"result")] public Wrapper Result { get; set; }

        public class Wrapper
        {
            [DeserializeAs(Name = @"mem_info")] public List<Discussion.Member> MemberList { get; set; }

            [DeserializeAs(Name = @"mem_status")] public List<Discussion.Member> MemberStatusList { get; set; }
        }
    }
}