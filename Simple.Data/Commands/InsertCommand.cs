﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using Simple.Data.Extensions;

namespace Simple.Data.Commands
{
    using System.Collections;

    class InsertCommand : ICommand
    {
        public bool IsCommandFor(string method)
        {
            return method.Equals("insert", StringComparison.InvariantCultureIgnoreCase);
        }

        public object Execute(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            var result = DoInsert(binder, args, dataStrategy, table.GetQualifiedName());
            
            var dictionary = result as IDictionary<string,object>;
            if (dictionary != null) return dictionary.ToDynamicRecord(table.GetQualifiedName(), dataStrategy);

            var list = result as IEnumerable<IDictionary<string, object>>;
            if (list != null) return new SimpleResultSet(list.Select(d => d.ToDynamicRecord(table.GetQualifiedName(), dataStrategy)));

            return null;
        }

        public Func<object[], object> CreateDelegate(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            throw new NotImplementedException();
        }

        private static object DoInsert(InvokeMemberBinder binder, object[] args, DataStrategy dataStrategy, string tableName)
        {
            if (binder.HasSingleUnnamedArgument())
            {
                return InsertEntity(args[0], dataStrategy, tableName, (r,e) => false, !binder.IsResultDiscarded());
            }

            if (args.Length == 2)
            {
                var onError = args[1] as Func<dynamic, Exception, bool>;
                if (onError != null)
                {
                    return InsertEntity(args[0], dataStrategy, tableName, onError, !binder.IsResultDiscarded());
                }
            }
            return InsertDictionary(binder, args, dataStrategy, tableName);
        }

        private static object InsertDictionary(InvokeMemberBinder binder, object[] args, DataStrategy dataStrategy, string tableName)
        {
            return dataStrategy.Insert(tableName, binder.NamedArgumentsToDictionary(args), !binder.IsResultDiscarded());
        }

        private static object InsertEntity(object entity, DataStrategy dataStrategy, string tableName, Func<dynamic,Exception,bool> onError, bool resultRequired)
        {
            var dictionary = entity as IDictionary<string, object>;
            if (dictionary != null)
                return dataStrategy.Insert(tableName, dictionary, resultRequired);

            var list = entity as IEnumerable<IDictionary<string, object>>;
            if (list != null)
                return dataStrategy.Insert(tableName, list, onError, resultRequired);

            var entityList = entity as IEnumerable;
            if (entityList != null)
            {
                var array = entityList.Cast<object>().ToArray();
                var rows = new List<IDictionary<string, object>>();
                foreach (var o in array)
                {
                    dictionary = (o as IDictionary<string, object>) ?? o.ObjectToDictionary();
                    if (dictionary.Count == 0)
                    {
                        throw new SimpleDataException("Could not discover data in object.");
                    }
                    rows.Add(dictionary);
                }

                return dataStrategy.Insert(tableName, rows, onError, resultRequired);
            }

            dictionary = entity.ObjectToDictionary();
            if (dictionary.Count == 0)
                throw new SimpleDataException("Could not discover data in object.");
            return dataStrategy.Insert(tableName, dictionary, resultRequired);
        }
    }
}
