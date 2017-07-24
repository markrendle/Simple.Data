using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Shitty.Data.Ado;
using Shitty.Data.Ado.Schema;

namespace Shitty.Data.TestHelper
{
    [TestFixture]
    public abstract class DatabaseSchemaTestsBase
    {
        private DatabaseSchema _schema;

        private DatabaseSchema GetSchema()
        {
            var adapter = GetDatabase().GetAdapter() as AdoAdapter;
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
