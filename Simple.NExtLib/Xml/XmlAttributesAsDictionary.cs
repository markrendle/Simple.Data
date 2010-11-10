using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Simple.NExtLib.Xml
{
    public class XmlAttributesAsDictionary
    {
        private readonly XElement _element;

        public XmlAttributesAsDictionary(XElement element)
        {
            _element = element;
        }

        public string this[string name]
        {
            get
            {
                var attr = _element.Attribute(_element.ResolveName(name)) ?? _element.Attribute(name);
                if (attr == null) return null;
                return attr.Value;
            }

            set
            {
                var xname = _element.ResolveName(name);
                var attr = _element.Attribute(xname);

                if (attr == null)
                {
                    _element.Add(new XAttribute(xname, value));
                }
                else
                {
                    attr.Value = value;
                }
            }
        }
    }
}
