namespace Simple.Data.Ado
{
    using Operations;

    internal class FunctionExecutor
    {
        public static OperationResult ExecuteFunction(FunctionOperation operation, AdoAdapter adapter, AdoAdapterTransaction transaction)
        {
            var result = adapter.Execute(operation.FunctionName, operation.Parameters, transaction);
            return new MultiDataResult(result);
        }
    }
}