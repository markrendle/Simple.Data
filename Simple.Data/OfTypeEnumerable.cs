using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.CSharp.RuntimeBinder;

namespace Shitty.Data
{
    class OfTypeEnumerable<T> : IEnumerable<T>
    {
        private readonly IEnumerable<dynamic> _source;
		
        public OfTypeEnumerable(IEnumerable<dynamic> source)
        {
            _source = source;
        }
		
        class CastEnumerator : IEnumerator<T>
        {
            private readonly IEnumerator<dynamic> _source;
            public CastEnumerator(IEnumerator<dynamic> source)
            {
                _source = source;
            }

            public T Current {
                get {
                    return _source.Current;
                }
            }

            public bool MoveNext ()
            {
                bool next;
                while (next = _source.MoveNext())
                {
                    try
                    {
                        T cast = _source.Current;
                        break;
                    }
                    catch (InvalidCastException)
                    {
                    }
                    catch (RuntimeBinderException)
                    {
                    }
                }
                return next;
            }

            public void Reset ()
            {
                _source.Reset();
            }

            object IEnumerator.Current {
                get {
                    return Current;
                }
            }

            public void Dispose ()
            {
                _source.Dispose();
            }
        }

        #region IEnumerable[T] implementation
        public IEnumerator<T> GetEnumerator ()
        {
            return new CastEnumerator(_source.GetEnumerator());
        }
        #endregion

        #region IEnumerable implementation
        IEnumerator IEnumerable.GetEnumerator ()
        {
            return GetEnumerator();
        }
        #endregion
    }
}