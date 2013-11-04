using System;
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
        /// <param name="dataStrategy">The database or transaction.</param>
        /// <param name="table">The table.</param>
        /// <param name="binder">The binder from the <see cref="DynamicTable"/> method invocation.</param>
        /// <param name="args">The arguments from the <see cref="DynamicTable"/> method invocation.</param>
        /// <returns></returns>
        object Execute(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args);
    }

    interface IQueryCompatibleCommand
    {
        object Execute(DataStrategy dataStrategy, SimpleQuery query, InvokeMemberBinder binder, object[] args);
    }

    interface ICreateDelegate
    {
        Func<object[], object> CreateDelegate(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder,
                                              object[] args);
    }
}
