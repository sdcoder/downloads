using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using LightStreamWeb.Extensions;
using FirstAgain.Common.Caching;
using FirstAgain.Domain.Common;
using FirstAgain.Domain.Lookups.FirstLook;
using System.Net.Http;
using System.Net;

namespace LightStreamWeb.Filters
{
    public class RssAuthorizeFilter : AuthorizationFilterAttribute
    {
        private string _acceptedDomain;

        private BusinessConstants _businessConstants => MachineCache.Get<BusinessConstants>("BusinessConstants");        

        public RssAuthorizeFilter()
        {
            _acceptedDomain = _businessConstants.BloggerURL;
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (actionContext.SkipAuthorization() || _businessConstants.Environment != EnvironmentLookup.Environment.Production ||
                                                     _businessConstants.Environment != EnvironmentLookup.Environment.Staging)
                return;

            var rssUrl = HttpContext.Current.Request.QueryString["rssUrl"];

            if (rssUrl == null || _acceptedDomain == null || !new Uri(rssUrl).Host.Equals(new Uri(_acceptedDomain).Host))
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }
    }
}