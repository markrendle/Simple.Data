namespace Simple.Data.Ado
{
    public class AdoOptions : OptionsBase
    {
        private readonly int _commandTimeout;
        private readonly bool _identityInsert;
        private bool _fireTriggersOnBulkInserts;

        public AdoOptions(int commandTimeout = -1, bool identityInsert = false, bool fireTriggersOnBulkInserts = false)
        {
            _commandTimeout = commandTimeout;
            _identityInsert = identityInsert;
            _fireTriggersOnBulkInserts = fireTriggersOnBulkInserts;
        }

        public int CommandTimeout
        {
            get { return _commandTimeout; }
        }

        public bool IdentityInsert
        {
            get { return _identityInsert; }
        }

        public bool FireTriggersOnBulkInserts
        {
            get { return _fireTriggersOnBulkInserts; }
        }
    }
}