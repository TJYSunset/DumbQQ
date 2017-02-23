namespace DumbQQ.Models
{
    /// <summary>
    ///     表示消息的接口。
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        ///     时间戳。
        /// </summary>
        long Timestamp { get; }

        /// <summary>
        ///     消息内容。
        /// </summary>
        string Content { get; }

        /// <summary>
        ///     发送者。
        /// </summary>
        User Sender { get; }

        /// <summary>
        ///     回复该消息。
        /// </summary>
        /// <param name="content">回复内容。</param>
        void Reply(string content);
    }
}