using System;
using System.Linq;
using HtmlAgilityPack;

namespace LightStreamWeb.Helpers
{
    public static class ADAHelper
    {
        public static string PreventMultipleBRsRepeatingBlank(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return html;
            var doc = new HtmlDocument();
            try
            {
                doc.LoadHtml(html);
            }
            catch (Exception ex)
            {
                return html;
            }

            var brList = doc.DocumentNode.Descendants().Where(d => d.Name == "br").ToList();
            if (brList.Count() < 1)
                return html;

            foreach (var br in brList)
            {
                if (br.PreviousSibling == null)
                    continue;

                HtmlNode previousSibling = br.PreviousSibling;

                if (previousSibling.Name == "#text" && previousSibling.InnerText.Length < 4 && previousSibling.PreviousSibling != null)
                    previousSibling = previousSibling.PreviousSibling;

                if (previousSibling.Name == "br")
                {
                    if (!br.Attributes.Any(a => a.Name == "aria-hidden"))
                        br.Attributes.Add("aria-hidden", "true");
                }
            }

            return doc.DocumentNode.OuterHtml;
        }

        public static bool AllImagesContainAlt(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return true;
            var doc = new HtmlDocument();
            try
            {
                doc.LoadHtml(html);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            var images = doc.DocumentNode.Descendants().Where(d => d.Name == "img");
            foreach (var img in images)
            {
                if (img.Attributes.Count(a => a.Name == "alt") == 0)
                    return false;
            }
            return true;
        }        
    }
}