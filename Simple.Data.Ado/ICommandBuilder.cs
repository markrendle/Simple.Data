using System.Data;

namespace Simple.Data.Ado
{
    public interface ICommandBuilder
    {
        string AddParameter(object value);
        void Append(string text);
        IDbCommand GetCommand(IDbConnection connection);
    }
}