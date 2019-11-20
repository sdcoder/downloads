using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LightStreamWeb.Filters
{
    public class RedirectIfNotLoggedInAttribute : ActionFilterAttribute
    {
        private string _redirectTo;
        public RedirectIfNotLoggedInAttribute(string url)
        {
            _redirectTo = url;
        }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new RedirectResult(_redirectTo);
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}