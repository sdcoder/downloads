using FirstAgain.Domain.SharedTypes.ContentManagement;
using LightStreamWeb.Helpers;
using LightStreamWeb.Models.Components;
using LightStreamWeb.Models.Middleware;
using LightStreamWeb.Models.Shared;

namespace LightStreamWeb.Models.PublicSite
{
    public class RatesPageModel : PublicSiteWithBannerModel, IPageWithHeading
    {
        private RatesPage _cmsRatesPage = null;

        public string Heading { get; set; }

        public string SubTitle
        {
            get
            {
                return _cmsRatesPage.SubTitle;
            }
        }

        public RatesPageModel() : this(new ContentManager(), AppSettings.Load().PageDefault.Rates)
        { }

        public RatesPageModel(ContentManager content, LightStreamPageDefault defaults)
        {
            BodyClass = defaults.BodyClass;
            NgApp = "LightStreamApp";
            Heading = defaults.Heading;
            PageTitle = defaults.PageTitle;

            Calculator = new RateCalculatorModel()
            {
                DisplayCalculator = true,
                DisplayRateMatch = AppSettingsHelper.IsRatchMatchOfferEnabled(),
                FirstAgainCodeTrackingId = FirstAgainCodeTrackingId,
                PurposeOfLoan = PurposeOfLoan.GetValueOrDefault(FirstAgain.Domain.Lookups.FirstLook.PurposeOfLoanLookup.PurposeOfLoan.NotSelected)
            };
            _cmsRatesPage = content.Get<RatesPage>();
            Banner = _cmsRatesPage.Banner ?? new BannerImage
            {
                Image = new WebImage { Alt = defaults.BannerAlt },
                HttpImageUrl = defaults.Banner
            };
            MetaImage = Banner.ToImageUrl().ToString();
        }

        public RateCalculatorModel Calculator { get; set; }
    }
}