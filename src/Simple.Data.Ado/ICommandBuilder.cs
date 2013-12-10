using System.Data;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Ado
{
    using System.Collections.Generic;

    public interface ICommandBuilder
    {
        ParameterTemplate AddParameter(object value);
        ParameterTemplate AddParameter(object value, Column column);
        void Append(string text);
        IDbCommand GetCommand(IDbConnection connection, AdoOptions options);
        IDbCommand GetRepeatableCommand(IDbConnection connection, AdoOptions options);
        CommandTemplate GetCommandTemplate(Table table);
        IEnumerable<KeyValuePair<ParameterTemplate, object>> Parameters { get; }
        string Text { get; }
    }
}