using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace Shitty.Data.SqlCe40Test
{
    [TestFixture]
    public class NorthwindTests
    {
        private static readonly string DatabasePath = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Substring(8)),
                    "Northwind.sdf");
        
        [Test]
        public void LikeQueryShouldRunAfterAnotherQuery()
        {
            var db = Database.OpenFile(DatabasePath);
            var products = db.Products.FindAll(db.Products.CategoryId == 4);
            var data = db.Products.FindAll(db.Products.ProductName.Like("C%"));
            Assert.Pass();
        }

        [Test]
        public void QueryShouldRunAfterALikeQuery()
        {
            var db = Database.OpenFile(DatabasePath);
            var data = db.Products.FindAll(db.Products.ProductName.Like("C%"));
            var products = db.Products.FindAll(db.Products.CategoryId == 4);
            Assert.Pass();
        }

        [Test]
        public void DistinctShouldReturnDistinctList()
        {
            var db = Database.OpenFile(DatabasePath);
            List<string> countries = db.Customers.All()
                .Select(db.Customers.Country)
                .Distinct()
                .ToScalarList<string>();

            Assert.AreEqual(countries.Distinct().Count(), countries.Count);
        }

        [Test]
        public void NestedFindAllIssue167()
        {
            var db = Database.OpenFile(DatabasePath);

            List<dynamic> customers = db.Customers.All().ToList();

            foreach (var customer in customers)
            {
                customer.Orders = db.Orders.FindAllByCustomerID(customer.CustomerID).ToList();
            }

            Assert.Pass();
        }
    }
}
