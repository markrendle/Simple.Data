using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using Simple.Data.Extensions;

namespace Simple.Data.Commands
{
    internal class UpdateCommand : ICommand
    {
        public bool IsCommandFor(string method)
        {
            return method.Equals("update", StringComparison.InvariantCultureIgnoreCase);
        }

        public object Execute(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            if (args.Length != 1) throw new ArgumentException("Incorrect number of arguments to Update method.");
            var keyFieldNames = dataStrategy.Adapter.GetKeyFieldNames(table.GetQualifiedName()).ToArray();
            if (keyFieldNames.Length == 0)
            {
                throw new NotSupportedException("Adapter does not support key-based update for this object.");
            }

            return UpdateByKeyFields(table.GetQualifiedName(), dataStrategy, args[0], keyFieldNames);
        }

        internal static object UpdateByKeyFields(string tableName, DataStrategy dataStrategy, object entity, IEnumerable<string> keyFieldNames)
        {
            var record = ObjectToDictionary(entity);
            var criteria = GetCriteria(keyFieldNames, record);
            var criteriaExpression = ExpressionHelper.CriteriaDictionaryToExpression(tableName, criteria);
            return dataStrategy.Update(tableName, record, criteriaExpression);
        }

        private static Dictionary<string, object> GetCriteria(IEnumerable<string> keyFieldNames, IDictionary<string, object> record)
        {
            var criteria = new Dictionary<string, object>();

            foreach (var keyFieldName in keyFieldNames)
            {
                if (!record.ContainsKey(keyFieldName))
                {
                    throw new InvalidOperationException("Key field value not set.");
                }

                criteria.Add(keyFieldName, record[keyFieldName]);
                record.Remove(keyFieldName);
            }
            return criteria;
        }

        private static IDictionary<string,object> ObjectToDictionary(object obj)
        {
            var dynamicRecord = obj as SimpleRecord;
            if (dynamicRecord != null)
            {
                return new Dictionary<string, object>(dynamicRecord, HomogenizedEqualityComparer.DefaultInstance);
            }

            var dictionary = obj as IDictionary<string, object>;
            if (dictionary != null)
            {
                return dictionary;
            }

            return RegularObjectToDictionary(obj);
        }

        private static IDictionary<string, object> RegularObjectToDictionary(object obj)
        {
            var record = new Dictionary<string, object>(HomogenizedEqualityComparer.DefaultInstance);
            foreach (var property in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = property.GetValue(obj, null);
                if (value != null)
                {
                    record.Add(property.Name, value);
                }
            }
            return record;
        }
    }
}
