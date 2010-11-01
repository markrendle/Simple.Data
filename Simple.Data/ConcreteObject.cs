using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Simple.Data
{
    internal class ConcreteObject
    {
        private static readonly object CastFailureObject = new object();
        private WeakReference _concreteObject;

        public object Get(Type type, HomogenizedKeyDictionary data)
        {
            if (_concreteObject == null || !_concreteObject.IsAlive)
            {
                return ConvertAndCacheReference(type, data);
            }

            if (!ReferenceEquals(CastFailureObject, _concreteObject.Target))
            {
                return _concreteObject.Target;
            }

            return null;
        }

        private object ConvertAndCacheReference(Type type, HomogenizedKeyDictionary data)
        {
            _concreteObject = null;
            object result;
            if (ConcreteTypeCreator.Get(type).TryCreate(data, out result))
            {
                Interlocked.CompareExchange(ref _concreteObject, new WeakReference(result), null);
                return _concreteObject.Target;
            }

            Interlocked.CompareExchange(ref _concreteObject, new WeakReference(CastFailureObject), null);
            return null;
        }
    }
}
