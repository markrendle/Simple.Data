using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.SqlTest
{
    using Resources;
    using Xunit;
    using Assert = NUnit.Framework.Assert;

    public class DeleteTest
    {
        public DeleteTest()
        {
            DatabaseHelper.Reset();
        }

        [Fact]
        public async void TestDeleteByColumn()
        {
            var db = DatabaseHelper.Open();
            await db.DeleteTest.Insert(Id: 1);
            var count = await db.DeleteTest.DeleteById(1);
            Assert.AreEqual(1, count);
        }

        [Fact]
        public async void TestDeleteAll()
        {
            var db = DatabaseHelper.Open();
            await db.DeleteTest.Insert(Id: 1);
            await db.DeleteTest.Insert(Id: 2);
            var count = await db.DeleteTest.DeleteAll();
            Assert.AreEqual(2, count);
        }

        [Fact]
        public async void TestDeleteByColumnInTransaction()
        {
            var db = DatabaseHelper.Open();
            var tx = db.BeginTransaction();
            await tx.DeleteTest.Insert(Id: 1);
            var count = await tx.DeleteTest.DeleteById(1);
            tx.Commit();
            Assert.AreEqual(1, count);
        }

        [Fact]
        public async void TestDeleteAllInTransaction()
        {
            var db = DatabaseHelper.Open();
            var tx = db.BeginTransaction();
            await tx.DeleteTest.Insert(Id: 1);
            await tx.DeleteTest.Insert(Id: 2);
            var count = await tx.DeleteTest.DeleteAll();
            tx.Commit();
            Assert.AreEqual(2, count);
        }
    }
}
