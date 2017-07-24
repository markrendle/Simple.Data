using System;
using System.Dynamic;

namespace Shitty.Data.Commands
{
    class AllCommand : ICommand
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
            return method.Equals("all", StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="dataStrategy">The database or transaction.</param>
        /// <param name="table"></param>
        /// <param name="binder">The binder from the <see cref="DynamicTable"/> method invocation.</param>
        /// <param name="args">The arguments from the <see cref="DynamicTable"/> method invocation.</param>
        /// <returns></returns>
        public object Execute(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            return new SimpleQuery(dataStrategy, table.GetQualifiedName());
        }
    }
}
