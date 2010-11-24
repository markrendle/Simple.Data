using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Simple.Data.Ado
{
    static class DbCommandExtensions
    {
        public static IEnumerable<IDictionary<string, object>> ToAsyncEnumerable(this IDbCommand command)
        {
            if (command.Connection == null) throw new InvalidOperationException("Command has no connection.");
            if (command.Connection.State != ConnectionState.Closed) throw new InvalidOperationException(MethodBase.GetCurrentMethod().Name + " must be called with a closed DbConnection.");
            return ToObservable(command).ToEnumerable();
        }

        public static IObservable<IDictionary<string, object>> ToObservable(this IDbCommand command)
        {
            if (command.Connection.State != ConnectionState.Closed) throw new InvalidOperationException(MethodBase.GetCurrentMethod().Name + " must be called with a closed DbConnection.");
            return new ObservableDataReader(command);
        }
    }
}
