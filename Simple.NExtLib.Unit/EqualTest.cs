using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NExtLib.TestExtensions
{
    public class EqualTest : IBinaryTest
    {
        public void Run<T>(T expected, T actual)
        {
            Assert.AreEqual(expected, actual);
        }
    }
}
