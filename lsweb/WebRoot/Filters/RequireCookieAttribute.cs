using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace LightStreamWeb.Filters
{
    // redirects to a URL if a cookie is not present
    public class RequireCookieAttribute : ActionFilterAttribute, IActionFilter
    {
        private string _cookieName;
        private string _redirectTo;

        public RequireCookieAttribute(string cookieName, string redirectTo)
        {
            _redirectTo = redirectTo;
            _cookieName = cookieName;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Request.Cookies[_cookieName] == null)
            {
                filterContext.Result = new RedirectResult(_redirectTo);
            }
            else
            {
                base.OnActionExecuting(filterContext);
            }
        }
    }
}