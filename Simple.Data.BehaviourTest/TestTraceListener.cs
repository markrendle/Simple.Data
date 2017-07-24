using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Shitty.Data.IntegrationTest
{
    class TestTraceListener : TraceListener
    {
        public TestTraceListener() : base(typeof(TestTraceListener).Name)
        {
            
        }

        private readonly StringBuilder _messages = new StringBuilder();

        /// <summary>
        /// When overridden in a derived class, writes the specified message to the listener you create in the derived class.
        /// </summary>
        /// <param name="message">A message to write. </param><filterpriority>2</filterpriority>
        public override void Write(string message)
        {
            _messages.Append(message);
        }

        /// <summary>
        /// When overridden in a derived class, writes a message to the listener you create in the derived class, followed by a line terminator.
        /// </summary>
        /// <param name="message">A message to write. </param><filterpriority>2</filterpriority>
        public override void WriteLine(string message)
        {
            _messages.AppendLine(message);
        }

        public string Messages
        {
            get { return _messages.ToString(); }
        }
    }
}
