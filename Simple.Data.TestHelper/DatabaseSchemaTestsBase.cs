using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;

namespace Simple.Data.TestHelper
{
    [TestFixture]
    public abstract class DatabaseSchemaTestsBase
    {
        private DatabaseSchema _schema;

        private DatabaseSchema GetSchema()
        {
            var adapter = GetDatabase().Adapter as AdoAdapter;
            if (adapter == null) Assert.Fail("Expected an ADO-based database adapter.");
            return adapter.GetSchema();
        }

        protected abstract Database GetDatabase();

        protected DatabaseSchema Schema
        {
            get { return (_schema ?? (_schema = GetSchema())); }
        }
    }
}
