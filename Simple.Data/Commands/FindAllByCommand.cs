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

        public object Execute(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            var criteria = ExpressionHelper.CriteriaDictionaryToExpression(table.GetQualifiedName(), MethodNameParser.ParseFromBinder(binder, args));
            var data = dataStrategy.Find(table.GetQualifiedName(), criteria);
            return new SimpleResultSet(data != null
                         ? data.Where(dict => dict != null).Select(dict => new SimpleRecord(dict, table.GetQualifiedName(), dataStrategy))
                         : Enumerable.Empty<SimpleRecord>());
        }

        public Func<object[], object> CreateDelegate(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            throw new NotImplementedException();
        }
    }
}
