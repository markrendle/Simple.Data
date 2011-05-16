using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    public struct Range<T> : IRange, IEquatable<Range<T>>
        where T : IComparable<T>
    {
        private readonly T _start;
        private readonly T _end;

        public Range(T start, T end)
            : this()
        {
            _start = start;
            _end = end;
        }

        public T End
        {
            get { return _end; }
        }

        public IEnumerable<object> AsEnumerable()
        {
            yield return _start;
            yield return _end;
        }

        public T Start
        {
            get { return _start; }
        }

        public override string ToString()
        {
            return string.Format("({0}..{1})", _start, _end);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Range<T> other)
        {
            return Equals(other._start, _start) && Equals(other._end, _end);
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        /// <param name="obj">Another object to compare to. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof(Range<T>)) return false;
            return Equals((Range<T>)obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            unchecked
            {
                return (_start.GetHashCode() * 397) ^ _end.GetHashCode();
            }
        }

        public static bool operator ==(Range<T> left, Range<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Range<T> left, Range<T> right)
        {
            return !left.Equals(right);
        }

        object IRange.Start
        {
            get { return Start; }
        }

        object IRange.End
        {
            get { return End; }
        }
    }
}
