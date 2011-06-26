using System;
using System.Collections.Concurrent;
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

        public Func<object[], object> CreateDelegate(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            if (dataStrategy is SimpleTransaction) return null;

            SimpleExpression criteriaExpression;
            if (binder.Name.Equals("FindBy") || binder.Name.Equals("find_by"))
            {
                criteriaExpression = ExpressionHelper.CriteriaDictionaryToExpression(table.GetQualifiedName(), binder.NamedArgumentsToDictionary(args));
            }
            else
            {
                criteriaExpression = ExpressionHelper.CriteriaDictionaryToExpression(table.GetQualifiedName(), MethodNameParser.ParseFromBinder(binder, args));
            }
            try
            {
                var func = dataStrategy.GetAdapter().CreateFindOneDelegate(table.GetQualifiedName(), criteriaExpression);
                return a =>
                           {
                               var data = func(a);
                               return (data != null && data.Count > 0)
                                          ? new SimpleRecord(data, table.GetQualifiedName(), dataStrategy)
                                          : null;
                           };
            }
            catch (NotImplementedException)
            {
                return null;
            }
        }

        public object Execute(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            var criteriaExpression = ExpressionHelper.CriteriaDictionaryToExpression(table.GetQualifiedName(), MethodNameParser.ParseFromBinder(binder, args));
            var data = dataStrategy.FindOne(table.GetQualifiedName(), criteriaExpression);
            return data != null ? new SimpleRecord(data, table.GetQualifiedName(), dataStrategy) : null;
        }
    }
}
