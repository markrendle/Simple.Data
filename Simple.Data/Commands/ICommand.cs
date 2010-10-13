using System.Dynamic;

namespace Simple.Data.Commands
{
    /// <summary>
    /// Provides the execution for methods invoked on <see cref="DynamicTable"/>.
    /// </summary>
    interface ICommand
    {
        /// <summary>
        /// Determines whether the instance is able to handle the specified method.
        /// </summary>
        /// <param name="method">The method name.</param>
        /// <returns>
        /// 	<c>true</c> if the instance is able to handle the specified method; otherwise, <c>false</c>.
        /// </returns>
        bool IsCommandFor(string method);

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="binder">The binder from the <see cref="DynamicTable"/> method invocation.</param>
        /// <param name="args">The arguments from the <see cref="DynamicTable"/> method invocation.</param>
        /// <returns></returns>
        object Execute(Database database, string tableName, InvokeMemberBinder binder, object[] args);
    }
}
