using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MongoDB.Bson;

namespace Simple.Data.MongoDb
{
    public static class BsonDocumentExtensions
    {
        public static IDictionary<string, object> ToDictionary(this BsonDocument document)
        {
            return document.Elements.ToDictionary(x => x.Name, x => ConvertValue(x.Value), HomogenizedEqualityComparer.DefaultInstance);
        }

        private static object ConvertValue(BsonValue value)
        {
            if (value.IsBsonDocument)
                return value.AsBsonDocument.ToDictionary();
            if (value.IsBsonArray)
                return value.AsBsonArray.Select(v => ConvertValue(v)).ToList();

            return value.RawValue;
        }
    }
}