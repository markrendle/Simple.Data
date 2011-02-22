using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MongoDB.Driver;

namespace Simple.Data.MongoDb
{
    public static class DatabaseOpenerExtensions
    {
        public static Database OpenMongo(this IDatabaseOpener opener, string connectionString)
        {
            return opener.Open("MongoDb", new { ConnectionString = connectionString });
        }

        public static Database OpenMongo(this IDatabaseOpener opener, MongoConnectionStringBuilder connectionStringBuilder)
        {
            return opener.Open("MongoDb", new { ConnectionString = connectionStringBuilder });
        }

        public static Database OpenMongo(this IDatabaseOpener opener, MongoUrl mongoUrl)
        {
            return opener.Open("MongoDb", new { ConnectionString = mongoUrl });
        }

        public static Database OpenMongo(this IDatabaseOpener opener, Uri uri)
        {
            return opener.Open("MongoDb", new { ConnectionString = uri });
        }

        public static Database OpenMongo(this IDatabaseOpener opener, MongoServerSettings settings, string databaseName)
        {
            return opener.Open("MongoDb", new { Settings = settings, DatabaseName = databaseName });
        }
    }
}