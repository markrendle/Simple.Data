namespace Shitty.Data
{
    using System.Dynamic;

    public abstract class SimpleReference : DynamicObject
    {
        private readonly string _alias;

        protected SimpleReference() : this(null)
        {
        }

        protected SimpleReference(string alias)
        {
            _alias = alias;
        }

        public string GetAlias()
        {
            return _alias;
        }

        /// <summary>
        /// Gets the name of the referenced object.
        /// </summary>
        /// <returns>The name.</returns>
        public virtual string GetAliasOrName()
        {
            return _alias;
        }

        protected internal virtual DataStrategy FindDataStrategyInHierarchy()
        {
            return null;
        }
    }

    public static class SimpleReferenceEx
    {
        public static bool IsNull(this SimpleReference reference)
        {
            return ReferenceEquals(reference, null);
        }
    }
}