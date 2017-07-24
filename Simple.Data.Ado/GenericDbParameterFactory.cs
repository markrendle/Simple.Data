namespace Shitty.Data.Ado
{
    using System;
    using System.Data;
    using Schema;

    internal class GenericDbParameterFactory : IDbParameterFactory
    {
        private readonly IDbCommand _command;

        public GenericDbParameterFactory(IDbCommand command)
        {
            if (command == null) throw new ArgumentNullException("command");
            _command = command;
        }

        public IDbDataParameter CreateParameter(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            var parameter = _command.CreateParameter();
            parameter.ParameterName = name;
            return parameter;
        }

        public IDbDataParameter CreateParameter(string name, Column column)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (column == null) throw new ArgumentNullException("column");
            var parameter = _command.CreateParameter();
            parameter.ParameterName = name;
            parameter.DbType = column.DbType;
            parameter.Size = column.DbType == DbType.StringFixedLength || column.DbType == DbType.AnsiStringFixedLength ? 0 : column.MaxLength;
            parameter.SourceColumn = column.ActualName;
            return parameter;
        }

        public IDbDataParameter CreateParameter(string name, DbType dbType, int size)
        {
            if (name == null) throw new ArgumentNullException("name");
            var parameter = _command.CreateParameter();
            parameter.ParameterName = name;
            parameter.DbType = dbType;
            parameter.Size = size;
            return parameter;
        }
    }
}