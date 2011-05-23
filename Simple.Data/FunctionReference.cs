using System.Collections.Generic;

namespace Simple.Data
{
    public class FunctionReference : SimpleReference
    {
        private static readonly HashSet<string> KnownFunctionNames = new HashSet<string>
                                                                         {
                                                                             "min", "max", "average", "length", "sum", "count",
                                                                         };

        private static readonly HashSet<string> AggregateFunctionNames = new HashSet<string>
                                                                             {
                                                                                 "min", "max", "average", "sum", "count",
                                                                             };
        private readonly string _name;
        private readonly SimpleReference _argument;
        private readonly bool _isAggregate;
        private readonly string _alias;

        internal FunctionReference(string name, SimpleReference argument)
        {
            _name = name;
            _argument = argument;
            _isAggregate = AggregateFunctionNames.Contains(name.ToLowerInvariant());
        }

        private FunctionReference(string name, SimpleReference argument, bool isAggregate, string alias)
        {
            _name = name;
            _argument = argument;
            _isAggregate = isAggregate;
            _alias = alias;
        }

        public FunctionReference As(string alias)
        {
            return new FunctionReference(_name, _argument, _isAggregate, alias);
        }

        public string Alias
        {
            get { return _alias; }
        }

        public bool IsAggregate
        {
            get { return _isAggregate; }
        }

        public string Name
        {
            get { return _name; }
        }

        public SimpleReference Argument
        {
            get { return _argument; }
        }

        public static bool TryCreate(string name, SimpleReference argument, out object functionReference)
        {
            if (!KnownFunctionNames.Contains(name.ToLowerInvariant()))
            {
                functionReference = null;
                return false;
            }

            functionReference = new FunctionReference(name, argument);
            return true;
        }
    }
}