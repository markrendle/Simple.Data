using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using Simple.Data.Extensions;

namespace Simple.Data.Commands
{
    using System.Collections;

    internal class UpdateCommand : ICommand
    {
        public bool IsCommandFor(string method)
        {
            return method.Equals("update", StringComparison.InvariantCultureIgnoreCase);
        }

        public object Execute(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            if (args.Length == 0 || args.Length > 2) throw new ArgumentException("Incorrect number of arguments to Update method.");

            if (args.Length == 1)
            {
                return UpdateUsingKeys(dataStrategy, table, args);
            }

            return UpdateUsingOriginalValues(dataStrategy, table, args);
        }

        private static object UpdateUsingOriginalValues(DataStrategy dataStrategy, DynamicTable table, object[] args)
        {
            var newValues = ObjectToDictionary(args[0]);
            var newValuesList = newValues as IList<IDictionary<string, object>>;
            if (newValuesList != null)
            {
                var originalValuesList = ObjectToDictionary(args[1]) as IList<IDictionary<string, object>>;
                if (originalValuesList == null) throw new InvalidOperationException("Parameter type mismatch; both parameters to Update should be same type.");
                return dataStrategy.Run.UpdateMany(table.GetQualifiedName(), newValuesList, originalValuesList);
            }

            var newValuesDict = newValues as IDictionary<string, object>;
            var originalValuesDict = ObjectToDictionary(args[1]) as IDictionary<string, object>;
            if (originalValuesDict == null) throw new InvalidOperationException("Parameter type mismatch; both parameters to Update should be same type.");
            return dataStrategy.Run.Update(table.GetQualifiedName(), newValuesDict, originalValuesDict);
        }

        private static object UpdateUsingKeys(DataStrategy dataStrategy, DynamicTable table, object[] args)
        {
            var record = ObjectToDictionary(args[0]);
            var list = record as IList<IDictionary<string, object>>;
            if (list != null) return dataStrategy.Run.UpdateMany(table.GetQualifiedName(), list);

            var dict = record as IDictionary<string, object>;
            if (dict == null) throw new InvalidOperationException("Could not resolve data from passed object.");
            var key = dataStrategy.GetAdapter().GetKey(table.GetQualifiedName(), dict);
            dict = dict.Where(kvp => key.All(keyKvp => keyKvp.Key.Homogenize() != kvp.Key.Homogenize())).ToDictionary();
            var criteria = ExpressionHelper.CriteriaDictionaryToExpression(table.GetQualifiedName(), key);
            return dataStrategy.Run.Update(table.GetQualifiedName(), dict, criteria);
        }

        internal static object ObjectToDictionary(object obj)
        {
            var dynamicRecord = obj as SimpleRecord;
            if (dynamicRecord != null)
            {
                return new Dictionary<string, object>(dynamicRecord, HomogenizedEqualityComparer.DefaultInstance);
            }

            var dictionary = obj as IDictionary<string, object>;
            if (dictionary != null)
            {
                return dictionary;
            }

            var list = obj as IEnumerable;
            if (list != null)
            {
                var originals = list.Cast<object>().ToList();
                var dictionaries = originals.Select(o => ObjectToDictionary(o) as IDictionary<string,object>).Where(o => o != null && o.Count > 0).ToList();
                if (originals.Count == dictionaries.Count)
                    return dictionaries;
            }

            return obj.ObjectToDictionary();
        }
    }
}
