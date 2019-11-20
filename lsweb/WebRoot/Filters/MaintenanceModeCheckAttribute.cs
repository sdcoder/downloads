using LightStreamWeb.Models.SignIn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace LightStreamWeb.Filters
{
    public class MaintenanceModeCheckAttribute : ActionFilterAttribute, IActionFilter
    {
        public MaintenanceModeCheckAttribute()
        {
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // if Maintenance flag is set, redirect to maintenance message page
            if (SignInModel.IsInMaintenanceMode())
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "action", "Maintenance" }, { "controller", "SignIn" } });
            }
            // if maintenance mode is scheduled, display that messages
            else if (MaintenanceConfiguration.ScheduledMaintenanceStartTime.HasValue && MaintenanceConfiguration.ScheduledMaintenanceMessage != null)
            {
                filterContext.Controller.TempData["Alert"] = string.Format(MaintenanceConfiguration.ScheduledMaintenanceMessage, MaintenanceConfiguration.ScheduledMaintenanceStartTime);
            }
        }
    }
}