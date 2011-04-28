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
            var deletedCount = 0;

            if (args.Length == 0)
            {
                deletedCount = dataStrategy.Delete(table.GetQualifiedName(), new SimpleEmptyExpression());
            }

            if (args.Length == 1 && args[0] is SimpleExpression)
            {
                deletedCount = dataStrategy.Delete(table.GetQualifiedName(), (SimpleExpression)args[0]);
            }

            return ConstructResultSet(deletedCount);
        }

        public Func<object[], object> CreateDelegate(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            throw new NotImplementedException();
        }

        private static object ConstructResultSet(int deletedCount)
        {
            var simpleResultSet = new SimpleResultSet(Enumerable.Empty<SimpleRecord>());
            simpleResultSet.SetOutputValues(new Dictionary<string, object> { { "__ReturnValue", deletedCount } });
            return simpleResultSet;
        }
    }
}
