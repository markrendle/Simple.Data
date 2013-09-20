namespace Simple.Data.Commands
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using Extensions;
    using Operations;

    internal class UpsertCommand : ICommand
    {
        public bool IsCommandFor(string method)
        {
            return method.Equals("upsert", StringComparison.InvariantCultureIgnoreCase);
        }

        public object Execute(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            object[] objects;
            if (binder.CallInfo.ArgumentNames.Count > 0 && binder.CallInfo.ArgumentNames.All(s => !string.IsNullOrWhiteSpace(s)))
            {
                objects = new object[] {binder.NamedArgumentsToDictionary(args)};
            }
            else if (args.Length == 0 || args.Length > 2)
            {
                throw new ArgumentException("Incorrect number of arguments to Upsert method.");
            }
            else
            {
                objects = args;
            }

            var result = UpsertUsingKeys(dataStrategy, table, objects, !binder.IsResultDiscarded());

            return ResultHelper.TypeResult(result, table, dataStrategy);
        }

        private static object UpsertUsingKeys(DataStrategy dataStrategy, DynamicTable table, object[] args, bool isResultRequired)
        {
            var record = ObjectToDictionary(args[0]);
            var list = record as IList<IReadOnlyDictionary<string, object>>;
            if (list != null)
            {
                ErrorCallback errorCallback = (args.Length == 2 ? args[1] as ErrorCallback : null) ??
                 ((item, exception) => false);
                return dataStrategy.Run.Upsert(new UpsertOperation(table.GetQualifiedName(), list, isResultRequired, null, errorCallback));
            }

            var dict = record as IReadOnlyDictionary<string, object>;
            if (dict == null) throw new InvalidOperationException("Could not resolve data from passed object.");
            var key = dataStrategy.GetAdapter().GetKey(table.GetQualifiedName(), dict);
            if (key == null) throw new InvalidOperationException(string.Format("No key columns defined for table \"{0}\"",table.GetQualifiedName()));
            if (key.Count == 0) return dataStrategy.Run.Insert(new InsertOperation(table.GetQualifiedName(), dict, isResultRequired));
            var criteria = ExpressionHelper.CriteriaDictionaryToExpression(table.GetQualifiedName(), key);
            return dataStrategy.Run.Upsert(new UpsertOperation(table.GetQualifiedName(), dict, isResultRequired));
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