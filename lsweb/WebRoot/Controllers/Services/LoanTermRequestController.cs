using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.SharedTypes.InterestRate;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Common.Caching;
using FirstAgain.Common.Logging;
using LightStreamWeb.App_State;
using LightStreamWeb.Controllers.Shared;
using LightStreamWeb.Filters;
using LightStreamWeb.Models.Rates;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FirstAgain.Domain.SharedTypes.Customer;
using LightStreamWeb.Models.LoanTermRequest;
using LightStreamWeb.Models.Shared;

namespace LightStreamWeb.Controllers.Services
{
    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    public class LoanTermRequestController : BaseController
    {
        [Inject]
        public LoanTermRequestController(ICurrentUser user) : base(user)
        {
        }

        //
        // POST: services/loanTermRequest/GetLatestLoanTermRequest/{id}
        [HttpGet]
        [InjectCustomerUserIdDataSet(Refresh = true)]
        public ActionResult GetLatestLoanTermRequest(CustomerUserIdDataSet customerData)
        {
            try
            {
                var applicationid = customerData.Application.Select(x => x.ApplicationId).FirstOrDefault();

                var counter = customerData.LoanTermsRequest.GetCounterLoanTerms(applicationid);
                var result = new LoanTermRequestModel();

                if (counter != null)
                {
                    result.MaxAmount = counter.Amount;
                    result.TermMonths = counter.TermMonths;
                }

                return new JsonNetResult()
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = result,
                };
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
                throw ex;
            }
        }

    }
}
