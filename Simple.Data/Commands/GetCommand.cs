using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Simple.Data.Commands
{
    public class GetCommand : ICommand
    {
        public bool IsCommandFor(string method)
        {
            return method.Equals("get", StringComparison.OrdinalIgnoreCase);
        }

        public object Execute(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            return dataStrategy.GetAdapter().Get(table.GetName(), args);
        }

        public Func<object[], object> CreateDelegate(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            var func = dataStrategy.GetAdapter().OptimizingDelegateFactory.CreateGetDelegate(dataStrategy.GetAdapter(),
                                                                                         table.GetName(), args);
                return a =>
                           {
                               var data = func(a);
                               return (data != null && data.Count > 0)
                                          ? new SimpleRecord(data, table.GetQualifiedName(), dataStrategy)
                                          : null;
                           };
        }
    }
}
