namespace Simple.Data.SqlServer
{
    using System.ComponentModel.Composition;
    using System.Data;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using Ado;

    [Export(typeof(ICommandExecutor))]
    public class SqlCommandExecutor : ICommandExecutor
    {
        private static readonly object Null = null;
        public async Task<IDataReader> ExecuteReader(IDbCommand command)
        {
            return await ((SqlCommand) command).ExecuteReaderAsync();
        }

        public Task<int> ExecuteNonQuery(IDbCommand command)
        {
            return ((SqlCommand) command).ExecuteNonQueryAsync();
        }

        public Task<object> ExecuteScalar(IDbCommand command)
        {
            return ((SqlCommand) command).ExecuteScalarAsync();
        }

        public Task<bool> NextResult(IDataReader reader)
        {
            return ((SqlDataReader) reader).NextResultAsync();
        }

        public Task OpenIfClosed(IDbConnection connection)
        {
            if (connection.State == ConnectionState.Closed)
            {
                return ((SqlConnection) connection).OpenAsync();
            }
            return Task.FromResult(Null);
        }
    }
}