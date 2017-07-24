using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shitty.Data.SqlTest
{
    using NUnit.Framework;

    [TestFixture]
    public class WeirdTypeTest
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            DatabaseHelper.Reset();
        }

        [Test]
        public void TestInsertOnGeography()
        {
            var db = DatabaseHelper.Open();
            var actual = db.GeographyTest.Insert(Description: "Test");
            Assert.IsNotNull(actual);
        }
        [Test]
        public void TestInsertOnGeometry()
        {
            var db = DatabaseHelper.Open();
            var actual = db.GeometryTest.Insert(Description: "Test");
            Assert.IsNotNull(actual);
        }
        [Test]
        public void TestInsertOnHierarchyId()
        {
            var db = DatabaseHelper.Open();
            var actual = db.HierarchyIdTest.Insert(Description: "Test");
            Assert.IsNotNull(actual);
        }
    }
}
