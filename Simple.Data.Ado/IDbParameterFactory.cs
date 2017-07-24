namespace Shitty.Data.Ado
{
    using System;
    using System.Data;
    using Schema;

    public interface IDbParameterFactory
    {
        IDbDataParameter CreateParameter(string name);
        IDbDataParameter CreateParameter(string name, Column column);
        IDbDataParameter CreateParameter(string name, DbType dbType, int maxLength);
    }
}