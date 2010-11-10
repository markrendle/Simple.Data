using NUnit.Framework;

namespace Simple.NExtLib.Unit
{
    public class EqualTest : IBinaryTest
    {
        public void Run<T>(T expected, T actual)
        {
            Assert.AreEqual(expected, actual);
        }
    }
}
