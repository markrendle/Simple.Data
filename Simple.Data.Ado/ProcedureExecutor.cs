using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using Simple.Data.Extensions;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Ado
{
    internal class ProcedureExecutor
    {
        private readonly AdoAdapter _adapter;
        private readonly ObjectName _procedureName;

        public ProcedureExecutor(AdoAdapter adapter, ObjectName procedureName)
        {
            _adapter = adapter;
            _procedureName = procedureName;
        }

        public IEnumerable<IEnumerable<KeyValuePair<string, object>>> Execute(IEnumerable<KeyValuePair<string, object>> suppliedParameters)
        {
            return Execute(suppliedParameters.ToDictionary());
        }

        public IEnumerable<IEnumerable<KeyValuePair<string, object>>> Execute(IDictionary<string, object> suppliedParameters)
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
                    return command.ToAsyncEnumerable();
                }
                catch (DbException ex)
                {
                    throw new AdoAdapterException(ex.Message, command);
                }
            }
        }

        private static void SetParameters(Procedure procedure, DbCommand cmd, IDictionary<string, object> suppliedParameters)
        {
            int i = 0;
            foreach (var parameter in procedure.Parameters)
            {
                object value;
                if (!suppliedParameters.TryGetValue(parameter.Name.Replace("@", ""), out value))
                {
                    suppliedParameters.TryGetValue(i.ToString(), out value);
                }
                cmd.AddParameter(parameter.Name, value);
                i++;
            }
        }
    }
}
