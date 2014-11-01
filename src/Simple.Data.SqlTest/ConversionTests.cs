using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Simple.Data.SqlTest
{
    using Xunit;
    using Assert = NUnit.Framework.Assert;

    public class ConversionTests
    {
        public ConversionTests()
        {
            DatabaseHelper.Reset();
        }

        [Fact]
        public async void WeirdTypeGetsConvertedToInt()
        {
            var weirdValue = new WeirdType(1);
            var db = DatabaseHelper.Open();
            var user = await db.Users.FindById(weirdValue);
            Assert.AreEqual(1, user.Id);
        }

        [Fact]
        public async void WeirdTypeUsedInQueryGetsConvertedToInt()
        {
            var weirdValue = new WeirdType(1);
            var db = DatabaseHelper.Open();
            var user = await db.Users.QueryById(weirdValue).FirstOrDefault();
            Assert.IsNotNull(user);
            Assert.AreEqual(1, user.Id);
        }

        [Fact]
        public async void InsertingWeirdTypesFromExpando()
        {
            dynamic expando = new ExpandoObject();
            expando.Name = new WeirdType("Oddball");
            expando.Password = new WeirdType("Fish");
            expando.Age = new WeirdType(3);
            expando.ThisIsNotAColumn = new WeirdType("Submit");

            var db = DatabaseHelper.Open();
            var user = await db.Users.Insert(expando);
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
