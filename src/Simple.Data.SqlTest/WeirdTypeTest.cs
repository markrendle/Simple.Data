using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.SqlTest
{
    using Xunit;
    using Assert = NUnit.Framework.Assert;

    public class WeirdTypeTest
    {
        public WeirdTypeTest()
        {
            DatabaseHelper.Reset();
        }

        [Fact]
        public async void TestInsertOnGeography()
        {
            var db = DatabaseHelper.Open();
            var actual = await db.GeographyTest.Insert(Description: "Test");
            Assert.IsNotNull(actual);
        }
        [Fact]
        public async void TestInsertOnGeometry()
        {
            var db = DatabaseHelper.Open();
            var actual = await db.GeometryTest.Insert(Description: "Test");
            Assert.IsNotNull(actual);
        }
        [Fact]
        public async void TestInsertOnHierarchyId()
        {
            var db = DatabaseHelper.Open();
            var actual = await db.HierarchyIdTest.Insert(Description: "Test");
            Assert.IsNotNull(actual);
        }
    }
}
