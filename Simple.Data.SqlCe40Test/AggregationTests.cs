using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace Simple.Data.SqlCe40Test
{
	[TestFixture]
	public class AggregationTests
	{
		private static readonly string DatabasePath = Path.Combine(
					Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Substring(8)),
					"TestDatabase.sdf");

		[Test]
		public void TestMax()
		{
			var db = Database.Opener.OpenFile(DatabasePath);
			var value = db.Items.MaxPrice();
			Assert.AreEqual(9.99m, value);
		}

		[Test]
		public void TestMin()
		{
			var db = Database.Opener.OpenFile(DatabasePath);
			var value = db.Items.MinPrice();
			Assert.AreEqual(1m, value);
		}

		[Test]
		public void TestMaxWithCriteria()
		{
			var db = Database.Opener.OpenFile(DatabasePath);
			var value = db.Items.MaxPrice(db.Items.Name == "Flange");
			Assert.AreEqual(1, value);
		}

		[Test]
		public void TestMinWithCriteria()
		{
			var db = Database.Opener.OpenFile(DatabasePath);
			var value = db.Items.MinPrice(db.Items.Name == "Widget");
			Assert.AreEqual(9.99m, value);
		}
	}
}