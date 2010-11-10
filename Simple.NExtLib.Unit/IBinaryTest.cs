namespace Simple.NExtLib.Unit
{
    public interface IBinaryTest
    {
        void Run<T>(T expected, T actual);
    }
}
