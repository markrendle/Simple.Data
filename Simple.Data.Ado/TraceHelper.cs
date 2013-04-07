using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Simple.Data.Ado
{
    public static class TraceHelper
    {
        private static readonly string EOL = Environment.NewLine;

        public static void WriteTrace (this IDbCommand command)
        {
            if (!SimpleDataTraceSources.TraceSource.Switch.Level.HasFlag(SourceLevels.Information))
                return;
            try
            {
                var sb = new StringBuilder();
                sb.AppendFormat("SQL command (CommandType={0}):{1}  {2}", command.CommandType, EOL, command.CommandText);
                if (command.Parameters.Count > 0)
                {
                    sb.AppendFormat("Parameters:{0}", EOL);
                    foreach (var parameter in command.Parameters.OfType<DbParameter>())
                    {
                        object strValue = parameter.Value is string ? string.Format("\"{0}\"", parameter.Value) : parameter.Value;
                        sb.AppendFormat("  {0} ({1}) = {2}{3}", parameter.ParameterName, parameter.DbType, strValue, EOL);
                    }
                }
                SimpleDataTraceSources.TraceSource.TraceEvent(TraceEventType.Information, SimpleDataTraceSources.SqlMessageId, sb.ToString());
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