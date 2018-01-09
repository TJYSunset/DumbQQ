using DumbQQ.Models.Abstract;

namespace DumbQQ.Models
{
    public class Message : IClientExclusive
    {
        public enum SourceType
        {
            Friend,
            Group,
            Discussion
        }

        public SourceType Type { get; internal set; }

        public ulong? Timestamp { get; internal set; }

        public string Content { get; internal set; }

        public ulong? SenderId { get; internal set; }

        public ulong? SourceId { get; internal set; }

        public DumbQQClient Client { get; set; }
    }
}