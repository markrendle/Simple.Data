using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.SqlTest
{
    using Xunit;
    using Assert = NUnit.Framework.Assert;

    public class SchemaQualifiedTests
    {
        public SchemaQualifiedTests()
        {
            DatabaseHelper.Reset();
        }

        [Fact]
        public async void TestFindAllByIdWithSchemaQualification()
        {
            var db = DatabaseHelper.Open();
            var dboCount = (await db.dbo.SchemaTable.FindAllById(1).ToList()).Count;
            var testCount = (await db.test.SchemaTable.FindAllById(1).ToList()).Count;
            Assert.AreEqual(1, dboCount);
            Assert.AreEqual(0, testCount);
        }

        [Fact]
        public async void TestFindWithSchemaQualification()
        {
            var db = DatabaseHelper.Open();

            var dboActual = await db.dbo.SchemaTable.FindById(1);
            var testActual = await db.test.SchemaTable.FindById(1);

            Assert.IsNotNull(dboActual);
            Assert.AreEqual("Pass", dboActual.Description);
            Assert.IsNull(testActual);
        }

        [Fact]
        public async void QueryWithSchemaQualifiedTableName()
        {
            var db = DatabaseHelper.Open();
            var result = await db.test.SchemaTable.QueryById(2)
                           .Select(db.test.SchemaTable.Id,
                                   db.test.SchemaTable.Description)
                           .Single();
            Assert.AreEqual(2, result.Id);
            Assert.AreEqual("Pass", result.Description);
        }

        [Fact]
        public async void QueryWithSchemaQualifiedTableNameAndAliases()
        {
            var db = DatabaseHelper.Open();
            var result = await db.test.SchemaTable.QueryById(2)
                           .Select(db.test.SchemaTable.Id.As("This"),
                                   db.test.SchemaTable.Description.As("That"))
                           .Single();
            Assert.AreEqual(2, result.This);
            Assert.AreEqual("Pass", result.That);
        }

    }
}
