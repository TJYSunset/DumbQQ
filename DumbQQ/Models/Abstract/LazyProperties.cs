using System;
using System.Collections.Generic;

namespace DumbQQ.Models.Abstract
{
    public class LazyProperties
    {
        private readonly Func<Dictionary<int, object>> _load;

        private Dictionary<int, object> _data;

        public bool IsLoaded { get; protected set; }

        public LazyProperties(Func<Dictionary<int, object>> load)
        {
            _load = load;
        }

        public void Load()
        {
            _data = _load();
            IsLoaded = true;
        }

        public dynamic this[int key]
        {
            get
            {
                if (!IsLoaded) Load();
                return _data[key];
            }
        }
    }
}