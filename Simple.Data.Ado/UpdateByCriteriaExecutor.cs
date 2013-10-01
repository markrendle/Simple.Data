namespace Simple.Data.Ado
{
    using Operations;

    internal class UpdateByCriteriaExecutor
    {
        public static OperationResult ExecuteUpdate(UpdateByCriteriaOperation operation, AdoAdapter adapter, AdoAdapterTransaction transaction)
        {
            ICommandBuilder commandBuilder = new UpdateHelper(adapter.GetSchema()).GetUpdateCommand(operation.TableName, operation.Data, operation.Criteria);
            return new CommandResult(adapter.Execute(commandBuilder));
        }
    }
}