using LightStreamWeb.ServerState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LightStreamWeb.Filters
{
    public class InjectCustomerApplicationsDates : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (MaintenanceConfiguration.IsInMaintenanceMode)
            {
                filterContext.Result = new RedirectResult("/customer-sign-in");
                return;
            }

            if (SessionUtility.AccountInfo != null)
            {
                filterContext.ActionParameters["applicationsDates"] = SessionUtility.AccountInfo.ApplicationsDates;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}