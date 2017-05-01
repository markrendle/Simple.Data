namespace Simple.Data.Ado
{
    public class AdoOptions : OptionsBase
    {
        private readonly int _commandTimeout;
        private readonly bool _identityInsert;
        private bool _fireTriggersOnBulkInserts;
        private readonly int _bulkCopyTimeout;

        public AdoOptions(int commandTimeout = -1, bool identityInsert = false, bool fireTriggersOnBulkInserts = false, int bulkCopyTimeout = 30)
        {
            _commandTimeout = commandTimeout;
            _identityInsert = identityInsert;
            _fireTriggersOnBulkInserts = fireTriggersOnBulkInserts;
            _bulkCopyTimeout = bulkCopyTimeout;
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

        public int BulkCopyTimeout
        {
            get { return _bulkCopyTimeout; }
        }
    }
}