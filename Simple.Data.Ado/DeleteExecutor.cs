namespace Simple.Data.Ado
{
    using Operations;

    internal class DeleteExecutor
    {
        public static CommandResult Execute(DeleteOperation operation, AdoAdapter adapter,
            AdoAdapterTransaction transaction)
        {
            ICommandBuilder commandBuilder = new DeleteHelper(adapter.GetSchema()).GetDeleteCommand(operation.TableName, operation.Criteria);
            return new CommandResult(adapter.Execute(commandBuilder, transaction));
        }
    }
}