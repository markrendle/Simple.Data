namespace Simple.Data.Commands
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using Extensions;

    internal class UpsertCommand : ICommand
    {
        public bool IsCommandFor(string method)
        {
            return method.Equals("upsert", StringComparison.InvariantCultureIgnoreCase);
        }

        public object Execute(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("Incorrect number of arguments to Update method.");
            }

            return UpsertUsingKeys(dataStrategy, table, args, !binder.IsResultDiscarded());
        }

        private static object UpsertUsingKeys(DataStrategy dataStrategy, DynamicTable table, object[] args, bool isResultRequired)
        {
            var record = ObjectToDictionary(args[0]);
            var list = record as IList<IDictionary<string, object>>;
            if (list != null) return dataStrategy.UpsertMany(table.GetQualifiedName(), list);

            var dict = record as IDictionary<string, object>;
            if (dict == null) throw new InvalidOperationException("Could not resolve data from passed object.");
            var key = dataStrategy.GetAdapter().GetKey(table.GetQualifiedName(), dict);
            dict = dict.Where(kvp => key.All(keyKvp => keyKvp.Key.Homogenize() != kvp.Key.Homogenize())).ToDictionary();
            var criteria = ExpressionHelper.CriteriaDictionaryToExpression(table.GetQualifiedName(), key);
            return dataStrategy.Upsert(table.GetQualifiedName(), dict, criteria, isResultRequired);
        }

        public Func<object[], object> CreateDelegate(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            throw new NotImplementedException();
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