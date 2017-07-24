namespace Shitty.Data.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using Extensions;

    class UpsertByCommand : ICommand
    {
        public bool IsCommandFor(string method)
        {
            return method.Homogenize().StartsWith("upsertby", StringComparison.InvariantCultureIgnoreCase);
        }

        public object Execute(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            object result;

            if (binder.HasSingleUnnamedArgument() || args.Length == 2 && args[1] is ErrorCallback)
            {
                result = UpsertByKeyFields(table.GetQualifiedName(), dataStrategy, args[0],
                                         MethodNameParser.ParseCriteriaNamesFromMethodName(binder.Name),
                                         !binder.IsResultDiscarded(),
                                         args.Length == 2 ? (ErrorCallback)args[1] : ((item, exception) => false));
            }
            else
            {
                var criteria = MethodNameParser.ParseFromBinder(binder, args);
                var criteriaExpression = ExpressionHelper.CriteriaDictionaryToExpression(table.GetQualifiedName(),
                                                                                         criteria);
                var data = binder.NamedArgumentsToDictionary(args);
                result = dataStrategy.Run.Upsert(table.GetQualifiedName(), data, criteriaExpression, !binder.IsResultDiscarded());
            }

            return ResultHelper.TypeResult(result, table, dataStrategy);
        }

        internal static object UpsertByKeyFields(string tableName, DataStrategy dataStrategy, object entity, IEnumerable<string> keyFieldNames, bool isResultRequired, ErrorCallback errorCallback)
        {
            var record = UpdateCommand.ObjectToDictionary(entity);
            var list = record as IList<IDictionary<string, object>>;
            if (list != null) return dataStrategy.Run.UpsertMany(tableName, list, keyFieldNames, isResultRequired, errorCallback);

            var dict = record as IDictionary<string, object>;
            var criteria = GetCriteria(keyFieldNames, dict);
            var criteriaExpression = ExpressionHelper.CriteriaDictionaryToExpression(tableName, criteria);
            return dataStrategy.Run.Upsert(tableName, dict, criteriaExpression, isResultRequired);
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
            }
            return criteria;
        }
    }
}