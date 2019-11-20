using FirstAgain.Common.Logging;
using FirstAgain.Domain.Lookups.FirstLook;
using LightStreamWeb.ServerState;
using LightStreamWeb.App_State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LightStreamWeb.Filters
{
    /// <summary>
    /// Extends RequireApplicationIdAttribute to also require an app to be in a particular status
    /// </summary>
    public class RestrictToStatusAttribute : RequireApplicationIdAttribute
    {
        ICurrentUser _user = new CurrentUser();
        ApplicationStatusTypeLookup.ApplicationStatusType _requiredStatus;
        LoanTermsRequestStatusLookup.LoanTermsRequestStatus? _ltrStatus = null;

        public RestrictToStatusAttribute(ApplicationStatusTypeLookup.ApplicationStatusType requiredStatus)
        {
            _requiredStatus = requiredStatus;
        }
        public RestrictToStatusAttribute(ApplicationStatusTypeLookup.ApplicationStatusType requiredStatus, LoanTermsRequestStatusLookup.LoanTermsRequestStatus ltrStatus)
        {
            _requiredStatus = requiredStatus;
            _ltrStatus = ltrStatus;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (_user == null || _user.ApplicationId == null || _user.ApplicationId == 0)
            {
                DisplayAppIdRequiredError(filterContext);
            }
            else
            {
                var lads = SessionUtility.GetApplicationData(_user.ApplicationId);
                if (lads != null)
                {
                    // if the base app status matches, all is ok
                    if (lads.Application[0].ApplicationStatusType == _requiredStatus)
                    {
                        base.OnActionExecuting(filterContext);
                        return;
                    }

                    // or, if the LTR status matches, we're ok
                    if (_ltrStatus.HasValue)
                    {
                        FirstAgain.Domain.SharedTypes.LoanApplication.LoanOfferDataSet loanOfferDs = SessionUtility.GetLoanOfferDataSet();
                        if (loanOfferDs != null && loanOfferDs.LatestLoanTermsRequest != null && loanOfferDs.LatestLoanTermsRequest.Status == _ltrStatus.Value)
                        {
                            base.OnActionExecuting(filterContext);
                            return;
                        }
                    }

                    // if both of the above checks failed, redirect
                    filterContext.Controller.TempData["Alert"] = Resources.FAMessages.UnexpectedStatusError;
                    filterContext.Result = new RedirectResult("/customer-sign-in");

                    LightStreamLogger.WriteWarning(string.Format("Application Id {0} not in expected status of {1}", _user.ApplicationId, _requiredStatus));
                }

                base.OnActionExecuting(filterContext);
            }
        }
    }
}