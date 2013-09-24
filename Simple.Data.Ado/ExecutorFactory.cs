namespace Simple.Data.Ado
{
    using System;
    using System.Collections.Generic;
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

        public bool TryGet(IOperation operation, out ExecuteFunc func)
        {
            return Functions.TryGetValue(operation.GetType(), out func);
        }
    }
}