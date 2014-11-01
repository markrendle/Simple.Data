namespace Simple.Data.Commands
{
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Operations;

    public struct CommandResultAwaitable : IAwaitable
    {
        private readonly IOperation _operation;
        private readonly DataStrategy _strategy;

        public CommandResultAwaitable(IOperation operation, DataStrategy strategy)
        {
            _operation = operation;
            _strategy = strategy;
        }

        public TaskAwaiter<dynamic> GetAwaiter()
        {
            return Execute().GetAwaiter();
        }

        private async Task<dynamic> Execute()
        {
            var commandResult = await _strategy.Run.Execute(_operation);
            return ((CommandResult) commandResult).RowsAffected;
        }
    }

    public interface IAwaitable
    {
        TaskAwaiter<dynamic> GetAwaiter();
    }
}