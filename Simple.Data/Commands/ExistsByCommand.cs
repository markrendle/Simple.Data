using System;
using System.Dynamic;

namespace Shitty.Data.Commands
{
    class ExistsByCommand : ICommand
    {
        /// <summary>
        /// Determines whether the instance is able to handle the specified method.
        /// </summary>
        /// <param name="method">The method name.</param>
        /// <returns>
        /// 	<c>true</c> if the instance is able to handle the specified method; otherwise, <c>false</c>.
        /// </returns>
        public bool IsCommandFor(string method)
        {
            return method.StartsWith("existsby", StringComparison.InvariantCultureIgnoreCase)
                   || method.StartsWith("exists_by", StringComparison.InvariantCultureIgnoreCase)
                   || method.StartsWith("anyby", StringComparison.InvariantCultureIgnoreCase)
                   || method.StartsWith("any_by", StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="dataStrategy">The data strategy.</param>
        /// <param name="table"></param>
        /// <param name="binder">The binder from the <see cref="DynamicTable"/> method invocation.</param>
        /// <param name="args">The arguments from the <see cref="DynamicTable"/> method invocation.</param>
        /// <returns></returns>
        public object Execute(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            var criteriaDictionary = ArgumentHelper.CreateCriteriaDictionary(binder, args, "ExistsBy", "exists_by", "AnyBy", "any_by");
            if (criteriaDictionary == null) return null;

            var criteria  = ExpressionHelper.CriteriaDictionaryToExpression(table.GetQualifiedName(), criteriaDictionary);
            return new SimpleQuery(dataStrategy, table.GetQualifiedName()).Where(criteria).Exists();
        }
    }
}