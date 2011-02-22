using MongoDB.Driver.Builders;

namespace Simple.Data.MongoDb
{
    public interface IExpressionFormatter
    {
        QueryComplete Format(SimpleExpression expression);
    }
}