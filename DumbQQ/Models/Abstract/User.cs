namespace DumbQQ.Models.Abstract
{
    public abstract class User : IClientExclusive
    {
        public virtual ulong Id { get; internal set; }
        public virtual string Name { get; internal set; }
        public virtual string NameAlias { get; internal set; }
        public DumbQQClient Client { get; set; }
    }
}