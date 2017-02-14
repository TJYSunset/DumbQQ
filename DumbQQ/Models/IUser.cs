namespace DumbQQ.Models
{
    /// <summary>
    ///     表示用户的接口。
    /// </summary>
    public interface IUser
    {
        /// <summary>
        ///     可用于发送消息的编号，不等于QQ号。
        /// </summary>
        long Id { get; set; }

        /// <summary>
        ///     昵称。
        /// </summary>
        string Nickname { get; set; }
    }
}