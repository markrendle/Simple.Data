namespace Shitty.Data.Ado
{
    using System;

    class FunctionNameConverter : IFunctionNameConverter
    {
        public string ConvertToSqlName(string simpleFunctionName)
        {
            if (simpleFunctionName.Equals("length", StringComparison.InvariantCultureIgnoreCase))
            {
                return "len";
            }
            if (simpleFunctionName.Equals("average", StringComparison.InvariantCultureIgnoreCase))
            {
                return "avg";
            }
            return simpleFunctionName;
        }
    }
}