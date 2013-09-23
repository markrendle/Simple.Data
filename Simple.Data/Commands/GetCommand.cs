using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using Simple.Data.Operations;

namespace Simple.Data.Commands
{
    using Extensions;

    public class GetCommand : ICommand, ICreateDelegate, IQueryCompatibleCommand
    {
        public bool IsCommandFor(string method)
        {
            return method.Equals("get", StringComparison.OrdinalIgnoreCase) || method.Equals("getscalar", StringComparison.OrdinalIgnoreCase);
        }

        public object Execute(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            var result = (DataResult)dataStrategy.Run.Execute(new GetOperation(table.GetName(), args));
            if (result == null) return null;
            var record = result.Data.FirstOrDefault();
            if (record == null) return null;
            return binder.Name.Equals("get", StringComparison.OrdinalIgnoreCase)
                       ? new SimpleRecord(record, table.GetQualifiedName(), dataStrategy)
                       : record.First().Value;
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
            return null;
        }
    }
}
