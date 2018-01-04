using System.Collections.Generic;
using RestSharp.Deserializers;

namespace DumbQQ.Models.Receipts
{
    internal struct MessageReceipt
    {
        [DeserializeAs(Name = @"retcode")]
        public int? Code { get; set; }
    }

    internal struct PollingReceipt
    {
        [DeserializeAs(Name = @"result")]
        public List<Message> MessageList { get; set; }
    }

    internal struct VfwebqqReceipt
    {
        public struct Wrapper
        {
            [DeserializeAs(Name = @"vfwebqq")]
            public string Vfwebqq { get; set; }
        }

        [DeserializeAs(Name = @"result")]
        public Wrapper Result { get; set; }
    }

    internal struct UinPsessionidReceipt
    {
        public struct Wrapper
        {
            [DeserializeAs(Name = @"uin")]
            public ulong Uin { get; set; }

            [DeserializeAs(Name = @"psessionid")]
            public string Psessionid { get; set; }
        }

        [DeserializeAs(Name = @"result")]
        public Wrapper Result { get; set; }
    }

    internal struct FriendsReceipt
    {
        public struct Wrapper
        {
            [DeserializeAs(Name = @"categories")]
            public List<string> CategoryNameList { get; set; }

            [DeserializeAs(Name = @"info")]
            public List<Friend> FriendList { get; set; }

            [DeserializeAs(Name = @"marknames")]
            public List<Friend> FriendNameAliasList { get; set; }

            [DeserializeAs(Name = @"friends")]
            public List<Friend> FriendCategoryIndexList { get; set; }
        }

        [DeserializeAs(Name = @"result")]
        public Wrapper Result { get; set; }
    }

    internal struct FriendPropertiesReceipt
    {
        public struct Wrapper
        {
            [DeserializeAs(Name = @"lnick")]
            public string Bio { get; set; }
        }

        [DeserializeAs(Name = @"result")]
        public Wrapper Result { get; set; }
    }

    internal struct GroupsReceipt
    {
        public struct Wrapper
        {
            [DeserializeAs(Name = @"gnamelist")]
            public List<Group> GroupList { get; set; }
        }

        [DeserializeAs(Name = @"result")]
        public Wrapper Result { get; set; }
    }

    internal struct GroupPropertiesReceipt
    {
        public struct Wrapper
        {
            [DeserializeAs(Name = @"ginfo")]
            public MiscellaneousWrapper Miscellaneous { get; set; }

            public struct MiscellaneousWrapper
            {
                [DeserializeAs(Name = @"owner")]
                public ulong OwnerId { get; set; } // TODO find out what this one does

                [DeserializeAs(Name = @"createtime")]
                public ulong Created { get; set; } // TODO find out what this one does

                [DeserializeAs(Name = @"memo")]
                public string PinnedAnnouncement { get; set; }
            }

            [DeserializeAs(Name = @"minfo")]
            public List<Group.Member> MemberList { get; set; }

            [DeserializeAs(Name = @"cards")]
            public List<Group.Member> MemberNameAliasList { get; set; }

            [DeserializeAs(Name = @"stats")]
            public List<Group.Member> MemberStatusList { get; set; }
        }

        [DeserializeAs(Name = @"result")]
        public Wrapper Result { get; set; }
    }

    internal struct DiscussionsReceipt
    {
        public struct Wrapper
        {
            [DeserializeAs(Name = @"dnamelist")]
            public List<Discussion> DiscussionList { get; set; }
        }

        [DeserializeAs(Name = @"result")]
        public Wrapper Result { get; set; }
    }

    internal struct DiscussionPropertiesReceipt
    {
        public struct Wrapper
        {
            [DeserializeAs(Name = @"mem_info")]
            public List<Discussion.Member> MemberList { get; set; }

            [DeserializeAs(Name = @"mem_status")]
            public List<Discussion.Member> MemberStatusList { get; set; }
        }

        [DeserializeAs(Name = @"result")]
        public Wrapper Result { get; set; }
    }
}