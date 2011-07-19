namespace Simple.Data.Ado
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq;

    static class DbCommandExtensions
    {
        public static IEnumerable<IDictionary<string, object>> ToEnumerable(this IDbCommand command, IDbConnection connection)
        {
            return ToEnumerable(command, connection, null);
        }

        public static IEnumerable<IDictionary<string, object>> ToEnumerable(this IDbCommand command, IDbConnection connection, IDictionary<string, int> index)
        {
            return new DataReaderEnumerator(command, connection, index).Wrap();
        }

        public static IObservable<IDictionary<string, object>> ToObservable(this IDbCommand command, IDbConnection connection, AdoAdapter adapter)
        {
            return ToObservable(command, connection, adapter, null);
        }

        public static IObservable<IDictionary<string, object>> ToObservable(this IDbCommand command, IDbConnection connection, AdoAdapter adapter, IDictionary<string, int> index)
        {
            var runner = adapter.ProviderHelper.GetCustomProvider<IObservableQueryRunner>(adapter.ConnectionProvider) ?? new ObservableQueryRunner();
            return runner.Run(command, connection, index);
        }

        public static IDbDataParameter AddParameter(this IDbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);
            return parameter;
        }

        public static IDataReader ExecuteReaderWithExceptionWrap(this IDbCommand command)
        {
            command.WriteTrace();
            try
            {
                return command.ExecuteReader();
            }
            catch (DbException ex)
            {
                throw new AdoAdapterException(ex.Message, command.CommandText,
                    command.Parameters.Cast<IDbDataParameter>()
                    .ToDictionary(p => p.ParameterName, p => p.Value));
            }
        }

        internal static void DisposeCommandAndReader(IDbConnection connection, IDbCommand command, IDataReader reader)
        {
            using (connection)
            using (command)
            using (reader)
            { /* NoOp */ }
        }
    }

    class EnumerableShim<T> : IEnumerable<T>
    {
        private readonly IEnumerator<T> _enumerator; 
        public EnumerableShim(IEnumerator<T> enumerator)
        {
            _enumerator = enumerator;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    static class EnumerableShim
    {
        public static IEnumerable<T> Wrap<T>(this IEnumerator<T> enumerator)
        {
            return new EnumerableShim<T>(enumerator);
        }
    }
}
