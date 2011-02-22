using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Dynamic;

namespace Simple.Data.MongoDb
{
    internal class MongoAdapterDeleter
    {
        private readonly MongoAdapter _adapter;
        private readonly IExpressionFormatter _expressionFormatter;

        public MongoAdapterDeleter(MongoAdapter adapter, IExpressionFormatter expressionFormatter)
        {
            if (adapter == null) throw new ArgumentNullException("adapter");
            _adapter = adapter;

            _expressionFormatter = expressionFormatter;
        }

        public int Delete(MongoCollection<BsonDocument> collection, SimpleExpression criteria)
        {
            var condition = _expressionFormatter.Format(criteria);

            var result = collection.Remove(condition);
            if (result != null)
                return result.DocumentsAffected;

            return int.MaxValue;
        }
    }
}