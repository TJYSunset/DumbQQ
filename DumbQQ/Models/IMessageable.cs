namespace DumbQQ.Models
{
    /// <summary>
    ///     表示可向其发送消息的类的接口。
    /// </summary>
    public interface IMessageable
    {
        /// <summary>
        ///     可用于发送消息的编号。
        /// </summary>
        long Id { get; }

        /// <summary>
        ///     名字。
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     发送消息。
        /// </summary>
        /// <param name="content">消息内容。</param>
        void Message(string content);
    }
}