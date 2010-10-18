using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace Simple.Data.Mocking.Ado
{
    class MockDataParameterCollection : DbParameterCollection
    {
        private readonly Collection<DbParameter> _collection = new Collection<DbParameter>();
        /// <summary>
        /// Adds a <see cref="T:System.Data.Common.DbParameter"/> item with the specified value to the <see cref="T:System.Data.Common.DbParameterCollection"/>.
        /// </summary>
        /// <returns>
        /// The index of the <see cref="T:System.Data.Common.DbParameter"/> object in the collection.
        /// </returns>
        /// <param name="value">The <see cref="P:System.Data.Common.DbParameter.Value"/> of the <see cref="T:System.Data.Common.DbParameter"/> to add to the collection.</param><filterpriority>1</filterpriority>
        public override int Add(object value)
        {
            _collection.Add((DbParameter)value);
            return _collection.Count;
        }

        /// <summary>
        /// Indicates whether a <see cref="T:System.Data.Common.DbParameter"/> with the specified <see cref="P:System.Data.Common.DbParameter.Value"/> is contained in the collection.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Data.Common.DbParameter"/> is in the collection; otherwise false.
        /// </returns>
        /// <param name="value">The <see cref="P:System.Data.Common.DbParameter.Value"/> of the <see cref="T:System.Data.Common.DbParameter"/> to look for in the collection.</param><filterpriority>1</filterpriority>
        public override bool Contains(object value)
        {
            return value != null && _collection.Contains((DbParameter)value);
        }

        /// <summary>
        /// Indicates whether a <see cref="T:System.Data.Common.DbParameter"/> with the specified name exists in the collection.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Data.Common.DbParameter"/> is in the collection; otherwise false.
        /// </returns>
        /// <param name="value">The name of the <see cref="T:System.Data.Common.DbParameter"/> to look for in the collection.</param><filterpriority>1</filterpriority>
        public override bool Contains(string value)
        {
            return value != null && _collection.Any(p => p.ParameterName == value);
        }

        /// <summary>
        /// Returns the index of the <see cref="T:System.Data.Common.DbParameter"/> object with the specified name.
        /// </summary>
        /// <returns>
        /// The index of the <see cref="T:System.Data.Common.DbParameter"/> object with the specified name.
        /// </returns>
        /// <param name="parameterName">The name of the <see cref="T:System.Data.Common.DbParameter"/> object in the collection.</param><filterpriority>2</filterpriority>
        public override int IndexOf(string parameterName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes the <see cref="T:System.Data.Common.DbParameter"/> object with the specified name from the collection.
        /// </summary>
        /// <param name="parameterName">The name of the <see cref="T:System.Data.Common.DbParameter"/> object to remove.</param><filterpriority>2</filterpriority>
        public override void RemoveAt(string parameterName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes all <see cref="T:System.Data.Common.DbParameter"/> values from the <see cref="T:System.Data.Common.DbParameterCollection"/>.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        public override void Clear()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the index of the specified <see cref="T:System.Data.Common.DbParameter"/> object.
        /// </summary>
        /// <returns>
        /// The index of the specified <see cref="T:System.Data.Common.DbParameter"/> object.
        /// </returns>
        /// <param name="value">The <see cref="T:System.Data.Common.DbParameter"/> object in the collection.</param><filterpriority>2</filterpriority>
        public override int IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Inserts the specified index of the <see cref="T:System.Data.Common.DbParameter"/> object with the specified name into the collection at the specified index.
        /// </summary>
        /// <param name="index">The index at which to insert the <see cref="T:System.Data.Common.DbParameter"/> object.</param><param name="value">The <see cref="T:System.Data.Common.DbParameter"/> object to insert into the collection.</param><filterpriority>1</filterpriority>
        public override void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes the specified <see cref="T:System.Data.Common.DbParameter"/> object from the collection.
        /// </summary>
        /// <param name="value">The <see cref="T:System.Data.Common.DbParameter"/> object to remove.</param><filterpriority>1</filterpriority>
        public override void Remove(object value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes the <see cref="T:System.Data.Common.DbParameter"/> object at the specified from the collection.
        /// </summary>
        /// <param name="index">The index where the <see cref="T:System.Data.Common.DbParameter"/> object is located.</param><filterpriority>2</filterpriority>
        public override void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Exposes the <see cref="M:System.Collections.IEnumerable.GetEnumerator"/> method, which supports a simple iteration over a collection by a .NET Framework data provider.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override IEnumerator GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        /// <summary>
        /// Returns the <see cref="T:System.Data.Common.DbParameter"/> object at the specified index in the collection.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Data.Common.DbParameter"/> object at the specified index in the collection.
        /// </returns>
        /// <param name="index">The index of the <see cref="T:System.Data.Common.DbParameter"/> in the collection.</param>
        protected override DbParameter GetParameter(int index)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns <see cref="T:System.Data.Common.DbParameter"/> the object with the specified name.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Data.Common.DbParameter"/> the object with the specified name.
        /// </returns>
        /// <param name="parameterName">The name of the <see cref="T:System.Data.Common.DbParameter"/> in the collection.</param>
        protected override DbParameter GetParameter(string parameterName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds an array of items with the specified values to the <see cref="T:System.Data.Common.DbParameterCollection"/>.
        /// </summary>
        /// <param name="values">An array of values of type <see cref="T:System.Data.Common.DbParameter"/> to add to the collection.</param><filterpriority>2</filterpriority>
        public override void AddRange(Array values)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Copies an array of items to the collection starting at the specified index.
        /// </summary>
        /// <param name="array">The array of items to copy to the collection.</param><param name="index">The index in the collection to copy the items.</param><filterpriority>2</filterpriority>
        public override void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Specifies the number of items in the collection.
        /// </summary>
        /// <returns>
        /// The number of items in the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override int Count
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Specifies the <see cref="T:System.Object"/> to be used to synchronize access to the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Object"/> to be used to synchronize access to the <see cref="T:System.Data.Common.DbParameterCollection"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override object SyncRoot
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Specifies whether the collection is a fixed size.
        /// </summary>
        /// <returns>
        /// true if the collection is a fixed size; otherwise false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool IsFixedSize
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Specifies whether the collection is read-only.
        /// </summary>
        /// <returns>
        /// true if the collection is read-only; otherwise false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Specifies whether the collection is synchronized.
        /// </summary>
        /// <returns>
        /// true if the collection is synchronized; otherwise false.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override bool IsSynchronized
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Sets the <see cref="T:System.Data.Common.DbParameter"/> object at the specified index to a new value. 
        /// </summary>
        /// <param name="index">The index where the <see cref="T:System.Data.Common.DbParameter"/> object is located.</param><param name="value">The new <see cref="T:System.Data.Common.DbParameter"/> value.</param>
        protected override void SetParameter(int index, DbParameter value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the <see cref="T:System.Data.Common.DbParameter"/> object with the specified name to a new value.
        /// </summary>
        /// <param name="parameterName">The name of the <see cref="T:System.Data.Common.DbParameter"/> object in the collection.</param><param name="value">The new <see cref="T:System.Data.Common.DbParameter"/> value.</param>
        protected override void SetParameter(string parameterName, DbParameter value)
        {
            throw new NotImplementedException();
        }
    }
}
