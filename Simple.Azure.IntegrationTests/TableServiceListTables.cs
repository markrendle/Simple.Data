using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Simple.Azure.Helpers;
using System.Diagnostics;

namespace Simple.Azure.IntegrationTests
{
    [TestFixture]
    public class TableServiceListTables : AssertionHelper
    {
        [Test]
        public void ListTablesDoesNotThrowAnException()
        {
            IAzure azure = new Azure()
            {
                Account = "marksandbox",
                SharedKey = "ELIDED"
            };

            var tableService = new TableService(azure);

            Trace.WriteLine("Working");

            Assert.That(() => tableService.ListAllTables(), Throws.Nothing);
            Assert.True(true);
        }
    }
}
