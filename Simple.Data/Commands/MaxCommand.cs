using System;
using System.Dynamic;

namespace Simple.Data.Commands
{
	class MaxCommand : ICommand
	{
		public bool IsCommandFor(string method)
		{
			return method.StartsWith("Max") || method.StartsWith("max_", StringComparison.InvariantCultureIgnoreCase);
		}

		public object Execute(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
		{
//			var criteriaExpression = ExpressionHelper.CriteriaDictionaryToExpression(table.GetQualifiedName(), MethodNameParser.ParseFromBinder(binder, args));
//			var data = dataStrategy.Find(table.GetQualifiedName(), criteriaExpression).FirstOrDefault();
//			return data != null ? new SimpleRecord(data, table.GetQualifiedName(), dataStrategy) : null;
			var data = dataStrategy.Max(table.GetQualifiedName(), "Id");
			return null;
		}
	}
}