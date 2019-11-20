using FirstAgain.Domain.SharedTypes.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.PublicSite
{
    public class SiteMapPageModel : BaseLightstreamPageModel
    {
        public string AlertHeading { get; set; }
        public string AlertMessage { get; set; }

        public SiteMapPageModel()
        {
            Title = "LightStream Loans Site Map";
            NgApp = "LightStreamApp";
            MetaDescription = "View the site map for LightStream.com website. LightStream online lending offers unsecured personal loans at low rates for those with good credit.";
        }

        public IEnumerable<LandingPageLink> LandingPageLinks
        {
            get
            {
                var contentManager = new LightStreamWeb.ContentManager();
                return contentManager.Get<LandingPageLinks>().FooterLinks;
            }
        }
    }
}