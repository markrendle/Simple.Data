namespace Simple.Data.Commands
{
    using System;
    using System.Dynamic;

    class QueryCommand : ICommand, ICreateDelegate
    {
        public bool IsCommandFor(string method)
        {
            return method.Equals("query", StringComparison.InvariantCultureIgnoreCase);
        }

        public object Execute(DataStrategy dataStrategy, SimpleQuery query, InvokeMemberBinder binder, object[] args)
        {
            throw new NotImplementedException();
        }

        public Func<object[], object> CreateDelegate(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            return a => new SimpleQuery(dataStrategy, table.GetQualifiedName());
        }

        public object Execute(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            return new SimpleQuery(dataStrategy, table.GetQualifiedName());
        }
    }
}