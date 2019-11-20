using LightStreamWeb.ServerState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LightStreamWeb.Filters
{
    public class InjectLoanOfferDataSet : ActionFilterAttribute
    {
        public bool Refresh { get; set; }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (MaintenanceConfiguration.IsInMaintenanceMode)
            {
                filterContext.Result = new RedirectResult("/customer-sign-in");
                return;
            }

            FirstAgain.Domain.SharedTypes.LoanApplication.LoanOfferDataSet loanOfferDataSet = null;
            if (!Refresh)
            {
                loanOfferDataSet = SessionUtility.GetLoanOfferDataSet();
            }

            if (loanOfferDataSet == null)
            {
                var user = new App_State.CurrentUser();
                if (user.ApplicationId.HasValue)
                {
                    loanOfferDataSet = FirstAgain.Domain.ServiceModel.Client.DomainServiceLoanApplicationOperations.GetLoanOffer(user.ApplicationId.Value);
                    SessionUtility.SetLoanOfferDataSet(loanOfferDataSet);
                }

            }
            filterContext.ActionParameters["loanOfferDataSet"] = loanOfferDataSet;
            base.OnActionExecuting(filterContext);
        }

    }
}