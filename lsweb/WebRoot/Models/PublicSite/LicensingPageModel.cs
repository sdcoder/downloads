using FirstAgain.Domain.SharedTypes.ContentManagement;
using LightStreamWeb.Models.Middleware;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.PublicSite
{
    public class LicensingPageModel : AboutPagesModel
    {
        private LightStreamPageDefault defaults;
        public LicensingPageModel() : this(new ContentManager(), AppSettings.Load().PageDefault.About)
        { }

        public LicensingPageModel(ContentManager content, LightStreamPageDefault defaults) : base(content, defaults)
        {
            this.defaults = defaults;
            SingleTabMode = true;
            AppendBodyClass("single-tab");
            OpenTab = "affiliate-program";
            MetaDescription = "Licensing information for LightStream loans for auto, home improvement and practically anything else, at low rates for those with good credit.";

            InitializeFromCMS();
        }

        public string AffiliateProgramCopy { get; private set; }

        protected new void InitializeFromCMS()
        {
            var cmsContent = ContentManager.Get<Licensing>();
            if (cmsContent != null)
            {
                Title = cmsContent.PageTitle;
                SubHeading = cmsContent.SubHeading;
                AffiliateProgramCopy = cmsContent.LicensingCopy;
                Banner = cmsContent.Banner ?? new BannerImage
                {
                    Image = new WebImage { Alt = defaults.BannerAlt },
                    HttpImageUrl = defaults.Banner
                };
                // setup omniture properties
                OmniHierarchy = cmsContent.Name;
                if (cmsContent.AdobeAnalyticsTrackingName != null)
                {
                    CustomOmniHierarchy = (ConfigurationManager.AppSettings["OmniAboutPagePrefix"] ?? "About-Page-Prefix-key-missing-from-web.config") + $"|{(cmsContent.AdobeAnalyticsTrackingName ?? "").ToScrubbed()}";
                }
            }
        }
    }
}