using System;

namespace Simple.Data
{
    public static class Maybe
    {
        public static Maybe<T> Some<T>(T value)
        {
            return Maybe<T>.Some(value);
        }
    }
    public abstract class Maybe<T> : IEquatable<Maybe<T>>
    {
        public static readonly Maybe<T> None = new NoneClass();
        public static Maybe<T> Some(T value)
        {
            return new SomeClass(value);
        }

        public abstract bool HasValue { get; }

        class NoneClass : Maybe<T>
        {
            public override T Value
            {
                get { return default(T); }
            }

            public override bool HasValue
            {
                get { return false; }
            }

            /// <summary>
            /// Indicates whether the current object is equal to another object of the same type.
            /// </summary>
            /// <returns>
            /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
            /// </returns>
            /// <param name="other">An object to compare with this object.</param>
            public override bool Equals(Maybe<T> other)
            {
                return other is NoneClass;
            }

            /// <summary>
            /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
            /// </summary>
            /// <returns>
            /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
            /// </returns>
            /// <filterpriority>2</filterpriority>
            public override string ToString()
            {
                return string.Empty;
            }

            /// <summary>
            /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
            /// </summary>
            /// <returns>
            /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
            /// </returns>
            /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param><filterpriority>2</filterpriority>
            public override bool Equals(object obj)
            {
                return obj is NoneClass;
            }

            public override int GetHashCode()
            {
                return 0;
            }
        }

        class SomeClass : Maybe<T>
        {
            private readonly T _value;

            public SomeClass(T value)
            {
                _value = value;
            }

            public override T Value
            {
                get { return _value; }
            }

            public override bool HasValue
            {
                get { return true; }
            }

            /// <summary>
            /// Indicates whether the current object is equal to another object of the same type.
            /// </summary>
            /// <returns>
            /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
            /// </returns>
            /// <param name="other">An object to compare with this object.</param>
            public override bool Equals(Maybe<T> other)
            {
                return other != null && other.HasValue && Equals(other.Value, _value);
            }

            /// <summary>
            /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
            /// </summary>
            /// <returns>
            /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
            /// </returns>
            /// <filterpriority>2</filterpriority>
            public override string ToString()
            {
                return _value.ToString();
            }

            /// <summary>
            /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
            /// </summary>
            /// <returns>
            /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
            /// </returns>
            /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param><filterpriority>2</filterpriority>
            public override bool Equals(object obj)
            {
                return Equals(obj as Maybe<T>);
            }

            public override int GetHashCode()
            {
                return _value.GetHashCode();
            }
        }

        public abstract T Value
        {
            get;
        }

        //public static implicit operator bool(Maybe<T> maybe)
        //{
        //    return maybe is SomeClass;
        //}

        public static implicit operator Maybe<T>(T value)
        {
            return new SomeClass(value);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public abstract bool Equals(Maybe<T> other);

        public static bool operator ==(Maybe<T> left, Maybe<T> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Maybe<T> left, Maybe<T> right)
        {
            return !Equals(left, right);
        }

        public abstract override bool Equals(object obj);

        public abstract override int GetHashCode();

        public abstract override string ToString();
    }
}
