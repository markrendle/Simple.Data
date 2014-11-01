using System;
using System.Dynamic;
using Simple.Data.Extensions;

namespace Simple.Data.Commands
{
    using Operations;

    class DeleteAllCommand : ICommand
    {
        public bool IsCommandFor(string method)
        {
            return method.Equals("deleteall", StringComparison.InvariantCultureIgnoreCase);
        }

        public object Execute(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            OperationResult result;

            if (args.Length == 0)
            {
                return new CommandResultAwaitable(new DeleteOperation(table.GetQualifiedName(), new SimpleEmptyExpression()), dataStrategy);
            }
            if (args.Length == 1 && args[0] is SimpleExpression)
            {
                return dataStrategy.Run.Execute(new DeleteOperation(table.GetQualifiedName(), (SimpleExpression)args[0]));
            }
            throw new InvalidOperationException();
        }
    }
}
