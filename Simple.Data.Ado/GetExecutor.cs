namespace Simple.Data.Ado
{
    using Operations;

    internal class GetExecutor
    {
        public static OperationResult ExecuteGet(GetOperation operation, AdoAdapter adapter, AdoAdapterTransaction transaction)
        {
            var getter = transaction == null
                ? new AdoAdapterGetter(adapter)
                : new AdoAdapterGetter(adapter, transaction.DbTransaction);
            return new DataResult(getter.Get(operation.TableName, operation.KeyValues));
        }
    }
}