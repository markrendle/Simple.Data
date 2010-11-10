using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Simple.NExtLib.Unit
{
    public static class ShouldExtensions
    {
        public static void ShouldEqual<T>(this T actual, T expected)
        {
            Assert.AreEqual(expected, actual);
        }

        public static void ShouldEqual<T>(this T actual, T expected, string message)
        {
            Assert.AreEqual(expected, actual, message);
        }

        public static void ShouldBeNull<T>(this T actual)
            where T : class
        {
            Assert.IsNull(actual);
        }

        public static void ShouldNotBeNull<T>(this T actual)
        where T : class
        {
            Assert.IsNotNull(actual);
        }

        public static void Should<T>(this T actual, IBinaryTest equal, T expected)
        {
            if (equal == null) throw new ArgumentNullException("equal");

            equal.Run(expected, actual);
        }

        public static void Should<T>(this IEnumerable<T> actual, IEnumerableTest test, T expected)
        {
            if (test == null) throw new ArgumentNullException("test");

            test.RunTest(expected, actual);
        }
    }
}
