using FirstAgain.Domain.SharedTypes.ContentManagement;
using FirstAgain.Domain.SharedTypes.ContentManagement.SiteContent.AboutUs;
using System.Collections.Generic;
using System.Linq;
using FirstAgain.Common.Extensions;
using FirstAgain.Domain.SharedTypes.ContentManagement.SiteContent.Sitewide;
using LightStreamWeb.Helpers;
using LightStreamWeb.Models.Middleware;
using System.Configuration;
using FirstAgain.Domain.Common; 

namespace LightStreamWeb.Models.PublicSite
{
    public class AboutPagesModel : BasePublicPageWithSections
    {
        private LightStreamPageDefault defaults;

        private readonly AboutUsHomePage _cmsContent;
        private readonly WhoWeAre _whoWeAreContent;
        private readonly TheAnythingLoan _anythingLoanContent;
        private readonly RateMatchPage _rateMatchContent;
        private readonly CustomerExperienceGuarantee _customerExperienceGuaranteeContent;
        private readonly AffiliateProgram _affiliateProgramContent;
        private readonly BusinessPartners _businessPartnersContent;
        private readonly CustomerComments _customerCommentContent;
        private readonly MergerInformation _mergerInfoProgramContent;

        public AboutPagesModel() : this(new ContentManager(), AppSettings.Load().PageDefault.About)
        {
        }

        public AboutPagesModel(ContentManager content, LightStreamPageDefault defaults) : base(content)
        {
            this.defaults = defaults;
            BodyClass = defaults.BodyClass;
            Heading = defaults.Heading;
            SubHeading = defaults.SubHeading;
            IntroParagraph = "";
            Title = "LightStream Loans- About Us";
            NgApp = "LightStreamApp";

            _cmsContent = ContentManager.Get<AboutUsHomePage>();
            _whoWeAreContent = ContentManager.Get<WhoWeAre>();
            _anythingLoanContent = ContentManager.Get<TheAnythingLoan>();
            _rateMatchContent = ContentManager.Get<RateMatchPage>(); 
            _customerExperienceGuaranteeContent= ContentManager.Get<CustomerExperienceGuarantee>(); 
            _affiliateProgramContent= ContentManager.Get<AffiliateProgram>(); 
            _businessPartnersContent = ContentManager.Get<BusinessPartners>();
            _customerCommentContent = ContentManager.Get<CustomerComments>();
            _mergerInfoProgramContent = ContentManager.Get<MergerInformation>(); 

            InitializeFromCMS();
        }

     

        protected virtual void InitializeFromCMS()
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
                case "who-we-are":
                case "meet-lightstream":
                    sectionCmsContent = _whoWeAreContent;
                    break;
                case "the-anything-loan":
                    sectionCmsContent = _anythingLoanContent;
                    break;
                case "rate-match":
                    sectionCmsContent = _rateMatchContent;
                    break;
                case "customer-experience-guarantee":
                    sectionCmsContent = _customerExperienceGuaranteeContent;
                    break;
                case "customer-testimonials":
                    sectionCmsContent = _customerCommentContent;
                    break;
                case "business-partners":
                    sectionCmsContent = _businessPartnersContent;
                    break;
                case "affiliate-program":
                    sectionCmsContent = _affiliateProgramContent;
                    break;
                case "merger-info":
                    sectionCmsContent = _mergerInfoProgramContent;
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
                MetaDescription = _cmsContent.MetaTagContent.MetaTagDescription;
                MetaKeywords = _cmsContent.MetaTagContent.MetaTagKeywords;
                Title = _cmsContent.MetaTagContent.PageTitle;

                // setup omniture properties
                OmniHierarchy = _cmsContent.Name;
                if (_cmsContent.AdobeAnalyticsTrackingName != null)
                {
                    CustomOmniHierarchy = (ConfigurationManager.AppSettings["OmniAboutPagePrefix"] ?? "About-Page-Prefix-key-missing-from-web.config") + $"|{(_cmsContent.AdobeAnalyticsTrackingName ?? "").ToScrubbed()}";
                }
            }
            
            if (_whoWeAreContent != null)
            {
                Banner = _whoWeAreContent.Banner ?? new BannerImage
                {
                    Image = new WebImage {Alt = defaults.BannerAlt},
                    HttpImageUrl = defaults.Banner
                };
                OmniHierarchy = _whoWeAreContent.Name;
            }
        }


        public override IEnumerable<AccordianSection> GetSections()
        {
            var sections = new List<AccordianSection>();
            
            if (BusinessConstants.Instance.IsNewCompanyDate())
            {
                sections.AddRange(new AccordianSection[] {
                   new AccordianSection
                    {
                        Title = "LightStream A Division Of Truist",
                        HREF = "merger-info"
                    }
               });
            }

            sections.AddRange(new AccordianSection[] {
                    new AccordianSection
                    {
                        Title = _whoWeAreContent.PageTitle,
                        HREF = "meet-lightstream"
                    }
            });

            if (ContentManager.Get<TheAnythingLoan>() != null) 
            {
                sections.Add(new AccordianSection
                {
                    Title = _anythingLoanContent.PageTitle, 
                    HREF = "the-anything-loan"
                });
            }
            if (AppSettingsHelper.IsRatchMatchOfferEnabled())
            {
                
                if (_rateMatchContent != null)
                {
                    sections.Add(new AccordianSection
                    {
                        Title = _rateMatchContent.PageTitle,
                        HREF = "rate-match"
                    });
                }
            }
            
            sections.AddRange(new AccordianSection[] {
                new AccordianSection
                {
                    Title = _customerExperienceGuaranteeContent.PageTitle,
                    HREF = "customer-experience-guarantee"
                },
                new AccordianSection
                {
                    Title = "We Plant A Tree With Every Loan",
                    HREF = "american-forest-event"
                },
                new AccordianSection
                {
                    Title = "What Our Customers Say", 
                    HREF = "customer-testimonials"
                },
                new AccordianSection
                {
                    Title = _affiliateProgramContent.PageTitle, 
                    HREF = "affiliate-program"
                },
                new AccordianSection
                {
                    Title = _businessPartnersContent.PageTitle, 
                    HREF = "business-partners"
                },
                new AccordianSection
                {
                    Title = "Media Room", 
                    HREF = "pr-media"
                }
            });


            // CMS migration - if there is no Intro text, then open the first section
            if (!SingleTabMode && string.IsNullOrWhiteSpace(IntroParagraph) && !sections.Any(s => s.HREF == OpenTab))
            {
                OpenTab = sections.First().HREF;
            }
            if (sections.Any(s => s.HREF == OpenTab))
            {
                sections.First(s => s.HREF == OpenTab).Selected = true;

                if (SingleTabMode)
                {
                    var query = sections.Where(s => s.HREF == OpenTab);
                    if (query.Any())
                    {
                        Title = query.First().Title;
                        return query;
                    }
                }
            }

            return sections;
        }
        #region Site Catalyst / Omniture
        private string omniPageName;
        public string OmniPageName
        {
            get
            {
                return OmniHierarchy + "|AboutUs";
            }
            set { omniPageName = value; }
        }

        private string omniHierarchy;
        public string OmniHierarchy
        {
            get
            {
                var prefix = ConfigurationManager.AppSettings["OmniAboutPagePrefix"];
                return prefix + "|" + omniHierarchy.ToScrubbed();
            }
            set { omniHierarchy = value; }
        }

        private string customOmniHierarchy;
        public string CustomOmniHierarchy
        {
            get { return customOmniHierarchy; }
            set
            {
                customOmniHierarchy = value;
            }
        }


        public bool IsRedirectTestBlankPageEnabled { get; private set; }

        #endregion
    }

}