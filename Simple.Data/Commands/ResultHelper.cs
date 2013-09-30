using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Commands
{
    using Extensions;

    internal enum ExpectedResultType
    {
        Single,
        Enumerable
    }
    internal static class ResultHelper
    {
        public static object TypeResult(object result, DynamicTable table, DataStrategy dataStrategy)
        {
            var dataResult = result as DataResult;
            if (dataResult != null)
            {
                if (dataResult.Data == null) return null;
                var checkedEnumerable = CheckedEnumerable.Create(dataResult.Data);
                if (checkedEnumerable.IsEmpty)
                {
                    return null;
                }
                if (checkedEnumerable.HasMoreThanOneValue)
                {
                    return new SimpleResultSet(checkedEnumerable.Select(d => d.ToDynamicRecord(table.GetQualifiedName(), dataStrategy)));
                }
                return checkedEnumerable.Single.ToDynamicRecord(table.GetQualifiedName(), dataStrategy);
            }
            var dictionary = result as IDictionary<string, object>;
            if (dictionary != null) return dictionary.ToDynamicRecord(table.GetQualifiedName(), dataStrategy);

            var list = result as IEnumerable<IDictionary<string, object>>;
            if (list != null)
            {
                return new SimpleResultSet(list.Select(d => d.ToDynamicRecord(table.GetQualifiedName(), dataStrategy)));
            }

            return null;
        }
    }
}
