namespace Simple.Data
{
    using System.Dynamic;

    public abstract class SimpleReference : DynamicObject
    {
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