namespace DumbQQ.Models
{
    /// <summary>
    ///     ��ʾ�û��Ľӿڡ�
    /// </summary>
    public abstract class User
    {
        /// <summary>
        ///     �����ڷ�����Ϣ�ı�ţ�������QQ�š�
        /// </summary>
        public abstract long Id { get; internal set; }

        /// <summary>
        ///     �ǳơ�
        /// </summary>
        public abstract string Nickname { get; internal set; }

        /// <summary>
        ///     QQ�š�
        /// </summary>
        public abstract long QQNumber { get; }

        /// <inheritdoc />
        public static bool operator ==(User left, User right) => left?.Id == right?.Id;

        /// <inheritdoc />
        public static bool operator !=(User left, User right) => !(left == right);

        /// <inheritdoc />
        protected bool Equals(User other)
        {
            return Id == other.Id;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as User;
            return other != null && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}