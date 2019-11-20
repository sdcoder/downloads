using FirstAgain.Domain.Lookups.FirstLook;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.SessionState;
using FirstAgain.Common;
using LightStreamWeb.Shared.Helpers;

namespace LightStreamWeb.Controllers.Services
{

    [SessionState(SessionStateBehavior.Disabled)]
    public class LookupsController : Controller
    {
        //
        // GET: /Lookups/PurposeOfLoan
        public ActionResult PurposeOfLoan(PurposeOfLoanLookup.PurposeOfLoan? id)
        {
            return new JsonResult()
            {
                Data = new
                {
                    @Result = PurposeOfLoanHelper.GetCaption(id.GetValueOrDefault())
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        //
        // GET: /Lookups/State
        public ActionResult State(StateLookup.State? id)
        {
            return new JsonResult()
            {
                Data = new
                {
                    @Result = StateLookup.GetCaption(id.GetValueOrDefault())
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        // GET: /Lookups/StaticLookups
        public ActionResult StaticLookups()
        {
            var result = new
            {
                countries = JsonUtility.ConvertLookupToJsonFriendlyObject(OFACCountryLookup.BindingSource),
                otherIncomeSources = JsonUtility.ConvertLookupToJsonFriendlyObject(OtherIncomeTypeLookup.BindingSource)
            };

            return new JsonResult()
            {
                Data = new
                {
                    @Result = result
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        // GET: /Services/Lookups/LoanInterestLookups
        public ActionResult LoanInterestLookups()
        {
            var filteredLookups = PurposeOfLoanLookup.GetFilteredBindingSource(PurposeOfLoanLookup.FilterType.Public)
                                                     .Where(lp =>
                                                     {
                                                         var purpose = PurposeOfLoanLookup.GetEnumerationFromCaption(lp.Caption);
                                                         return purpose.IsAuto() 
                                                            || purpose == PurposeOfLoanLookup.PurposeOfLoan.MedicalExpense
                                                            || purpose == PurposeOfLoanLookup.PurposeOfLoan.HomeImprovement
                                                            || purpose == PurposeOfLoanLookup.PurposeOfLoan.CreditCardConsolidation
                                                            || purpose == PurposeOfLoanLookup.PurposeOfLoan.Other;
                                                     }).ToList();

            var returnItems = new List<object>();

            filteredLookups.ForEach(lp =>
            {
                string label;

                switch (lp.Enumeration)
                {
                    case PurposeOfLoanLookup.PurposeOfLoan.MedicalExpense:
                        label = "Fertility/Medical";
                        break;
                    case PurposeOfLoanLookup.PurposeOfLoan.HomeImprovement:
                        label = "Home Improvement";
                        break;
                    case PurposeOfLoanLookup.PurposeOfLoan.CreditCardConsolidation:
                        label = "Debt Consolidation";
                        break;
                    case PurposeOfLoanLookup.PurposeOfLoan.Other:
                        label = "Other";
                        break;
                    default:
                        label = lp.Caption;
                        break;
                };

                returnItems.Add(new
                {
                    caption = lp.Caption,
                    id = (int)PurposeOfLoanLookup.GetEnumerationFromCaption(lp.Caption),
                    label = label
                });
            });

            return new JsonResult()
            {
                Data = new
                {
                    @Result = new { LoanInterestTypes = returnItems }
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
    }
}
