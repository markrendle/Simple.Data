using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NExtLib.TestExtensions
{
    public abstract class TestBase
    {
        protected static readonly IBinaryTest Equal = new EqualTest();
        protected static readonly IEnumerableTest Contain = new ContainTest();
    }
}
