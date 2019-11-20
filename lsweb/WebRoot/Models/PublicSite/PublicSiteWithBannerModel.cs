using FirstAgain.Domain.SharedTypes.ContentManagement;
using LightStreamWeb.Models.Middleware;
using Ninject;

namespace LightStreamWeb.Models.PublicSite
{
    public class PublicSiteWithBannerModel : BaseLightstreamPageModel
    {
        public PublicSiteWithBannerModel()
        {
        }

        public string PageTitle { get; protected set; }
        public BannerImage Banner { get; protected set; }
        public BannerImage BannerMobile { get; protected set; }

    }
}