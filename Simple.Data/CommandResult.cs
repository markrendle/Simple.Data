namespace Simple.Data
{
    public sealed class CommandResult : OperationResult
    {
        public static readonly CommandResult Empty = new CommandResult(-1);
        private readonly int _rowsAffected;

        public int RowsAffected
        {
            get { return _rowsAffected; }
        }

        public CommandResult(int rowsAffected) : base(0)
        {
            _rowsAffected = rowsAffected;
        }
    }
}