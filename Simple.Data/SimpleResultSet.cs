using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Microsoft.CSharp.RuntimeBinder;

namespace Simple.Data
{
    public sealed class SimpleResultSet : DynamicObject, IEnumerable
    {
        public static readonly SimpleResultSet Empty = new SimpleResultSet();

        private readonly IEnumerable<IEnumerable<dynamic>> _sources;
        private readonly IEnumerator<IEnumerable<dynamic>> _sourceEnumerator;
        private bool _hasCurrent;

        private SimpleResultSet() : this(Enumerable.Empty<dynamic>())
        {
            
        }

        public SimpleResultSet(params IEnumerable<dynamic>[] sources) : this (sources.AsEnumerable())
        {
        }

        public SimpleResultSet(IEnumerable<IEnumerable<dynamic>> sources)
        {
            _sources = sources;
            _sourceEnumerator = _sources.GetEnumerator();
            _hasCurrent = _sourceEnumerator.MoveNext();
        }

        public bool MoveNext()
        {
            return _hasCurrent = (_hasCurrent && _sourceEnumerator.MoveNext());
        }

        public IEnumerable<T> Cast<T>()
        {
            return _sourceEnumerator.Current.Select(item => (T) item);
        }

        public IEnumerable<T> OfType<T>()
        {
            foreach (var item in _sourceEnumerator.Current)
            {
                bool success = true;
                T cast;
                try
                {
                    cast = (T) item;
                }
                catch (RuntimeBinderException)
                {
                    cast = default(T);
                    success = false;
                }
                if (success)
                {
                    yield return cast;
                }
            }
        }

        public IList<dynamic> ToList()
        {
            return _sourceEnumerator.Current.ToList();
        }

        public dynamic[] ToArray()
        {
            return _sourceEnumerator.Current.ToArray();
        }

        public IList<T> ToList<T>()
        {
            return Cast<T>().ToList();
        }

        public T[] ToArray<T>()
        {
            return Cast<T>().ToArray();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.IEnumerator"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator GetEnumerator()
        {
            return new DynamicEnumerator(_sourceEnumerator.Current);
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (binder.Type == typeof(IEnumerable<object>))
            {
                result = Cast<object>();
                return true;
            }
            return base.TryConvert(binder, out result);
        }

        class DynamicEnumerator : IEnumerator, IDisposable
        {
            private readonly IEnumerator<dynamic> _enumerator;

            public DynamicEnumerator(IEnumerable<dynamic> source)
            {
                _enumerator = source.GetEnumerator();
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            /// <filterpriority>2</filterpriority>
            public void Dispose()
            {
                _enumerator.Dispose();
            }

            /// <summary>
            /// Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>
            /// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
            /// </returns>
            /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception><filterpriority>2</filterpriority>
            public bool MoveNext()
            {
                return _enumerator.MoveNext();
            }

            /// <summary>
            /// Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception><filterpriority>2</filterpriority>
            public void Reset()
            {
                _enumerator.Reset();
            }

            /// <summary>
            /// Gets the current element in the collection.
            /// </summary>
            /// <returns>
            /// The current element in the collection.
            /// </returns>
            /// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element.</exception><filterpriority>2</filterpriority>
            public object Current
            {
                get { return _enumerator.Current; }
            }
        }

        /// <summary>
        /// Creates a single-set <see cref="SimpleResultSet"/> from the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        public static SimpleResultSet Create(IEnumerable<IEnumerable<KeyValuePair<string,object>>> source)
        {
            var q = from dict in source
                    select new SimpleRecord(dict);
            return new SimpleResultSet(q);
        }

        /// <summary>
        /// Creates a multi-set <see cref="SimpleResultSet"/> from the specified sources.
        /// </summary>
        /// <param name="sources">The sources.</param>
        /// <returns></returns>
        public static SimpleResultSet Create(IEnumerable<IEnumerable<IEnumerable<KeyValuePair<string, object>>>> sources)
        {
            var q = from source in sources
                    select from dict in source
                    select new SimpleRecord(dict);
            return new SimpleResultSet(q);
        }
    }
}
