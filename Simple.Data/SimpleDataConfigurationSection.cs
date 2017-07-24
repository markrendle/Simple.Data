using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shitty.Data
{
    using System.Configuration;
    using System.Diagnostics;

    public class SimpleDataConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("traceLevel", DefaultValue = TraceLevel.Info, IsRequired = false)]
        public TraceLevel TraceLevel
        {
            get { return (TraceLevel) this["traceLevel"]; }
            set { this["traceLevel"] = value; }
        }
    }
}
