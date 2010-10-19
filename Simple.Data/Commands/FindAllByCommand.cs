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

        public object Execute(Database database, string tableName, InvokeMemberBinder binder, object[] args)
        {
            var criteria = ExpressionHelper.CriteriaDictionaryToExpression(tableName, MethodNameParser.ParseFromBinder(binder, args));
            var data = database.Adapter.FindAll(tableName, criteria);
            return data != null
                         ? data.Select(dict => new DynamicRecord(dict, tableName, database))
                         : Enumerable.Empty<object>();
        }
    }
}
