using LightStreamWeb.ServerState;
using LightStreamWeb.App_State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LightStreamWeb.Filters
{
    public class InjectLoanApplicationDataSetAttribute : ActionFilterAttribute
    {
        public bool Refresh { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (MaintenanceConfiguration.IsInMaintenanceMode)
            {
                filterContext.Result = new RedirectResult("/customer-sign-in");
                return;
            }

            if (Refresh)
            {
                filterContext.ActionParameters["lads"] = SessionUtility.ReloadApplicationData(new CurrentUser().ApplicationId.GetValueOrDefault());
            }
            else
            {
                filterContext.ActionParameters["lads"] = SessionUtility.GetApplicationData(new CurrentUser().ApplicationId);
            }
            base.OnActionExecuting(filterContext);
        }
    }
}