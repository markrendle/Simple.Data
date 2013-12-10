using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    public class ArgumentCollection : Collection<Argument>
    {
        public IDictionary<string, object> NamedArgumentsToDictionary()
        {
            return this.Where(arg => !string.IsNullOrWhiteSpace(arg.Name))
                .ToDictionary(arg => arg.Name, arg => arg.Value);
        }
    }
}
