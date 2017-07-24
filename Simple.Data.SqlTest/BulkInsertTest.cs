using Shitty.Data.Ado;

namespace Shitty.Data.SqlTest
{
    using System.Collections.Generic;
    using System.Diagnostics;
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

        [Test]
        public void BulkInsertUsesSchemaAndFireTriggers()
        {
            var db = DatabaseHelper.Open();

            using (var tx = db.BeginTransaction())
            {
                tx.WithOptions(new AdoOptions(commandTimeout: 60000, fireTriggersOnBulkInserts: true));
                tx.test.SchemaTable.DeleteAll();
                tx.test.SchemaTable.Insert(GenerateItems());

                tx.Commit();
            }

            int rowsWhichWhereUpdatedByTrigger = db.test.SchemaTable.GetCountBy(Optional: "Modified By Trigger");

            Assert.AreEqual(1000, rowsWhichWhereUpdatedByTrigger);
        }


        [Test]
        public void BulkInsertRecordsWithDifferentColumnsProperlyInsertsData() 
        {
            DatabaseHelper.Reset();
            
            var db = DatabaseHelper.Open();
            dynamic r1 = new SimpleRecord();
            r1.FirstName = "Bob";
            r1.LastName = "Dole";

            dynamic r2 = new SimpleRecord();
            r2.FirstName = "Bob";
            r2.MiddleInitial = "L";
            r2.LastName = "Saget";

            db.OptionalColumnTest.Insert(new[] { r2, r1 });

            var objs = db.OptionalColumnTest.All().ToList<OptionalColumnTestObject>();

            var expected = new[] {new OptionalColumnTestObject("Bob", "Dole"), new OptionalColumnTestObject("Bob", "Saget", "L"),};

            Assert.That(objs, Is.EquivalentTo(expected));

        }

        [Test]
        public void BulkInsertRecordsWithDifferentColumnsAndFewerColumnsInFirstRecordProperlyInsertsData()
        {
            DatabaseHelper.Reset();

            var db = DatabaseHelper.Open();

            dynamic r1 = new SimpleRecord();
            r1.FirstName = "Bob";
            r1.LastName = "Dole";

            dynamic r2 = new SimpleRecord();
            r2.FirstName = "Bob";
            r2.MiddleInitial = "L";
            r2.LastName = "Saget";

            db.OptionalColumnTest.Insert(new[] { r1, r2 });

            var objs = db.OptionalColumnTest.All().ToList<OptionalColumnTestObject>();

            var expected = new[] { new OptionalColumnTestObject("Bob", "Dole"), new OptionalColumnTestObject("Bob", "Saget", "L"), };

            Assert.That(objs, Is.EquivalentTo(expected));

        }


        private static IEnumerable<SchemaItem> GenerateItems()
        {
            for (int i = 0; i < 1000; i++)
            {
                yield return new SchemaItem(i, i.ToString());
            }
        }
    }

    class OptionalColumnTestObject
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleInitial { get; set; }

        public OptionalColumnTestObject() {}

        public OptionalColumnTestObject(string first, string last, string middle = null) 
        {
            FirstName = first;
            LastName = last;
            MiddleInitial = middle;
        }

        public override string ToString() 
        {
            return string.Format("<FirstName={0}, LastName={1}, MiddleInitial={2}>", FirstName, LastName, MiddleInitial);
        }

        public override bool Equals(object obj) 
        {
            var other = obj as OptionalColumnTestObject;
            if (other == null) return false;
            return other.FirstName == FirstName && other.LastName == LastName && other.MiddleInitial == MiddleInitial;
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