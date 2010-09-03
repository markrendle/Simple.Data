using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data;
using System.Dynamic;
using Simple.Data.Ado;
using Simple.Data.Schema;

namespace Simple.Data
{
    public class Database : DynamicObject
    {
        private readonly IAdapter _adapter;
        private readonly IConnectionProvider _connectionProvider;
        private readonly IDbConnection _connection;
        private readonly string _connectionString;

        private Database(string connectionString)
        {
            _connectionString = connectionString;
        }

        internal Database(IAdapter adapter)
        {
            _adapter = adapter;
        }

        internal Database(IDbConnection connection)
        {
            _connection = connection;
        }

        internal Database(IConnectionProvider connectionProvider)
        {
            _adapter = new AdoAdapter(this, connectionProvider);
            _connectionProvider = connectionProvider;
        }

        public IAdapter Adapter
        {
            get { return _adapter; }
        }

        public static dynamic Open()
        {
            return new Database(new SqlProvider(Properties.Settings.Default.ConnectionString));
        }

        public static dynamic OpenConnection(string connectionString)
        {
            return new Database(new SqlProvider(connectionString));
        }

        public static dynamic OpenFile(string filename)
        {
            return new Database(ProviderHelper.GetProviderByFilename(filename));
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
