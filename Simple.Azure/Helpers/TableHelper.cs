using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Simple.NExtLib.IO;

namespace Simple.Azure.Helpers
{
    internal static class TableHelper
    {
        /// <summary>
        /// Reads the table list from a Stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        internal static IEnumerable<string> ReadTableList(Stream stream)
        {
            var element = XElement.Parse(QuickIO.StreamToString(stream));

            return
                element.Elements(null, "entry").Select(
                    e => e.Element(null, "content").Element("m", "properties").Element("d", "TableName").Value);
        }
    }
}
