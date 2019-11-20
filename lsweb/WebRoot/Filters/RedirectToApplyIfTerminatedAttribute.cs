using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using LightStreamWeb.App_State;
using LightStreamWeb.ServerState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LightStreamWeb.Filters
{
    public class RedirectToApplyIfTerminatedAttribute : ActionFilterAttribute
    {
        public bool Refresh { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            LoanApplicationDataSet lad;

            if (Refresh)
            {
                lad = SessionUtility.ReloadApplicationData(new CurrentUser().ApplicationId.GetValueOrDefault()); 
                filterContext.ActionParameters["lads"] = lad;
            }
            else
            {
                lad = SessionUtility.GetApplicationData(new CurrentUser().ApplicationId);
                filterContext.ActionParameters["lads"] = lad;
            }

            if(lad?.ApplicationStatus == ApplicationStatusTypeLookup.ApplicationStatusType.Terminated)
            {
                filterContext.Result = new RedirectResult("/apply");
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}