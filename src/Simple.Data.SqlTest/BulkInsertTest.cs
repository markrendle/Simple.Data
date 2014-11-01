using Simple.Data.Ado;

namespace Simple.Data.SqlTest
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using NUnit.Framework;
    using Xunit;
    using Assert = NUnit.Framework.Assert;

    public class BulkInsertTest
    {
        public BulkInsertTest()
        {
            DatabaseHelper.Reset();
        }

        //TODO: [Fact]
        public async void BulkInsertUsesSchema()
        {
            var db = DatabaseHelper.Open();
            List<dynamic> list;
            Promise<int> count;
            using (var tx = db.BeginTransaction())
            {
                await tx.test.SchemaTable.DeleteAll();
                await tx.test.SchemaTable.Insert(GenerateItems());

                list = await tx.test.SchemaTable.All().WithTotalCount(out count).ToList();
                tx.Rollback();
            }
            Assert.AreEqual(1000, count.Value);
            Assert.AreEqual(1000, list.Count);
        }

        //TODO: [Fact]
        public async void BulkInsertUsesSchemaAndFireTriggers()
        {
            var db = DatabaseHelper.Open();

            using (var tx = db.BeginTransaction())
            {
                tx.WithOptions(new AdoOptions(commandTimeout: 60000, fireTriggersOnBulkInserts: true));
                await tx.test.SchemaTable.DeleteAll();
                await tx.test.SchemaTable.Insert(GenerateItems());

                tx.Commit();
            }

            int rowsWhichWhereUpdatedByTrigger = await db.test.SchemaTable.GetCountBy(Optional: "Modified By Trigger");

            Assert.AreEqual(1000, rowsWhichWhereUpdatedByTrigger);
        }

        private static IEnumerable<SchemaItem> GenerateItems()
        {
            for (int i = 0; i < 1000; i++)
            {
                yield return new SchemaItem(i, i.ToString());
            }
        }
    }

    class SchemaItem
    {
        public SchemaItem(int id, string description)
        {
            Id = id;
            Description = description;
        }

        public int Id { get; set; }
        public string Description { get; set; }
    }
}