namespace Simple.Data.Ado
{
    public class AdoOptions : OptionsBase
    {
        private readonly int _commandTimeout;
        private readonly bool _identityInsert;

        public AdoOptions(int commandTimeout = -1, bool identityInsert = false)
        {
            _commandTimeout = commandTimeout;
            _identityInsert = identityInsert;
        }

        public int CommandTimeout
        {
            get { return _commandTimeout; }
        }

        public bool IdentityInsert
        {
            get { return _identityInsert; }
        }
    }
}