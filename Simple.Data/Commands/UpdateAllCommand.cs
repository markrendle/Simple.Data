using System;
using System.Dynamic;
using System.Linq;
using Simple.Data.Extensions;

namespace Simple.Data.Commands
{
    using System.Collections.Generic;
    using Operations;

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

            if (data.Count == 0)
                data = args.OfType<IDictionary<string, object>>().SingleOrDefault();

            if (data == null)
            {
                throw new SimpleDataException("Could not resolve data.");
            }

            return 
                dataStrategy.Run.Execute(new UpdateByCriteriaOperation(table.GetQualifiedName(), criteria,
                    data.ToReadOnly()));
        }
    }
}
