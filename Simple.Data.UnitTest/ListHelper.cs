using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Shitty.Data.UnitTest
{
    static class ListHelper
    {
        public static void AssertAreEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            using (IEnumerator<T> e1 = expected.GetEnumerator(), e2 = actual.GetEnumerator())
            {
                bool found1;
                bool found2;

                while ((found1 = e1.MoveNext()) | (found2 = e2.MoveNext()))
                {
                    if (!(found1 && found2))
                    {
                        Assert.Fail("Lists are of different lengths.");
                    }
                    Assert.AreEqual(e1.Current, e2.Current);
                }
            }
        }
    }
}
