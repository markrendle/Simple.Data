using System;
using System.Collections.Generic;

using MongoDB.Bson.DefaultSerializer;

namespace Simple.Data.MongoDbTest
{
    class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public int Age { get; set; }

        public Address Address { get; set; }

        public List<string> EmailAddresses { get; set; }

        public List<Dependent> Dependents { get; set; }
    }

    class Address
    {
        public string Line { get; set; }
        public string City { get; set; }
        public string State { get; set; }
    }

    class Dependent
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
