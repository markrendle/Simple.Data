using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Simple.Data
{
    sealed class BufferedEnumerable<T> : IEnumerable<T>
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly List<T> _buffer = new List<T>();
        private bool _done;

        internal void Iterate(Func<Maybe<T>> iterator)
        {
            Maybe<T> maybe;
            while ((maybe = iterator()).HasValue)
            {
                Add(maybe.Value);
            }
            SetDone();
        }

        private void SetDone()
        {
            _lock.EnterWriteLock();
            try
            {
                _done = true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private void Add(T item)
        {
            _lock.EnterWriteLock();
            try
            {
                _buffer.Add(item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private T this[int index]
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _buffer[index];
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        private int Count
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _buffer.Count;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        private bool Done
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _done;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        class BufferedEnumerator : IEnumerator<T>
        {
            private readonly BufferedEnumerable<T> _bufferedEnumerable;

            public BufferedEnumerator(BufferedEnumerable<T> bufferedEnumerable)
            {
                _bufferedEnumerable = bufferedEnumerable;
            }

            private int _current = -1;

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            /// <filterpriority>2</filterpriority>
            public void Dispose()
            {

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
                ++_current;

                if (_bufferedEnumerable._done)
                {
                    return _current < _bufferedEnumerable.Count;
                }

                WaitForBuffer();
                return _current < _bufferedEnumerable.Count;
            }

            private void WaitForBuffer()
            {
                // Block until next item delivered or iterator is done.
                SpinWait.SpinUntil(() => _bufferedEnumerable.Done || _bufferedEnumerable.Count > _current);
            }

            /// <summary>
            /// Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception><filterpriority>2</filterpriority>
            public void Reset()
            {
                _current = -1;
            }

            /// <summary>
            /// Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <returns>
            /// The element in the collection at the current position of the enumerator.
            /// </returns>
            public T Current
            {
                get
                {
                    if (_current >= _bufferedEnumerable.Count) throw new InvalidOperationException();
                    return _bufferedEnumerable[_current];
                }
            }

            /// <summary>
            /// Gets the current element in the collection.
            /// </summary>
            /// <returns>
            /// The current element in the collection.
            /// </returns>
            /// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element.</exception><filterpriority>2</filterpriority>
            object IEnumerator.Current
            {
                get { return Current; }
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<T> GetEnumerator()
        {
            return new BufferedEnumerator(this);
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
