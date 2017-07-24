namespace Shitty.Data
{
    public class AllColumnsSpecialReference : SpecialReference
    {
        private readonly ObjectReference _table;

        public AllColumnsSpecialReference() : this(null)
        {
        }

        public AllColumnsSpecialReference(ObjectReference table) : base("*")
        {
            _table = table;
        }

        public ObjectReference Table
        {
            get { return _table; }
        }
    }
}