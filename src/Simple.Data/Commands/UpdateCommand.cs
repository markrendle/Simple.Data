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
    using System.Collections.ObjectModel;
    using Operations;

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
            var newValuesList = newValues as IList<IReadOnlyDictionary<string, object>>;
            if (newValuesList != null)
            {
                var originalValuesList = ObjectToDictionary(args[1]) as IList<IReadOnlyDictionary<string, object>>;
                if (originalValuesList == null) throw new InvalidOperationException("Parameter type mismatch; both parameters to Update should be same type.");
                return dataStrategy.Run.Execute(new UpdateEntityOperation(table.GetQualifiedName(), newValuesList, originalValuesList));
            }

            var newValuesDict = newValues as IReadOnlyDictionary<string, object>;

            var originalValuesDict = ObjectToDictionary(args[1]) as IReadOnlyDictionary<string, object>;
            if (originalValuesDict == null) throw new InvalidOperationException("Parameter type mismatch; both parameters to Update should be same type.");
            return dataStrategy.Run.Execute(new UpdateEntityOperation(table.GetQualifiedName(), newValuesDict, originalValuesDict));
        }

        private static object UpdateUsingKeys(DataStrategy dataStrategy, DynamicTable table, object[] args)
        {
            var record = ObjectToDictionary(args[0]);
            var list = record as IList<IReadOnlyDictionary<string, object>>;
            if (list != null) return dataStrategy.Run.Execute(new UpdateEntityOperation(table.GetQualifiedName(), list));

            var dict = record as IReadOnlyDictionary<string, object>;
            if (dict == null) throw new InvalidOperationException("Could not resolve data from passed object.");
            return dataStrategy.Run.Execute(new UpdateEntityOperation(table.GetQualifiedName(), dict));
        }

        internal static object ObjectToDictionary(object obj)
        {
            var readOnlyDictionary = obj as IReadOnlyDictionary<string, object>;
            if (readOnlyDictionary != null)
            {
                return readOnlyDictionary;
            }

            var dictionary = obj as IDictionary<string, object>;
            if (dictionary != null)
            {
                return dictionary.ToReadOnly();
            }

            var list = obj as IEnumerable;
            if (list != null)
            {
                var originals = list.Cast<object>().ToList();
                var dictionaries = originals.Select(o => ObjectToDictionary(o) as IReadOnlyDictionary<string,object>).Where(o => o != null && o.Count > 0).ToList();
                if (originals.Count == dictionaries.Count)
                    return dictionaries;
            }

            return obj.ObjectToDictionary();
        }
    }
}
