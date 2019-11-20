using FirstAgain.Domain.Lookups.FirstLook;
using LightStreamWeb.App_State;
using LightStreamWeb.Controllers.Shared;
using LightStreamWeb.Models.Rates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using Ninject;
using LightStreamWeb.Models.PublicSite;
using FirstAgain.Common.Logging;
using LightStreamWeb.Models.Middleware;
using static FirstAgain.Domain.SharedTypes.LoanApplication.LoanApplicationDataSet;

namespace LightStreamWeb.Controllers
{
    [SessionState(SessionStateBehavior.Disabled)]
    public class RatesController : BaseController
    {
        private IAppSettings Settings;

        [Inject]
        public RatesController(ICurrentUser user, IAppSettings settings) : base(user)
        {
            Settings = settings;
        }

        //
        // GET: /Rates
        // GET: /Rates?purposeOfLoan=1
        // GET: /Rates?purposeOfLoan=HomeImprovement
        // GET: /Rates/HomeImprovement
        public ActionResult Index(string PurposeOfLoan, string id)
        {
            var model = new RatesPageModel(new ContentManager(), Settings.PageDefault.Rates)
            {
                Canonical = "https://www.lightstream.com/rates-loan-calculator",
                Title = "LightStream Loan Rate Calculator",
                FirstAgainCodeTrackingId = WebUser.FirstAgainCodeTrackingId,
                PurposeOfLoan = ParsePurposeOfLoan(PurposeOfLoan, id)
            };

            return View(model);
        }

        // GET: /Rates/Widget
        public ActionResult Widget(int? fact, string purposeOfLoan, bool? suppressStyles = false)
        {
            var model = new RatesPageModel();
            model.FirstAgainCodeTrackingId = fact.GetValueOrDefault(1);
            model.Calculator.PurposeOfLoan = ParsePurposeOfLoan(purposeOfLoan);
            if (fact.GetValueOrDefault() == KnownFACTs.SUNTRUST_AUTO_FINANCING) 
            {
                model.Calculator.TypeOfCalculator = FirstAgain.Domain.SharedTypes.ContentManagement.LandingPageContent.RateTableCalculatorType.Auto;
            }

            ViewBag.SuppressStyles = suppressStyles.GetValueOrDefault();

            WebUser.SetFACT(fact);

            return View(model);
        }

        private PurposeOfLoanLookup.PurposeOfLoan ParsePurposeOfLoan(string purposeOfLoan, string id = null)
        {
            try
            {
                if (string.IsNullOrEmpty(purposeOfLoan) && !string.IsNullOrEmpty(id))
                {
                    purposeOfLoan = id;
                }
                if (!string.IsNullOrEmpty(purposeOfLoan))
                {
                    // old links would use "PurposeOfLoan=3"
                    int numericId;
                    if (int.TryParse(purposeOfLoan, out numericId))
                    {
                        return Enum.IsDefined(typeof(PurposeOfLoanLookup.PurposeOfLoan), (short)numericId) ? (PurposeOfLoanLookup.PurposeOfLoan)numericId : PurposeOfLoanLookup.PurposeOfLoan.NotSelected;
                    }
                    else
                    {
                        return PurposeOfLoanLookup.GetEnumeration(purposeOfLoan);
                    }
                }
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
            }

            return PurposeOfLoanLookup.PurposeOfLoan.NotSelected;
        }

    }
}