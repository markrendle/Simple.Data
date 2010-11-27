using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace Simple.Data.Ado
{
    public static class DbCommandObservableEx
    {
        public static IEnumerable<IDictionary<string, object>> ToAsyncEnumerable(this DbCommand command)
        {
            if (command.Connection.State != ConnectionState.Closed) throw new InvalidOperationException(MethodBase.GetCurrentMethod().Name + " must be called with a closed DbConnection.");
            return ToObservable(command).ToEnumerable();
        }

        public static IObservable<IDictionary<string,object>> ToObservable(this DbCommand command)
        {
            if (command.Connection.State != ConnectionState.Closed) throw new InvalidOperationException(MethodBase.GetCurrentMethod().Name + " must be called with a closed DbConnection.");
            return new ObservableDataReader(command);
        }
    }
}