using System;
using System.Data.Common;
using System.Linq;
using System.Data;

namespace Simple.Data.Mocking.Ado
{
    class MockDbCommand : DbCommand
    {
        private readonly MockDbConnection _connection;

        public MockDbCommand(MockDbConnection connection)
        {
            _connection = connection;
        }

        private readonly MockDataParameterCollection _parameters = new MockDataParameterCollection();

        protected override DbParameter CreateDbParameter()
        {
            return new MockDataParameter();
        }

        /// <summary>
        /// Executes the command text against the connection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.Common.DbDataReader"/>.
        /// </returns>
        /// <param name="behavior">An instance of <see cref="T:System.Data.CommandBehavior"/>.</param>
        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            _connection.MockDatabase.Record(this);
            if (_connection != null && _connection.DummyDataTable != null)
            {
                return _connection.DummyDataTable.CreateDataReader();
            }
            return new MockDataReader(Enumerable.Empty<IDataRecord>());
        }

        /// <summary>
        /// Attempts to cancels the execution of a <see cref="T:System.Data.Common.DbCommand"/>.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        public override void Cancel()
        {
            throw new NotImplementedException();
        }

        public override int ExecuteNonQuery()
        {
            _connection.MockDatabase.Record(this);
            return 1;
        }

        public override object ExecuteScalar()
        {
            _connection.MockDatabase.Record(this);
            return null;
        }

        /// <summary>
        /// Indicates or specifies how the <see cref="P:System.Data.Common.DbCommand.CommandText"/> property is interpreted.
        /// </summary>
        /// <returns>
        /// One of the <see cref="T:System.Data.CommandType"/> values. The default is Text.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override CommandType CommandType { get; set; }

        /// <summary>
        /// Gets or sets how command results are applied to the <see cref="T:System.Data.DataRow"/> when used by the Update method of a <see cref="T:System.Data.Common.DbDataAdapter"/>.
        /// </summary>
        /// <returns>
        /// One of the <see cref="T:System.Data.UpdateRowSource"/> values. The default is Both unless the command is automatically generated. Then the default is None.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override UpdateRowSource UpdatedRowSource { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="T:System.Data.Common.DbConnection"/> used by this <see cref="T:System.Data.Common.DbCommand"/>.
        /// </summary>
        /// <returns>
        /// The connection to the data source.
        /// </returns>
        protected override DbConnection DbConnection { get; set; }

        /// <summary>
        /// Gets the collection of <see cref="T:System.Data.Common.DbParameter"/> objects.
        /// </summary>
        /// <returns>
        /// The parameters of the SQL statement or stored procedure.
        /// </returns>
        protected override DbParameterCollection DbParameterCollection
        {
            get { return _parameters; }
        }

        /// <summary>
        /// Gets or sets the <see cref="P:System.Data.Common.DbCommand.DbTransaction"/> within which this <see cref="T:System.Data.Common.DbCommand"/> object executes.
        /// </summary>
        /// <returns>
        /// The transaction within which a Command object of a .NET Framework data provider executes. The default value is a null reference (Nothing in Visual Basic).
        /// </returns>
        protected override DbTransaction DbTransaction { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the command object should be visible in a customized interface control.
        /// </summary>
        /// <returns>
        /// true, if the command object should be visible in a control; otherwise false. The default is true.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override bool DesignTimeVisible { get; set; }

        public override void Prepare()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets or sets the text command to run against the data source.
        /// </summary>
        /// <returns>
        /// The text command to execute. The default value is an empty string ("").
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override string CommandText { get; set; }

        /// <summary>
        /// Gets or sets the wait time before terminating the attempt to execute a command and generating an error.
        /// </summary>
        /// <returns>
        /// The time in seconds to wait for the command to execute.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int CommandTimeout { get; set; }
    }
}
