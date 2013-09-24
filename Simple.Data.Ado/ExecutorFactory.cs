namespace Simple.Data.Ado
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Operations;
    using ExecuteFunc = System.Func<Operations.IOperation, AdoAdapter, AdoAdapterTransaction, OperationResult>;
    using FuncDict = System.Collections.Generic.Dictionary<System.Type, System.Func<Operations.IOperation, AdoAdapter, AdoAdapterTransaction, OperationResult>>;

    internal class ExecutorFactory
    {
        private static readonly FuncDict Functions = CreateFunctionDictionary();

        private static FuncDict CreateFunctionDictionary()
        {
            ICollection<KeyValuePair<Type, ExecuteFunc>> dict = new FuncDict();
            dict.Add(CreateFunction<QueryOperation>(ExecuteQuery));
            dict.Add(CreateFunction<InsertOperation>(ExecuteInsert));
            dict.Add(CreateFunction<UpdateEntityOperation>(ExecuteUpdateEntity));
            return (FuncDict)dict;
        }

        private static KeyValuePair<Type, ExecuteFunc> CreateFunction<T>(Func<T, AdoAdapter, AdoAdapterTransaction, OperationResult> target)
        {
            ExecuteFunc func = (o, a, t) => target((T)o, a, t);
            return new KeyValuePair<Type, ExecuteFunc>(typeof(T), func);
        }

        private static QueryResult ExecuteQuery(QueryOperation query, AdoAdapter adapter, AdoAdapterTransaction transaction)
        {
            var queryRunner = new AdoAdapterQueryRunner(adapter, transaction);
            IEnumerable<SimpleQueryClauseBase> unhandled;
            var data = queryRunner.RunQuery(query.Query, out unhandled);
            return new QueryResult(data, unhandled);
        }

        private static DataResult ExecuteInsert(InsertOperation operation, AdoAdapter adapter, AdoAdapterTransaction transaction)
        {
            var checkedEnumerable = CheckedEnumerable.Create(operation.Data);
            if (checkedEnumerable.IsEmpty) return DataResult.Empty;
            var inserter = transaction == null ?
                new AdoAdapterInserter(adapter)
                :
                new AdoAdapterInserter(adapter, transaction.DbTransaction);

            if (checkedEnumerable.HasMoreThanOneValue)
            {
                return new DataResult(
                    inserter.InsertMany(operation.TableName, checkedEnumerable, operation.ErrorCallback,
                        operation.ResultRequired));
            }
            return new DataResult(inserter.Insert(operation.TableName, checkedEnumerable.Single, operation.ResultRequired));
        }

        private static CommandResult ExecuteUpdateEntity(UpdateEntityOperation operation, AdoAdapter adapter,
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

            return new CommandResult(Update(adapter, operation.TableName, checkedEnumerable.Single, Adapter.GetCriteria(operation.TableName, keyFieldNames, ref dict), GetDbTransaction(transaction)));
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
                        operation.CriteriaFieldNames,
                        GetDbTransaction(transaction)));
            }
            return
                new CommandResult(bulkUpdater.Update(adapter, operation.TableName, checkedEnumerable,
                    GetDbTransaction(transaction)));
        }

        public bool TryGet(IOperation operation, out ExecuteFunc func)
        {
            return Functions.TryGetValue(operation.GetType(), out func);
        }

        private static IDbTransaction GetDbTransaction(AdoAdapterTransaction transaction)
        {
            return transaction != null ? transaction.DbTransaction : null;
        }
    }
}