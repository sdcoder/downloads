using FirstAgain.Common.Extensions;
using FirstAgain.Domain.Common;
using FirstAgain.Domain.Lookups.FirstLook;
using LightStreamWeb.App_State;
using System;
using LightStreamWeb.Helpers;
using Ninject;
using System.Configuration;
using FirstAgain.Domain.SharedTypes.ContentManagement;
using FirstAgain.Domain.ServiceModel.Client;
using LightStreamWeb.Shared.Rates;
using LightStreamWeb.Models.Middleware;
using System.ComponentModel;
using FirstAgain.Common.Wcf;
using System.ServiceModel;
using System.Diagnostics;

namespace LightStreamWeb.Models
{
    public abstract class BaseLightstreamPageModel
    {
        protected string _cdnBaseUrl;
        protected readonly ICurrentUser _user;
        protected readonly ICurrentHttpRequest _httpRequest;

        [ReadOnly(true)]
        protected ICurrentUser WebUser
        {
            get
            {
                return _user;
            }
        }

        [ReadOnly(true)]
        public string CdnBaseUrl
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_cdnBaseUrl))
                {
                    _cdnBaseUrl = App_Start.NinjectWebCommon.Bootstrapper.Kernel.Get<IAppSettings>().CdnBaseUrl;
                }

                return _cdnBaseUrl;
            }
        }

        public BaseLightstreamPageModel()
        {
            Title = "LightStream - Loans for Practically Anything";
            if (_user == null)
            {
                _user = App_Start.NinjectWebCommon.Bootstrapper.Kernel.Get<ICurrentUser>();

            }

            if (_httpRequest == null)
            {
                _httpRequest = App_Start.NinjectWebCommon.Bootstrapper.Kernel.Get<ICurrentHttpRequest>();
            }
        }

        [Inject]
        public BaseLightstreamPageModel(ICurrentUser user)
        {
            _user = user;

            if (_httpRequest == null)
            {
                _httpRequest = App_Start.NinjectWebCommon.Bootstrapper.Kernel.Get<ICurrentHttpRequest>();
            }
        }

        public BaseLightstreamPageModel(ICurrentUser user, ICurrentHttpRequest httpRequest)
        {
            _user = user;
            _httpRequest = httpRequest;

            if (_httpRequest == null)
            {
                _httpRequest = App_Start.NinjectWebCommon.Bootstrapper.Kernel.Get<ICurrentHttpRequest>();
            }
        }

        [ReadOnly(true)]
        public string Title { get; set; }
        [ReadOnly(true)]
        public string Canonical { get; set; }
        [ReadOnly(true)]
        public string HeadTitle { get; set; }
        [ReadOnly(true)]
        public string MetaDescription { get; set; }
        [ReadOnly(true)]
        public string MetaKeywords { get; set; }
        [ReadOnly(true)]
        public string MetaImage { get; set; }
        [ReadOnly(true)]
        public string MetaCanonical { get; set; }
        [ReadOnly(true)]
        public string BodyClass { get; set; } // for design, styling
        [ReadOnly(true)]
        public string NgApp { get; protected set; } // for angularJS - one app per page
        [ReadOnly(true)]
        public string NavClass { get; set; } = "darktint"; //darktint/bluetint homepage/redesign
        [ReadOnly(true)]
        public string NavAutoLabel { get; set; } = "Vehicle"; //Auto on redesign page
        [ReadOnly(true)]
        public string LogoUrl { get; set; } = "/";
        [ReadOnly(true)]
        public virtual string VideoUrl { get; set; }

        [ReadOnly(true)]
        public bool CanIFrame
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_httpRequest.UrlReferrer))
                {
                    return TrustedSiteHelper.CanIframe(_httpRequest.UrlReferrer, _httpRequest.UrlRequested);
                }

                return false;
            }
        }

        protected const string ROOT_CANONICAL = "https://www.lightstream.com/";

        [ReadOnly(true)]
        public bool EnableFileUpload { get; set; }

        private bool? _enableRUM = null;
        [ReadOnly(true)]
        public bool EnableRUM
        {
            set
            {
                _enableRUM = value;
            }
            get
            {
                if (_enableRUM == null)
                {
                    // throttle
                    if (DateTime.Now.Ticks % 3 == 0)
                    {
                        return false;
                    }
                    return ConfigurationManager.AppSettings["EnableRUM"] != null && ConfigurationManager.AppSettings["EnableRUM"] == "true";
                }

                return _enableRUM.GetValueOrDefault(false);
            }
        }

        [ReadOnly(true)]
        public Exception GenericExeption { get; protected set; }

        protected void SetGenericErrorFlag(Exception ex)
        {
            GenericExeption = ex;
            BodyClass += " error";
        }

        public void AppendBodyClass(string className)
        {
            BodyClass += " " + className;
        }

        [ReadOnly(true)]
        public string SessionApplyCookie
        {
            get
            {
                return _user.SessionApplyCookie;
            }
        }
        #region social media icons
        [ReadOnly(true)]
        public string FacebookURL
        {
            get
            {
                return BusinessConstants.Instance.FacebookURL;
            }
        }
        [ReadOnly(true)]
        public string TwitterURL
        {
            get
            {
                return BusinessConstants.Instance.TwitterURL;
            }
        }
        [ReadOnly(true)]
        public string GoogleplusURL
        {
            get
            {
                return BusinessConstants.Instance.GoogleplusURL;
            }
        }
        [ReadOnly(true)]
        public string YoutubeURL
        {
            get
            {
                return BusinessConstants.Instance.YoutubeURL;
            }
        }

        [ReadOnly(true)]
        public string BloggerURL
        {
            get
            {
                return BusinessConstants.Instance.BloggerURL;
            }
        }

        [ReadOnly(true)]
        public string CareersLinkURL
        {
            get
            {
                var url = BusinessConstants.Instance.CareersLinkURL;
                if (string.IsNullOrWhiteSpace(url))
                    return "https://jobs.suntrust.com/ListJobs/All/Search/State/CA/keyword/sandiego";
                return url;
            }
        }
        #endregion
   

        public PurposeOfLoanLookup.PurposeOfLoan? PurposeOfLoan { get; set; }
        public int? FirstAgainCodeTrackingId { get; set; }

        [ReadOnly(true)]
        public DisplayRateModel DisplayRate
        {
            get
            {
                var rates = DomainServiceInterestRateOperations.GetCachedInterestRates();
                if (PurposeOfLoan.HasValue && (int)PurposeOfLoan != 0 && PurposeOfLoan.Value != PurposeOfLoanLookup.PurposeOfLoan.NotSelected)
                {
                    return new DisplayRateModel(rates).GetForPurposeOfLoan(PurposeOfLoan.Value, FirstAgainCodeTrackingId);
                }

                return new DisplayRateModel(rates).GetLowestRate(FirstAgainCodeTrackingId);
            }
        }

        public string GetFileHash(string filePath)
        {
            return Startup.GetFileHash(filePath);
        }

        public void SetMetadataContent(WebPageContentBase content, string bannerUrl = "")
        {
            if (content?.MetaTagContent == null) return;

            HeadTitle = content.MetaTagContent.PageTitle;
            MetaDescription = content.MetaTagContent.MetaTagDescription;
            MetaKeywords = content.MetaTagContent.MetaTagKeywords;
            MetaImage = bannerUrl;
        }

        public string PublicRestApiUri
        {
            get
            {
                return new Uri(WebServiceRegistrar.GetUrl("LightStream.Web.PublicRestApi")).AbsoluteUri;
            }
        }

        public string ParentCompanyName
        {
            get
            {
                return BusinessConstants.Instance.MergerNameStatement();
            }
        }
    }
}