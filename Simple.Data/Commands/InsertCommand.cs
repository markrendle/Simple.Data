using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using Simple.Data.Extensions;

namespace Simple.Data.Commands
{
    using System.Collections;
    using System.Collections.ObjectModel;
    using Operations;

    class InsertCommand : ICommand
    {
        public bool IsCommandFor(string method)
        {
            return method.Equals("insert", StringComparison.InvariantCultureIgnoreCase);
        }

        public object Execute(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            var result = DoInsert(binder, args, dataStrategy, table.GetQualifiedName());

            return ResultHelper.TypeResult(result, table, dataStrategy);
        }

        private static object DoInsert(InvokeMemberBinder binder, object[] args, DataStrategy dataStrategy, string tableName)
        {
            if (binder.HasSingleUnnamedArgument())
            {
                return InsertEntity(args[0], dataStrategy, tableName, (r,e) => false, !binder.IsResultDiscarded());
            }

            if (args.Length == 2)
            {
                var onError = args[1] as ErrorCallback;
                if (onError != null)
                {
                    return InsertEntity(args[0], dataStrategy, tableName, onError, !binder.IsResultDiscarded());
                }
            }
            return InsertDictionary(binder, args, dataStrategy, tableName);
        }

        private static DataResult InsertDictionary(InvokeMemberBinder binder, IEnumerable<object> args, DataStrategy dataStrategy, string tableName)
        {
            var operation = new InsertOperation(tableName, Enumerable.Repeat(binder.NamedArgumentsToDictionary(args), 1),
                !binder.IsResultDiscarded());
            return (DataResult)dataStrategy.Run.Execute(operation);
        }

        private static DataResult InsertEntity(object entity, DataStrategy dataStrategy, string tableName, ErrorCallback onError, bool resultRequired)
        {
            var readOnlyDictionary = entity as IReadOnlyDictionary<string, object>;
            if (readOnlyDictionary != null)
            {
                var operation = new InsertOperation(tableName, readOnlyDictionary, resultRequired);
                return (DataResult)dataStrategy.Run.Execute(operation);
            }
            
            var dictionary = entity as IDictionary<string, object>;
            if (dictionary != null)
            {
                var operation = new InsertOperation(tableName, dictionary, resultRequired);
                return (DataResult)dataStrategy.Run.Execute(operation);
            }

            var readonlyDictionaryList = entity as IEnumerable<IReadOnlyDictionary<string, object>>;
            if (readonlyDictionaryList != null)
            {
                return (DataResult)dataStrategy.Run.Execute(new InsertOperation(tableName, readonlyDictionaryList, resultRequired, onError));
            }
            
            var dictionaryList = entity as IEnumerable<IDictionary<string, object>>;
            if (dictionaryList != null)
            {
                return (DataResult)dataStrategy.Run.Execute(new InsertOperation(tableName, dictionaryList, resultRequired, onError));
            }

            var entityList = entity as IEnumerable;
            if (entityList != null)
            {
                var array = entityList.Cast<object>().ToArray();
                var rows = new List<IReadOnlyDictionary<string, object>>();
                foreach (var o in array)
                {
                    readOnlyDictionary = (o as IReadOnlyDictionary<string, object>) ?? o.ObjectToDictionary();
                    if (readOnlyDictionary.Count == 0)
                    {
                        throw new SimpleDataException("Could not discover data in object.");
                    }
                    rows.Add(readOnlyDictionary);
                }

                return (DataResult)dataStrategy.Run.Execute(new InsertOperation(tableName, rows, resultRequired, onError));
            }

            readOnlyDictionary = entity.ObjectToDictionary();
            if (readOnlyDictionary.Count == 0)
                throw new SimpleDataException("Could not discover data in object.");
            return (DataResult)dataStrategy.Run.Execute(new InsertOperation(tableName, readOnlyDictionary, resultRequired));
        }
    }
}
