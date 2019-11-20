using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.SharedTypes.ContentManagement;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.PublicSite
{
    public class FAQLandingPageModel : BasePublicPageWithAdObjects
    {
        public FAQLandingPageModel()
        {
            BodyClass = "questions sub";
        }

        public List<FAQ> FAQs { get; protected set; }
        public List<LandingPageLink> VerticalLinks { get; protected set; }
        public string PageHeader { get; protected set; }

        private string omniPageName;
        public string OmniPageName
        {
            get
            {
                return OmniHierarchy + "|FAQ";
            }
            set { omniPageName = value; }
        }

        private string omniHierarchy;
        public string OmniHierarchy
        {
            get
            {
                var prefix = ConfigurationManager.AppSettings["OmniLandingPagePrefix"];
                return prefix + "|" + omniHierarchy.ToScrubbed();
            }
            set { omniHierarchy = value; }
        }

        private string _landingPageUrl;
        public string LandingPageUrl
        {
            get
            {
                return _landingPageUrl;
            }
            set
            {
                _landingPageUrl = value;
                LoadLandingPageContent(_landingPageUrl);
            }
        }

        public static string GetQueryParameterValue(int webContentId)
        {
            WebPageContentMap contentMap = DomainServiceContentManagementOperations.GetCachedPublishedWebContent();
            return (contentMap[webContentId] as LandingPageContent).QueryParameterValue;
        }

        private void LoadLandingPageContent(string urlRewritePath)
        {
            Title = "FAQ";

            var cms = new LightStreamWeb.ContentManager();
            VerticalLinks = cms.Get<LandingPageLinks>().FooterLinks;
            var content = cms.GetLandingPageByQueryParameterValue(urlRewritePath);

            // setup omniture properties
            OmniHierarchy = content.Name;

            PageHeader = content.LandingPageFAQs.PageHeader;

            var pageTitle = content.LandingPageFAQs.MetaTagContent.PageTitle;
            if (!string.IsNullOrWhiteSpace(pageTitle))
            {
                Title = pageTitle;
            }

            var metaDescription = content.LandingPageFAQs.MetaTagContent.MetaTagDescription;
            if (!string.IsNullOrWhiteSpace(metaDescription))
            {
                MetaDescription = metaDescription;
            }

            var metaKeywords = content.LandingPageFAQs.MetaTagContent.MetaTagKeywords;
            if (!string.IsNullOrWhiteSpace(metaKeywords))
            {
                MetaKeywords = metaKeywords;
            }

            FAQs = content.LandingPageFAQs.FAQs;

            //uxPageHeader.InnerHtml = content.LandingPageFAQs.PageHeader;

            //if (content.LandingPageFAQs.MetaTagContent.PageTitle != String.Empty)
            //{
            //    ((Assets_MasterPages_SubPage)Master).Page.Title = content.LandingPageFAQs.MetaTagContent.PageTitle;
            //}

            //uxSideNavHeader.Text = links.SideNavText;

            Canonical = ROOT_CANONICAL + "faq/" + content.QueryParameterValue;
        }

    }
}