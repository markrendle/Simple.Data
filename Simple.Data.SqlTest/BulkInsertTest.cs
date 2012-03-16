namespace Simple.Data.SqlTest
{
    using System.Collections.Generic;
    using NUnit.Framework;

    [TestFixture]
    public class BulkInsertTest
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            DatabaseHelper.Reset();
        }

        [Test]
        public void BulkInsertUsesSchema()
        {
            var db = DatabaseHelper.Open();
            List<dynamic> list;
            Promise<int> count;
            using (var tx = db.BeginTransaction())
            {
                tx.test.SchemaTable.DeleteAll();
                tx.test.SchemaTable.Insert(GenerateItems());

                list = tx.test.SchemaTable.All().WithTotalCount(out count).ToList();
                tx.Rollback();
            }
            Assert.AreEqual(1000, count.Value);
            Assert.AreEqual(1000, list.Count);
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