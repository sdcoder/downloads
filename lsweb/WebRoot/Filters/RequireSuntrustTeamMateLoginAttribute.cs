using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LightStreamWeb.Filters
{
    public class RequireSuntrustTeamMateLoginAttribute : ActionFilterAttribute, IActionFilter
    {
        public const string COOKIE_NAME = "SuntrustTeamMate_v2";
        public const string REDIRECT_SESSION_KEY = "SuntrustTeamMateRedirect";

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string redirect = GetRedirect(filterContext);

            if (filterContext.HttpContext.Request.Cookies[COOKIE_NAME] == null)
            {
                filterContext.HttpContext.Session[REDIRECT_SESSION_KEY] = redirect;
                filterContext.HttpContext.Response.SetCookie(new HttpCookie(REDIRECT_SESSION_KEY)
                {
                    Expires = DateTime.Now.AddYears(100),
                    Value = redirect
                });
                filterContext.Result = new RedirectResult("/SuntrustTeamMate");
            }
            else
            {
                base.OnActionExecuting(filterContext);
            }
        }

        // overly complex code to handle veracode "Improper Neutralization of CRLF Sequences in HTTP Headers ('HTTP Response Splitting')"
        private static string GetRedirect(ActionExecutingContext filterContext)
        {
            string redirect = "/SuntrustTeamMate";
            string requestedUrl = filterContext.HttpContext.Request.Url.AbsolutePath;
            if (requestedUrl.StartsWith("/branch", StringComparison.InvariantCultureIgnoreCase))
            {
                redirect = "/branch";
            }
            else if (requestedUrl.StartsWith("/chanop", StringComparison.InvariantCultureIgnoreCase))
            {
                redirect = "/chanop";
            }
            else if (requestedUrl.StartsWith("/premierbanking", StringComparison.InvariantCultureIgnoreCase))
            {
                redirect = "/premierbanking";
            }
            else if (requestedUrl.StartsWith("/pwm", StringComparison.InvariantCultureIgnoreCase))
            {
                redirect = "/pwm";
            }

            return redirect;
        }
    }
}