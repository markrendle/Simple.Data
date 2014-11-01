using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ResultSet = System.Collections.Generic.IEnumerable<System.Collections.Generic.IDictionary<string, object>>;

namespace Simple.Data.Ado
{
    using System.Threading.Tasks;

    public partial class AdoAdapter
	{
	    private readonly ConcurrentDictionary<string, IProcedureExecutor> _executors = new ConcurrentDictionary<string, IProcedureExecutor>();

	    public override bool IsValidFunction(string functionName)
	    {
	        return _connectionProvider.SupportsStoredProcedures && _schema.IsProcedure(functionName);
	    }

        public Task<IEnumerable<ResultSet>> Execute(string functionName, IDictionary<string, object> parameters, IAdapterTransaction transaction)
        {
            var executor = _executors.GetOrAdd(functionName, f => _connectionProvider.GetProcedureExecutor(this, _schema.BuildObjectName(f)));
            return transaction == null
                ? executor.Execute(parameters)
                : executor.Execute(parameters, ((AdoAdapterTransaction) transaction).DbTransaction);

        }
	}
}
