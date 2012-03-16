using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using Simple.Data.Extensions;

namespace Simple.Data
{
    internal static class MethodNameParser
    {
        internal static IDictionary<string, object> ParseFromBinder(InvokeMemberBinder binder, IList<object> args)
        {
            if (binder == null) throw new ArgumentNullException("binder");
            if (binder.CallInfo.ArgumentNames != null && binder.CallInfo.ArgumentNames.Count > 0)
            {
                return ParseFromMethodName(binder.Name, binder.NamedArgumentsToDictionary(args));
            }
            if (binder.Name.Equals("findby", StringComparison.OrdinalIgnoreCase) || binder.Name.Equals("find_by", StringComparison.OrdinalIgnoreCase))
            {
                if (args.Count == 1)
                {
                    return args[0].ObjectToDictionary();
                }
                throw new InvalidOperationException("Invalid criteria specification.");
            }
            return ParseFromMethodName(binder.Name, args);
        }

        internal static IDictionary<string, object> ParseFromMethodName(string methodName, IList<object> args)
        {
            if (args == null) throw new ArgumentNullException("args");
            if (args.Count == 0) throw new ArgumentException("No parameters specified.");

            var columns = GetColumns(RemoveCommandPart(methodName));

            if (columns.Count == 0) throw new ArgumentException("No columns specified.");

            return columns.Select((s,i) => new KeyValuePair<string, object>(s, args[i])).ToDictionary();
        }

        internal static IEnumerable<string> ParseCriteriaNamesFromMethodName(string methodName)
        {
            var columns = GetColumns(RemoveCommandPart(methodName));

            if (columns.Count == 0) throw new ArgumentException("No columns specified.");

            return columns.AsEnumerable();
        }

        internal static IDictionary<string, object> ParseFromMethodName(string methodName, IDictionary<string, object> args)
        {
            if (args == null) throw new ArgumentNullException("args");
            if (args.Count == 0) throw new ArgumentException("No parameters specified.");

            var columns = GetColumns(RemoveCommandPart(methodName));

            if (columns.Count == 0) throw new ArgumentException("No columns specified.");

            return columns.Select(s => new KeyValuePair<string, object>(s, args[s])).ToDictionary();

        }

        internal static string RemoveCommandPart(string methodName)
        {
            if (methodName == null) throw new ArgumentNullException("methodName");
            if (!methodName.Contains("By")) return methodName;
            return methodName.Substring(methodName.IndexOf("By", StringComparison.Ordinal) + 2);
        }

        internal static IList<string> GetColumns(string methodName)
        {
            var columns = methodName.Split(new[] { "And" }, StringSplitOptions.RemoveEmptyEntries);
            return columns;
        }
    }
}
