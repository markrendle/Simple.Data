// ReSharper disable InconsistentNaming
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Simple.Data.Mocking.Test
{
    /// <summary>
    ///This is a test class for XmlStubAdapterTest and is intended
    ///to contain all XmlStubAdapterTest Unit Tests
    ///</summary>
    [TestFixture]
    public class XmlMockAdapterTest
    {
        private XmlMockAdapter _mockAdapter;

        [TestFixtureSetUp]
        public void MyTestInitialize()
        {
            _mockAdapter =
                new XmlMockAdapter(
                    @"<Root><Users _keys=""Id"" Id=""System.Int32"" Key=""System.Guid"">
<User Id=""1"" Email=""foo"" Password=""bar"" Key=""4A1c8a8a-238d-443e-8ab2-bdf046a91fd7"">
  <Pets><Pet Name=""Fido""/></Pets>
</User>
<User Id=""2"" Email=""bar"" Password=""quux""/>
<User Id=""3"" Email=""baz"" Password=""quux""/>
<User Id=""4"" Email=""baz"" Password=""quux""/>
</Users></Root>");
            MockHelper.UseMockAdapter(_mockAdapter);
        }

        /// <summary>
        ///A test for Find
        ///</summary>
        [Test]
        public void FindByEmail_ShouldFindRecord()
        {
            dynamic user = Database.Default.Users.FindByEmail("foo");
            Assert.AreEqual(1, user.Id);
            Assert.AreEqual("foo", user.Email);
            Assert.AreEqual("bar", user.Password);
        }

        /// <summary>
        ///A test for Find
        ///</summary>
        [Test]
        public void FindByKey_ShouldFindRecord()
        {
            var key = Guid.Parse("4a1c8a8a-238d-443e-8ab2-bdf046a91fd7");
            dynamic user = Database.Default.Users.FindByKey(key);
            Assert.IsNotNull(user);
            Assert.AreEqual(1, user.Id);
            Assert.AreEqual("foo", user.Email);
            Assert.AreEqual("bar", user.Password);
        }

        /// <summary>
        ///A test for Find
        ///</summary>
        [Test]
        public void FindById_ShouldFindRecord()
        {
            dynamic user = Database.Default.Users.FindById(1);
            Assert.AreEqual(1, user.Id);
        }

        [Test]
        public void UserShouldHavePet()
        {
            dynamic user = Database.Default.Users.FindById(1);
            Assert.IsNotNull(user.Pets);
        }

        [Test]
        public void All_ShouldReturnTwoUsers()
        {
            IEnumerable<object> users = Database.Default.Users.All().Cast<object>();
            Assert.AreEqual(_mockAdapter.Data.Element("Users").Elements().Count(), users.Count());
        }

        [Test]
        public void TestUpdateBy()
        {
            int updated = Database.Default.Users.UpdateById(Id: 1, Email: "quux");
            Assert.AreEqual(1, updated);
            var element = _mockAdapter.Data.Element("Users").Elements().Where(e => e.Attribute("Id") != null && e.Attribute("Id").Value == "1").Single();
            Assert.AreEqual("quux", element.Attribute("Email").Value);
        }

        [Test]
        public void TestUpdate()
        {
            dynamic record = new DynamicRecord();
            record.Id = 4;
            record.Email = "quux";
            Database.Default.Users.Update(record);
            var element = _mockAdapter.Data.Element("Users").Elements().Where(e => e.Attribute("Id") != null && e.Attribute("Id").Value == "4").Single();
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
            var row = Database.Default.Users.Insert(Id: 5, Email: "bob", Password: "secret");
            Assert.AreEqual(5, row.Id);
            Assert.AreEqual("bob", row.Email);
            Assert.AreEqual("secret", row.Password);
            var element = _mockAdapter.Data.Element("Users").Elements().Where(e => e.Attribute("Id") != null && e.Attribute("Id").Value == "5").SingleOrDefault();
            Assert.IsNotNull(element);
            Assert.AreEqual("5", element.Attribute("Id").Value);
            Assert.AreEqual("bob", element.Attribute("Email").Value);
            Assert.AreEqual("secret", element.Attribute("Password").Value);
        }

        [Test]
        public void IsValidRelation_Users_Pets_ShouldReturnTrue()
        {
            Assert.IsTrue(_mockAdapter.IsValidRelation("Users", "Pets"));
        }

        [Test]
        public void Users_Pets_ShouldReturn_OneRow_WithName_Fido()
        {
            IEnumerable<dynamic> pets = Database.Default.Users.FindById(1).Pets;
            Assert.AreEqual(1, pets.Count());
            Assert.AreEqual("Fido", pets.Single().Name);
        }
    }
}

// ReSharper restore InconsistentNaming