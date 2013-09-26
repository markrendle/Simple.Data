namespace Simple.Data.Ado
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Operations;

    internal class UpdateEntityExecutor
    {
        public static CommandResult ExecuteUpdateEntity(UpdateEntityOperation operation, AdoAdapter adapter,
            AdoAdapterTransaction transaction)
        {
            var checkedEnumerable = CheckedEnumerable.Create(operation.Data);
            if (checkedEnumerable.IsEmpty) return CommandResult.Empty;

            if (checkedEnumerable.HasMoreThanOneValue)
            {
                return BulkUpdateEntity(operation, adapter, transaction, checkedEnumerable);
            }

            string[] keyFieldNames = adapter.GetKeyNames(operation.TableName).ToArray();
            if (keyFieldNames.Length == 0) throw new AdoAdapterException(string.Format("No primary key found for implicit update of table '{0}'.", operation.TableName));
            var dict = checkedEnumerable.Single;

            return new CommandResult(Update(adapter, operation.TableName, checkedEnumerable.Single, Adapter.GetCriteria(operation.TableName, keyFieldNames, ref dict), transaction.TransactionOrDefault()));
        }

        private static int Update(AdoAdapter adapter, string tableName, IReadOnlyDictionary<string, object> data, SimpleExpression criteria, IDbTransaction transaction)
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
    }
}