using FirstAgain.Common.Extensions;
using FirstAgain.Domain.SharedTypes.ContentManagement.SiteContent.Sitewide;
using System.Collections.Generic;
using System.Linq;

namespace LightStreamWeb.Models.PublicSite
{
    public class AdObjectFactory
    {
        private ContentManager Content;

        public AdObjectFactory() : this(new ContentManager())
        { }

        public AdObjectFactory(ContentManager content)
        {
            Content = content;
        }

        public SiteAdObject AdObject(SiteAdObject.AdHorizontalPositionType horizontal, bool ActiveOnly)
        {
            var adList = AdObjectList(null, horizontal, null, ActiveOnly);
            SiteAdObject adObject;
            if (adList.Count > 0)
            {
                adObject = adList.RandomElement();
            }
            else
            {
                switch (horizontal)
                {
                    case SiteAdObject.AdHorizontalPositionType.Left:
                        adObject = DefaultAd(SiteAdObject.SiteAdType.NewsArticle);
                        break;

                    default:
                        adObject = DefaultAd(SiteAdObject.SiteAdType.ApplyNow);
                        break;
                }
            }
            return adObject;
        }

        public SiteAdObject AdObject(SiteAdObject.AdVerticalPositionType vertical, bool ActiveOnly)
        {
            var adList = AdObjectList(null, null, vertical, ActiveOnly);
            SiteAdObject adObject;
            if (adList.Count > 0)
            {
                adObject = adList.RandomElement();
            }
            else
            {
                switch (vertical)
                {
                    case SiteAdObject.AdVerticalPositionType.Bottom:
                        adObject = DefaultAd(SiteAdObject.SiteAdType.NewsArticle);
                        break;

                    default:
                        adObject = DefaultAd(SiteAdObject.SiteAdType.ApplyNow);
                        break;
                }
            }
            return adObject;
        }

        private void AdPositionDefaults(SiteAdObject.SiteAdType adType, out SiteAdObject.AdHorizontalPositionType horizontal, out SiteAdObject.AdVerticalPositionType vertical)
        {
            switch (adType)
            {
                case SiteAdObject.SiteAdType.ApplyNow:
                    vertical = SiteAdObject.AdVerticalPositionType.Top;
                    horizontal = SiteAdObject.AdHorizontalPositionType.Right;
                    break;

                case SiteAdObject.SiteAdType.NewsArticle:
                    vertical = SiteAdObject.AdVerticalPositionType.Bottom;
                    horizontal = SiteAdObject.AdHorizontalPositionType.Left;
                    break;

                default:
                    vertical = SiteAdObject.AdVerticalPositionType.Top;
                    horizontal = SiteAdObject.AdHorizontalPositionType.Right;
                    break;
            }
        }

        public List<SiteAdObject> AdObjectList(SiteAdObject.SiteAdType? adType, SiteAdObject.AdHorizontalPositionType? horizontal, SiteAdObject.AdVerticalPositionType? vertical, bool activeAd)
        {
            var collection = new List<SiteAdObject>();
            var SiteAds = Content.GetMultiple<SiteAds>().AsEnumerable();
            if (adType.HasValue)
                SiteAds = SiteAds.Where(ad => ad.AdObject.Type == adType);
            if (horizontal.HasValue)
                SiteAds = SiteAds.Where(ad => ad.AdObject.HorizontalPosition == horizontal);
            if (vertical.HasValue)
                SiteAds = SiteAds.Where(ad => ad.AdObject.VerticalPosition == vertical);
            if (activeAd)
                SiteAds = SiteAds.Where(ad => ad.AdObject.Active);
            foreach (var ad in SiteAds)
            {
                collection.Add(ad.AdObject);
            }
            return collection;
        }

        protected SiteAdObject DefaultAd(SiteAdObject.SiteAdType adType)
        {
            SiteAdObject.AdHorizontalPositionType horizontal;
            SiteAdObject.AdVerticalPositionType vertical;
            LoanPurposeAdObject loanAdData = null;
            NewsQuoteAdObject newsAdData = null;
            string link = "";
            string info = "";
            AdPositionDefaults(adType, out horizontal, out vertical);
            switch (adType)
            {
                case SiteAdObject.SiteAdType.ApplyNow:
                    loanAdData = new LoanPurposeAdObject
                    {
                        PurposeOfLoan = FirstAgain.Domain.Lookups.FirstLook.PurposeOfLoanLookup.PurposeOfLoan.HomeImprovement
                    };
                    link = "/home-improvement-loan";
                    info = "It's time to see those home improvement dreams realized.";
                    break;

                case SiteAdObject.SiteAdType.NewsArticle:
                    newsAdData = new NewsQuoteAdObject
                    {
                        Date = "October 28, 2013",
                        Source = "Reuters.com"
                    };
                    link = "http://www.reuters.com/article/2013/10/28/us-loans-unsecured-idUSBRE99R0YI20131028";
                    info = "LightStream, the online lending division of SunTrust Banks Inc., is taking aim at a niche space: Low interest unsecured loans for highly qualified customers.";
                    break;

                default:
                    break;
            }
            return new SiteAdObject
            {
                Type = adType,
                Active = true,
                HorizontalPosition = horizontal,
                VerticalPosition = vertical,
                HyperLink = link,
                Tagline = info,
                LoanPurposeAd = loanAdData,
                NewsQuoteAd = newsAdData,
                Default = true
            };
        }
    }
}