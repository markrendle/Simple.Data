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
}