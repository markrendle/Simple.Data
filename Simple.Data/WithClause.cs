namespace Simple.Data
{
    public class WithClause : SimpleQueryClauseBase
    {
        private readonly ObjectReference _objectReference;
        private readonly WithMode _mode;
        private readonly WithType _type;

        public WithClause(ObjectReference objectReference) : this(objectReference, WithType.NotSpecified)
        {
        }

        public WithClause(ObjectReference objectReference, WithType type) : this(objectReference, WithMode.NotSpecified, type)
        {
        }

        public WithClause(ObjectReference objectReference, WithMode mode) : this(objectReference, mode, WithType.NotSpecified)
        {
        }

        public WithClause(ObjectReference objectReference, WithMode mode, WithType type)
        {
            _objectReference = objectReference;
            _mode = mode;
            _type = type;
        }

        public WithMode Mode
        {
            get { return _mode; }
        }

        public WithType Type
        {
            get { return _type; }
        }

        public ObjectReference ObjectReference
        {
            get { return _objectReference; }
        }
    }
}