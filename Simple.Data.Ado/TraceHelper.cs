using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Simple.Data.Ado
{
    using System.Security;

    public static class TraceHelper
    {
        public static void WriteTrace(this IDbCommand command)
        {
            if (Database.TraceLevel < TraceLevel.Info) return;
            try
            {
                var str = new StringBuilder();
                str.AppendLine();
                str.AppendLine(command.CommandType.ToString());
                str.AppendLine(command.CommandText);
                foreach (var parameter in command.Parameters.OfType<DbParameter>())
                {
                    str.AppendFormat("{0} ({1}) = {2}", parameter.ParameterName, parameter.DbType, parameter.Value);
                    str.AppendLine();
                }

                Trace.WriteLine(str.ToString(), "Simple.Data.Ado");
            }
            catch (Exception)
            {
#if(DEBUG)
                throw;
#endif
            }
        }
    }
}
