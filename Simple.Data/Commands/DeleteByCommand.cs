using System;
using System.Collections.Generic;
using System.Dynamic;

namespace Simple.Data.Commands
{
    using Extensions;

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

        public object Execute(DataStrategy dataStrategy, SimpleQuery query, InvokeMemberBinder binder, object[] args)
        {
            throw new NotImplementedException();
        }

        public Func<object[], object> CreateDelegate(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            throw new NotImplementedException();
        }

        private static SimpleExpression GetCriteriaExpression(InvokeMemberBinder binder, object[] args, DynamicTable table)
        {
            IDictionary<string, object> criteria;
            if (binder.Name.Equals("delete", StringComparison.InvariantCultureIgnoreCase))
            {
                criteria = binder.NamedArgumentsToDictionary(args);
                if (criteria.Count == 0 && args.Length == 1)
                {
                    criteria = args[0] as IDictionary<string, object> ?? args[0].ObjectToDictionary();
                }
            }
            else
            {
                criteria = MethodNameParser.ParseFromBinder(binder, args);
            }

            if (criteria.Count == 0) throw new InvalidOperationException("No criteria specified for Delete. To delete all data, use DeleteAll().");

            return ExpressionHelper.CriteriaDictionaryToExpression(table.GetQualifiedName(), criteria);
        }
    }
}
