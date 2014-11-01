namespace Simple.Data.Ado
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Operations;

    internal class UpdateEntityExecutor
    {
        public static async Task<OperationResult> ExecuteUpdateEntity(UpdateEntityOperation operation, AdoAdapter adapter,
            AdoAdapterTransaction transaction)
        {
            var checkedEnumerable = CheckedEnumerable.Create(operation.Data);
            if (checkedEnumerable.IsEmpty) return CommandResult.Empty;

            if (checkedEnumerable.HasMoreThanOneValue)
            {
                return BulkUpdateEntity(operation, adapter, transaction, checkedEnumerable);
            }

            SimpleExpression criteria;
            if (operation.Originals != null)
            {
                var newValuesDict = checkedEnumerable.Single;
                var originalValuesDict = operation.Originals.Single();
                criteria = CreateCriteriaFromOriginalValues(adapter, operation.TableName, newValuesDict, originalValuesDict);
                var changedValuesDict = CreateChangedValuesDict(newValuesDict, originalValuesDict);
                return new CommandResult(await Update(adapter, operation.TableName, changedValuesDict, criteria, transaction.TransactionOrDefault()));
            }

            string[] keyFieldNames = adapter.GetKeyNames(operation.TableName).ToArray();
            if (keyFieldNames.Length == 0) throw new AdoAdapterException(string.Format("No primary key found for implicit update of table '{0}'.", operation.TableName));
            var dict = checkedEnumerable.Single;

            criteria = Adapter.GetCriteria(operation.TableName, keyFieldNames, ref dict);
            return new CommandResult(await Update(adapter, operation.TableName, dict, criteria, transaction.TransactionOrDefault()));
        }

        private static Task<int> Update(AdoAdapter adapter, string tableName, IReadOnlyDictionary<string, object> data, SimpleExpression criteria, IDbTransaction transaction)
        {
            ICommandBuilder commandBuilder = new UpdateHelper(adapter.GetSchema()).GetUpdateCommand(tableName, data, criteria);
            return transaction == null ? adapter.Execute(commandBuilder) : adapter.Execute(commandBuilder, transaction);
        }

        private static CommandResult BulkUpdateEntity(UpdateEntityOperation operation, AdoAdapter adapter,
            AdoAdapterTransaction transaction, IEnumerable<IReadOnlyDictionary<string, object>> checkedEnumerable)
        {
            IBulkUpdater bulkUpdater = adapter.ProviderHelper.GetCustomProvider<IBulkUpdater>(adapter.ConnectionProvider) ??
                                       new BulkUpdater();
            if (operation.CriteriaFieldNames != null)
            {
                return
                    new CommandResult(bulkUpdater.Update(adapter, operation.TableName, checkedEnumerable,
                        operation.CriteriaFieldNames, transaction.TransactionOrDefault()));
            }
            return
                new CommandResult(bulkUpdater.Update(adapter, operation.TableName, checkedEnumerable,
                    transaction.TransactionOrDefault()));
        }

        private static Dictionary<string, object> CreateChangedValuesDict(
            IEnumerable<KeyValuePair<string, object>> newValuesDict, IReadOnlyDictionary<string, object> originalValuesDict)
        {
            var changedValuesDict =
                newValuesDict.Where(
                    kvp =>
                    (!originalValuesDict.ContainsKey(kvp.Key)) || !(Equals(kvp.Value, originalValuesDict[kvp.Key])))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return changedValuesDict;
        }

        private static SimpleExpression CreateCriteriaFromOriginalValues(AdoAdapter adapter, string tableName,
                                                                           IReadOnlyDictionary<string, object> newValuesDict,
                                                                           IReadOnlyDictionary<string, object>
                                                                               originalValuesDict)
        {
            var criteriaValues = adapter.GetKey(tableName, originalValuesDict).ToDictionary();

            foreach (var kvp in originalValuesDict
                .Where(
                    originalKvp =>
                    newValuesDict.ContainsKey(originalKvp.Key) &&
                    !(Equals(newValuesDict[originalKvp.Key], originalKvp.Value))))
            {
                if (!criteriaValues.ContainsKey(kvp.Key))
                {
                    criteriaValues.Add(kvp);
                }
            };

            return ExpressionHelper.CriteriaDictionaryToExpression(tableName, criteriaValues);
        }
    }
}