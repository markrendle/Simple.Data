using System;
using NUnit.Framework;

namespace Simple.Data.Ado.Test
{
	[TestFixture()]
	public class ProviderTest
	{
		[Test()]
		public void DataSourceShouldExtractProperly ()
		{
			var connectionString = "data source=foo;initial catalog=bar;";
			var expected = "foo";
			var actual = Simple.Data.Ado.ProviderHelper.GetDataSourceName(connectionString);
			Assert.AreEqual(expected, actual);
		}
		[Test()]
		public void DataSourceShouldExtractProperlyWithSomethingBefore ()
		{
			var connectionString = "provider=SqlClient;data source=foo;initial catalog=bar;";
			var expected = "foo";
			var actual = Simple.Data.Ado.ProviderHelper.GetDataSourceName(connectionString);
			Assert.AreEqual(expected, actual);
		}
		[Test()]
		public void DataSourceShouldExtractProperlyWithFilename ()
		{
			var connectionString = "data source=foo.sdf;initial catalog=bar;";
			var expected = "foo.sdf";
			var actual = Simple.Data.Ado.ProviderHelper.GetDataSourceName(connectionString);
			Assert.AreEqual(expected, actual);
		}
	}
}