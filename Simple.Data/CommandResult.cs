namespace Simple.Data
{
    public sealed class CommandResult : OperationResult
    {
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