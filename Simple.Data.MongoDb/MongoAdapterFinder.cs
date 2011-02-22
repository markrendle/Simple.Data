using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Simple.Data.MongoDb
{
    internal class MongoAdapterFinder
    {
        private readonly MongoAdapter _adapter;
        private readonly IExpressionFormatter _expressionFormatter;

        public MongoAdapterFinder(MongoAdapter adapter, IExpressionFormatter expressionFormatter)
        {
            if (adapter == null) throw new ArgumentNullException("adapter");
            _adapter = adapter;
            _expressionFormatter = expressionFormatter;
        }

        public IEnumerable<IDictionary<string, object>> Find(MongoCollection<BsonDocument> collection, SimpleExpression criteria)
        {
            if (criteria == null) return FindAll(collection);

            var query = _expressionFormatter.Format(criteria);

            return collection.Find(query).Select(x => x.ToDictionary());
        }

        public IEnumerable<IDictionary<string, object>> FindAll(MongoCollection<BsonDocument> collection)
        {
            return collection.FindAll().Select(d => d.ToDictionary());
        }


    }
}