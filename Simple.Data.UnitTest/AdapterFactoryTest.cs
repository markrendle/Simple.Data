namespace Simple.Data.UnitTest
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;

    [TestFixture]
    public class AdapterFactoryTest
    {
        private static AdapterFactory CreateTarget()
        {
            return new CachingAdapterFactory(new StubComposer());
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateWithAnonymousObjectWithoutConnectionStringThrowsArgumentException()
        {
            CreateTarget().Create(new { X = "" });
        }

        [Test]
        public void CreateWithName()
        {
            var actual = CreateTarget().Create("Stub", null);
            Assert.IsNotNull(actual);
        }
    }

    class StubComposer : Composer
    {
        public override T Compose<T>()
        {
            return (T) Create();
        }

        public override T Compose<T>(string contractName)
        {
            return (T)Create();
        }

        private object Create()
        {
            return new StubAdapter();
        }
    }

    class StubAdapter : Adapter
    {
        public override IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<IDictionary<string, object>> RunQuery(SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            throw new NotImplementedException();
        }

        public override IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data)
        {
            throw new NotImplementedException();
        }

        public override int Update(string tableName, IDictionary<string, object> data, SimpleExpression criteria)
        {
            throw new NotImplementedException();
        }

        public override int Delete(string tableName, SimpleExpression criteria)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<string> GetKeyFieldNames(string tableName)
        {
            throw new NotImplementedException();
        }
    }
}
