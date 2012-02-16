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
        public override IDictionary<string, object> GetKey(string tableName, IDictionary<string, object> record)
        {
            throw new NotImplementedException();
        }

        public override IList<string> GetKeyNames(string tableName)
        {
            throw new NotImplementedException();
        }

        public override IDictionary<string, object> Get(string tableName, params object[] keyValues)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<IDictionary<string, object>> RunQuery(SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            throw new NotImplementedException();
        }

        public override IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data, bool resultRequired)
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

        public override IEnumerable<IEnumerable<IDictionary<string, object>>> RunQueries(SimpleQuery[] queries, List<IEnumerable<SimpleQueryClauseBase>> unhandledClauses)
        {
            throw new NotImplementedException();
        }

        public override bool IsExpressionFunction(string functionName, params object[] args)
        {
            throw new NotImplementedException();
        }
    }
}
