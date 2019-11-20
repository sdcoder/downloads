using LightStreamWeb.Models.Middleware;

namespace LightStreamWeb.Models.PublicSite
{
    public static class AboutPagesModelFactory
    {
        public static AboutPagesModel Create(string model, LightStreamPageDefault defaults)
        {
            var aboutPageType = model.ToLower();
            if (aboutPageType == "affiliate-program")
                return new AffiliateProgramPageModel(new ContentManager(), defaults);
            else if (aboutPageType == "licensing")
                return new LicensingPageModel(new ContentManager(), defaults);
            else
                return new AboutPagesModel(new ContentManager(), defaults);
        }
    }
}