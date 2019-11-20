using FirstAgain.Common.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;

namespace LightStreamWeb.Helpers
{
    public class ENoticeHtmlHelper
    {
        internal static string FixUpDeclineNoticeHtml(string html)
        {
            if (html == null) return null;
            //fix special characters not accepted by xml
            html = html.Replace("&", "&amp;");

            using (XmlTextReader reader = new XmlTextReader(new StringReader(html)))
            {
                reader.Namespaces = false;
                reader.DtdProcessing = DtdProcessing.Ignore;
                reader.ReadToFollowing("body");

                XmlDocument doc = new XmlDocument();
                doc.XmlResolver = null;
                doc.LoadXml(reader.ReadOuterXml());
                XmlElement table = (XmlElement)doc.SelectSingleNode("/body/table");
                if (table != null)
                {
                    CssStyleAttribute style = new CssStyleAttribute(table.GetAttribute("style"));
                    style.RemoveStyle("width");
                    table.SetAttribute("style", style.ToString());
                }
                //return back corrected xml content
                return doc.DocumentElement.InnerXml.Replace("&amp;","&");
            }
        }

        internal static string FixUpCreditDisclosureNotice(string html)
        {
            if (html == null) return null;

            using (XmlTextReader reader = new XmlTextReader(new StringReader(html)))
            {
                reader.Namespaces = false;
                reader.DtdProcessing = DtdProcessing.Ignore;
                reader.ReadToFollowing("body");

                XmlDocument doc = new XmlDocument();
                doc.XmlResolver = null;
                doc.LoadXml(reader.ReadOuterXml());
                return doc.DocumentElement.InnerXml;
            }
        }



    }
}