namespace Simple.Data.Ado
{
    using System.Threading.Tasks;
    using Operations;

    internal class InsertExecutor
    {
        public static async Task<OperationResult> ExecuteInsert(InsertOperation operation, AdoAdapter adapter, AdoAdapterTransaction transaction)
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
                    await inserter.InsertMany(operation.TableName, checkedEnumerable, operation.ErrorCallback,
                        operation.ResultRequired));
            }
            var single = checkedEnumerable.Single;
            var insert = inserter.Insert(operation.TableName, single, operation.ResultRequired);
            if (insert != null)
            {
                var data = await insert;
                return new DataResult(data);
            }
            return null;
        }
    }
}