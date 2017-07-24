using System;
using System.Dynamic;

namespace Shitty.Data.Commands
{
    class ExistsCommand : ICommand
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
            return method.Equals("exists", StringComparison.InvariantCultureIgnoreCase) || method.Equals("any", StringComparison.InvariantCultureIgnoreCase);
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
            var query = new SimpleQuery(dataStrategy, table.GetQualifiedName());

            if (args.Length == 1)
            {
                var criteria = args[0] as SimpleExpression;
                if (criteria != null)
                {
                    query = query.Where(criteria);
                }
                else
                {
                    throw new BadExpressionException(binder.Name + " requires an expression.");
                }
            }
            else if (args.Length != 0)
            {
                throw new BadExpressionException(binder.Name + " requires an expression.");
            }

            return query.Exists();
        }
    }
}