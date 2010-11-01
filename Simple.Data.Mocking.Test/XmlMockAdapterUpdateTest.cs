using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Simple.Data.Mocking.Test
{
    [TestFixture]
    public class XmlMockAdapterUpdateTest
    {
        private XmlMockAdapter _mockAdapter;
        [TestFixtureSetUp]
        public void SetUp()
        {
            _mockAdapter =
                new XmlMockAdapter(
                    @"<Root><Users Id=""System.Int32""><User Id=""1"" Email=""foo"" Password=""bar""/><User Id=""2"" Email=""bar"" Password=""quux""/><User Id=""3"" Email=""baz"" Password=""quux""/></Users></Root>");
            MockHelper.UseMockAdapter(_mockAdapter);
        }
        [Test]
        public void TestUpdate()
        {
            int updated = Database.Default.Users.UpdateById(Id: 1, Email: "quux");
            Assert.AreEqual(1, updated);
            var element = _mockAdapter.Data.Element("Users").Elements().Where(e => e.Attribute("Id") != null && e.Attribute("Id").Value == "1").Single();
            Assert.AreEqual("quux", element.Attribute("Email").Value);
        }

        [Test]
        public void TestDelete()
        {
            int deleted = Database.Default.Users.Delete(Id: 2);
            Assert.AreEqual(1, deleted);
            var element = _mockAdapter.Data.Element("Users").Elements().Where(e => e.Attribute("Id") != null && e.Attribute("Id").Value == "2").SingleOrDefault();
            Assert.IsNull(element);
        }

        [Test]
        public void TestDeleteBy()
        {
            int deleted = Database.Default.Users.DeleteById(3);
            Assert.AreEqual(1, deleted);
            var element = _mockAdapter.Data.Element("Users").Elements().Where(e => e.Attribute("Id") != null && e.Attribute("Id").Value == "3").SingleOrDefault();
            Assert.IsNull(element);
        }

        [Test]
        public void TestInsert()
        {
            var row = Database.Default.Users.Insert(Id: 4, Email: "bob", Password: "secret");
            Assert.AreEqual(4, row.Id);
            Assert.AreEqual("bob", row.Email);
            Assert.AreEqual("secret", row.Password);
            var element = _mockAdapter.Data.Element("Users").Elements().Where(e => e.Attribute("Id") != null && e.Attribute("Id").Value == "4").SingleOrDefault();
            Assert.IsNotNull(element);
            Assert.AreEqual("4", element.Attribute("Id").Value);
            Assert.AreEqual("bob", element.Attribute("Email").Value);
            Assert.AreEqual("secret", element.Attribute("Password").Value);
        }
    }
}
