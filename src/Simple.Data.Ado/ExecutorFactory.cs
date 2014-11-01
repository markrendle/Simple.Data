namespace Simple.Data.Ado
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Operations;
    using ExecuteFunc = System.Func<Operations.IOperation, AdoAdapter, AdoAdapterTransaction, System.Threading.Tasks.Task<OperationResult>>;
    using FuncDict = System.Collections.Generic.Dictionary<System.Type, System.Func<Operations.IOperation, AdoAdapter, AdoAdapterTransaction, System.Threading.Tasks.Task<OperationResult>>>;

    internal class ExecutorFactory
    {
        private static readonly FuncDict Functions = CreateFunctionDictionary();

        private static FuncDict CreateFunctionDictionary()
        {
            ICollection<KeyValuePair<Type, ExecuteFunc>> dict = new FuncDict();
            dict.Add(CreateFunction<QueryOperation>(QueryExecutor.ExecuteQuery));
            dict.Add(CreateFunction<InsertOperation>(InsertExecutor.ExecuteInsert));
            dict.Add(CreateFunction<UpdateEntityOperation>(UpdateEntityExecutor.ExecuteUpdateEntity));
            dict.Add(CreateFunction<DeleteOperation>(DeleteExecutor.Execute));
            dict.Add(CreateFunction<UpdateByCriteriaOperation>(UpdateByCriteriaExecutor.ExecuteUpdate));
            dict.Add(CreateFunction<FunctionOperation>(FunctionExecutor.ExecuteFunction));
            dict.Add(CreateFunction<UpsertOperation>(ExecuteUpsert));
            return (FuncDict)dict;
        }

        private static async Task<OperationResult> ExecuteUpsert(UpsertOperation operation, AdoAdapter adapter, AdoAdapterTransaction transaction)
        {
            var checkedEnumerable = CheckedEnumerable.Create(operation.Data);
            if (checkedEnumerable.IsEmpty) return CommandResult.Empty;
            var upserter = new AdoAdapterUpserter(adapter, transaction.TransactionOrDefault());
            DataResult dataResult;
            if (checkedEnumerable.HasMoreThanOneValue)
            {
                dataResult =
                    new DataResult(await upserter.UpsertMany(operation.TableName, checkedEnumerable.ToList(),
                        operation.ResultRequired,
                        (d, e) => operation.ErrorCallback(d, e)));
            }
            else
            {
                var row = checkedEnumerable.Single;
                dataResult = new DataResult(await upserter.Upsert(operation.TableName, checkedEnumerable.Single, operation.Criteria, operation.ResultRequired));
            }
            return dataResult;
        }

        private static KeyValuePair<Type, ExecuteFunc> CreateFunction<T>(Func<T, AdoAdapter, AdoAdapterTransaction, Task<OperationResult>> target)
        {
            ExecuteFunc func = (o, a, t) => target((T)o, a, t);
            return new KeyValuePair<Type, ExecuteFunc>(typeof(T), func);
        }

        public bool TryGet(IOperation operation, out ExecuteFunc func)
        {
            return Functions.TryGetValue(operation.GetType(), out func);
        }
    }
}