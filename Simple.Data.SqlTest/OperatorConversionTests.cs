using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Simple.Data.SqlTest
{
    [TestFixture]
    public class OperatorConversionTests
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            DatabaseHelper.Reset();
        }

        [Test]
        public void TestAllWithOperatorConversionIds()
        {
            dynamic db = DatabaseHelper.Open();

            foreach (Note n in db.Notes.All())
            {
                Assert.NotNull(n.NoteId);
                Assert.NotNull(n.CustomerId);
            }
        }

        [Test]
        public void TestFindAllByWithIntId()
        {
            dynamic db = DatabaseHelper.Open();

            Note note = db.Notes.FindAllByNoteId(1).FirstOrDefault();

            Assert.AreEqual(1, note.NoteId.Id);
            Assert.AreEqual(1, note.CustomerId.Id);
        }

        [Test]
        public void TestFindAllByWithNonPrimaryKeyOperatorConversionId()
        {
            dynamic db = DatabaseHelper.Open();

            IEnumerable<Note> notes = db.Notes.FindAllByCustomerId(new CustomerId(1));

            Assert.AreEqual(2, notes.Count());
        }

        [Test]
        public void TestFindAllByWithOperatorConversionId()
        {
            dynamic db = DatabaseHelper.Open();

            Note note = db.Notes.FindAllByNoteId(new NoteId(1)).FirstOrDefault();

            Assert.AreEqual(1, note.NoteId.Id);
            Assert.AreEqual(1, note.CustomerId.Id);
        }

        [Test]
        public void TestGetWithOperatorConversionId()
        {
            dynamic db = DatabaseHelper.Open();

            Note note = db.Notes.Get(2);

            Assert.AreEqual(2, note.NoteId.Id);
            Assert.AreEqual(1, note.CustomerId.Id);
        }

        [Test]
        public void TestInsertWithOperatorConversionIds()
        {
            dynamic db = DatabaseHelper.Open();
            db.Notes.Insert(new Note
                                {
                                    NoteId = new NoteId(3),
                                    CustomerId = new CustomerId(1),
                                    Text = "This is a new note."
                                });

            Note note = db.Notes.Get(3);

            Assert.AreEqual(3, note.NoteId.Id);
            Assert.AreEqual(1, note.CustomerId.Id);
        }
    }

    public class CustomerId
    {
        public CustomerId(int id)
        {
            Id = id;
        }

        public CustomerId() {}
        public int Id { get; protected set; }

        public override string ToString()
        {
            return "customer-" + Id;
        }

        public static explicit operator int(CustomerId id)
        {
            return id.Id;
        }

        public static explicit operator CustomerId(int id)
        {
            return new CustomerId(id);
        }
    }

    public class NoteId
    {
        public NoteId(int id)
        {
            Id = id;
        }

        public NoteId() {}
        public int Id { get; protected set; }

        public override string ToString()
        {
            return "note-" + Id;
        }

        public static explicit operator int(NoteId id)
        {
            return id.Id;
        }

        public static explicit operator NoteId(int id)
        {
            return new NoteId(id);
        }
    }

    public class Note
    {
        public CustomerId CustomerId { get; set; }
        public NoteId NoteId { get; set; }
        public string Text { get; set; }
    }
}