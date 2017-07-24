using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shitty.Data
{
    using System.Threading;

    public class Future<T>
    {
        private T _value;
        private bool _hasValue;

        private Future()
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

        public static Future<T> Create(out Action<T> setAction)
        {
            var future = new Future<T>();
            setAction = future.Set;
            return future;
        }

        public static implicit operator T(Future<T> future)
        {
            return future.Value;
        }
    }
}
