using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.SharedTypes.ContentManagement;
using LightStreamWeb.Helpers;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace LightStreamWeb
{
    public class URLRewriteModule : IHttpModule
    {
        const string CMSResourceRegEx = @"/cms/";
        const string WebResourceRegEx = @"\.(flv|swf|gif|jpeg|jpg|png|pdf|css|js|mp3|ico|axd)$|(webresource.axd)";

        public String ModuleName
        {
            get { return "URLRewriteModule"; }
        }

        // In the Init function, register for HttpApplication 
        // events by adding your handlers.
        public void Init(HttpApplication application)
        {
            application.BeginRequest += (new EventHandler(Application_BeginRequest));
        }

        private void Application_BeginRequest(object sender, EventArgs e)
        {
            var app = (HttpApplication)sender;
            var req = app.Request;
            var absolutePath = req.Url.AbsolutePath;
            var hasCMS = Regex.IsMatch(absolutePath, CMSResourceRegEx, RegexOptions.IgnoreCase);

            if (!hasCMS && Regex.IsMatch(absolutePath, WebResourceRegEx, RegexOptions.IgnoreCase))
            {
                return;
            }

            // check if it's an actual file (WebForms default)
            if (File.Exists(req.PhysicalPath))
            {
                return;
            }

            // check for cms rewrite rules
            var requestedPath = VirtualPathUtility.ToAppRelative(absolutePath).Replace("'", "").Replace("\"", "");
            HandleCMSRewrite(app, requestedPath);
        }

        #region CMS
        private bool HandleRewriteUrlRewrite(HttpApplication app, string requestedPath)
        {
            // Start addition to disable this code path.
            string enableOldWebRedirectsString = WebConfigurationManager.AppSettings["EnableOldWebRedirects"];
            bool enableOldWebRedirects = false;

            int? fact = null;
            if (Boolean.TryParse(enableOldWebRedirectsString, out enableOldWebRedirects) && !enableOldWebRedirects)
            {
                return false;
            }
            // End addition to disable this code path.

            var rewriteUrl = DomainServiceLoanApplicationOperations.GetCachedMarketingData().GetRewriteUrl(requestedPath, out fact);

            if (rewriteUrl != null)
            {
                var urlHelper =  new UrlHelper(HttpContext.Current.Request.RequestContext);
                if (!urlHelper.IsLocalUrl(rewriteUrl))
                {
                    return false;
                }

                rewriteUrl = VirtualPathUtility.ToAbsolute(rewriteUrl);

                if (fact != null)
                {
                    rewriteUrl = $"{rewriteUrl}?fact={fact}";
                }

                if (app.Context.Request.Params["revision"] != null && new LightStreamWeb.ContentManager().IsCMSPreviewAllowed())
                {
                    rewriteUrl += $"{(rewriteUrl.Contains("?") ? "&" : "?")}{"revision=" + app.Context.Request.Params["revision"]}";
                }
                if (app.Context.Request.Params["id"] != null && new LightStreamWeb.ContentManager().IsCMSPreviewAllowed())
                {
                    rewriteUrl += $"{(rewriteUrl.Contains("?") ? "&" : "?")}{"id=" + app.Context.Request.Params["id"]}";
                }
                if (rewriteUrl.Equals(requestedPath.Substring(0, 1) == "~" ? requestedPath.Remove(0, 1) : requestedPath) || rewriteUrl.Equals(app.Request.RawUrl))
                {
                    return false;
                }
                
                app.Response.RedirectPermanent(rewriteUrl, true);
                
                return true;
            }

            return false;
        }

        private bool HandleCMSRewrite(HttpApplication app, string requestedPath)
        {
            // Start addition to disable this code path.
            string enableNewWebRedirectsString = WebConfigurationManager.AppSettings["EnableNewWebRedirects"];
            var webRedirectHelper = new WebRedirectHelper();
            bool enableNewWebRedirects = false;

            if (Boolean.TryParse(enableNewWebRedirectsString, out enableNewWebRedirects)
                && enableNewWebRedirects
                && webRedirectHelper.GetWebRedirectGroup(
                    new Uri(requestedPath, UriKind.RelativeOrAbsolute),
                    null, // We are only concerned about checking against the vanity here.
                    DomainServiceContentManagementOperations.GetCachedWebRedirectGroups()
                ) != null)
            {
                return false;
            }
            // End addition to disable this code path.


            if (HandleRewriteUrlRewrite(app, requestedPath))
            {
                return true;
            }

            if (requestedPath.Length < 3 || !requestedPath.StartsWith("~/"))
            {
                return false;
            }

            var webFileContentPath = string.Empty;
            var context = app.Context;

            if (GetCMSFilePath(app, requestedPath, "~/cms/", "content/cmsfile", out webFileContentPath))
            {
                context.RewritePath(webFileContentPath);
                return true;
            }

            var urlRewritePath = requestedPath.EndsWith("/") ? requestedPath.Substring(2, requestedPath.Length - 3) : requestedPath.Substring(2);

            if (urlRewritePath.StartsWith("r/"))
            {
                urlRewritePath = urlRewritePath.Substring(2);
            }

            var contentMap = DomainServiceContentManagementOperations.GetCachedPublishedWebContent();
            var content = contentMap[urlRewritePath];
            if (content == null)
            {
                string url = contentMap.CheckForCaseInsensitveMatch(urlRewritePath);
                if (url != "")
                {
                    var replace = context.Request.RawUrl.Replace(urlRewritePath, url);

                    var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
                    if (urlHelper.IsLocalUrl(replace))
                    {
                        app.Response.RedirectPermanent(replace);
                        return true;
                    }

                    return false;
                }
            }

            string revision = context.Request.Params["revision"];

            if (revision != null && new LightStreamWeb.ContentManager().IsCMSPreviewAllowed())
            {
                content = new ContentManager().GetById();
            }

            if (content != null)
            {
                var sb = new StringBuilder(context.Request.ApplicationPath);

                if (!context.Request.ApplicationPath.EndsWith("/"))
                {
                    sb.Append("/");
                }

                //if (isLandingPageFaq)
                //{
                //    sb.Append("Faq/LandingPage.aspx");
                //}
                //else
                //{
                // temp code for responsive + non-responsive previews
                if (!string.IsNullOrEmpty(content.CMSAttribute.SecondaryUrlPath) && content.CMSAttribute.SecondaryUrlPath.Equals(context.Request.Path, StringComparison.InvariantCultureIgnoreCase))
                {
                    sb.Append(content.CMSAttribute.SecondaryUrlPath.StartsWith("/") ? content.CMSAttribute.SecondaryUrlPath.Substring(1) : content.CMSAttribute.SecondaryUrlPath);
                }
                else
                {
                    sb.Append(content.CMSAttribute.UrlPath.StartsWith("/") ? content.CMSAttribute.UrlPath.Substring(1) : content.CMSAttribute.UrlPath);
                }
                //}

                sb.Append("?id=");
                sb.Append(content.WebContentId);
                if (revision != null)
                {
                    sb.Append("&revision=");
                    sb.Append(content.Revision);
                }
                // handle landing pages
                if (content.CMSAttribute.SecondaryUrlPath == "/r/LandingPage" || content.CMSAttribute.UrlPath == "/LandingPage")
                {
                    sb.Append("&urlRewritePath=" + urlRewritePath);
                }

                var parameters = context.Request.QueryString.AllKeys.Where(a => a != null && !a.Equals("id") && !a.Equals("revision")).ToList();
                foreach (string key in parameters)
                {
                    sb.Append("&");
                    sb.Append(key);
                    sb.Append("=");
                    sb.Append(HttpUtility.UrlEncode(context.Request.QueryString[key]));
                }

                context.RewritePath(sb.ToString(), true);
                return true;
            }
            else
            {
                var redirectKey = contentMap.GetRedirectKey(urlRewritePath);
                if (redirectKey != null)
                {
                    var redirect = context.Request.RawUrl.Replace(urlRewritePath, redirectKey);

                    var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);

                    if (urlHelper.IsLocalUrl(redirect))
                    {
                        app.Response.RedirectPermanent(redirect);
                        return true;
                    }

                    return false;
                }
            }

            return false;
        }

        private static bool GetCMSFilePath(HttpApplication application, string requestedPath,
            string appRelativeDir, string pagePath, out string webFileContentPath)
        {
            webFileContentPath = null;

            int webFileContentId = -1;
            if (requestedPath.StartsWith(appRelativeDir) && requestedPath.Length > appRelativeDir.Length)
            {
                string id = requestedPath.Substring(appRelativeDir.Length);

                int index = id.IndexOf('.');
                if (index > 0)
                {
                    id = id.Substring(0, index);
                }

                if (id.IndexOf("/") > -1)
                {
                    WebPageContentMap contentMap = DomainServiceContentManagementOperations.GetCachedPublishedWebContent();
                    FileContent fc = contentMap.GetFileContent(id);
                    if (fc == null)
                    {
                        return false;
                    }

                    webFileContentId = fc.WebFileContentId;
                }
                else if (!int.TryParse(id, out webFileContentId))
                {
                    return false;
                }

                StringBuilder sb = GetApplicationPath(application.Context);
                sb.Append(pagePath);
                sb.Append("?id=");
                sb.Append(webFileContentId);
                webFileContentPath = sb.ToString();

                return true;
            }
            else if (requestedPath.IndexOf("~/lightstream/media/images") > -1)//bypass images that should be coming from our 3rd party cms system
            {
                return false;
            }
            else
            {
                int index = requestedPath.IndexOf("/images/");
                if (index > -1)
                {
                    StringBuilder sb = GetApplicationPath(application.Context);
                    sb.Append("Assets/Images/");
                    sb.Append(requestedPath.Substring(index + 8));
                    webFileContentPath = sb.ToString();
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region UrlRewrite
        private static StringBuilder GetApplicationPath(HttpContext context)
        {
            StringBuilder sb = new StringBuilder(context.Request.ApplicationPath);
            if (sb[sb.Length - 1] != '/')
            {
                sb.Append("/");
            }

            return sb;
        }
        #endregion

        public void Dispose()
        {

        }
    }
}