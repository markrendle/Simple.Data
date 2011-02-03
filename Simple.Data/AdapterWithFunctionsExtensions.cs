using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Data.Commands;

namespace Simple.Data
{
    //internal static class AdapterWithFunctionsExtensions
    //{
    //    public static bool IsValidFunction(this Adapter adapter, string functionName)
    //    {
    //        var adapterWithFunctions = adapter as IAdapterWithFunctions;
    //        if (adapterWithFunctions == null) return false;
    //        return adapterWithFunctions.IsValidFunction(functionName);
    //    }

    //    public static bool Execute(this Adapter adapter, string functionName, IDictionary<string,object> parameters, out object result)
    //    {
    //        var adapterWithFunctions = adapter as IAdapterWithFunctions;
    //        if (adapterWithFunctions == null) throw new NotSupportedException("Adapter does not support Function calls.");
    //        var command = new ExecuteFunctionCommand(adapterWithFunctions, functionName, parameters);
    //        return command.Execute(out result);
    //    }
    //}
}
