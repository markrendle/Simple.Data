using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Simple.Azure
{
    public static class Namespaces
    {
        public static readonly XNamespace DataServices = XNamespace.Get("http://schemas.microsoft.com/ado/2007/08/dataservices");
        public static readonly XNamespace DataServicesMetadata = XNamespace.Get("http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
    }
}
