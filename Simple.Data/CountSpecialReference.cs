using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    public abstract class SpecialReference : DynamicReference
    {
        protected SpecialReference(string name) : base(name)
        {
        }
    }
    public class CountSpecialReference : SpecialReference
    {
        public CountSpecialReference() : base("COUNT")
        {
            
        }
    }
}
