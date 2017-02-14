using DumbQQ.Client;

namespace DumbQQ.Models
{
    /// <summary>
    ///     表示消息的接口。
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        ///     发送者ID。
        /// </summary>
        long UserId { get; set; }

        /// <summary>
        ///     用于回复的ID。
        /// </summary>
        long RepliableId { get; set; }

        /// <summary>
        ///     消息时间戳。
        /// </summary>
        long Timestamp { get; set; }

        /// <summary>
        ///     消息文字内容。
        /// </summary>
        string Content { get; set; }

        /// <summary>
        ///     回复类型。
        /// </summary>
        DumbQQClient.TargetType Type { get; }
    }
}