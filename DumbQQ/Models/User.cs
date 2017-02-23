namespace DumbQQ.Models
{
    /// <summary>
    ///     表示用户的接口。
    /// </summary>
    public abstract class User
    {
        /// <summary>
        ///     可用于发送消息的编号，不等于QQ号。
        /// </summary>
        public abstract long Id { get; internal set; }

        /// <summary>
        ///     昵称。
        /// </summary>
        public abstract string Nickname { get; internal set; }

        /// <summary>
        ///     QQ号。
        /// </summary>
        public abstract long QQNumber { get; }

        public static bool operator ==(User left, User right) => left?.Id == right?.Id;
        public static bool operator !=(User left, User right) => !(left == right);

        protected bool Equals(User other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as User;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}