namespace Simple.Data.Ado
{
    public interface IFunctionNameConverter
    {
        string ConvertToSqlName(string simpleFunctionName);
    }
}