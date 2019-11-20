using LightStreamWeb.ServerState;
using FirstAgain.Common.Extensions;
using System;
using System.Linq;
using System.Web.Mvc;
using FirstAgain.Common.Web;
using FirstAgain.Common.Logging;
using LightStreamWeb.App_State;
using FirstAgain.Domain.ServiceModel.Client;
using LightStreamWeb.Helpers;
using FirstAgain.Domain.Lookups.FirstLook;

namespace LightStreamWeb.Filters
{
    public class InjectCurrentApplicationData : ActionFilterAttribute
    {
        #region properties
        public bool Refresh { get; set; }
        public ApplicationStatusTypeLookup.ApplicationStatusType[] ExpectedStatus { get; set; }
        #endregion

        #region constructors
        public InjectCurrentApplicationData()
        {
        }
        public InjectCurrentApplicationData(bool refresh)
        {
            this.Refresh = refresh;
        }
        public InjectCurrentApplicationData(bool refresh, ApplicationStatusTypeLookup.ApplicationStatusType expectedStatus)
        {
            this.Refresh = refresh;
            this.ExpectedStatus = new[] { expectedStatus };
        }
        public InjectCurrentApplicationData(ApplicationStatusTypeLookup.ApplicationStatusType expectedStatus)
        {
            this.ExpectedStatus = new[] { expectedStatus };
        }

        public InjectCurrentApplicationData(bool Refresh, ApplicationStatusTypeLookup.ApplicationStatusType[] expectedStatuses)
        {
            this.Refresh = Refresh;
            this.ExpectedStatus = expectedStatuses;
        }
        #endregion

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // check for maintenance mode
            if (MaintenanceConfiguration.IsInMaintenanceMode)
            {
                filterContext.Result = new RedirectResult("/customer-sign-in");
                return;
            }

            // grab the data
            ICurrentApplicationData appData = null;
            int? applicationId = GetApplicationId(filterContext);

            if (applicationId.HasValue)
            {
                appData = (!Refresh) ? null : SessionUtility.GetCurrentApplicationData(applicationId.Value);
                if (appData == null)
                {
                    try
                    {
                        // TODO: change this to a back end method that returns everything, instead of making two calls.
                        // ensure this is on the new WCF service client
                        var accountInfo = SessionUtility.AccountInfo ?? DomainServiceCustomerOperations.GetAccountInfoByCustomerUserId(filterContext.HttpContext.User.Identity.Name);
                        if (accountInfo == null || accountInfo.CustomerUserIdDataSet == null || !accountInfo.CustomerUserIdDataSet.Application.Any(a => a.ApplicationId == applicationId.Value))
                        {
                            throw new InvalidOperationException("Account info or application id not found");
                        }

                        var loanOfferDataSet = SessionUtility.GetLoanOfferDataSet() ?? DomainServiceLoanApplicationOperations.GetLoanOffer(applicationId.Value);

                        appData = DataSetToSessionStateMapper.Map(applicationId.Value, accountInfo, loanOfferDataSet);

                        SessionUtility.SetCurrentApplicationData(applicationId.Value, appData);

                        // check for an unexpected status
                        if (ExpectedStatus != null && ExpectedStatus.Length > 0)
                        {
                            var user = new App_State.CurrentUser();
                            if (user.ApplicationId.HasValue)
                            {
                                if (!ExpectedStatus.Contains(appData.ApplicationStatus))
                                {
                                    filterContext.Controller.TempData["Alert"] = Resources.FAMessages.UnexpectedStatusError;
                                    filterContext.Result = new RedirectResult("/customer-sign-in");

                                    LightStreamLogger.WriteWarning($"Application id {user.ApplicationId} not in expected status {ExpectedStatus.ExtToString()}");
                                }
                            }
                        }
                    }
                    catch (InvalidOperationException) // session not initialized, most likely not authenticated
                    {
                        filterContext.Result = new RedirectResult("/customer-sign-in");
                    }
                }
            }

            if (appData == null)
            {
                filterContext.Result = new RedirectResult("/customer-sign-in");
            }
            else
            {
                filterContext.ActionParameters["applicationData"] = appData;
            }

            base.OnActionExecuting(filterContext);

        }

        private static int? GetApplicationId(ActionExecutingContext filterContext)
        {
            int? applicationId = null;

            var ctx = filterContext.RequestContext.HttpContext.Request.Params["ctx"];
            if (ctx.IsNotNull())
            {
                try
                {
                    applicationId = WebSecurityUtility.Descramble(ctx);
                }
                catch (Exception ex)
                {
                    LightStreamLogger.WriteWarning("Error decrypting application id ctx param + " + ctx + ", " + ex.Message);
                }
            }
            if (!applicationId.HasValue)
            {
                applicationId = new CurrentUser().ApplicationId;
            }

            return applicationId;
        }
    }

}