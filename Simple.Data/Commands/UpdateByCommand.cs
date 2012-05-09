using System;
using System.Dynamic;
using System.Linq;
using Simple.Data.Extensions;

namespace Simple.Data.Commands
{
    using System.Collections.Generic;

    class UpdateByCommand : ICommand
    {
        public bool IsCommandFor(string method)
        {
            return method.Homogenize().StartsWith("updateby", StringComparison.InvariantCultureIgnoreCase);
        }

        public object Execute(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            if (binder.HasSingleUnnamedArgument())
            {
                return UpdateByKeyFields(table.GetQualifiedName(), dataStrategy, args[0],
                                                MethodNameParser.ParseCriteriaNamesFromMethodName(binder.Name));
            }

            var criteria = MethodNameParser.ParseFromBinder(binder, args);
            var criteriaExpression = ExpressionHelper.CriteriaDictionaryToExpression(table.GetQualifiedName(), criteria);
            var data = binder.NamedArgumentsToDictionary(args)
                .Where(kvp => !criteria.ContainsKey(kvp.Key))
                .ToDictionary();
            return dataStrategy.Run.Update(table.GetQualifiedName(), data, criteriaExpression);
        }

        internal static object UpdateByKeyFields(string tableName, DataStrategy dataStrategy, object entity, IEnumerable<string> keyFieldNames)
        {
            var record = UpdateCommand.ObjectToDictionary(entity);
            var list = record as IList<IDictionary<string, object>>;
            if (list != null) return dataStrategy.Run.UpdateMany(tableName, list, keyFieldNames);

            var dict = record as IDictionary<string, object>;
            var criteria = GetCriteria(keyFieldNames, dict);
            var criteriaExpression = ExpressionHelper.CriteriaDictionaryToExpression(tableName, criteria);
            return dataStrategy.Run.Update(tableName, dict, criteriaExpression);
        }

        private static IEnumerable<KeyValuePair<string, object>> GetCriteria(IEnumerable<string> keyFieldNames, IDictionary<string, object> record)
        {
            var criteria = new Dictionary<string, object>();

            foreach (var keyFieldName in keyFieldNames)
            {
                var name = keyFieldName;
                var keyValuePair = record.SingleOrDefault(kvp => kvp.Key.Homogenize().Equals(name.Homogenize()));
                if (string.IsNullOrWhiteSpace(keyValuePair.Key))
                {
                    throw new InvalidOperationException("Key field value not set.");
                }

                criteria.Add(keyFieldName, keyValuePair.Value);
                record.Remove(keyValuePair);
            }
            return criteria;
        }
    }
}
