using System;
using System.Dynamic;
using System.Linq;
using Simple.Data.Ado.Schema;
using Simple.Data.Extensions;

namespace Simple.Data.Commands
{
    class UpdateByCommand : ICommand
    {
        public bool IsCommandFor(string method)
        {
            return method.Homogenize().StartsWith("updateby", StringComparison.InvariantCultureIgnoreCase);
        }

        public object Execute(Database database, string tableName, InvokeMemberBinder binder, object[] args)
        {
            var criteria = MethodNameParser.ParseFromBinder(binder, args);
            var criteriaExpression = ExpressionHelper.CriteriaDictionaryToExpression(tableName, criteria);
            var data = binder.NamedArgumentsToDictionary(args)
                .Where(kvp => !criteria.ContainsKey(kvp.Key))
                .ToDictionary();
            return database.Adapter.Update(tableName, data, criteriaExpression);
        }
    }
}
