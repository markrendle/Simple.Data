using System;
using System.Dynamic;
using Shitty.Data.Extensions;

namespace Shitty.Data.Commands
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
                deletedCount = dataStrategy.Run.Delete(table.GetQualifiedName(), new SimpleEmptyExpression());
            }

            if (args.Length == 1 && args[0] is SimpleExpression)
            {
                deletedCount = dataStrategy.Run.Delete(table.GetQualifiedName(), (SimpleExpression)args[0]);
            }

            return deletedCount.ResultSetFromModifiedRowCount();
        }
    }
}
