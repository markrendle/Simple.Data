using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.SqlServer
{
    using System.ComponentModel.Composition;
    using System.Data.SqlClient;
    using System.Text.RegularExpressions;
    using Ado;

    [Export(typeof(CommandOptimizer))]
    public class SqlCommandOptimizer : CommandOptimizer
    {
        public override System.Data.IDbCommand OptimizeFindOne(System.Data.IDbCommand command)
        {
            command.CommandText = Regex.Replace(command.CommandText, "^SELECT ", "SELECT TOP 1 ",
                                                RegexOptions.IgnoreCase);
            return command;
        }
    }
}
