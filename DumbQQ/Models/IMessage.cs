namespace DumbQQ.Models
{
    /// <summary>
    ///     表示消息的接口。
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        ///     回复该消息。
        /// </summary>
        /// <param name="content">回复内容。</param>
        void Reply(string content);
    }
}