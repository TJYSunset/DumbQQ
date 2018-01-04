namespace DumbQQ.Models.Abstract
{
    public abstract class User : IClientExclusive
    {
        public DumbQQClient Client { get; set; }

        public virtual ulong Id { get; internal set; }
        public virtual string Name { get; internal set; }
        public virtual string NameAlias { get; internal set; }
    }
}