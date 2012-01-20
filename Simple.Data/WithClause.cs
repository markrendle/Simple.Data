namespace Simple.Data
{
    public class WithClause : SimpleQueryClauseBase
    {
        private readonly ObjectReference _objectReference;
        private readonly WithMode _mode;

        public WithClause(ObjectReference objectReference, WithMode mode)
        {
            _objectReference = objectReference;
            _mode = mode;
        }

        public WithMode Mode
        {
            get { return _mode; }
        }

        public ObjectReference ObjectReference
        {
            get { return _objectReference; }
        }
    }
}