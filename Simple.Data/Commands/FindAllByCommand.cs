using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Shitty.Data.Commands
{
    class FindAllByCommand : ICommand
    {
        public bool IsCommandFor(string method)
        {
            return method.StartsWith("FindAllBy") || method.StartsWith("find_all_by_", StringComparison.InvariantCultureIgnoreCase);
        }

        public object Execute(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            if (binder.Name.Equals("FindAllBy") || binder.Name.Equals("find_all_by"))
            {
                ArgumentHelper.CheckFindArgs(args, binder);
            }

            var criteriaDictionary = ArgumentHelper.CreateCriteriaDictionary(binder, args, "FindAllBy", "find_all_by");
            var criteriaExpression = ExpressionHelper.CriteriaDictionaryToExpression(table.GetQualifiedName(),
                                                                                                  criteriaDictionary);
            return new SimpleQuery(dataStrategy, table.GetQualifiedName()).Where(criteriaExpression);
        }
    }
}
