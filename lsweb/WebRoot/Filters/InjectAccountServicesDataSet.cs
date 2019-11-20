using FirstAgain.LoanServicing.SharedTypes;
using LightStreamWeb.ServerState;
using System.Web.Mvc;

namespace LightStreamWeb.Filters
{
    public class InjectAccountServicesDataSet : ActionFilterAttribute
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
                SessionUtility.SetUpAccountServicesData();
            }

            filterContext.ActionParameters["accountServicesData"] = SessionUtility.AccountServicesData;
            filterContext.ActionParameters["businessCalendar"] = SessionUtility.BusinessCalendar;

            base.OnActionExecuting(filterContext);
        }
    }
}