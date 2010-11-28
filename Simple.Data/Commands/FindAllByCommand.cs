using System;
using System.Dynamic;
using System.Linq;

namespace Simple.Data.Commands
{
    class FindAllByCommand : ICommand
    {
        public bool IsCommandFor(string method)
        {
            return method.StartsWith("FindAllBy") || method.StartsWith("find_all_by_", StringComparison.InvariantCultureIgnoreCase);
        }

        public object Execute(Database database, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            var criteria = ExpressionHelper.CriteriaDictionaryToExpression(table.GetQualifiedName(), MethodNameParser.ParseFromBinder(binder, args));
            var data = database.Adapter.Find(table.GetQualifiedName(), criteria);
            return new DynamicEnumerable(data != null
                         ? data.Select(dict => new DynamicRecord(dict, table.GetQualifiedName(), database))
                         : Enumerable.Empty<DynamicRecord>());
        }
    }
}
