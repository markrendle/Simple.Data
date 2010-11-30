using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Simple.Data.Commands
{
    class FindByCommand : ICommand
    {
        public bool IsCommandFor(string method)
        {
            return method.StartsWith("FindBy") || method.StartsWith("find_by_", StringComparison.InvariantCultureIgnoreCase);
        }

        public object Execute(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            var criteriaExpression = ExpressionHelper.CriteriaDictionaryToExpression(table.GetQualifiedName(), MethodNameParser.ParseFromBinder(binder, args));
            var data = dataStrategy.Find(table.GetQualifiedName(), criteriaExpression).FirstOrDefault();
            return data != null ? new DynamicRecord(data, table.GetQualifiedName(), dataStrategy) : null;
        }
    }
}
