using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using Simple.Data.Extensions;

namespace Simple.Data.Ado
{
    static class DbCommandExtensions
    {
        public static IEnumerable<IDictionary<string,object>> ToBufferedEnumerable(this IDbCommand command, IDbConnection connection)
        {
            try
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();
            }
            catch (DbException ex)
            {
                throw new AdoAdapterException(ex.Message, ex);
            }
            var reader = command.ExecuteReaderWithExceptionWrap();
            var index = reader.CreateDictionaryIndex();
            return BufferedEnumerable.Create(() => reader.Read()
                                                       ? Maybe.Some(reader.ToDictionary(index))
                                                       : Maybe<IDictionary<string, object>>.None,
                                                       () => DisposeCommandAndReader(connection, command, reader));
        }

        public static Dictionary<string, int> CreateDictionaryIndex(this IDataReader reader)
        {
            var keys =
                reader.GetFieldNames().Select((s, i) => new KeyValuePair<string, int>(s, i)).ToDictionary();
            return new Dictionary<string, int>(keys, HomogenizedEqualityComparer.DefaultInstance);
        }

        public static IEnumerable<IDictionary<string, object>> ToAsyncEnumerable(this IDbCommand command)
        {
            if (command.Connection == null) throw new InvalidOperationException("Command has no connection.");
            return ToObservable(command).ToEnumerable();
        }

        public static IObservable<IDictionary<string, object>> ToObservable(this IDbCommand command)
        {
            return new ObservableDataReader(command);
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

        private static void DisposeCommandAndReader(IDbConnection connection, IDbCommand command, IDataReader reader)
        {
            using (command.Connection)
            using (command)
            using (reader)
            { /* NoOp */ }
        }
    }
}
