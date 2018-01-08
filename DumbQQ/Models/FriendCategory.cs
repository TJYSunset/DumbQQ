using DumbQQ.Models.Abstract;
using RestSharp.Deserializers;

namespace DumbQQ.Models
{
    public class FriendCategory : IClientExclusive
    {
        /// <summary>
        ///     The system-given index, presumably ascending by time created.
        /// </summary>
        [DeserializeAs(Name = @"index")]
        public ulong Index { get; internal set; }

        /// <summary>
        ///     The user-set index, as seen in the actual QQ GUI.
        /// </summary>
        [DeserializeAs(Name = @"sort")]
        public ulong Order { get; internal set; }

        [DeserializeAs(Name = @"name")] public string Name { get; internal set; }

        public DumbQQClient Client { get; set; }
    }
}