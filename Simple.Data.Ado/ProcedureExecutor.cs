using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Simple.Data.Extensions;
using Simple.Data.Ado.Schema;
using ResultSet = System.Collections.Generic.IEnumerable<System.Collections.Generic.IDictionary<string, object>>;

namespace Simple.Data.Ado
{
    public interface IProcedureExecutor
    {
        IEnumerable<ResultSet> Execute(IDictionary<string, object> suppliedParameters);
        IEnumerable<ResultSet> ExecuteReader(IDbCommand command);
    }

    public class ProcedureExecutor : IProcedureExecutor
    {
        private const string SimpleReturnParameterName = "@__Simple_ReturnValue";

        private readonly AdoAdapter _adapter;
        private readonly ObjectName _procedureName;
        private Func<IDbCommand, IEnumerable<ResultSet>> _executeImpl;

        public ProcedureExecutor(AdoAdapter adapter, ObjectName procedureName)
        {
            _adapter = adapter;
            _procedureName = procedureName;
            _executeImpl = ExecuteReader;
        }

        public IEnumerable<ResultSet> Execute(IDictionary<string, object> suppliedParameters)
        {
            var procedure = _adapter.GetSchema().FindProcedure(_procedureName);
            if (procedure == null)
            {
                throw new UnresolvableObjectException(_procedureName.ToString());
            }

            using (var cn = _adapter.CreateConnection())
            using (var command = cn.CreateCommand())
            {
                command.CommandText = procedure.QuotedName;
                command.CommandType = CommandType.StoredProcedure;
                SetParameters(procedure, command, suppliedParameters);
                try
                {
                    var result = _executeImpl(command);
                    suppliedParameters["__ReturnValue"] = command.Parameters.GetValue(SimpleReturnParameterName);
                    RetrieveOutputParameterValues(procedure, command, suppliedParameters);
                    return result;
                }
                catch (DbException ex)
                {
                    throw new AdoAdapterException(ex.Message, command);
                }
            }
        }

        private static void RetrieveOutputParameterValues(Procedure procedure, IDbCommand command, IDictionary<string, object> suppliedParameters)
        {
            foreach (var outputParameter in procedure.Parameters.Where(p => p.Direction == ParameterDirection.InputOutput || p.Direction == ParameterDirection.Output))
            {
                suppliedParameters[outputParameter.Name.Replace("@", "")] =
                    command.Parameters.GetValue(outputParameter.Name);
            }
        }

        public IEnumerable<ResultSet> ExecuteReader(IDbCommand command)
        {
            command.Connection.Open();
            using (var reader = command.ExecuteReader())
            {
                if (reader.FieldCount > 0)
                {
                    return reader.ToMultipleDictionaries();
                }

                // Don't call ExecuteReader for this function again.
                _executeImpl = ExecuteNonQuery;
                return Enumerable.Empty<ResultSet>();
            }
        }

        private static IEnumerable<ResultSet> ExecuteNonQuery(IDbCommand command)
        {
            Trace.TraceInformation("ExecuteNonQuery");
            command.Connection.Open();
            command.ExecuteNonQuery();
            return Enumerable.Empty<ResultSet>();
        }

        private static void SetParameters(Procedure procedure, IDbCommand cmd, IDictionary<string, object> suppliedParameters)
        {
            AddReturnParameter(cmd);

            int i = 0;
            foreach (var parameter in procedure.Parameters)
            {
                object value;
                if (!suppliedParameters.TryGetValue(parameter.Name.Replace("@", ""), out value))
                {
                    suppliedParameters.TryGetValue("_" + i, out value);
                }
                cmd.AddParameter(parameter.Name, value);
                i++;
            }
        }

        private static void AddReturnParameter(IDbCommand cmd)
        {
            var returnParameter = cmd.CreateParameter();
            returnParameter.ParameterName = SimpleReturnParameterName;
            returnParameter.Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add(returnParameter);
        }
    }
}
