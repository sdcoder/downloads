using FirstAgain.Domain.SharedTypes.ContentManagement;
using FirstAgain.Domain.Common;
using LightStreamWeb.Models.Rates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.IO;
using LightStreamWeb.Shared.Rates;

namespace LightStreamWeb.Helpers
{
    public static class CMSContentHelper
    {
        public static MvcHtmlString ToImageTag(this WebImage image, bool isRolePresentation = false, params string[] classes)
        {
            if (image == null)
            {
                return new MvcHtmlString("");
            }
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("<img class=\"cms {0}\" ", string.Join(" ", classes));
            sb.AppendFormat("src=\"{0}\" ", VirtualPathUtility.ToAbsolute(FirstAgain.Web.UI.CMSFileHyperLink.GetCMSFileHyperLink(image.WebFileContentId)));
            sb.AppendFormat("alt=\"{0}\" ", image.Alt);
            if(isRolePresentation)
            {
                sb.Append("role=\"presentation\" ");
            }
            sb.AppendFormat("height=\"{0}\" ", image.Height);
            sb.AppendFormat("width=\"{0}\" ", image.Width);
            sb.Append(" />");

            return new MvcHtmlString(sb.ToString());
        }

        public static MvcHtmlString ToImageUrl(this WebImage image)
        {
            if (image == null)
            {
                return new MvcHtmlString("");
            }
            return new MvcHtmlString(VirtualPathUtility.ToAbsolute(FirstAgain.Web.UI.CMSFileHyperLink.GetCMSFileHyperLink(image.WebFileContentId)));
        }

        public static MvcHtmlString ToImageUrl(this BannerImage banner)
        {
            if (string.IsNullOrEmpty(banner.HttpImageUrl))
                return banner.Image.ToImageUrl();
            else
                return MvcHtmlString.Create(banner.HttpImageUrl);
        }

        public static MvcHtmlString ToFileUrl(this CollateralAsset asset)
        {
            if (asset == null)
            {
                return new MvcHtmlString("");
            }
            return new MvcHtmlString(VirtualPathUtility.ToAbsolute(FirstAgain.Web.UI.CMSFileHyperLink.GetCMSFileHyperLink(asset.WebFileContentId)));
        }

        public static IEnumerable<CustomerComment> GetLightstreamCustomerComments(this ContentManager cms)
        {
            CustomerComments comments = new LightStreamWeb.ContentManager().Get<CustomerComments>();
            if (comments == null)
            {
                return new List<CustomerComment>();
            }

            return comments.Comments.Where(c => c.TimeStamp > new DateTime(2013, 1, 1));
        }

        public static IEnumerable<MediaCoverage> GetLightstreamMediaCoverage(this ContentManager cms)
        {
            MediaCoverages mediaCoverages = new LightStreamWeb.ContentManager().Get<MediaCoverages>();
            if (mediaCoverages == null)
            {
                return new List<MediaCoverage>();
            }

            return mediaCoverages.MediaCoveragesCollection.Where(c => c.TimeStamp > new DateTime(2013, 1, 1));
        }

        public static MvcHtmlString FormatRateWithDataAttributes(this HtmlHelper helper, DisplayRateModel displayRate, bool useSupForAsterisk = false)
        {
            // rate displayed with data attributes, for Javascript population of other CMS content
            string asteriskElement = useSupForAsterisk ? "sup" : "span";
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0:0.00##%}", displayRate.MinRate);
            sb.AppendFormat("<");
            sb.AppendFormat(asteriskElement);
            sb.AppendFormat(" id='DisplayRate' ");
            sb.AppendFormat(" data-min-amount='{0}'", displayRate.MinAmount.ToString("$##,000"));
            sb.AppendFormat(" data-max-amount='{0}'", displayRate.MaxAmount.ToString("$##,000"));
            sb.AppendFormat(" data-min-rate='{0}'", displayRate.MinRate.ToString("0.00##%"));
            sb.AppendFormat(" data-max-rate='{0}'", displayRate.MaxRate.ToString("0.00##%"));
            sb.AppendFormat(" data-min-term='{0}'", displayRate.MinTerm);
            sb.AppendFormat(" data-max-term='{0}'", displayRate.MaxTerm);
            sb.AppendFormat(" data-invoice-penalty='{0}'>", displayRate.InvoicePenalty.ToString("0.00##%"));
            sb.AppendFormat("*</");
            sb.AppendFormat(asteriskElement);
            sb.AppendFormat(">");
            return new MvcHtmlString(sb.ToString());
        }

        public static MvcHtmlString DisplayBullet(this HtmlHelper helper, FeatureBullet featureBullet)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<div class=\"small-6 medium-3 columns cms\" >");

            if (featureBullet.Graphic == null) return new MvcHtmlString(string.Empty);

            // customer testimonials always gets the same modal
            if (featureBullet.Graphic.IconName == "CustomerTestimonials")
            {
                featureBullet.IsModal = true;
                featureBullet.HREF = "/modals/customer-testimonials";
            }

            if (featureBullet.Graphic.IconName == "Clock")
            {
                featureBullet.AltText = GetClockModalAltText();
            }

            if (!string.IsNullOrEmpty(featureBullet.VideoHREF) && featureBullet.IsVideo)
            {
                DisplayVideoModal(featureBullet, sb);

                sb.Append(Environment.NewLine);
            }
            else
            {
                if (!string.IsNullOrEmpty(featureBullet.HREF))
                {
                    if (featureBullet.IsModal)
                    {
                        if (featureBullet.HREF.EndsWith("CustomerExperienceGuarantee"))
                        {
                            sb.AppendFormat("<a href=\"{0}\"  data-open=\"AjaxModalLarge\" data-reveal-ajax=\"true\">", featureBullet.HREF);
                        }
                        else
                        {
                            sb.AppendFormat("<a href=\"{0}\"  data-open=\"AjaxModal\" data-reveal-ajax=\"true\">", featureBullet.HREF);
                        }
                    }
                    else
                    {
                        sb.AppendFormat("<a href=\"{0}\" {1}>", featureBullet.HREF, (featureBullet.HREF.StartsWith("http") ? " data-jump=\"true\" " : ""));
                    }

                    sb.Append(Environment.NewLine);
                }
                else if (featureBullet.IsModal && !string.IsNullOrEmpty(featureBullet.AltText))
                {
                    if (!featureBullet.AltText.EndsWith("."))
                    {
                        featureBullet.AltText += ".";
                    }
                    string id = Path.GetFileNameWithoutExtension(featureBullet.Graphic.DefaultGraphic?.FileName) + "Modal";

                    sb.AppendFormat("<div id='{0}' data-dropdown data-position=\"auto\" data-alignment=\"center\" class=\"f-dropdown dropdown-pane\"><p tabindex=\"0\">{1}</p></div>", id, featureBullet.AltText);
                    sb.AppendFormat("<a href=\"#\" onclick=\"return false;\" data-toggle=\"{0}\" >", id);
                }
            }

            if (featureBullet.Graphic.IE8Graphic == null)
                sb.Append(ToImageTag(featureBullet.Graphic.DefaultGraphic).ToHtmlString());
            else
            {
                sb.Append(ToImageTag(featureBullet.Graphic.DefaultGraphic, false, "hideie8").ToHtmlString());
                sb.Append(ToImageTag(featureBullet.Graphic.IE8Graphic, false, "showie8").ToHtmlString());
            }

            // oopsie
            featureBullet.Text = featureBullet.Text?.Replace("....", "...");
            sb.AppendLine(string.Format("<p>{0}</p>", featureBullet.Text));

            if (featureBullet.IsVideo && !string.IsNullOrEmpty(featureBullet.VideoHREF))
            {
                sb.AppendLine("<img src=\"/content/images/video_icon.svg\" class=\"cms video_icon hideie8\" width=\"30\" height=\"30\" alt=\"\">");
                sb.AppendLine("<img src=\"/content/images/video_icon.jpg\" class=\"cms video_icon showie8\" width=\"30\" height=\"30\" alt=\"\">");
            }

            if (!string.IsNullOrEmpty(featureBullet.HREF) || (featureBullet.IsModal && !string.IsNullOrEmpty(featureBullet.AltText)) || (featureBullet.IsVideo && !string.IsNullOrEmpty(featureBullet.VideoHREF)))
            {
                sb.AppendLine("</a>");
            }

            sb.AppendLine("</div>");

            return new MvcHtmlString(sb.ToString());
        }

        public static MvcHtmlString DisplayBulletQuad(this HtmlHelper helper, List<FeatureBullet> bullets)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < bullets.Count(); i++)
            {
                var b = bullets[i];
                var newRow = i == 0 || i % 2 == 0; // two bullets per row

                if (newRow) { sb.Append("<div class=\"row\">"); }

                sb.Append("<div class=\"small-6 columns cms\">");
                sb.Append("<div class=\"row\">");
                sb.Append("<div class=\"small-2 columns\">");

                if (b.Graphic.IE8Graphic == null)
                {
                    sb.Append(ToImageTag(b.Graphic.DefaultGraphic).ToHtmlString());
                }
                else
                {
                    sb.Append(ToImageTag(b.Graphic.DefaultGraphic, false, "hideie8").ToHtmlString());
                    sb.Append(ToImageTag(b.Graphic.IE8Graphic, false, "showie8").ToHtmlString());
                }

                sb.Append("</div>");
                sb.Append("<div class=\"small-10 columns bullet-text-container\">");
                sb.Append($"<p class=\"bullet-text\">{ b.Text }</p>");
                sb.Append($"<p class=\"bullet-sub-text\">{ b.SubText }</p>");
                sb.Append("</div></div></div>");

                if (!newRow) { sb.Append("</div>"); }
            }

            return new MvcHtmlString(sb.ToString());
        }

        public static MvcHtmlString DisplayBulletStacked(this HtmlHelper helper, List<FeatureBullet> bullets)
        {
            StringBuilder sb = new StringBuilder();

            bullets.ForEach(b =>
            {
                sb.Append("<div class=\"feature-container cms\">");
                sb.Append("<div class=\"row\">");
                sb.Append("<div class=\"small-12 columns\">");
                if (b.Graphic.IE8Graphic == null)
                {
                    sb.Append(ToImageTag(b.Graphic.DefaultGraphic).ToHtmlString());
                }
                else
                {
                    sb.Append(ToImageTag(b.Graphic.DefaultGraphic, false, "hideie8").ToHtmlString());
                    sb.Append(ToImageTag(b.Graphic.IE8Graphic, false, "showie8").ToHtmlString());
                }
                sb.Append($"<p class=\"bullet-text\">{ b.Text }</p>");
                sb.Append($"<p class=\"bullet-sub-text\">{ b.SubText }</p>");
                sb.Append("</div>");
                sb.Append("</div>");
                sb.Append("</div>");
            });

            return new MvcHtmlString(sb.ToString());
        }

        private static void DisplayVideoModal(FeatureBullet b, StringBuilder sb)
        {
            if (b.IsVideo)
            {
                string id = Path.GetFileNameWithoutExtension(b.Graphic.DefaultGraphic?.FileName) + "Modal";

                sb.AppendFormat("<!-- Reveal Modals begin -->");
                sb.AppendFormat("<div id=\"{0}\" class=\"reveal large\" data-reveal aria-labelledby=\"videoModalTitle\" aria-hidden=\"true\" role=\"dialog\" data-video=\"true\">", id);
                sb.AppendFormat("	<div class=\"flex-video vimeo\">");
                sb.AppendFormat("		<iframe id=\"lightstreamVideo\" src=\"{0}\" width=\"500\" height=\"281\" frameborder=\"0\"></iframe>", b.VideoHREF);
                sb.AppendFormat("	</div>");
                sb.AppendFormat("	<a class=\"close-reveal-modal\" aria-label=\"Close\">&#215;</a>");
                sb.AppendFormat("</div>");
                sb.AppendFormat("<!-- Reveal Modals end -->");

                sb.AppendFormat("<!-- Vimeo Video Modal -->");
                sb.AppendFormat("<!-- Triggers the modals -->");
                sb.AppendFormat("<a href=\"#\" data-open=\"{0}\" class=\"ls-pull-left\">", id);
            }
        }

        public static MvcHtmlString DisplayAccordion(this HtmlHelper helper, List<FeatureBullet> bullets)
        {
            if (bullets == null || !bullets.Any())
                return new MvcHtmlString(String.Empty);


            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<ul class=\"accordion\" data-accordion data-allow-all-closed=\"true\" data-multi-expand=\"false\" data-slide-speed=\"0\">");
            int bulletCounter = 1;

            foreach (var b in bullets)
            {
                string panelName = String.Format("panel{0}", bulletCounter++);

                sb.AppendLine("<li class=\"accordion-item\" data-accordion-item>");

                if (!b.CanDisplayVideo())
                {
                    sb.Append("<a href=\"#\" class=\"accordion-title ls-pull-left\">");
                }
                else
                {
                    DisplayVideoModal(b, sb);
                }

                DisplayAccordianIcon(sb, b);

                sb.AppendLine("<span class=\"small-9 ls-pull-left title\">");
                sb.AppendLine(b.Text);
                sb.AppendLine("</span>");

                if (!b.CanDisplayVideo())
                {
                    sb.AppendLine("<div class=\"ls-expander ls-plus ls-pull-right\"></div>");
                    sb.AppendLine("<div class=\"ls-expander ls-minus ls-pull-right\"></div>");
                }
                sb.AppendFormat("</a>");

                if (!string.IsNullOrEmpty(b.HREF))
                    sb.Append($"<div id=\"{panelName}\" class=\"accordion-content\" data-tab-content data-load-href=\"{b.HREF}\" data-load-href-target=\"{panelName}\">");
                else
                    sb.Append("<div class=\"accordion-content\" data-tab-content>");

                if (b.Graphic != null && b.Graphic.IconName == "Clock")
                    b.AltText = GetClockModalAltText();

                if (b.CanDisplayVideo())
                {
                    sb.AppendLine("<img src=\"/content/images/video_icon.svg\" class=\"cms video_icon hideie8\" alt=\"\">");
                    sb.AppendFormat("	<div class=\"flex-video vimeo\">");
                    sb.AppendFormat("		<iframe id=\"lightstreamVideo\" src=\"{0}\" width=\"500\" height=\"281\" frameborder=\"0\"></iframe>", b.VideoHREF);
                    sb.AppendFormat("	</div>");
                    sb.AppendFormat("	<a class=\"close-reveal-modal\" aria-label=\"Close\">&#215;</a>");
                }

                sb.Append(Environment.NewLine);

                if (!String.IsNullOrEmpty(b.AltText) && !String.IsNullOrWhiteSpace(b.AltText))
                {
                    if (b.AltText.Contains("<div"))
                        sb.AppendLine(b.AltText);
                    else
                    {
                        sb.AppendLine("<p>");
                        sb.AppendLine(b.AltText);
                        sb.AppendLine("</p>");
                    }
                }

                sb.AppendLine("</div>");
                sb.AppendLine("</li>");
            }

            sb.AppendLine("</ul>");

            return new MvcHtmlString(sb.ToString());
        }

        private static void DisplayAccordianIcon(StringBuilder sb, FeatureBullet b)
        {
            if (b.CanDisplayVideo())
            {
                sb.AppendLine("<img src=\"/content/images/video_icon.svg\" class=\"ls-pull-left cms hideie8\" alt=\"\">");
            }
            else if (b.Graphic != null)
            {
                if (b.Graphic.IE8Graphic == null)
                    sb.Append(ToImageTag(b.Graphic.DefaultGraphic, false, "ls-pull-left").ToHtmlString());
                else
                {
                    sb.Append(ToImageTag(b.Graphic.DefaultGraphic,false, "hideie8", "ls-pull-left").ToHtmlString());
                    sb.Append(ToImageTag(b.Graphic.IE8Graphic, false, "showie8", "ls-pull-left").ToHtmlString());
                }
            }
        }

        private static string GetClockModalAltText()
        {
            var customerSvcHours = BusinessConstants.Instance.BusinessHours.GetFormattedHours(FirstAgain.Common.TimeZoneUS.EasternStandardTime, "-", "to", ",", true);
            var appProcessingHours = BusinessConstants.Instance.BusinessHoursAppProcessing.GetFormattedHours(FirstAgain.Common.TimeZoneUS.EasternStandardTime, "-", "to", ",", true);

            return "Business hours<br />" +
                    "<b>Account Services</b><br />" +
                    AddBreakAfterTo(RemoveHoursFromTime(customerSvcHours[0])) +
                    " &<br />" +
                    RemoveHoursFromTime(customerSvcHours[1]) +
                    "<br /><b>Application Processing</b><br />" +
                    AddBreakAfterTo(RemoveHoursFromTime(appProcessingHours[0])) +
                    " &<br />" +
                    RemoveHoursFromTime(appProcessingHours[1]);
        }

        private static string RemoveHoursFromTime(string timeString)
        {
            return timeString.Replace(":00", string.Empty);
        }

        private static string AddBreakAfterTo(string timeString)
        {
            return timeString.Insert(timeString.IndexOf("to") + 2, "<br >");
        }
    }
}