using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    public interface IPluralizer
    {
        bool IsPlural(string word);
        bool IsSingular(string word);
        string Pluralize(string word);
        string Singularize(string word);
    }
}
