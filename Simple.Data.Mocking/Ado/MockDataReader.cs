using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Simple.Data.Mocking.Ado
{
    class MockDataReader : DbDataReader
    {
        private readonly IEnumerator<IDataRecord> _records;

        public MockDataReader(IEnumerable<IDataRecord> records)
        {
            _records = records.GetEnumerator();
        }

        /// <summary>
        /// Closes the <see cref="T:System.Data.Common.DbDataReader"/> object.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        public override void Close()
        {
            
        }

        /// <summary>
        /// Returns a <see cref="T:System.Data.DataTable"/> that describes the column metadata of the <see cref="T:System.Data.Common.DbDataReader"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.DataTable"/> that describes the column metadata.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Data.SqlClient.SqlDataReader"/> is closed. </exception><filterpriority>1</filterpriority>
        public override DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Advances the reader to the next result when reading the results of a batch of statements.
        /// </summary>
        /// <returns>
        /// true if there are more result sets; otherwise false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool NextResult()
        {
            return false;
        }

        /// <summary>
        /// Advances the reader to the next record in a result set.
        /// </summary>
        /// <returns>
        /// true if there are more rows; otherwise false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool Read()
        {
            return _records.MoveNext();
        }

        /// <summary>
        /// Gets a value indicating the depth of nesting for the current row.
        /// </summary>
        /// <returns>
        /// The depth of nesting for the current row.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override int Depth
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Data.Common.DbDataReader"/> is closed.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Data.Common.DbDataReader"/> is closed; otherwise false.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Data.SqlClient.SqlDataReader"/> is closed. </exception><filterpriority>1</filterpriority>
        public override bool IsClosed
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the number of rows changed, inserted, or deleted by execution of the SQL statement. 
        /// </summary>
        /// <returns>
        /// The number of rows changed, inserted, or deleted. -1 for SELECT statements; 0 if no rows were affected or the statement failed.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override int RecordsAffected
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the value of the specified column as a Boolean.
        /// </summary>
        /// <returns>
        /// The value of the specified column.
        /// </returns>
        /// <param name="ordinal">The zero-based column ordinal.</param><exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception><filterpriority>1</filterpriority>
        public override bool GetBoolean(int ordinal)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the value of the specified column as a byte.
        /// </summary>
        /// <returns>
        /// The value of the specified column.
        /// </returns>
        /// <param name="ordinal">The zero-based column ordinal.</param><exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception><filterpriority>1</filterpriority>
        public override byte GetByte(int ordinal)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads a stream of bytes from the specified column, starting at location indicated by <paramref name="dataOffset"/>, into the buffer, starting at the location indicated by <paramref name="bufferOffset"/>.
        /// </summary>
        /// <returns>
        /// The actual number of bytes read.
        /// </returns>
        /// <param name="ordinal">The zero-based column ordinal.</param><param name="dataOffset">The index within the row from which to begin the read operation.</param><param name="buffer">The buffer into which to copy the data.</param><param name="bufferOffset">The index with the buffer to which the data will be copied.</param><param name="length">The maximum number of characters to read.</param><exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception><filterpriority>1</filterpriority>
        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the value of the specified column as a single character.
        /// </summary>
        /// <returns>
        /// The value of the specified column.
        /// </returns>
        /// <param name="ordinal">The zero-based column ordinal.</param><exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception><filterpriority>1</filterpriority>
        public override char GetChar(int ordinal)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads a stream of characters from the specified column, starting at location indicated by <paramref name="dataOffset"/>, into the buffer, starting at the location indicated by <paramref name="bufferOffset"/>.
        /// </summary>
        /// <returns>
        /// The actual number of characters read.
        /// </returns>
        /// <param name="ordinal">The zero-based column ordinal.</param><param name="dataOffset">The index within the row from which to begin the read operation.</param><param name="buffer">The buffer into which to copy the data.</param><param name="bufferOffset">The index with the buffer to which the data will be copied.</param><param name="length">The maximum number of characters to read.</param><filterpriority>1</filterpriority>
        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the value of the specified column as a globally-unique identifier (GUID).
        /// </summary>
        /// <returns>
        /// The value of the specified column.
        /// </returns>
        /// <param name="ordinal">The zero-based column ordinal.</param><exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception><filterpriority>1</filterpriority>
        public override Guid GetGuid(int ordinal)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the value of the specified column as a 16-bit signed integer.
        /// </summary>
        /// <returns>
        /// The value of the specified column.
        /// </returns>
        /// <param name="ordinal">The zero-based column ordinal.</param><exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception><filterpriority>2</filterpriority>
        public override short GetInt16(int ordinal)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the value of the specified column as a 32-bit signed integer.
        /// </summary>
        /// <returns>
        /// The value of the specified column.
        /// </returns>
        /// <param name="ordinal">The zero-based column ordinal.</param><exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception><filterpriority>1</filterpriority>
        public override int GetInt32(int ordinal)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the value of the specified column as a 64-bit signed integer.
        /// </summary>
        /// <returns>
        /// The value of the specified column.
        /// </returns>
        /// <param name="ordinal">The zero-based column ordinal.</param><exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception><filterpriority>2</filterpriority>
        public override long GetInt64(int ordinal)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the value of the specified column as a <see cref="T:System.DateTime"/> object.
        /// </summary>
        /// <returns>
        /// The value of the specified column.
        /// </returns>
        /// <param name="ordinal">The zero-based column ordinal.</param><exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception><filterpriority>1</filterpriority>
        public override DateTime GetDateTime(int ordinal)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the value of the specified column as an instance of <see cref="T:System.String"/>.
        /// </summary>
        /// <returns>
        /// The value of the specified column.
        /// </returns>
        /// <param name="ordinal">The zero-based column ordinal.</param><exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception><filterpriority>1</filterpriority>
        public override string GetString(int ordinal)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the value of the specified column as an instance of <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// The value of the specified column.
        /// </returns>
        /// <param name="ordinal">The zero-based column ordinal.</param><filterpriority>1</filterpriority>
        public override object GetValue(int ordinal)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Populates an array of objects with the column values of the current row.
        /// </summary>
        /// <returns>
        /// The number of instances of <see cref="T:System.Object"/> in the array.
        /// </returns>
        /// <param name="values">An array of <see cref="T:System.Object"/> into which to copy the attribute columns.</param><filterpriority>1</filterpriority>
        public override int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a value that indicates whether the column contains nonexistent or missing values.
        /// </summary>
        /// <returns>
        /// true if the specified column is equivalent to <see cref="T:System.DBNull"/>; otherwise false.
        /// </returns>
        /// <param name="ordinal">The zero-based column ordinal.</param><filterpriority>1</filterpriority>
        public override bool IsDBNull(int ordinal)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the number of columns in the current row.
        /// </summary>
        /// <returns>
        /// The number of columns in the current row.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">There is no current connection to an instance of SQL Server. </exception><filterpriority>1</filterpriority>
        public override int FieldCount
        {
            get { return 1; }
        }

        /// <summary>
        /// Gets the value of the specified column as an instance of <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// The value of the specified column.
        /// </returns>
        /// <param name="ordinal">The zero-based column ordinal.</param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. </exception><filterpriority>1</filterpriority>
        public override object this[int ordinal]
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the value of the specified column as an instance of <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// The value of the specified column.
        /// </returns>
        /// <param name="name">The name of the column.</param><exception cref="T:System.IndexOutOfRangeException">No column with the specified name was found. </exception><filterpriority>1</filterpriority>
        public override object this[string name]
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the value of the specified column as a <see cref="T:System.Decimal"/> object.
        /// </summary>
        /// <returns>
        /// The value of the specified column.
        /// </returns>
        /// <param name="ordinal">The zero-based column ordinal.</param><exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception><filterpriority>1</filterpriority>
        public override decimal GetDecimal(int ordinal)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the value of the specified column as a double-precision floating point number.
        /// </summary>
        /// <returns>
        /// The value of the specified column.
        /// </returns>
        /// <param name="ordinal">The zero-based column ordinal.</param><exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception><filterpriority>1</filterpriority>
        public override double GetDouble(int ordinal)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the value of the specified column as a single-precision floating point number.
        /// </summary>
        /// <returns>
        /// The value of the specified column.
        /// </returns>
        /// <param name="ordinal">The zero-based column ordinal.</param><exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception><filterpriority>2</filterpriority>
        public override float GetFloat(int ordinal)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a value that indicates whether this <see cref="T:System.Data.Common.DbDataReader"/> contains one or more rows.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Data.Common.DbDataReader"/> contains one or more rows; otherwise false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool HasRows
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the name of the column, given the zero-based column ordinal.
        /// </summary>
        /// <returns>
        /// The name of the specified column.
        /// </returns>
        /// <param name="ordinal">The zero-based column ordinal.</param><filterpriority>1</filterpriority>
        public override string GetName(int ordinal)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the column ordinal given the name of the column.
        /// </summary>
        /// <returns>
        /// The zero-based column ordinal.
        /// </returns>
        /// <param name="name">The name of the column.</param><exception cref="T:System.IndexOutOfRangeException">The name specified is not a valid column name.</exception><filterpriority>1</filterpriority>
        public override int GetOrdinal(string name)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets name of the data type of the specified column.
        /// </summary>
        /// <returns>
        /// A string representing the name of the data type.
        /// </returns>
        /// <param name="ordinal">The zero-based column ordinal.</param><exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception><filterpriority>1</filterpriority>
        public override string GetDataTypeName(int ordinal)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the data type of the specified column.
        /// </summary>
        /// <returns>
        /// The data type of the specified column.
        /// </returns>
        /// <param name="ordinal">The zero-based column ordinal.</param><exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception><filterpriority>1</filterpriority>
        public override Type GetFieldType(int ordinal)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns an <see cref="T:System.Collections.IEnumerator"/> that can be used to iterate through the rows in the data reader.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> that can be used to iterate through the rows in the data reader.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }


    }
}
