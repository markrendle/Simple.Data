namespace Simple.Data
{
    public class SimpleEmptyExpression : SimpleExpression
    {
        public SimpleEmptyExpression() : base(null, null, SimpleExpressionType.Empty)
        {
        }
    }
}