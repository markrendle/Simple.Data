using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Data;
using Simple.Data.Ado.Schema;
using System.ComponentModel.Composition;

namespace Simple.Data.Ado.Test
{
    [TestFixture]
    public class ProviderHelperTest
    {
        [Test]
        public void ShouldNotRequestExportableTypeFromServiceProvider()
        {
            var helper = new ProviderHelper();
            var connectionProvider = new StubConnectionAndServiceProvider();
            var actual = helper.GetCustomProvider<ITestInterface>(connectionProvider);
            Assert.IsNull(connectionProvider.RequestedServiceType);
        }

        [Test]
        public void ShouldRequestNonExportedTypeFromServiceProvider()
        {
            var helper = new ProviderHelper();
            var connectionProvider = new StubConnectionAndServiceProvider();
            var actual = helper.GetCustomProvider<IQueryPager>(connectionProvider);
            Assert.AreEqual(typeof(IQueryPager), connectionProvider.RequestedServiceType);
        }

        [Test]
        public void ShouldReturnNonExportedTypeFromServiceProvider()
        {
            var helper = new ProviderHelper();
            var connectionProvider = new StubConnectionAndServiceProvider();
            var actual = helper.GetCustomProvider<IQueryPager>(connectionProvider);
            Assert.IsInstanceOf(typeof(IQueryPager), actual);
        }

        public class StubConnectionAndServiceProvider : IConnectionProvider, IServiceProvider
        {
            public void SetConnectionString(string connectionString)
            {
                throw new NotImplementedException();
            }

            public IDbConnection CreateConnection()
            {
                throw new NotImplementedException();
            }

            public ISchemaProvider GetSchemaProvider()
            {
                throw new NotImplementedException();
            }

            public string ConnectionString
            {
                get { throw new NotImplementedException(); }
            }

            public bool SupportsCompoundStatements
            {
                get { throw new NotImplementedException(); }
            }

            public string GetIdentityFunction()
            {
                throw new NotImplementedException();
            }

            public bool SupportsStoredProcedures
            {
                get { throw new NotImplementedException(); }
            }

            public IProcedureExecutor GetProcedureExecutor(AdoAdapter adapter, ObjectName procedureName)
            {
                throw new NotImplementedException();
            }

            public Type RequestedServiceType { get; private set; }
            public Object GetService(Type serviceType)
            {
                this.RequestedServiceType = serviceType;
                return new StubQueryPager();
            }
        }

        public class StubQueryPager : IQueryPager
        {
            public IEnumerable<string> ApplyPaging(string sql, int skip, int take)
            {
                throw new NotImplementedException();
            }
        }

        public interface ITestInterface { }
        [Export(typeof(ITestInterface))]
        public class TestClass : ITestInterface { }
    }
}
