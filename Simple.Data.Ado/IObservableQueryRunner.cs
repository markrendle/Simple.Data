using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shitty.Data.Ado
{
    using System.Data;
    using System.Data.Common;

    public interface IObservableQueryRunner
    {
        IObservable<IDictionary<string, object>> Run(IDbCommand command, IDbConnection connection, IDictionary<string,int> index);
    }

    class ObservableQueryRunner : IObservableQueryRunner
    {
        public IObservable<IDictionary<string, object>> Run(IDbCommand command, IDbConnection connection, IDictionary<string, int> index)
        {
            try
            {
                connection.OpenIfClosed();
            }
            catch (DbException ex)
            {
                throw new AdoAdapterException(ex.Message, ex);
            }
            var reader = command.TryExecuteReader();
            if (index == null) index = reader.CreateDictionaryIndex();

            return ColdObservable.Create<IDictionary<string, object>>(o => RunObservable(command, connection, reader, index, o));
        }

        private static IDisposable RunObservable(IDbCommand command, IDbConnection connection, IDataReader reader,
                                             IDictionary<string, int> index, IObserver<IDictionary<string, object>> o)
        {
            try
            {
                while (reader.Read())
                {
                    o.OnNext(reader.ToDictionary(index));
                }
                o.OnCompleted();
            }
            catch (Exception ex)
            {
                o.OnError(ex);
            }
            finally
            {
                DisposeCommandAndReader(connection, command, reader);
            }
            return ColdObservable.EmptyDisposable;
        }

        internal static void DisposeCommandAndReader(IDbConnection connection, IDbCommand command, IDataReader reader)
        {
            using (connection)
            using (command)
            using (reader)
            { /* NoOp */ }
        }
    }
}
