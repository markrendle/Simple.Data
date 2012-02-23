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
        IEnumerable<ResultSet> Execute(IDictionary<string, object> suppliedParameters, IDbTransaction transaction);
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
            return Execute(suppliedParameters, null);
        }

        public IEnumerable<ResultSet> Execute(IDictionary<string, object> suppliedParameters, IDbTransaction transaction)
        {
            var procedure = _adapter.GetSchema().FindProcedure(_procedureName);
            if (procedure == null)
            {
                throw new UnresolvableObjectException(_procedureName.ToString());
            }

            var cn = transaction == null ? _adapter.CreateConnection() : transaction.Connection;
            using (cn.MaybeDisposable())
            using (var command = cn.CreateCommand())
            {
                command.Transaction = transaction;
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
            command.Connection.OpenIfClosed();
            using (var reader = command.ExecuteReader())
            {
                // Reader isn't always returned - added check to stop NullReferenceException
                if ((reader != null) && (reader.FieldCount > 0))
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
#if(DEBUG)
            Trace.TraceInformation("ExecuteNonQuery", "Simple.Data.SqlTest");
#endif
            command.Connection.OpenIfClosed();
            command.ExecuteNonQuery();
            return Enumerable.Empty<ResultSet>();
        }

        private static void SetParameters(Procedure procedure, IDbCommand cmd, IDictionary<string, object> suppliedParameters)
        {
            var returnParameter = procedure.Parameters.FirstOrDefault(p => p.Direction == ParameterDirection.ReturnValue);
            if (returnParameter!=null)
            {
                var cmdParameter = cmd.CreateParameter();
                cmdParameter.ParameterName = SimpleReturnParameterName;
                cmdParameter.Size = returnParameter.Size;
                cmdParameter.Direction = ParameterDirection.ReturnValue;
                cmdParameter.DbType = returnParameter.Dbtype;
                cmd.Parameters.Add(cmdParameter);
            }

            int i = 0;
            
            foreach (var parameter in procedure.Parameters.Where(p => p.Direction != ParameterDirection.ReturnValue))
            {
                //Tim Cartwright: Allows for case insensive parameters
                var value = suppliedParameters.FirstOrDefault(sp => 
                    sp.Key.Equals(parameter.Name.Replace("@", ""), StringComparison.InvariantCultureIgnoreCase)
                    || sp.Key.Equals("_" + i)
                );
                var cmdParameter = cmd.CreateParameter();
                //Tim Cartwright: method AddParameter does not allow for the "default" keyword to ever be passed into 
                //  parameters in stored procedures with defualt values. Null is always sent in. This will allow for default 
                //  values to work properly. Not sure why this is so, in both cases the value gets set. Just is.
                //var cmdParameter = cmd.AddParameter(parameter.Name, value);
                cmdParameter.ParameterName = parameter.Name;
                cmdParameter.Value = value.Value; 
                cmdParameter.Direction = parameter.Direction;
                //Tim Cartwright: I added size and dbtype so inout/out params would function properly.
                //not setting the proper dbtype and size with out put parameters causes the exception: "Size property has an invalid size of 0"
                // Mark: Just adding a quick check here so that if the Provider-specific type has been set by setting the value, this will not
                // override that.
                if (cmdParameter.DbType != parameter.Dbtype)
                {
                    cmdParameter.DbType = parameter.Dbtype;
                }
                cmdParameter.Size = parameter.Size;
                cmd.Parameters.Add(cmdParameter);
                i++;
            }
        }

    }
}
