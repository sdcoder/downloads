using FirstAgain.Domain.SharedTypes.ContentManagement;
using FirstAgain.Domain.SharedTypes.ContentManagement.SiteContent.Sitewide;
using LightStreamWeb.Helpers;
using System.Collections.Generic;

namespace LightStreamWeb.Models.PublicSite
{
    public class HomePageModel : PublicSiteWithBannerModel
    {
        private HomePage _homePageContent;
        private AdObjectFactory _AdFactory;
        private ContentManager contentManager;

        public string BenefitsStatement { get; set; }
        public string BenefitsStatementSmallScreens { get; set; }
        public bool DisplayRateMatch { get; set; }
        public List<FeatureBullet> FeatureBullets { get; private set; }
        public string PageTitleSmallScreens { get; set; }

        public HomePageModel() : this(new ContentManager(), Middleware.AppSettings.Load().PageDefault.HomePage)
        { }

        public HomePageModel(ContentManager content, Middleware.LightStreamPageDefault defaults)
        {
            contentManager = content;

            NgApp = "LightStreamApp";

            _homePageContent = contentManager.Get<HomePage>();
            FeatureBullets = _homePageContent.FeatureBullets;

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
    }
}