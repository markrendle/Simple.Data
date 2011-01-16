using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Data.Commands;

namespace Simple.Data
{
    internal static class AdapterWithFunctionsExtensions
    {
        public static bool IsValidFunction(this Adapter adapter, string functionName)
        {
            var adapterWithFunctions = adapter as IAdapterWithFunctions;
            if (adapterWithFunctions == null) return false;
            return adapterWithFunctions.IsValidFunction(functionName);
        }

        public static bool Execute(this Adapter adapter, string functionName, IEnumerable<KeyValuePair<string,object>> parameters, out object result)
        {
            var adapterWithFunctions = adapter as IAdapterWithFunctions;
            if (adapterWithFunctions == null) throw new NotSupportedException("Adapter does not support Function calls.");
            var command = new ExecuteFunctionCommand(adapterWithFunctions, functionName, parameters);
            return command.Execute(out result);
        }

        public static Func<string, IEnumerable<KeyValuePair<string, object>>, object> GetRelevantFunction(this IAdapterWithFunctions adapter, FunctionReturnType resultType)
        {
            Func<string, IEnumerable<KeyValuePair<string, object>>, object> func;

            switch (resultType)
            {
                case FunctionReturnType.ResultSet:
                    func = adapter.ExecuteResultSet;
                    break;
                case FunctionReturnType.MultipleResultSets:
                    func = adapter.ExecuteMultipleResultSets;
                    break;
                default:
                    func = adapter.ExecuteScalar;
                    break;
            }
            return func;
        }
    }
}
