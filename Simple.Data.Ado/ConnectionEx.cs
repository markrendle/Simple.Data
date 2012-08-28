using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Ado
{
    using System.Data;

    public static class ConnectionEx
    {
        public static IDisposable MaybeDisposable(this IDbConnection connection)
        {
            if (connection == null || connection.State == ConnectionState.Open) return ActionDisposable.NoOp;
            return new ActionDisposable(connection.Dispose);
        }
    }
}
