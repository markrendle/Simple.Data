namespace Simple.Data.Ado
{
    using System.Threading.Tasks;
    using Operations;

    internal class UpdateByCriteriaExecutor
    {
        public static async Task<OperationResult> ExecuteUpdate(UpdateByCriteriaOperation operation, AdoAdapter adapter, AdoAdapterTransaction transaction)
        {
            ICommandBuilder commandBuilder = new UpdateHelper(adapter.GetSchema()).GetUpdateCommand(operation.TableName, operation.Data, operation.Criteria);
            return new CommandResult(await adapter.Execute(commandBuilder));
        }
    }
}