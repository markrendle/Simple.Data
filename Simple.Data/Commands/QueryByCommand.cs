using System;
using System.Dynamic;

namespace Shitty.Data.Commands
{
    class QueryByCommand : ICommand
    {
        public bool IsCommandFor(string method)
        {
            return method.StartsWith("QueryBy") || method.StartsWith("query_by_", StringComparison.InvariantCultureIgnoreCase);
        }

        public object Execute(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            return CreateSimpleQuery(table, binder, args, dataStrategy);
        }

        private static object CreateSimpleQuery(DynamicTable table, InvokeMemberBinder binder, object[] args, DataStrategy dataStrategy)
        {
            var criteriaExpression = ExpressionHelper.CriteriaDictionaryToExpression(table.GetQualifiedName(), MethodNameParser.ParseFromBinder(binder, args));
            return new SimpleQuery(dataStrategy, table.GetQualifiedName()).Where(criteriaExpression);
        }
    }
}