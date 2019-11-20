using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.SharedTypes.Customer;
using LightStreamWeb.ServerState;
using System.Linq;
using System.Web.Mvc;
using FirstAgain.Common.Logging;
using FirstAgain.Common.Extensions;

namespace LightStreamWeb.Filters
{
    public class InjectCustomerUserIdDataSet : ActionFilterAttribute
    {
        public InjectCustomerUserIdDataSet()
        {
        }
        public InjectCustomerUserIdDataSet(bool Refresh)
        {
            this.Refresh = Refresh;
        }
        public InjectCustomerUserIdDataSet(bool Refresh, ApplicationStatusTypeLookup.ApplicationStatusType ExpectedStatus)
        {
            this.Refresh = Refresh;
            this.ExpectedStatus = new[] { ExpectedStatus };
        }
        public InjectCustomerUserIdDataSet(bool Refresh, ApplicationStatusTypeLookup.ApplicationStatusType[] ExpectedStatus)
        {
            this.Refresh = Refresh;
            this.ExpectedStatus = ExpectedStatus;
        }
        public bool Refresh { get; set; }
        public ApplicationStatusTypeLookup.ApplicationStatusType[] ExpectedStatus { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (MaintenanceConfiguration.IsInMaintenanceMode)
            {
                filterContext.Result = new RedirectResult("/customer-sign-in");
                return;
            }

            CustomerUserIdDataSet customerData = null;
            if (!Refresh)
            {
                if (SessionUtility.AccountInfo != null)
                {
                    customerData = SessionUtility.AccountInfo.CustomerUserIdDataSet;
                }
            }

            if (customerData == null)
            {
                var user = new App_State.CurrentUser();
                if (user.ApplicationId.HasValue)
                {
                    var accountInfo = FirstAgain.Domain.ServiceModel.Client.DomainServiceCustomerOperations.GetAccountInfoByApplicationId(user.ApplicationId.Value);
                    if (accountInfo != null)
                    {
                        SessionUtility.AccountInfo = accountInfo;
                        customerData = accountInfo.CustomerUserIdDataSet;
                    }
                }
            }

            if (ExpectedStatus != null && ExpectedStatus.Length > 0)
            {
                var user = new App_State.CurrentUser();
                if (user.ApplicationId.HasValue)
                {
                    var app = customerData.Application.FirstOrDefault(a => a.ApplicationId == user.ApplicationId);
                    if (app == null || !ExpectedStatus.Contains(app.ApplicationStatusType))
                    {
                        filterContext.Controller.TempData["Alert"] = Resources.FAMessages.UnexpectedStatusError;
                        filterContext.Result = new RedirectResult("/customer-sign-in");

                        LightStreamLogger.WriteWarning($"Application id {user.ApplicationId} not in expected status {ExpectedStatus.ExtToString()}");
                    }
                }
            }
            filterContext.ActionParameters["customerData"] = customerData;
            
            base.OnActionExecuting(filterContext);
        }
    }
}