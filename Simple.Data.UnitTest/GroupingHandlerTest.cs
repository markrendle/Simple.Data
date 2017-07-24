using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Shitty.Data.QueryPolyfills;

namespace Shitty.Data.UnitTest
{
    [TestFixture]
    class GroupingHandlerTest
    {
        [Test]
        public void GroupingOnSingleKeyWorks()
        {
            var source = new List<IDictionary<string, object>>
                             {
                                 new Dictionary<string, object> {{"Id", 1}, {"Size", 1}},
                                 new Dictionary<string, object> {{"Id", 1}, {"Size", 2}},
                                 new Dictionary<string, object> {{"Id", 1}, {"Size", 3}},
                                 new Dictionary<string, object> {{"Id", 2}, {"Size", 4}},
                                 new Dictionary<string, object> {{"Id", 2}, {"Size", 5}},
                                 new Dictionary<string, object> {{"Id", 2}, {"Size", 6}},
                             };
            var target = new GroupingHandler("Id");
            var actual = target.Group(source).ToList();
            Assert.AreEqual(2, actual.Count);
        }
        [Test]
        public void GroupingOnDoubleKeyWorks()
        {
            var source = new List<IDictionary<string, object>>
                             {
                                 new Dictionary<string, object> {{"Id", 1}, {"Type", "A"}, {"Size", 1}},
                                 new Dictionary<string, object> {{"Id", 1}, {"Type", "A"}, {"Size", 2}},
                                 new Dictionary<string, object> {{"Id", 1}, {"Type", "B"}, {"Size", 3}},
                                 new Dictionary<string, object> {{"Id", 1}, {"Type", "B"}, {"Size", 4}},
                                 new Dictionary<string, object> {{"Id", 2}, {"Type", "A"}, {"Size", 5}},
                                 new Dictionary<string, object> {{"Id", 2}, {"Type", "A"}, {"Size", 6}},
                                 new Dictionary<string, object> {{"Id", 2}, {"Type", "B"}, {"Size", 7}},
                                 new Dictionary<string, object> {{"Id", 2}, {"Type", "B"}, {"Size", 8}},
                             };
            var target = new GroupingHandler("Id", "Type");
            var actual = target.Group(source).ToList();
            Assert.AreEqual(4, actual.Count);
        }
    }
}
