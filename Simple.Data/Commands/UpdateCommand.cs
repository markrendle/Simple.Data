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
            if (args.Length != 1) throw new ArgumentException("Incorrect number of arguments to Update method.");

            var record = ObjectToDictionary(args[0]);
            var list = record as IList<IDictionary<string, object>>;
            if (list != null) return dataStrategy.UpdateMany(table.GetQualifiedName(), list);

            var dict = record as IDictionary<string, object>;
            return dataStrategy.Update(table.GetQualifiedName(), dict);
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
