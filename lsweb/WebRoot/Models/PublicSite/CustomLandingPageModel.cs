using System.Configuration;
using FirstAgain.Domain.SharedTypes.ContentManagement;
using LightStreamWeb.Helpers.Exceptions;

namespace LightStreamWeb.Models.PublicSite
{
    public class CustomLandingPageModel : BaseLightstreamPageModel
    {
        private readonly CustomContent _pageContent;

        public CustomLandingPageModel()
        {
            _pageContent = new ContentManager().Get<CustomContent>();

            if (_pageContent == null)
            {
                throw new RedirectException("~/");
            }

            Title = _pageContent.PageTitle;

            if (_pageContent.MetaTagContent != null)
            {
                MetaDescription = _pageContent.MetaTagContent.MetaTagDescription;
                MetaKeywords = _pageContent.MetaTagContent.MetaTagKeywords;
                Title = _pageContent.MetaTagContent.PageTitle;
            }
            
            IsRedirectTestBlankPageEnabled = _pageContent.EnableRedirectTestBlankPage;

            // setup omniture properties
            OmniHierarchy = _pageContent.Name;

            BodyClass = "";
        }

        public CustomContent Content
        {
            get
            {
                return _pageContent;
            }
        }

        #region Site Catalyst / Omniture
        private string omniPageName;
        public string OmniPageName
        {
            get
            {
                return OmniHierarchy + "|MainPage";
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

        public bool IsRedirectTestBlankPageEnabled { get; private set; }

        #endregion
    }
}