using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.ServiceModel.Syndication;

namespace Simple.NExtLib.Xml.Syndication
{
    public static class SyndicationItemExtensions
    {
        public static XElement ContentAsXElement(this SyndicationItem item)
        {
            var content = item.Content as XmlSyndicationContent;

            if (content == null) return null;

            return XElement.ReadFrom(content.GetReaderAtContent()) as XElement;
        }

        public static IEnumerable<XElement> ContentsAsXElements(this SyndicationFeed feed)
        {
            XElement content;

            foreach (var item in feed.Items)
            {
                if ((content = item.ContentAsXElement()) != null)
                {
                    yield return content;
                }
            }
        }
    }
}
