using FirstAgain.Common.Extensions;
using FirstAgain.Domain.SharedTypes.ContentManagement;
using FirstAgain.Domain.SharedTypes.ContentManagement.SiteContent.Sitewide;
using System.Linq;

namespace LightStreamWeb.Models.PublicSite
{
    public class BasePublicPageWithAdObjects : PublicSiteWithBannerModel
    {
        protected ContentManager _cms;
        private AdObjectFactory _AdFactory;

        public BasePublicPageWithAdObjects() : this(new ContentManager())
        {
        }

        public BasePublicPageWithAdObjects(ContentManager content)
        {
            _cms = content;
            _AdFactory = new AdObjectFactory(_cms);
        }

        public ContentManager ContentManager
        {
            get
            {
                return _cms;
            }
        }

        public virtual SiteAdObject GetFirstAdObject()
        {
            var ad = _AdFactory.AdObject(SiteAdObject.AdVerticalPositionType.Top, !_cms.IsCmsPreview());
            if (ad.Default)
                ad.Html = "<div class=\"ad apply clearfix\"><span class=\"message right\">It's time to see those<br>home improvement<br>dreams realized.</span><a href=\"/apply?purposeOfLoan=HomeImprovement\" class=\"button right\">Apply Now</a><a href=\"/home-improvement-loan\" class=\"small light right learnmore\">Learn More »</a></div>";
            return ad;
        }

        public virtual SiteAdObject GetSecondAdObject()
        {
            var ad = _AdFactory.AdObject(SiteAdObject.AdVerticalPositionType.Bottom, !_cms.IsCmsPreview());
            if (ad.Default)
                ad.Html = "<div class=\"ad news\"><a href='http://www.reuters.com/article/2013/10/28/us-loans-unsecured-idUSBRE99R0YI20131028' data-jump='true' target='_blank'><span class=\"message\">&quot;LightStream, the online lending division of SunTrust Banks Inc., is taking aim at a niche space: Low interest unsecured loans for highly qualified customers.&quot;<cite class=\"brand\">– October 28, 2013<br>Reuters.com</cite></span></a></div>";
            return ad;
        }

        private CustomerComment _randomComment = null;

        public CustomerComment RandomComment
        {
            get
            {
                if (_randomComment != null)
                {
                    return _randomComment;
                }

                CustomerComments comments = _cms.Get<CustomerComments>();
                if (comments.Comments.Any(c => c.AllowOnHomePage))
                {
                    _randomComment = comments.Comments.Where(c => c.AllowOnHomePage).RandomElement<CustomerComment>();
                }

                return _randomComment;
            }
        }
    }
}