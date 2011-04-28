using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Simple.Data.Commands
{
    class FindAllByCommand : ICommand
    {
        public bool IsCommandFor(string method)
        {
            return method.StartsWith("FindAllBy") || method.StartsWith("find_all_by_", StringComparison.InvariantCultureIgnoreCase);
        }

        public object Execute(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            var criteria = ExpressionHelper.CriteriaDictionaryToExpression(table.GetQualifiedName(), MethodNameParser.ParseFromBinder(binder, args));
            return new SimpleQuery(dataStrategy.Adapter, table.GetQualifiedName()).Where(criteria);
            //var data = dataStrategy.Find(table.GetQualifiedName(), criteria);
            //return CreateSimpleResultSet(table, dataStrategy, data);
        }

        private static object CreateSimpleResultSet(DynamicTable table, DataStrategy dataStrategy, IEnumerable<IDictionary<string, object>> data)
        {
            return new SimpleResultSet(data != null
                                           ? data.Where(dict => dict != null).Select(dict => new SimpleRecord(dict, table.GetQualifiedName(), dataStrategy))
                                           : Enumerable.Empty<SimpleRecord>());
        }

        public Func<object[], object> CreateDelegate(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            throw new NotImplementedException();
            var criteriaExpression = ExpressionHelper.CriteriaDictionaryToExpression(table.GetQualifiedName(), MethodNameParser.ParseFromBinder(binder, args));
            try
            {
                var func = dataStrategy.Adapter.CreateFindDelegate(table.GetQualifiedName(), criteriaExpression);
                return a => CreateSimpleResultSet(table, dataStrategy, func(a));
            }
            catch (NotImplementedException)
            {
                return null;
            }
        }
    }
}
