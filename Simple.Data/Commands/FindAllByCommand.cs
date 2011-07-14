using System;
using System.Collections.Generic;
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
            SimpleExpression criteriaExpression;
            if (binder.Name.Equals("FindAllBy") || binder.Name.Equals("find_all_by"))
            {
                criteriaExpression = ExpressionHelper.CriteriaDictionaryToExpression(table.GetQualifiedName(), binder.NamedArgumentsToDictionary(args));
            }
            else
            {
                criteriaExpression = ExpressionHelper.CriteriaDictionaryToExpression(table.GetQualifiedName(), MethodNameParser.ParseFromBinder(binder, args));
            }
            return new SimpleQuery(dataStrategy.GetAdapter(), table.GetQualifiedName()).Where(criteriaExpression);
        }

        public Func<object[], object> CreateDelegate(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            throw new NotImplementedException();
        }
    }
}
