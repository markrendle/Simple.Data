using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Collections.ObjectModel;

namespace Simple.Data
{
    public class SimpleFunction
    {
        private readonly string _name;
        private readonly ReadOnlyCollection<object> _args;

        public string Name
        {
            get { return _name; }
        }

        public ReadOnlyCollection<object> Args
        {
            get { return _args; }
        }

        public SimpleFunction(string name, object[] args)
        {
            _name = name;
            _args = args.ToList().AsReadOnly();
        }
    }
}