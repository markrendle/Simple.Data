namespace Simple.Data.Ado
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Dynamic;
    using System.Linq;

    static class DbCommandExtensions
    {
        public static IEnumerable<IDictionary<string, object>> ToEnumerable(this IDbCommand command, Func<IDbConnection> createConnection)
        {
            return ToEnumerable(command, createConnection, null);
        }

        public static IEnumerable<IEnumerable<IDictionary<string, object>>> ToEnumerables(this IDbCommand command, IDbConnection connection)
        {
            return new DataReaderMultipleEnumerator(command, connection).Wrap();
        }

        public static IEnumerable<IDictionary<string, object>> ToEnumerable(this IDbCommand command, Func<IDbConnection> createConnection, IDictionary<string, int> index)
        {
            return new DataReaderEnumerable(command, createConnection, index);
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
            parameter.Value = FixObjectType(value);
            command.Parameters.Add(parameter);
            return parameter;
        }

        private static object FixObjectType(object value)
        {
            if (value == null) return DBNull.Value;
            if (TypeHelper.IsKnownType(value.GetType())) return value;
            var dynamicObject = value as DynamicObject;
            if (dynamicObject != null)
            {
                return dynamicObject.ToString();
            }
            return value;
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

        public static void SetParameterValues(this IDbCommand command, IList<object> values)
        {
            int index = 0;
            foreach (var parameter in command.Parameters.Cast<IDbDataParameter>())
            {
                parameter.Value = CommandHelper.FixObjectType(values[index]);
                index++;
            }
        }

        public static void ClearParameterValues(this IDbCommand command)
        {
            foreach (var parameter in command.Parameters.Cast<IDbDataParameter>())
            {
                parameter.Value = DBNull.Value;
            }
        }

        public static void SetParameterValue(this IDbCommand command, int index, object value)
        {
            ((IDbDataParameter) command.Parameters[index]).Value = CommandHelper.FixObjectType(value);
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
