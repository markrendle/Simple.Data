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

        public static bool Execute(this Adapter adapter, string functionName, IEnumerable<string> parameterNames, IEnumerable<object> parameterValues, out object result)
        {
            var adapterWithFunctions = adapter as IAdapterWithFunctions;
            if (adapterWithFunctions == null) throw new NotSupportedException("Adapter does not support Function calls.");
            var command = new ExecuteFunctionCommand(adapterWithFunctions, functionName,
                                                                     parameterNames, parameterValues);
            return command.Execute(out result);
        }
    }
}
