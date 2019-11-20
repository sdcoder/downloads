using FirstAgain.Common.Logging;
using LightStreamWeb.Models;
using LightStreamWeb.Models.PublicSite;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace LightStreamWeb.Controllers
{
    [SessionState(System.Web.SessionState.SessionStateBehavior.Disabled)]
    public class ErrorController : Controller
    {
        //
        // GET: /Error/General
        public ActionResult General()
        {
            Response.StatusCode = 500;
            return View("~/Views/PublicSite/GeneralError.cshtml", new BasePublicSiteModel());
        }

        //
        // GET: /Error/NotFound
        public ActionResult NotFound()
        {
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            Response.TrySkipIisCustomErrors = true;
            return View("~/Views/PublicSite/SiteMap.cshtml", new SiteMapPageModel()
            {
                AlertHeading = "Page Not Found",
                AlertMessage = "We're sorry, but the page you have requested is not available. Please use the navigation below to find what you are looking for."
            });
        }

        [AllowAnonymous]
        public ActionResult ServerError()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            Response.TrySkipIisCustomErrors = true;
            return View("~/Views/PublicSite/SiteMap.cshtml", new SiteMapPageModel()
            {
                AlertHeading = "Page Not Found",
                AlertMessage = "We're sorry, but the page you have requested is not available. Please use the navigation below to find what you are looking for."
            });
        }

        // POST /Error/LogJSError
        [HttpPost]
        public ActionResult LogJSError(string msg, string url, string line)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Client-Side JS Error:");
            sb.AppendLine("msg:" + msg);
            sb.AppendLine("url:" + url);
            sb.AppendLine("line:" + line);
            sb.AppendLine("User Agent: " + Request.ServerVariables["HTTP_USER_AGENT"]);
            sb.AppendLine("Referrer: " + Request.UrlReferrer);
            LightStreamLogger.WriteWarning(sb.ToString());
            return new EmptyResult();
        }

        // POST /Error/LogAngularError
#pragma warning disable SCS0017 // Request validation is disabled
        [HttpPost, ValidateInput(false)]
#pragma warning restore SCS0017 // Request validation is disabled
        public ActionResult LogAngularError(string exception, string cause)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("AngularJS Error:");
            sb.AppendLine("exception:" + exception);
            sb.AppendLine("cause:" + cause);
            sb.AppendLine("User Agent: " + Request.ServerVariables["HTTP_USER_AGENT"]);
            sb.AppendLine("Referrer: " + Request.UrlReferrer);
            LightStreamLogger.WriteWarning(sb.ToString());
            return new EmptyResult();
        }

    }
}
