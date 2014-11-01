namespace Simple.Data.Ado
{
    using System.Threading.Tasks;
    using Operations;

    internal class DeleteExecutor
    {
        public static async Task<OperationResult> Execute(DeleteOperation operation, AdoAdapter adapter, AdoAdapterTransaction transaction)
        {
            ICommandBuilder commandBuilder = new DeleteHelper(adapter.GetSchema()).GetDeleteCommand(operation.TableName, operation.Criteria);
            return new CommandResult(await adapter.Execute(commandBuilder, transaction));
        }
    }
}