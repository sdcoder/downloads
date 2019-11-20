using FirstAgain.Domain.SharedTypes.ContentManagement;
using LightStreamWeb.Models.Middleware;
using LightStreamWeb.Models.Shared;

namespace LightStreamWeb.Models.PublicSite
{
    public class RateMatchPageModel : BasePublicPageWithAdObjects, IPageWithHeading
    {
        private readonly RateMatchPage _rateMatchPageContent;

        public string Heading { get; private set; }

        public string RateMatchCopy { get; private set; }

        public RateMatchPageModel() : this(new ContentManager(), AppSettings.Load().PageDefault.RateBeat)
        { }

        public RateMatchPageModel(ContentManager content, LightStreamPageDefault defaults)
        {
            BodyClass = defaults.BodyClass;

            _rateMatchPageContent = content.Get<RateMatchPage>() ?? new RateMatchPage();
            PageTitle = string.IsNullOrEmpty(_rateMatchPageContent.PageTitle) ? defaults.PageTitle : _rateMatchPageContent.PageTitle;
            Heading = string.IsNullOrEmpty(_rateMatchPageContent.Header) ? defaults.Heading : _rateMatchPageContent.Header;
            RateMatchCopy = _rateMatchPageContent.RateMatchCopy;

            Banner = _rateMatchPageContent.Banner ?? new BannerImage {
                Image = new WebImage { Alt = defaults.BannerAlt },
                HttpImageUrl = defaults.Banner
            };

            base.SetMetadataContent(_rateMatchPageContent);
        }
    }
}