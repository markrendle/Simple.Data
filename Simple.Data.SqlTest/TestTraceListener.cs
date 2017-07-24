using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Shitty.Data.SqlTest
{
    public class TestTraceListener : TraceListener
    {
        private readonly StringBuilder _builder = new StringBuilder();

        public override void Write(string message, string category)
        {
            if (category.Equals("Simple.Data.SqlTest"))
            {
                Write(message);
            }
        }
        /// <summary>
        /// When overridden in a derived class, writes the specified message to the listener you create in the derived class.
        /// </summary>
        /// <param name="message">A message to write. </param><filterpriority>2</filterpriority>
        public override void Write(string message)
        {
            _builder.Append(message);
        }

        /// <summary>
        /// When overridden in a derived class, writes a message to the listener you create in the derived class, followed by a line terminator.
        /// </summary>
        /// <param name="message">A message to write. </param><filterpriority>2</filterpriority>
        public override void WriteLine(string message)
        {
            _builder.AppendLine(message);
        }

        public string Output
        {
            get { return _builder.ToString(); }
        }
    }
}
