using System.Collections.Generic;
using System.Linq;
using NExtLib.Unit;
using NUnit.Framework;
using Simple.NExtLib.Tests.Properties;
using Simple.NExtLib.Xml;

namespace Simple.NExtLib.Tests.Xml
{
    [TestFixture]
    public class XmlElementAsDictionaryReadTests
    {
        private static IEnumerable<XmlElementAsDictionary> ParseDescendantsUnderTest
        {
            get { return XmlElementAsDictionary.ParseDescendants(Resources.TwitterStatusesSample, "status"); }
        }

        [Test]
        public void FirstDescendantIsTweetOne()
        {
            XmlElementAsDictionary actual = ParseDescendantsUnderTest.First();
            actual["text"].Value.ShouldEqual("Tweet one.");
        }

        [Test]
        public void SecondDescendantIsTweetTwo()
        {
            XmlElementAsDictionary actual = ParseDescendantsUnderTest.Skip(1).First();
            actual["text"].Value.ShouldEqual("Tweet two.");
        }

        [Test]
        public void ParseDescendantsReturnsTwoItems()
        {
            ParseDescendantsUnderTest.Count().ShouldEqual(2);
        }

        [Test]
        public void UserNameReturnedCorrectly()
        {
            var one = ParseDescendantsUnderTest.First();

            one["user"]["name"].Value.ShouldEqual("Doug Williams");
        }
    }
}