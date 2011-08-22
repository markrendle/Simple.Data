namespace Simple.Data.SqlCe40
{
    using System.ComponentModel.Composition;
    using System.Data;
    using System.Data.SqlServerCe;
    using Ado;
    using Ado.Schema;

    [Export(typeof(IDbParameterFactory))]
    public class SqlCeDbParameterFactory : IDbParameterFactory
    {
        public IDbDataParameter CreateParameter(string name, Column column)
        {
            var sqlCeColumn = (SqlCeColumn) column;
            return new SqlCeParameter(name, sqlCeColumn.SqlDbType, column.MaxLength, column.ActualName);
        }

        public IDbDataParameter CreateParameter(string name, DbType dbType, int maxLength)
        {
            IDbDataParameter parameter = new SqlCeParameter
                       {
                           ParameterName = name,
                           Size = maxLength
                       };
            parameter.DbType = dbType;
            return parameter;
        }
    }
}