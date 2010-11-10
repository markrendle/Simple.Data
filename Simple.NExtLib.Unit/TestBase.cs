namespace Simple.NExtLib.Unit
{
    public abstract class TestBase
    {
        protected static readonly IBinaryTest Equal = new EqualTest();
        protected static readonly IEnumerableTest Contain = new ContainTest();
    }
}
