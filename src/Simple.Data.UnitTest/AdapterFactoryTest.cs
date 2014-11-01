using Simple.Data.Operations;

namespace Simple.Data.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
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
        public override IEqualityComparer<string> KeyComparer
        {
            get { return StringComparer.OrdinalIgnoreCase; }
        }

        public override IReadOnlyDictionary<string, object> GetKey(string tableName, IReadOnlyDictionary<string, object> record)
        {
            throw new NotImplementedException();
        }

        public override IList<string> GetKeyNames(string tableName)
        {
            throw new NotImplementedException();
        }

        public override Task<OperationResult> Execute(IOperation operation)
        {
            throw new NotImplementedException();
        }

        public override bool IsExpressionFunction(string functionName, params object[] args)
        {
            throw new NotImplementedException();
        }
    }
}
