using System.Data;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Ado
{
    public interface ICommandBuilder
    {
        ParameterTemplate AddParameter(object value, Column column);
        void Append(string text);
        IDbCommand GetCommand(IDbConnection connection);
        CommandTemplate GetCommandTemplate();
    }
}