using FirstAgain.Domain.ServiceModel.Contracts.MessageContracts.CustomerOperations;
using LightStreamWeb.ServerState;
using System;
using System.Web.Mvc;

namespace LightStreamWeb.Filters
{
    public class InjectAccountInfo : ActionFilterAttribute
    {
        public bool Refresh { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (MaintenanceConfiguration.IsInMaintenanceMode)
            {
                filterContext.Result = new RedirectResult("/customer-sign-in");
                return;
            }

            GetAccountInfoResponse accountInfo = SessionUtility.AccountInfo; 
            if (accountInfo == null || Refresh)
            {
                try
                {
                    accountInfo = SessionUtility.RefreshAccountInfo();
                }
                catch (InvalidOperationException) // session not initialized, most likely not authenticated
                {
                    filterContext.Result = new RedirectResult("/customer-sign-in");
                }
            }
            filterContext.ActionParameters["accountInfo"] = accountInfo;

            base.OnActionExecuting(filterContext);
        }

    }
}