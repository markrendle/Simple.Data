namespace Simple.Data.Ado
{
    using System.Threading.Tasks;
    using Operations;

    internal class FunctionExecutor
    {
        public static async Task<OperationResult> ExecuteFunction(FunctionOperation operation, AdoAdapter adapter, AdoAdapterTransaction transaction)
        {
            var result = await adapter.Execute(operation.FunctionName, operation.Parameters, transaction);
            return new MultiDataResult(result);
        }
    }
}