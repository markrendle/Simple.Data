using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NExtLib.TestExtensions
{
    public interface IEnumerableTest
    {
        void RunTest<T>(T expected, IEnumerable<T> actual);
    }
}
