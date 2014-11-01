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
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Operations;

    class InsertCommand : ICommand
    {
        public bool IsCommandFor(string method)
        {
            return method.Equals("insert", StringComparison.InvariantCultureIgnoreCase);
        }

        public object Execute(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            var operation = DoInsert(binder, args, dataStrategy, table.GetQualifiedName());
            return new InsertAwaitable(dataStrategy, operation, table);
        }

        private static InsertOperation DoInsert(InvokeMemberBinder binder, object[] args, DataStrategy dataStrategy, string tableName)
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

        private static InsertOperation InsertDictionary(InvokeMemberBinder binder, IEnumerable<object> args, DataStrategy dataStrategy, string tableName)
        {
            return new InsertOperation(tableName, Enumerable.Repeat(binder.NamedArgumentsToDictionary(args), 1),
                !binder.IsResultDiscarded());
        }

        private static InsertOperation InsertEntity(object entity, DataStrategy dataStrategy, string tableName, ErrorCallback onError, bool resultRequired)
        {
            var readOnlyDictionary = entity as IReadOnlyDictionary<string, object>;
            if (readOnlyDictionary != null)
            {
                return new InsertOperation(tableName, readOnlyDictionary, resultRequired);
            }
            
            var dictionary = entity as IDictionary<string, object>;
            if (dictionary != null)
            {
                return new InsertOperation(tableName, dictionary, resultRequired);
            }

            var readonlyDictionaryList = entity as IEnumerable<IReadOnlyDictionary<string, object>>;
            if (readonlyDictionaryList != null)
            {
                return new InsertOperation(tableName, readonlyDictionaryList, resultRequired, onError);
            }
            
            var dictionaryList = entity as IEnumerable<IDictionary<string, object>>;
            if (dictionaryList != null)
            {
                return new InsertOperation(tableName, dictionaryList, resultRequired, onError);
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

                return new InsertOperation(tableName, rows, resultRequired, onError);
            }

            readOnlyDictionary = entity.ObjectToDictionary();
            if (readOnlyDictionary.Count == 0)
                throw new SimpleDataException("Could not discover data in object.");
            return new InsertOperation(tableName, readOnlyDictionary, resultRequired);
        }
    }

    public class InsertAwaitable : IAwaitable
    {
        private readonly DataStrategy _dataStrategy;
        private readonly IOperation _operation;
        private readonly DynamicTable _table;

        public InsertAwaitable(DataStrategy dataStrategy, IOperation operation, DynamicTable table)
        {
            _dataStrategy = dataStrategy;
            _operation = operation;
            _table = table;
        }

        public TaskAwaiter<dynamic> GetAwaiter()
        {
            return Execute().GetAwaiter();
        }

        public async Task<IEnumerable<dynamic>> ToList()
        {
            IEnumerable<dynamic> enumerable = await Execute();
            return enumerable.ToList();
        }

        public async Task<IEnumerable<T>> ToList<T>()
        {
            IEnumerable<dynamic> enumerable = await Execute();
            return new List<T>(enumerable.Select(item => (T)item));
        }

        private async Task<dynamic> Execute()
        {
            var result = await _dataStrategy.Run.Execute(_operation);
            return ResultHelper.TypeResult(result, _table, _dataStrategy);
        }
    }
}
