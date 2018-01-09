using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DumbQQ.Models.Abstract
{
    public abstract class UserCollection<T> : IEnumerable<T>, IClientExclusive where T : User
    {
        public virtual ulong Id { get; internal set; }
        public virtual string Name { get; internal set; }
        public virtual ReadOnlyDictionary<ulong, T> Members { get; internal set; }
        public DumbQQClient Client { get; set; }

        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}