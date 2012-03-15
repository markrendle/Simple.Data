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
            db.test.SchemaTable.DeleteAll();
            db.test.SchemaTable.Insert(GenerateItems());

            var list = db.test.SchemaTable.All().ToList();
            Assert.AreEqual(1000, list.Count);

            db.test.SchemaTable.DeleteAll();
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