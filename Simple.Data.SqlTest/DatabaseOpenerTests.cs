using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Simple.Data.SqlTest
{
    [TestFixture]
    public class DatabaseOpenerTests
    {
        [Test]
        public void OpenNamedConnectionTest()
        {
            var db = Database.OpenNamedConnection("Test");
            Assert.IsNotNull(db);
            var user = db.Users.FindById(1);
            Assert.AreEqual(1, user.Id);
        }
    }
}
