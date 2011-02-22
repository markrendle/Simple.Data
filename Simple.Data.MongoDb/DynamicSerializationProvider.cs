using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization;
using System.Dynamic;
using MongoDB.Bson.DefaultSerializer;

namespace Simple.Data.MongoDb
{
    public class DynamicSerializationProvider : IBsonSerializationProvider
    {
        static DynamicSerializationProvider()
        {
            BsonDefaultSerializer.Initialize();
        }

        public IBsonSerializer GetSerializer(Type type)
        {
            if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(type))
                return new DynamicBsonSerializer();

            return BsonDefaultSerializer.Instance.GetSerializer(type);
        }
    }
}