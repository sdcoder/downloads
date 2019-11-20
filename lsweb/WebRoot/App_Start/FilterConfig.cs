using System;
using System.Web.Mvc;
using FirstAgain.Common.Extensions;
using FirstAgain.Common.Logging;
using LightStreamWeb.App_State;
using LightStreamWeb.Helpers;
using Ninject;

namespace LightStreamWeb
{
    public class FilterConfig
    {
        private static ICurrentHttpRequest _httpRequest;

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            // Use our own http request to leverage customized methods for getting urls
            _httpRequest = App_Start.NinjectWebCommon.Bootstrapper.Kernel.Get<ICurrentHttpRequest>();

            filters.Add(new XframeOptionsFilter());
        }

        public class XframeOptionsFilter : ActionFilterAttribute
        {
            public override void OnResultExecuted(ResultExecutedContext filterContext)
            {
                try
                {
                    // ignore bad requests - possible request header splitting attack signature
                    if (filterContext.HttpContext.Request.ServerVariables["HTTP_REFERER"] != null && 
                        (filterContext.HttpContext.Request.ServerVariables["HTTP_REFERER"].Contains("+") || filterContext.HttpContext.Request.ServerVariables["HTTP_REFERER"].Contains(" "))
                        )
                    {
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(_httpRequest.UrlReferrer) || !TrustedSiteHelper.CanIframe(_httpRequest.UrlReferrer, _httpRequest.UrlRequested))
                    {
                        //TODO: this should be replaced with the frame-ancestors directive of CSP once it is widely supported, x-frame-option is a deadend
                        filterContext.HttpContext.Response.Headers.Set("x-frame-options", "SAMEORIGIN");
                    }
                }
                catch (Exception ex)
                {
                    //HttpContext.Current.Request.UrlReferrer.IsNotNull() ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : string.Empty;
                    LightStreamLogger.WriteInfo(filterContext.HttpContext.Request.ServerVariables["HTTP_REFERER"]);
                    throw ex;
                }
            }
        }
    }
}