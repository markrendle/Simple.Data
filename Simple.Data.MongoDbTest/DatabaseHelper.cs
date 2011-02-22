using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MongoDB.Bson.Serialization;
using MongoDB.Driver;

using Simple.Data.MongoDb;

namespace Simple.Data.MongoDbTest
{
    internal static class DatabaseHelper
    {
        static DatabaseHelper()
        {
            BsonSerializer.SerializationProvider = new DynamicSerializationProvider();
        }

        public static dynamic Open()
        {
            return Database.Opener.OpenMongo("mongodb://localhost/simpleDataTests?safe=true");
        }

        public static void Reset()
        {
            var server = MongoServer.Create("mongodb://localhost/?safe=true");
            server.Connect();
            server.DropDatabase("simpleDataTests");
            InsertData(server.GetDatabase("simpleDataTests"));
        }

        private static void InsertData(MongoDatabase db)
        {
            //INSERT INTO [dbo].[Users] ([Id], [Name], [Password], [Age]) VALUES (1,'Bob','Bob',32)
            //INSERT INTO [dbo].[Users] ([Id], [Name], [Password], [Age]) VALUES (2,'Charlie','Charlie',49)
            //INSERT INTO [dbo].[Users] ([Id], [Name], [Password], [Age]) VALUES (3,'Dave','Dave',12)

            var users = new[] 
            {
                new User { Id = 1, Name = "Bob", Password = "Bob", Age = 32, Address = new Address { Line = "123 Way", City = "Dallas", State = "TX" }, EmailAddresses = new List<string> { "bob@bob.com", "b@b.com" }, Dependents = new List<Dependent>{ new Dependent { Name = "Jane", Age = 12 }, new Dependent { Name = "Jimmy", Age = 11 } } },
                new User { Id = 2, Name = "Charlie", Password = "Charlie", Age = 49, Address = new Address { Line = "234 Way", City = "San Francisco", State = "CA" }, EmailAddresses = new List<string> { "charlie@charlier.com" }, Dependents = new List<Dependent>{ new Dependent { Name = "Joanne", Age = 12 } }   },
                new User { Id = 3, Name = "Dave", Password = "Dave", Age = 49, Address = new Address { Line = "345 Way", City = "Austin", State = "TX" } }
            };

            db.GetCollection("Users").InsertBatch(users);
        }
    }
}