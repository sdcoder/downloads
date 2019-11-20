using FirstAgain.Domain.SharedTypes.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.Components
{
    public class LandingPageLinksModel
    {
        public string FooterText { get; private set; }

        public IEnumerable<LandingPageLink> VehicleLinks { get; private set; }
        public IEnumerable<LandingPageLink> HomeImprovementLinks { get; private set; }
        public IEnumerable<LandingPageLink> RecreationLinks { get; private set; }
        public IEnumerable<LandingPageLink> FamilyLinks { get; private set; }
        public IEnumerable<LandingPageLink> OtherLinks { get; private set; }

        public LandingPageLinksModel()
        {
            var contentManager = new LightStreamWeb.ContentManager();

            FooterText = contentManager.Get<LandingPageLinks>().FooterText;

            PopulateFooterLinks(contentManager.Get<LandingPageLinks>().FooterLinks);
        }

        private void PopulateFooterLinks(List<LandingPageLink> links)
        {
            if (links.All(l => l.Category == LandingPageLink.LandingPageLinkCategory.Other))
            {
                // CMS not populated yet, default them 
                // TODO e3p0: remove in 1403/1404 release, once CMS is updated in prod
                VehicleLinks = links.Where(l => new string[] { "Auto Refinance", 
                                                                "Classic Car Financing", 
                                                                "Motorcycle Loans", 
                                                                "New Car Loans", 
                                                                "RV Loans", 
                                                                "Used Car Loan" }.Contains(l.DisplayText)).OrderBy(l => l.DisplayText);
                HomeImprovementLinks = links.Where(l => new string[] { "Basement Remodels",
                                                                        "Hot Tub Financing",
                                                                        "Kitchen and bath Remodels",
                                                                        "Landscape Financing",
                                                                        "Solar Financing",
                                                                        "Swimming Pool Financing", 
                                                                        "Home Improvement Loans", 
                                                                        "Swimming Pool Loans" }.Contains(l.DisplayText)).OrderBy(l => l.DisplayText);
                RecreationLinks = links.Where(l => new string[] { "Boat Loans",
                                                                    "Destination Club Financing",
                                                                    "Fractional Loans",
                                                                    "Golf Membership Financing",
                                                                    "Marine Products Loans",
                                                                    "Timeshare Loans" }.Contains(l.DisplayText)).OrderBy(l => l.DisplayText);
                FamilyLinks = links.Where(l => new string[] { "Adoption Loans",
                                                                "Dental Financing",
                                                                "IVF Financing",
                                                                "Medical Financing",
                                                                "Pre-K – 12 Financing",
                                                                "PreK-12 Financing", 
                                                                "Wedding Loans" }.Contains(l.DisplayText)).OrderBy(l => l.DisplayText);
                OtherLinks = links.Where(o =>
                    !VehicleLinks.Any(v => v.QueryParameterValue == o.QueryParameterValue) &&
                    !HomeImprovementLinks.Any(h => h.QueryParameterValue == o.QueryParameterValue) &&
                    !RecreationLinks.Any(r => r.QueryParameterValue == o.QueryParameterValue) &&
                    !FamilyLinks.Any(f => f.QueryParameterValue == o.QueryParameterValue)
                    ).OrderBy(l => l.DisplayText);
            }
            else
            {
                VehicleLinks = links.Where(l => l.Category == LandingPageLink.LandingPageLinkCategory.Vehicle).OrderBy(l => l.DisplayText);
                HomeImprovementLinks = links.Where(l => l.Category == LandingPageLink.LandingPageLinkCategory.HomeImprovement).OrderBy(l => l.DisplayText);
                RecreationLinks = links.Where(l => l.Category == LandingPageLink.LandingPageLinkCategory.Recreation).OrderBy(l => l.DisplayText);
                FamilyLinks = links.Where(l => l.Category == LandingPageLink.LandingPageLinkCategory.Family).OrderBy(l => l.DisplayText);
                OtherLinks = links.Where(l => l.Category == LandingPageLink.LandingPageLinkCategory.Other).OrderBy(l => l.DisplayText);
            }

        }

    }
}