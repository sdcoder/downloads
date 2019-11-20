using LightStreamWeb.App_State;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LightStreamWeb.Filters
{
    /// <summary>
    /// Redirects the user to the login page if an application id is not set on the current user (session or cookie)
    /// </summary>
    public class RequireApplicationIdAttribute : ActionFilterAttribute, IActionFilter
    {
        ICurrentUser _user = new CurrentUser();
        ResultType _resultType;

        public enum ResultType
        {
            Redirect = 0,
            JSON = 1
        }
        public RequireApplicationIdAttribute(ResultType ResultType = ResultType.Redirect)
        {
            _resultType = ResultType;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (_user == null || _user.ApplicationId == null || _user.ApplicationId == 0)
            {
                DisplayAppIdRequiredError(filterContext);
            }
            else
            {
                base.OnActionExecuting(filterContext);
            }
        }

        protected void DisplayAppIdRequiredError(ActionExecutingContext filterContext)
        {
            if (_resultType == ResultType.Redirect)
            {
                filterContext.Controller.TempData["Alert"] = "A current application is required to access that page";
                filterContext.Result = new RedirectResult("/customer-sign-in");
            }
            else
            {
                filterContext.Result = new HttpStatusCodeResult(401);
            }
        }

    }
}