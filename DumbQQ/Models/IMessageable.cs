using DumbQQ.Client;

namespace DumbQQ.Models
{
    /// <summary>
    /// 表示可以向此对象发送消息的接口。
    /// </summary>
    public interface IMessageable
    {
        DumbQQClient.TargetType TargetType { get; }
        long Id { get; set; }
    }
}
