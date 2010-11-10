using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Net
{
    internal static class WebRequestExtensions
    {
        internal static void SetContent(this WebRequest request, string content)
        {
            using (var writer = new System.IO.StreamWriter(request.GetRequestStream()))
            {
                writer.Write(content);
            }
        }
    }
}
