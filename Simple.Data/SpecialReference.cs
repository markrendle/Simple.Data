namespace Shitty.Data
{
    public abstract class SpecialReference : SimpleReference
    {
        private readonly string _name;

        protected SpecialReference(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }
    }
}