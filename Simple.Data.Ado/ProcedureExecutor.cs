using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
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
                command.CommandText = procedure.QualifiedName;
                command.CommandType = CommandType.StoredProcedure;
                SetParameters(procedure, command, suppliedParameters);
                try
                {
                    var result = _executeImpl(command);
                    if (command.Parameters.Contains(SimpleReturnParameterName))
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
            command.WriteTrace();
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
            command.WriteTrace();
            Trace.TraceInformation("ExecuteNonQuery", "Simple.Data.SqlTest");
            command.Connection.Open();
            command.ExecuteNonQuery();
            return Enumerable.Empty<ResultSet>();
        }

        private static void SetParameters(Procedure procedure, IDbCommand cmd, IDictionary<string, object> suppliedParameters)
        {
            if (procedure.Parameters.Any(p => p.Direction == ParameterDirection.ReturnValue))
                AddReturnParameter(cmd);

            int i = 0;
            
            foreach (var parameter in procedure.Parameters.Where(p => p.Direction != ParameterDirection.ReturnValue))
            {
                //Tim Cartwright: Allows for case insensive parameters
                var value = suppliedParameters.FirstOrDefault(sp => 
                    sp.Key.Equals(parameter.Name.Replace("@", ""), StringComparison.InvariantCultureIgnoreCase)
                    || sp.Key.Equals("_" + i)
                );
                var cmdParameter = cmd.CreateParameter();
                //Tim Cartwright: Using AddParameter does not allow for the "default" keyword to ever be passed into 
                //  parameters in stored procedures with defualt values. Null is always sent in. This will allow for default 
                //  values to work properly. Not sure why this is so, in both cases the value gets set. Just is.
                //var cmdParameter = cmd.AddParameter(parameter.Name, value);
                cmdParameter.ParameterName = parameter.Name;
                cmdParameter.Value = value.Value; 
                cmdParameter.Direction = parameter.Direction;
                //Tim Cartwright: I added size and dbtype so inout/out params would function properly.
                //not setting the proper dbtype and size with out put parameters causes the exception: "Size property has an invalid size of 0"
                cmdParameter.DbType = parameter.Dbtype;
                cmdParameter.Size = parameter.Size;
                cmd.Parameters.Add(cmdParameter);
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
