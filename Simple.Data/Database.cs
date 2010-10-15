using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using Simple.Data.Ado;
using Simple.Data.Properties;
using Simple.Data.Schema;

namespace Simple.Data
{
    public class Database : DynamicObject
    {
        private readonly IAdapter _adapter;

        internal Database(IAdapter adapter)
        {
            _adapter = adapter;
        }

        internal Database(IConnectionProvider connectionProvider)
        {
            _adapter = new AdoAdapter(this, connectionProvider);
        }

        public IAdapter Adapter
        {
            get { return _adapter; }
        }

        public static dynamic Open()
        {
            return DatabaseOpener.OpenDefault();
        }

        public static dynamic OpenConnection(string connectionString)
        {
            return DatabaseOpener.OpenConnection(connectionString);
        }

        public static dynamic OpenFile(string filename)
        {
            return DatabaseOpener.OpenFile(filename);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return base.TryGetMember(binder, out result)
                   || NewDynamicTable(binder, out result);
        }

        private bool NewDynamicTable(GetMemberBinder binder, out object result)
        {
            result = new DynamicTable(binder.Name, this);
            return true;
        }

        internal DatabaseSchema GetSchema()
        {
            return new DatabaseSchema(this);
        }
    }
}