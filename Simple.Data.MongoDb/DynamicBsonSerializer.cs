using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.IO;
using System.Dynamic;
using MongoDB.Bson.DefaultSerializer;
using System.Linq.Expressions;

using Microsoft.CSharp.RuntimeBinder;
using System.Runtime.CompilerServices;
using System.Collections.Concurrent;

namespace Simple.Data.MongoDb
{
    public class DynamicBsonSerializer : BsonBaseSerializer
    {
        private static DynamicBsonSerializer singleton = new DynamicBsonSerializer();

        public override object Deserialize(BsonReader bsonReader, Type nominalType, IBsonSerializationOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Serialize(BsonWriter bsonWriter, Type nominalType, object value, IBsonSerializationOptions options)
        {
            if (value == null)
            {
                bsonWriter.WriteNull();
                return;
            }
            var metaObject = ((IDynamicMetaObjectProvider)value).GetMetaObject(Expression.Constant(value));
            var memberNames = metaObject.GetDynamicMemberNames().ToList();
            if (memberNames.Count == 0)
            {
                bsonWriter.WriteNull();
                return;
            }

            bsonWriter.WriteStartDocument();
            foreach (var memberName in memberNames)
            {
                bsonWriter.WriteName(memberName);
                var memberValue = BinderHelper.GetMemberValue(value, memberName);
                if (memberValue == null)
                    bsonWriter.WriteNull();
                else
                {
                    var serializer = BsonSerializer.LookupSerializer(memberValue.GetType());
                    serializer.Serialize(bsonWriter, nominalType, memberValue, options);
                }
            }
            bsonWriter.WriteEndDocument();
        }

        private static class BinderHelper
        {
            private static readonly ConcurrentDictionary<string, CallSite<Func<CallSite, object, object>>> _cache = new ConcurrentDictionary<string, CallSite<Func<CallSite, object, object>>>();

            public static object GetMemberValue(object owner, string memberName)
            {
                var getSite = _cache.GetOrAdd(
                    memberName,
                    key => CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, key, typeof(BinderHelper), new [] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) })));
                
                return getSite.Target(getSite, owner);
            }

        }
    }
}