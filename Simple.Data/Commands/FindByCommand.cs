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

        public object Execute(Database database, string tableName, InvokeMemberBinder binder, object[] args)
        {
            var criteriaExpression = ExpressionHelper.CriteriaDictionaryToExpression(tableName, MethodNameParser.ParseFromBinder(binder, args));
            var data = database.Adapter.Find(tableName, criteriaExpression);
            return data != null ? new DynamicRecord(data, tableName, database) : null;
        }
    }
}
