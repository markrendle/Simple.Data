using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shitty.Data.SqlTest
{
    using NUnit.Framework;

    [TestFixture]
    class SchemaQualifiedTests
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            DatabaseHelper.Reset();
        }

        [Test]
        public void TestFindAllByIdWithSchemaQualification()
        {
            var db = DatabaseHelper.Open();
            var dboCount = db.dbo.SchemaTable.FindAllById(1).ToList().Count;
            var testCount = db.test.SchemaTable.FindAllById(1).ToList().Count;
            Assert.AreEqual(1, dboCount);
            Assert.AreEqual(0, testCount);
        }

        [Test]
        public void TestFindWithSchemaQualification()
        {
            var db = DatabaseHelper.Open();

            var dboActual = db.dbo.SchemaTable.FindById(1);
            var testActual = db.test.SchemaTable.FindById(1);

            Assert.IsNotNull(dboActual);
            Assert.AreEqual("Pass", dboActual.Description);
            Assert.IsNull(testActual);
        }

        [Test]
        public void QueryWithSchemaQualifiedTableName()
        {
            var db = DatabaseHelper.Open();
            var result = db.test.SchemaTable.QueryById(2)
                           .Select(db.test.SchemaTable.Id,
                                   db.test.SchemaTable.Description)
                           .Single();
            Assert.AreEqual(2, result.Id);
            Assert.AreEqual("Pass", result.Description);
        }

        [Test]
        public void QueryWithSchemaQualifiedTableNameAndAliases()
        {
            var db = DatabaseHelper.Open();
            var result = db.test.SchemaTable.QueryById(2)
                           .Select(db.test.SchemaTable.Id.As("This"),
                                   db.test.SchemaTable.Description.As("That"))
                           .Single();
            Assert.AreEqual(2, result.This);
            Assert.AreEqual("Pass", result.That);
        }

    }
}
