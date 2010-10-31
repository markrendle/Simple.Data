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
        [Test]
        public void TestUpdate()
        {
            var mockAdapter =
                new XmlMockAdapter(
                    @"<Root><Users Id=""System.Int32""><User Id=""1"" Email=""foo"" Password=""bar""/><User Id=""2"" Email=""baz"" Password=""quux""/></Users></Root>");
            MockHelper.UseMockAdapter(mockAdapter);

            int updated = Database.Default.Users.UpdateById(Id: 1, Email: "quux");
            Assert.AreEqual(1, updated);
            var element = mockAdapter.Data.Element("Users").Elements().Where(e => e.Attribute("Id") != null && e.Attribute("Id").Value == "1").Single();
            Assert.AreEqual("quux", element.Attribute("Email").Value);
        }
    }
}
