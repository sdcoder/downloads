using System;
using System.Collections.Specialized;
using System.Web;
using FirstAgain.Common.Extensions;
using FirstAgain.Common.Logging;

namespace LightStreamWeb.App_State
{
    public class CurrentHttpRequest : ICurrentHttpRequest
    {
        public int Port
        {
            get
            {
                return HttpContext.Current.Request.Url.Port;
            }
        }

        public NameValueCollection Params
        {
            get
            {
                return HttpContext.Current.Request.Params;
            }
        }

        public NameValueCollection QueryString
        {
            get
            {
                return HttpContext.Current.Request.QueryString;
            }
        }

        public string RootPath
        {
            get
            {
                return HttpContext.Current.Request.ApplicationPath;
            }
        }

        public string UrlRequested
        {
            get
            {
                var request = HttpContext.Current.Request;

                var scheme = request.Url.Scheme;
                var host = request.Url.Authority;
                var rawUrl = request.RawUrl.Contains("?") ? request.RawUrl.Remove(request.RawUrl.IndexOf('?')) : request.RawUrl;

                var canonicalUrl = string.Format("{0}://{1}{2}", scheme, host, rawUrl);
                
                return canonicalUrl;
            }
        }

        public string UrlReferrer
        {
            get
            {
                try
                {
                    // ignore bad requests - possible request header splitting attack signature
                    if (HttpContext.Current.Request.ServerVariables["HTTP_REFERER"] != null && HttpContext.Current.Request.ServerVariables["HTTP_REFERER"].Contains("+"))
                    {
                        return string.Empty;
                    }

                    return HttpContext.Current.Request.UrlReferrer.IsNotNull() ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : string.Empty;
                }
                catch (Exception ex)
                {
                    LightStreamLogger.WriteWarning(ex, "Possible invalid referred {0}", HttpContext.Current.Request.ServerVariables["HTTP_REFERER"]);
                    return string.Empty;
                }
            }
        }
    }
}