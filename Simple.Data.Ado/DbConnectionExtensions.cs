using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Simple.Data.Ado
{
public static class DbConnectionExtensions
{
    public static DataTable GetSchema(this IDbConnection connection, string collectionName, params string[] constraints)
    {
        var adoConnection = connection as DbConnection;

        if (adoConnection != null)
            return adoConnection.GetSchema(collectionName, constraints);

        var schemaGetter = connection as ISchemaGetter;

        if (schemaGetter != null)
        {
            return schemaGetter.GetSchema(collectionName, constraints);
        }

        throw new InvalidOperationException(string.Format("The IDbConnection type {0} does not provide a GetSchema method.", connection.GetType().Name));
    }
}
}
