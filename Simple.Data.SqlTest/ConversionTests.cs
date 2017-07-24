using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Shitty.Data.SqlTest
{
    [TestFixture]
    public class ConversionTests
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            DatabaseHelper.Reset();
        }

        [Test]
        public void WeirdTypeGetsConvertedToInt()
        {
            var weirdValue = new WeirdType(1);
            var db = DatabaseHelper.Open();
            var user = db.Users.FindById(weirdValue);
            Assert.AreEqual(1, user.Id);
        }

        [Test]
        public void WeirdTypeUsedInQueryGetsConvertedToInt()
        {
            var weirdValue = new WeirdType(1);
            var db = DatabaseHelper.Open();
            var user = db.Users.QueryById(weirdValue).FirstOrDefault();
            Assert.IsNotNull(user);
            Assert.AreEqual(1, user.Id);
        }

        [Test]
        public void InsertingWeirdTypesFromExpando()
        {
            dynamic expando = new ExpandoObject();
            expando.Name = new WeirdType("Oddball");
            expando.Password = new WeirdType("Fish");
            expando.Age = new WeirdType(3);
            expando.ThisIsNotAColumn = new WeirdType("Submit");

            var db = DatabaseHelper.Open();
            var user = db.Users.Insert(expando);
            Assert.IsInstanceOf<int>(user.Id);
            Assert.AreEqual("Oddball", user.Name);
            Assert.AreEqual("Fish", user.Password);
            Assert.AreEqual(3, user.Age);
        }
    }

    class WeirdType : DynamicObject
    {
        private readonly object _value;

        public WeirdType(object value)
        {
            _value = value;
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            result = Convert.ChangeType(_value, binder.Type);
            return true;
        }

        public override string ToString()
        {
            return _value.ToString();
        }
    }
}
