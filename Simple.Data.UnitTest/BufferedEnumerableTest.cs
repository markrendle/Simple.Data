using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace Simple.Data.UnitTest
{
    [TestFixture]
    public class BufferedEnumerableTest
    {
        [Test]
        public void ShouldCallCleanup()
        {
            // Arrange
            bool cleanedUp = false;

            var list = BufferedEnumerable.Create("ABC".ToIterator(), () => cleanedUp = true)
                    .ToList();

            // Act
            Assert.AreEqual(3, list.Count);

            SpinWait.SpinUntil(() => cleanedUp, 1000);

            Assert.True(cleanedUp);
        }
    }
}
