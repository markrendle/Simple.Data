using System;
using System.Dynamic;
using System.Linq;
using Simple.Data.Extensions;

namespace Simple.Data.Commands
{
    class UpdateAllCommand : ICommand
    {
        public bool IsCommandFor(string method)
        {
            return method.Equals("updateall", StringComparison.InvariantCultureIgnoreCase);
        }

        public object Execute(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            var criteria = args.OfType<SimpleExpression>().SingleOrDefault() ?? new SimpleEmptyExpression();

            var data = binder.NamedArgumentsToDictionary(args).Where(kv=>!(kv.Value is SimpleExpression)).ToDictionary();

            var updatedCount = dataStrategy.Update(table.GetQualifiedName(), data, criteria);
            
            return updatedCount.ResultSetFromModifiedRowCount();
        }

        public Func<object[], object> CreateDelegate(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            throw new NotImplementedException();
        }
    }
}
