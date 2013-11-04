using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    public abstract class Command
    {
        private readonly Database _database;
        private readonly string _tableName;

        protected Command(Database database, string tableName)
        {
            _tableName = tableName;
            _database = database;
        }

        public Database Database
        {
            get { return _database; }
        }

        public string TableName
        {
            get { return _tableName; }
        }
    }
}
