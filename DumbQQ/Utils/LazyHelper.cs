using System;

namespace DumbQQ.Utils
{
    internal class LazyHelper<T>
    {
        private readonly object _lock = new object();
        private T _value;
        public bool IsInitialized { get; private set; }

        internal T Value
        {
            get
            {
                if (!IsInitialized)
                    throw new FieldAccessException(
                        "This value needs to be set by using the GetValue() method or setting the Value property");
                return _value;
            }
        }

        public T GetValue(Func<T> producer)
        {
            if (producer == null)
                throw new ArgumentNullException(nameof(producer));

            if (IsInitialized) return _value;
            lock (_lock)
            {
                if (IsInitialized) return _value;
                _value = producer();
                IsInitialized = true;
            }
            return _value;
        }
    }
}