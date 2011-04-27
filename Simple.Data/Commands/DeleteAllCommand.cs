using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Simple.Data.Commands
{
    class DeleteAllCommand : ICommand
    {
        public bool IsCommandFor(string method)
        {
            return method.Equals("deleteall", StringComparison.InvariantCultureIgnoreCase);
        }

        public object Execute(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            if (args.Length == 1 && args[0] is SimpleExpression)
            {
                var deletedCount = dataStrategy.Delete(table.GetQualifiedName(), (SimpleExpression)args[0]);
                var simpleResultSet = new SimpleResultSet(Enumerable.Empty<SimpleRecord>());
                simpleResultSet.SetOutputValues(new Dictionary<string, object> { { "__ReturnValue", deletedCount } });
                return simpleResultSet;
            }

            return null;
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
