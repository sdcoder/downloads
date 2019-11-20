using FirstAgain.Domain.SharedTypes.ContentManagement;
using LightStreamWeb.Models.Middleware;
using System.Collections.Generic;
using LightStreamWeb.Helpers;
using System.Linq;

namespace LightStreamWeb.Models.PublicSite
{
    public class PrivacySecurityModel : BasePublicPageWithSections
    {
        private readonly LightStreamPageDefault _defaults;
        private readonly PrivacySecurityHomePage _cmsContent;
        private readonly PrivacyPage _privacyPageContent;
        private readonly PrivacyPolicy _privacyPolicyContent;
        private readonly OnlinePrivacyPractices _onlinePrivacyPracticesContent;
        private readonly SecurityPolicy _securityPolicyContent;

        public PrivacySecurityModel() : this(new ContentManager(), AppSettings.Load().PageDefault.PrivacySecurity)
        { }

        public PrivacySecurityModel(ContentManager content, LightStreamPageDefault defaults) : base(content)
        {
            _defaults = defaults;
            BodyClass = defaults.BodyClass;
            Heading = defaults.Heading;
            SubHeading = defaults.SubHeading;
            IntroParagraph = "";

            _cmsContent = ContentManager.Get<PrivacySecurityHomePage>();
            _privacyPageContent = ContentManager.Get<PrivacyPage>();
            _privacyPolicyContent = ContentManager.Get<PrivacyPolicy>();
            _onlinePrivacyPracticesContent = ContentManager.Get<OnlinePrivacyPractices>();
            _securityPolicyContent = ContentManager.Get<SecurityPolicy>();


            InitializeFromCMS();
        }

        private void InitializeFromCMS()
        {
            SetHomePageCmsContent();
            SetSectionMetadataContent();
        }

        private void SetSectionMetadataContent()
        {
            WebPageContentBase sectionCmsContent;
            var selectedSecion = _httpRequest.UrlRequested.Split('/').Last();

            switch (selectedSecion)
            {
                case "privacy-security":
                    sectionCmsContent = _privacyPageContent;
                    break;
                case "privacy-policy":
                    sectionCmsContent = _privacyPolicyContent;
                    break;
                case "online-privacy":
                    sectionCmsContent = _onlinePrivacyPracticesContent;
                    break;
                case "security-policy":
                    sectionCmsContent = _securityPolicyContent;
                    break;
                default:
                    sectionCmsContent = _cmsContent;
                    break;
            }

            SetMetadataContent(sectionCmsContent, Banner.ToImageUrl().ToString());
        }

        private void SetHomePageCmsContent()
        {

            if (_cmsContent != null)
            {
                Heading = _cmsContent.Heading;
                SubHeading = _cmsContent.SubHeading;
                IntroParagraph = _cmsContent.IntroParagraphCopy;
                SetMetadataContent(_cmsContent);
            }

            Banner = _cmsContent?.Banner ?? new BannerImage
            {
                Image = new WebImage { Alt = _defaults.BannerAlt },
                HttpImageUrl = _defaults.Banner
            };
        }

        public override IEnumerable<AccordianSection> GetSections()
        {
            var sections = new List<AccordianSection>() {
                new AccordianSection() {
                    Title = _privacyPageContent.Name,
                    HREF = "privacy-security"
                },
                new AccordianSection() {
                    Title = _privacyPolicyContent.Name,
                    HREF = "privacy-policy"
                },
                new AccordianSection() {
                    Title = _onlinePrivacyPracticesContent.Name,
                    HREF = "online-privacy"
                },
                new AccordianSection() {
                    Title = _securityPolicyContent.Name,
                    HREF = "security-policy"
                }
            };

            // CMS migration - if there is no Intro text, then open the first section
            if (!SingleTabMode && !string.IsNullOrWhiteSpace(IntroParagraph) && OpenTab == "privacy-security")
            {
                OpenTab = string.Empty;
            }

            if (sections.Any(s => s.HREF == OpenTab))
            {
                sections.First(s => s.HREF == OpenTab).Selected = true;
            }

            return sections;
        }
    }
}