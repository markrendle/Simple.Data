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
            dict.Add(CreateFunction<QueryOperation>(QueryExecutor.ExecuteQuery));
            dict.Add(CreateFunction<InsertOperation>(InsertExecutor.ExecuteInsert));
            dict.Add(CreateFunction<UpdateEntityOperation>(UpdateEntityExecutor.ExecuteUpdateEntity));
            dict.Add(CreateFunction<DeleteOperation>(DeleteExecutor.Execute));
            dict.Add(CreateFunction<GetOperation>(GetExecutor.ExecuteGet));
            dict.Add(CreateFunction<UpdateByCriteriaOperation>(UpdateByCriteriaExecutor.ExecuteUpdate));
            dict.Add(CreateFunction<FunctionOperation>(ExecuteFunction));
            return (FuncDict)dict;
        }

        private static OperationResult ExecuteFunction(FunctionOperation operation, AdoAdapter adapter, AdoAdapterTransaction transaction)
        {
            var result = adapter.Execute(operation.FunctionName, operation.Parameters, transaction);
            return new MultiDataResult(result);
        }

        private static KeyValuePair<Type, ExecuteFunc> CreateFunction<T>(Func<T, AdoAdapter, AdoAdapterTransaction, OperationResult> target)
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