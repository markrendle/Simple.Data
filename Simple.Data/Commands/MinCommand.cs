using System.Dynamic;

namespace Simple.Data.Commands
{
	class MinCommand : ICommand
	{
		public bool IsCommandFor(string method)
		{
			return method.StartsWith("Min");
		}

		public object Execute(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
		{
			SimpleExpression criteriaExpression = null;
			if (args.Length == 1 && args[0] is SimpleExpression)
			{
				criteriaExpression = args[0] as SimpleExpression;
			}

			var columName = binder.Name.Substring(3);
			var data = dataStrategy.Min(table.GetQualifiedName(), columName, criteriaExpression);
			return data;
		}
	}
}