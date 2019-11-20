using FirstAgain.Common.Wcf;
using FirstAgain.Domain.SharedTypes.ContentManagement;
using FirstAgain.Web.Cookie;
using LightStreamWeb.Helpers;
using LightStreamWeb.Helpers.Exceptions;
using LightStreamWeb.Models.Components;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using FirstAgain.Common.Extensions;
using FirstAgain.Domain.ServiceModel.Client;
using System.Text.RegularExpressions;

namespace LightStreamWeb.Models.PublicSite
{
    public class LandingPageModel : PublicSiteWithBannerModel
    {
        private readonly ICookieUtility _cookieUtility;
        private readonly IFactUtility _factUtility;

        public LandingPageContent _content = null;
        private string _urlRewritePath = null;
        private const string DEFAULT_VIDEO_URL = "https://player.vimeo.com/video/190778568";

        public List<FeatureBullet> FeatureBullets { get; private set; }
        public FeatureBulletSectionCustomizations FeatureBulletSectionCustomizations { get; private set; }

        public List<ComponentUrlSetting> ComponentUrlSettings = new List<ComponentUrlSetting>();

        public LandingPageModel(ICookieUtility cookieUtility, IFactUtility factUtility)
        {
            _cookieUtility = cookieUtility;
            _factUtility = factUtility;
        }

        internal string GetBannerUrl()
        {
            if (Banner != null)
            {
                return Banner.ToImageUrl().ToString();
            }
            else if (_content.CenterGraphic != null)
            {
                return _content.CenterGraphic.ToImageUrl().ToString();
            }
            return "";
        }

        public LandingPageModel(string urlRewritePath)
            : this(new CookieUtilityWrapper(), new FactUtility())
        {
            NgApp = "LightStreamApp";
            _urlRewritePath = urlRewritePath;
            _content = new ContentManager().Get<LandingPageContent>();

            if (TemplateName == "AffordableHomeImprovementProgram")
            {
                Content.PurposeOfLoan = FirstAgain.Domain.Lookups.FirstLook.PurposeOfLoanLookup.PurposeOfLoan.CommunityReinvestmentAct;
            }
            PurposeOfLoan = DisplayRate.PurposeOfLoan = Content.PurposeOfLoan;

            if (_content == null)
            {
                throw new RedirectException("~/");
            }

            if (_content != null && _content.MetaTagContent != null)
            {
                HeadTitle = _content.MetaTagContent.PageTitle;
                MetaDescription = _content.MetaTagContent.MetaTagDescription;
                MetaKeywords = _content.MetaTagContent.MetaTagKeywords;
                MetaImage = GetBannerUrl();
                MetaCanonical = _content.MetaTagContent.MetaTagCanonical;
            }
            if (MetaCanonical != null)
            {
                Canonical = ROOT_CANONICAL + MetaCanonical;
            }
            else
            {
                Canonical = ROOT_CANONICAL + urlRewritePath;
            }

            Title = _content.PageTitle;
            if (Title.IsNullOrEmpty() && _content.MetaTagContent != null)
            {
                Title = _content.MetaTagContent.PageTitle;
            }
            IsRedirectTestBlankPageEnabled = _content.EnableRedirectTestBlankPage;

            // setup omniture properties
            OmniHierarchy = _content.Name;

            BodyClass = "rates home landing-page";

            // for legacy landing pages, add another class
            switch (_content.TemplateType)
            {
                case LandingPageContent.VerticalMarketTemplateType.PartnerOption1:
                    BodyClass += " legacy-landing-page partne-option-1-page";
                    break;
                case LandingPageContent.VerticalMarketTemplateType.PartnerOption2:
                    BodyClass += " legacy-landing-page partne-option-2-page";
                    break;
                case LandingPageContent.VerticalMarketTemplateType.PartnerOption3:
                    BodyClass += " legacy-landing-page partne-option-3-page";
                    break;
                case LandingPageContent.VerticalMarketTemplateType.VerticalMarketV1:
                    BodyClass += " legacy-landing-page vertical-market-1-page";
                    break;
                case LandingPageContent.VerticalMarketTemplateType.VerticalMarketV2:
                    BodyClass += " legacy-landing-page vertical-market-2-page";
                    break;
                case LandingPageContent.VerticalMarketTemplateType.ResponsivePartnerOption1:
                    BodyClass += " partner_light partner responsive-partner-option-1-page";
                    break;
                case LandingPageContent.VerticalMarketTemplateType.ResponsiveNoHeader:
                    BodyClass += " partner_light partner no-header";
                    break;
                case LandingPageContent.VerticalMarketTemplateType.ResponsiveRedesignV1:
                    BodyClass += " responsive-redesign-v1-page";
                    break;
                case LandingPageContent.VerticalMarketTemplateType.ResponsiveRedesignV2:
                    BodyClass = "rates sub rate-calculator responsive-redesign-v2-page";
                    break;
                case LandingPageContent.VerticalMarketTemplateType.DirectMailCampaign:
                    BodyClass += " direct-mail-campaign-page";
                    break;
                case LandingPageContent.VerticalMarketTemplateType.AffordableHomeImprovementProgram:
                    BodyClass += " ahip-page";
                    break;
                case LandingPageContent.VerticalMarketTemplateType.Video:
                    CustomOmniHierarchy = (ConfigurationManager.AppSettings["VideoPagePrefix"] ?? "Video-Page-Prefix-key-missing-from-web.config") + $"|{(_content.AdobeAnalyticsTrackingName ?? "").ToScrubbed()}";
                    BodyClass += " video-landing-page";
                    break;
                case LandingPageContent.VerticalMarketTemplateType.VideoAllPurpose:
                    CustomOmniHierarchy = (ConfigurationManager.AppSettings["VideoAllPurposePagePrefix"] ?? "Video-All-Purpose-Page-Prefix-key-missing-from-web.config") + $"|{(_content.AdobeAnalyticsTrackingName ?? "").ToScrubbed()}";
                    BodyClass += " video-all-purpose-landing-page";
                    break;
                case LandingPageContent.VerticalMarketTemplateType.PaidSearch:
                    BodyClass += " paid-search-page";
                    CustomOmniHierarchy = (ConfigurationManager.AppSettings["PaidSearchPagePrefix"] ?? "Paid-Search-Page-Prefix-key-missing-from-web.config") + $"|{(_content.AdobeAnalyticsTrackingName ?? "").ToScrubbed()}";
                    break;
                case LandingPageContent.VerticalMarketTemplateType.LoanAggregator:
                    CustomOmniHierarchy = (ConfigurationManager.AppSettings["LoanAggregatorPagePrefix"] ?? "Loan-Aggregator-Page-Prefix-key-missing-from-web.config") + $"|{(_content.AdobeAnalyticsTrackingName ?? "").ToScrubbed()}";
                    BodyClass += " loan-aggregator-page";
                    break;
                case LandingPageContent.VerticalMarketTemplateType.ComponentTemplate:
                    CustomOmniHierarchy = (ConfigurationManager.AppSettings["ComponentTemplatePrefix"] ?? "Component-Template-Prefix-key-missing-from-web.config") + $"|{(_content.AdobeAnalyticsTrackingName ?? "").ToScrubbed()}";
                    BodyClass += " loan-aggregator-page component-template-page responsive-partner-option-1-page";
                    break;
                case LandingPageContent.VerticalMarketTemplateType.MultipurposeAuto:
                    CustomOmniHierarchy = (ConfigurationManager.AppSettings["MultipurposeAutoPrefix"] ?? "MultipurposeAuto-Prefix-key-missing-from-web.config") + $"|{(_content.AdobeAnalyticsTrackingName ?? "").ToScrubbed()}";
                    BodyClass += " multipurpose-auto-page";
                    break;
                case LandingPageContent.VerticalMarketTemplateType.NavigationPurpose:
                    CustomOmniHierarchy = (ConfigurationManager.AppSettings["NavigationPurposePrefix"] ?? "NavigationPurpose-Prefix-key-missing-from-web.config") + $"|{(_content.AdobeAnalyticsTrackingName ?? "").ToScrubbed()}";
                    BodyClass += " navigation-purpose-page";
                    break;
                case LandingPageContent.VerticalMarketTemplateType.Event:
                    CustomOmniHierarchy = (ConfigurationManager.AppSettings["EventPagePrefix"] ?? "ForestPage-Prefix-key-missing-from-web.config") + $"|{(_content.AdobeAnalyticsTrackingName ?? "").ToScrubbed()}";
                    BodyClass += " event-page";
                    break;
            }

            FeatureBullets = _content.FeatureBullets;
            FeatureBulletSectionCustomizations = _content.FeatureBulletSectionCustomizations;
            _content.MobileCenterGraphic = _content.MobileCenterGraphic ?? _content.CenterGraphic;
        }

        public string BenefitLine
        {
            get
            {
                string text = "A hassle-free loan that rewards you for good credit.";

                if (!string.IsNullOrEmpty(Content.BulletHeader))
                {
                    text = Content.BulletHeader.Replace("<br>", "");
                    text = text.Replace("<br/>", "");
                    text = text.Replace("<b>", "");
                    text = text.Replace("</b>", "");
                    if (!text.EndsWith(".") && !Regex.Replace(text, "<.*?>", String.Empty).IsNullOrEmpty())
                    {
                        text += ".";
                    }
                }
                return text;
            }
        }

        public string BulletSubHeader
        {
            get
            {
                return Content.BulletSubHeader ?? string.Empty;
            }
        }

        public string CustomCSS
        {
            get
            {
                return Content.CustomCSS;
            }

        }
        public string CustomCSSMinified
        {
            get
            {
                return Content.CustomCSSMinified;
            }
        }

        public string TitleRemoveFrom
        {
            get
            {
                return _content.PageTitle.Replace(" FROM", "").Replace(" From", "").Replace("from", "");
            }
        }

        public override string VideoUrl
        {
            get
            {

                return Content.VideoUrl ?? DEFAULT_VIDEO_URL;
            }
        }

        public string YouTubeId
        {
            get
            {
                if (string.IsNullOrWhiteSpace(VideoUrl) || !VideoUrl.Contains("/"))
                    return VideoUrl;
                return VideoUrl.Split('/').Last();
            }
        }

        public string SubscribeButtonText
        {
            get
            {
                return Content.SubscribeButtonText ?? "Subscribe";
            }
        }

        public LandingPageContent Content
        {
            get
            {
                return _content;
            }
        }

        public string TemplateName
        {
            get
            {
                return _content.TemplateType.ToString();
            }
        }

        public List<OpenText> OpenTexts
        {
            get
            {
                if (Content.OpenTexts != null && Content.OpenTexts.Count > 0)
                {
                    return Content.OpenTexts;
                }
                else if (Content.OpenText != null)
                {
                    var openTexts = new List<OpenText>();
                    openTexts.Add(Content.OpenText);
                    return openTexts;
                }
                return null;
            }
        }

        public List<FeatureBullet> PaidSearchBannerFeatureBullets
        {
            get
            {
                if (Content.PaidSearchBannerFeatureBullets != null 
                    && Content.PaidSearchBannerFeatureBullets.Count > 0)
                    return Content.PaidSearchBannerFeatureBullets;
                return Content.FeatureBullets;
            }
        }

        public RateCalculatorModel Calculator
        {
            get
            {
                return new RateCalculatorModel
                {
                    DisplayCalculator = true,
                    FirstAgainCodeTrackingId = FirstAgainCodeTrackingId,
                    PurposeOfLoan = _content.PurposeOfLoan,
                    DisplayRateMatch = _content.ShowRateMatchProgramLogo && LightStreamWeb.Helpers.AppSettingsHelper.IsRatchMatchOfferEnabled(),
                    CustomRateDisclosureContent = _content.RatesDisclosure,
                    DisplayItsEasyToFindYourRate = ShouldDisplayItsEasyToFindYourRate(_content.TemplateType),
                    TypeOfCalculator = Content.TypeOfRateTableCalculator,
                    TypeOfDisplay = Content.TypeOfRateTableCalculatorDisplay
                };
            }
        }

        public bool DisplaysCustomRates()
        {
            switch (Content.TemplateType)
            {
                case LandingPageContent.VerticalMarketTemplateType.ResponsiveRedesignV1:
                case LandingPageContent.VerticalMarketTemplateType.ResponsivePartnerOption1:
                case LandingPageContent.VerticalMarketTemplateType.PaidSearch:
                case LandingPageContent.VerticalMarketTemplateType.Video:
                case LandingPageContent.VerticalMarketTemplateType.VideoAllPurpose:
                case LandingPageContent.VerticalMarketTemplateType.DirectMailCampaign:
                    return true;
                default:
                    return false;
            }
        }
        public bool IfAnyFeatureBulletsDefined()
        {
            if (FeatureBullets == null || FeatureBullets.Count == 0)
            {
                return false;
            }
            return FeatureBullets.Any(f => f.Graphic != null && f.Graphic.DefaultGraphic != null || !string.IsNullOrEmpty(f.Text));
        }

        public bool IsFeatureBulletCustomizationsDefined { get { return FeatureBulletSectionCustomizations != null; } }
        public bool IsFeatureBulletSectionThemeDefined
        {
            get
            {
                if (IsFeatureBulletCustomizationsDefined)
                    return this.FeatureBulletSectionCustomizations.IsFeatureBulletSectionThemeDefined;
                return false;
            }
        }
        public bool IsFeatureBulletsSectionHeaderDefined
        {
            get
            {
                if (FeatureBulletSectionCustomizations != null)
                    return this.FeatureBulletSectionCustomizations.IsFeatureBulletsSectionHeaderDefined;
                return false;

            }
        }
        public bool IsFeatureBulletsSectionSubheaderDefined
        {
            get
            {
                if (FeatureBulletSectionCustomizations != null)
                    return this.FeatureBulletSectionCustomizations.IsFeatureBulletsSectionSubheaderDefined;
                return false;
            }
        }

        public bool ShouldDisplayItsEasyToFindYourRate(LandingPageContent.VerticalMarketTemplateType templateType)
        {
            switch (templateType)
            {
                case LandingPageContent.VerticalMarketTemplateType.PaidSearch:
                case LandingPageContent.VerticalMarketTemplateType.Video:
                case LandingPageContent.VerticalMarketTemplateType.VideoAllPurpose:
                case LandingPageContent.VerticalMarketTemplateType.LoanAggregator:
                case LandingPageContent.VerticalMarketTemplateType.ComponentTemplate:
                    return true;
                default:
                    return false;
            }
        }

        public void CheckAndSetFactCookie(string factParam)
        {
            int fact;

            if (int.TryParse(factParam, out fact))
            {
                _cookieUtility.ResetExistingFactCookie(fact);
                if (_factUtility.IsValidFact(fact))
                {
                    _cookieUtility.SetFactAndFair(fact, null, string.Empty);
                }
            }

            FirstAgainCodeTrackingId = WebUser.FirstAgainCodeTrackingId;
        }

        public string GetMinMaxRateRange()
        {

            var interestRates = DomainServiceInterestRateOperations.GetCachedInterestRates();
            var rateRange = interestRates.GetCachedRateRangeForAllTermsAmountsPurposesAndTiers(FirstAgain.Domain.Lookups.FirstLook.PaymentTypeLookup.PaymentType.AutoPay, null);

            return $"{ rateRange.MinRate.Rate.ToString("0.00##%") } - { rateRange.MaxRate.Rate.ToString("0.00##%") }";
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