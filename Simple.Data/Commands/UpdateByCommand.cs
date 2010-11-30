using System;
using System.Dynamic;
using System.Linq;
using Simple.Data.Extensions;

namespace Simple.Data.Commands
{
    class UpdateByCommand : ICommand
    {
        public bool IsCommandFor(string method)
        {
            return method.Homogenize().StartsWith("updateby", StringComparison.InvariantCultureIgnoreCase);
        }

        public object Execute(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            if (binder.HasSingleUnnamedArgument())
            {
                return UpdateCommand.UpdateByKeyFields(table.GetQualifiedName(), dataStrategy, args[0],
                                                MethodNameParser.ParseCriteriaNamesFromMethodName(binder.Name));
            }

            var criteria = MethodNameParser.ParseFromBinder(binder, args);
            var criteriaExpression = ExpressionHelper.CriteriaDictionaryToExpression(table.GetQualifiedName(), criteria);
            var data = binder.NamedArgumentsToDictionary(args)
                .Where(kvp => !criteria.ContainsKey(kvp.Key))
                .ToDictionary();
            return dataStrategy.Update(table.GetQualifiedName(), data, criteriaExpression);
        }
    }
}
