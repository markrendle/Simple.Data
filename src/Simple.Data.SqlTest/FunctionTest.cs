using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.SqlTest
{
    using Xunit;
    using Assert = NUnit.Framework.Assert;

    public class FunctionTest
    {
        public FunctionTest()
        {
            DatabaseHelper.Reset();
        }

        [Fact]
        public async void CoalesceFunctionWorks()
        {
            var db = DatabaseHelper.Open();
            var date = new DateTime(1900, 1, 1);
            List<dynamic> q = await db.Orders.Query(db.Orders.OrderDate.Coalesce(date) < DateTime.Now).ToList();
            Assert.AreNotEqual(0, q.Count);
        }
    }
}
