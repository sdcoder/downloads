using FirstAgain.Domain.SharedTypes.ContentManagement;
using FirstAgain.Domain.SharedTypes.ContentManagement.SiteContent.Sitewide;
using LightStreamWeb.Helpers;
using System.Collections.Generic;

namespace LightStreamWeb.Models.PublicSite
{
    public class RedesignPageModel : PublicSiteWithBannerModel
    {
        private RedesignPage _homePageContent;
        private AdObjectFactory _AdFactory;
        private ContentManager contentManager;

        public string BenefitsStatement { get; set; }
        public string BenefitsStatementSmallScreens { get; set; }
        public bool DisplayRateMatch { get; set; }
        public List<FeatureBullet> FeatureBullets { get; private set; }
        public FeatureBulletSectionCustomizations FeatureBulletSectionCustomizations { get; private set; }
        public string PageTitleSmallScreens { get; set; }
        public bool ShowCTAWhyLightStreamLink { get; set; }
        public List<string> CTABarDescriptions { get; set; }

        public CTABarModel CTABar
        {
            get
            {
                return new CTABarModel()
                {
                    CTABarDescriptions = this.CTABarDescriptions,
                    ShowCTAWhyLightStreamLink = this.ShowCTAWhyLightStreamLink
                };
            }
        }

        public string RateTitle { get; set; }
        public string RateSubTitle { get; set; }
        public WebImage TestimonialCommentsGraphic { get; set; }
        public WebImage MobileTestimonialCommentsGraphic { get; set; }
        public WebImage MediaCoverageGraphic { get; set; }
        public WebImage MobileMediaCoverageGraphic { get; set; }
        public bool DisplayApplyInMinutes { get; set; } = true;
        public string CustomCSS { get; set; }
        public string CustomCSSMinified { get; set; }
        public string LightStreamExperienceVideoUrl { get; set; }
        public string DisclosureCopy { get; set; }        
        public string PartnerEventUrl { get; set; }
        public bool AdobeTargetBodyHiding { get; set; }

        public RedesignPageModel() : this(new ContentManager(), Middleware.AppSettings.Load().PageDefault.HomePage)
        { }

        public RedesignPageModel(ContentManager content, Middleware.LightStreamPageDefault defaults)
        {
            contentManager = content;

            NgApp = "LightStreamApp";

            _homePageContent = contentManager.Get<RedesignPage>();
            FeatureBullets = _homePageContent.FeatureBullets;

            ShowCTAWhyLightStreamLink = _homePageContent.ShowCTAWhyLightStreamLink;
            CTABarDescriptions = _homePageContent.CTABarDescriptions;
            VideoUrl = _homePageContent.VideoUrl;
            RateTitle = _homePageContent.RateTitle;
            RateSubTitle = _homePageContent.RateSubtitle;
            TestimonialCommentsGraphic = _homePageContent.TestimonialCommentsGraphic;
            MobileTestimonialCommentsGraphic = _homePageContent.MobileTestimonialCommentsGraphic ?? TestimonialCommentsGraphic;
            MediaCoverageGraphic = _homePageContent.MediaCoverageGraphic;
            MobileMediaCoverageGraphic = _homePageContent.MobileMediaCoverageGraphic ?? MediaCoverageGraphic;
            DisplayApplyInMinutes = _homePageContent.DisplayApplyInMinutes;
            CustomCSS = _homePageContent.CustomCSS;
            CustomCSSMinified = _homePageContent.CustomCSSMinified;
            LightStreamExperienceVideoUrl = _homePageContent.LightStreamExperienceVideoUrl;
            FeatureBulletSectionCustomizations = _homePageContent.FeatureBulletSectionCustomizations;
            DisclosureCopy = ADAHelper.PreventMultipleBRsRepeatingBlank(_homePageContent.DisclosureCopy);
            AdobeTargetBodyHiding = _homePageContent.AdobeTargetBodyHiding; 

            BenefitsStatement = _homePageContent.BenefitStatement ?? defaults.BenefitStatement;
            BenefitsStatementSmallScreens = _homePageContent.BenefitStatementSmallScreens ?? defaults.BenefitStatement;
            BodyClass = defaults.BodyClass;
            DisplayRateMatch = AppSettingsHelper.IsRatchMatchOfferEnabled();
            PageTitle = _homePageContent.PageTitle ?? defaults.PageTitle;
            PageTitleSmallScreens = _homePageContent.PageTitleSmallScreens ?? defaults.PageTitle;

            Banner = _homePageContent.Banner ?? new BannerImage
            {
                Image = new WebImage { Alt = defaults.BannerAlt },
                HttpImageUrl = defaults.Banner
            };
            BannerMobile = _homePageContent.BannerMobile ?? Banner;

            base.SetMetadataContent(new ContentManager().Get<RedesignPage>(), Banner.ToImageUrl().ToString());

            _AdFactory = new AdObjectFactory(contentManager);
        }

        public SiteAdObject FirstAdObject
        {
            get
            {
                var ad = _AdFactory.AdObject(SiteAdObject.AdHorizontalPositionType.Left, !contentManager.IsCmsPreview());
                if (ad.Default)
                    ad.Html = "<a href='http://www.reuters.com/article/2013/10/28/us-loans-unsecured-idUSBRE99R0YI20131028' data-jump='true' target='_blank'><img src='/content/images/reuters-ad-desktop.jpg' /></a>";
                return ad;
            }
        }

        public SiteAdObject SecondAdObject
        {
            get
            {
                var ad = _AdFactory.AdObject(SiteAdObject.AdHorizontalPositionType.Right, !contentManager.IsCmsPreview());
                if (ad.Default)
                    ad.Html = "<div class=\"temp-home-improvement-ad\" id=\"adobject\"><p>It's time to see those home improvement dreams realized.</p><a class=\"button button-med\" href='/apply?purposeOfLoan=HomeImprovement'>Apply Now</a><p><a href='/home-improvement-loan'>Learn More »</a></p></div>";
                return ad;
            }
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
                if (this.FeatureBulletSectionCustomizations != null)
                    return this.FeatureBulletSectionCustomizations.IsFeatureBulletsSectionHeaderDefined;
                return false;

            }
        }
        public bool IsFeatureBulletsSectionSubheaderDefined
        {
            get
            {
                if (this.FeatureBulletSectionCustomizations != null)
                    return this.FeatureBulletSectionCustomizations.IsFeatureBulletsSectionSubheaderDefined;
                return false;
            }
        }
    }
}