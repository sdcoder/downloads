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

namespace LightStreamWeb.Controllers.Services
{
    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    public class RateCalculatorController : BaseController
    {
        [Inject]
        public RateCalculatorController(ICurrentUser user) : base(user)
        {
        }

        // for the bots
        // GET: /RateCalculator/GetRateTable
        [HttpGet]
        public ActionResult GetRateTable()
        {
            return new HttpNotFoundResult();
        }

        //
        // POST: /RateCalculator/GetRateTable
        // POST: /services/rates
        [HttpPost]
        public ActionResult GetRateTable(RateTableModelWrapper model)
        {
            try
            {
                if (model != null)
                {
                    if (model.FirstAgainCodeTrackingId == null)
                    {
                        model.FirstAgainCodeTrackingId = this.WebUser.FirstAgainCodeTrackingId;
                    }
                    model.Populate();
                }
            }
            catch (Exception ex)
            {
                FirstAgain.Common.ExceptionUtility.AddObjectStateToExceptionData(ex, model);
                LightStreamLogger.WriteError(ex);
            }
            return new JsonNetResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = model,
            };
        }

        // POST / GET: /Services/Widget
        [OutputCache(Duration = 10, VaryByParam = "LoanAmount;LoanTermMonths;PaymentMethod;PurposeOfLoan;")]
        public ActionResult Widget(WidgetModel model)
        {
            if (model == null)
            {
                return new EmptyResult();
            }

            try
            {
                var rates = new RateTableModelWrapper();
                rates.PurposeOfLoan = model.PurposeOfLoan;
                rates.LoanAmount = model.LoanAmount;
                rates.LoanTermMonths = model.LoanTermMonths;
                rates.PaymentMethod = model.PaymentMethod;
                rates.Populate();

                model.Rate = rates.Rate;
                model.EstimatedMonthlyPayment = rates.EstimatedMonthlyPayment;
                model.LoanAmounts = new FirstAgain.Common.DecimalRange()
                {
                    Min = rates.MinLoanAmount,
                    Max = rates.MaxLoanAmount
                };
                return new JsonNetResult()
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = model,
                };
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
                return new EmptyResult();
            }
        }
    }
}
