using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using Simple.Data.Extensions;

namespace Simple.Data
{
    static class BinderHelper
    {
        private static IDictionary<string, object> NamedArgumentsToDictionary(IEnumerable<string> argumentNames, IEnumerable<object> args)
        {
            return argumentNames
                .Reverse()
                .Zip(args.Reverse(), (k, v) => new KeyValuePair<string, object>(k, v))
                .Reverse()
                .ToDictionary(StringComparer.InvariantCultureIgnoreCase);
        }

        private static IDictionary<string, object> ArgumentsToDictionary(IEnumerable<String> argumentNames, IEnumerable<object> args)
        {
            var argsArray = args.ToArray();
            if (argsArray.Length == 1 && argsArray[0] is IDictionary<string, object>)
                return (IDictionary<string, object>)argsArray[0];

            return argsArray.Reverse()
                .Zip(argumentNames.Reverse().ExtendInfinite(), (v, k) => new KeyValuePair<string, object>(k, v))
                .Reverse()
                .Select((kvp, i) => kvp.Key == null ? new KeyValuePair<string, object>("_" + i.ToString(), kvp.Value ?? DBNull.Value) : kvp)
                .ToDictionary(StringComparer.InvariantCultureIgnoreCase);
        }

        internal static IDictionary<string, object> NamedArgumentsToDictionary(this InvokeMemberBinder binder, IEnumerable<object> args)
        {
            return NamedArgumentsToDictionary(binder.CallInfo.ArgumentNames, args);
        }

        public static IDictionary<string, object> ArgumentsToDictionary(this InvokeMemberBinder binder, IEnumerable<object> args)
        {
            return ArgumentsToDictionary(binder.CallInfo.ArgumentNames, args);
        }

        internal static IDictionary<string, object> NamedArgumentsToDictionary(this InvokeBinder binder, IEnumerable<object> args)
        {
            return NamedArgumentsToDictionary(binder.CallInfo.ArgumentNames, args);
        }

        public static IDictionary<string, object> ArgumentsToDictionary(this InvokeBinder binder, IEnumerable<object> args)
        {
            return ArgumentsToDictionary(binder.CallInfo.ArgumentNames, args);
        }
    }
}
