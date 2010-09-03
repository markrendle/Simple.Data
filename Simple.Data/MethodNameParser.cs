using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    internal static class MethodNameParser
    {
        internal static IDictionary<string, object> ParseFromBinder(InvokeMemberBinder binder, IList<object> args)
        {
            if (binder == null) throw new ArgumentNullException("binder");
            return ParseFromMethodName(binder.Name, args);
        }

        internal static IDictionary<string, object> ParseFromMethodName(string methodName, IList<object> args)
        {
            if (args == null) throw new ArgumentNullException("args");
            if (args.Count == 0) throw new ArgumentException("No parameters specified.");

            var columns = GetColumns(RemoveCommandPart(methodName));

            if (columns.Count == 0) throw new ArgumentException("No columns specified.");
            if (columns.Count != args.Count) throw new ArgumentException("Parameter count mismatch.");

            return columns.Select((s, i) => new KeyValuePair<string, object>(s, args[i])).ToDictionary();
           
        }

        internal static string RemoveCommandPart(string methodName)
        {
            if (methodName == null) throw new ArgumentNullException("methodName");
            if (!methodName.Contains("By")) return methodName;
            return methodName.Substring(methodName.IndexOf("By") + 2);
        }

        internal static IList<string> GetColumns(string methodName)
        {
            var columns = methodName.Split(new[] { "And" }, StringSplitOptions.RemoveEmptyEntries);
            return columns;
        }
    }
}
