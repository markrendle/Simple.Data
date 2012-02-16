using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Simple.Data.Commands
{
    using Extensions;

    public class GetCommand : ICommand
    {
        public bool IsCommandFor(string method)
        {
            return method.Equals("get", StringComparison.OrdinalIgnoreCase) || method.Equals("getscalar", StringComparison.OrdinalIgnoreCase);
        }

        public object Execute(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            var result = dataStrategy.Get(table.GetName(), args);
            if (result == null || result.Count == 0) return null;
            return binder.Name.Equals("get", StringComparison.OrdinalIgnoreCase)
                       ? new SimpleRecord(result, table.GetQualifiedName(), dataStrategy)
                       : result.First().Value;
        }

        public object Execute(DataStrategy dataStrategy, SimpleQuery query, InvokeMemberBinder binder, object[] args)
        {
            var keyNames = dataStrategy.GetAdapter().GetKeyNames(query.TableName);
            var dict = keyNames.Select((k, i) => new KeyValuePair<string, object>(k, args[i]));
            query = query.Where(ExpressionHelper.CriteriaDictionaryToExpression(query.TableName, dict)).Take(1);
            var result = query.FirstOrDefault();
            if (result == null) return null;
            return binder.Name.Equals("get", StringComparison.OrdinalIgnoreCase) ? result : ((IDictionary<string, object>) result).First().Value;
        }

        public Func<object[], object> CreateDelegate(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            if (dataStrategy is SimpleTransaction) return null;

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
