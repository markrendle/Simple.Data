using System;
using System.Threading;

namespace Simple.Data
{
    public class Promise<T>
    {
        private T _value;
        private bool _hasValue;

        private Promise()
        {

        }

        public T Value
        {
            get
            {
                SpinWait.SpinUntil(() => _hasValue);
                return _value;
            }
        }

        public bool HasValue
        {
            get { return _hasValue; }
        }

        private void Set(T value)
        {
            _value = value;
            _hasValue = true;
        }

        public static Promise<T> Create(out Action<T> setAction)
        {
            var future = new Promise<T>();
            setAction = future.Set;
            return future;
        }

        public static implicit operator T(Promise<T> future)
        {
            return future.Value;
        }
    }
}