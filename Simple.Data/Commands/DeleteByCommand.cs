using System;
using System.Collections.Generic;
using System.Dynamic;

namespace Simple.Data.Commands
{
    class DeleteByCommand : ICommand
    {
        public bool IsCommandFor(string method)
        {
            return method.Equals("delete", StringComparison.InvariantCultureIgnoreCase) || method.StartsWith("deleteby", StringComparison.InvariantCultureIgnoreCase);
        }

        public object Execute(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            SimpleExpression criteriaExpression = GetCriteriaExpression(binder, args, table);
            return dataStrategy.Delete(table.GetQualifiedName(), criteriaExpression);
        }

        public Func<object[], object> CreateDelegate(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            throw new NotImplementedException();
        }

        private static SimpleExpression GetCriteriaExpression(InvokeMemberBinder binder, object[] args, DynamicTable table)
        {
            var criteria = binder.Name.Equals("delete", StringComparison.InvariantCultureIgnoreCase) ?
                                                                                                         binder.NamedArgumentsToDictionary(args)
                               :
                                   MethodNameParser.ParseFromBinder(binder, args);

            return ExpressionHelper.CriteriaDictionaryToExpression(table.GetQualifiedName(), criteria);
        }
    }
}
