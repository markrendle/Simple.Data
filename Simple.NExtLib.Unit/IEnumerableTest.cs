using System.Collections.Generic;

namespace Simple.NExtLib.Unit
{
    public interface IEnumerableTest
    {
        void RunTest<T>(T expected, IEnumerable<T> actual);
    }
}
