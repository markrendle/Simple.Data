using System;
using System.Collections.Generic;

namespace Simple.Data
{
    using System.Collections;
    using System.Linq;
    using System.Threading;

    public static class CheckedEnumerable
    {
        public static CheckedEnumerable<T> Create<T>(IEnumerable<T> source)
        {
            return new CheckedEnumerable<T>(source);
        }
    }

    public class CheckedEnumerable<T> : IEnumerable<T>
    {
        private readonly object _sync = new object();
        private readonly IEnumerable<T> _source;
        private IEnumerator<T> _sourceEnumerator;
        private Enumerator _enumerator;
        private T _first;

        public T Single
        {
            get
            {
                Check();
                if (_count != 1) throw new InvalidOperationException("Enumerable does not contain exactly one element.");
                return _first;
            }
        }

        private T _second;
        private int _count = -1;
        private int _used;

        public CheckedEnumerable(IEnumerable<T> source)
        {
            _source = source;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (Interlocked.CompareExchange(ref _used, 1, 0) == 1)
            {
                throw new InvalidOperationException("CheckedEnumerable may only be enumerated once.");
            }

            Check();

            return _count == 0
                ? Enumerable.Empty<T>().GetEnumerator()
                : new Enumerator(_count, _first, _second, _sourceEnumerator);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool IsEmpty
        {
            get
            {
                Check();
                return _count == 0;
            }
        }
        public bool HasMoreThanOneValue
        {
            get
            {
                Check();
                return _count > 1;
            }
        }

        private void Check()
        {
            if (_sourceEnumerator == null)
            {
                lock (_sync)
                {
                    if (_sourceEnumerator != null) return;
                    _sourceEnumerator = _source.GetEnumerator();
                    if (!_sourceEnumerator.MoveNext())
                    {
                        _count = 0;
                        return;
                    }
                    _first = _sourceEnumerator.Current;
                    if (_sourceEnumerator.MoveNext())
                    {
                        _count = 2;
                        _second = _sourceEnumerator.Current;
                        return;
                    }
                    _count = 1;
                }
            }
        }

        private class Enumerator : IEnumerator<T>
        {
            private readonly IEnumerator<T> _wrapped;
            private int _check;
            private readonly T _first;
            private readonly T _second;

            public Enumerator(int check, T first, T second, IEnumerator<T> wrapped)
            {
                _check = check;
                _first = first;
                _second = second;
                _wrapped = wrapped;
            }

            public void Dispose()
            {
                _wrapped.Dispose();
            }

            public bool MoveNext()
            {
                switch (_check--)
                {
                    case 2:
                        Current = _first;
                        return true;
                    case 1:
                        Current = _second;
                        return true;
                    default:
                        bool moved = _wrapped.MoveNext();
                        if (moved) Current = _wrapped.Current;
                        return moved;
                }
            }

            public void Reset()
            {
                _check = 0;
                _wrapped.Reset();
            }

            public T Current { get; private set; }

            object IEnumerator.Current
            {
                get { return Current; }
            }
        }
    }
}
