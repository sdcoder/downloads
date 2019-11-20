using FirstAgain.Common.Logging;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Client;
using LightStreamWeb.App_State;
using LightStreamWeb.Controllers.Shared;
using LightStreamWeb.Filters;
using Ninject;
using System;
using System.Web.Mvc;
using BusinessCalendarClient = FirstAgain.Domain.ServiceModel.Client.BusinessCalendar;

namespace LightStreamWeb.Controllers
{
    public class BankingInformationController : BaseController
    {
        [Inject]
        public BankingInformationController(ICurrentUser user) : base(user)
        {
        }

        //
        // GET: /BankingInformation/?routingNumber
        [RequireApplicationId(RequireApplicationIdAttribute.ResultType.JSON)]
        public ActionResult Index(string routingNumber)
        {
            if (string.IsNullOrEmpty(routingNumber))
            {
                return HttpNotFound();
            }

            try
            {
                var bankInfo = DomainServiceLoanApplicationOperations.GetBankingInstitution(
                                    routingNumber,
                                    BankAccountActionTypeLookup.BankAccountActionType.Credit,
                                    WebUser.ApplicationId.Value);
                return new JsonResult()
                {
                    Data = bankInfo ?? new object(), 
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
                return HttpNotFound();
            }
        }

        // GET or POST /Services/BankingInformation/GetFirstPaymentDate?dayOfMonth=1&fundingDate=01/01/2014
        public ActionResult GetFirstPaymentDate(int? dayOfMonth, string fundingDate)
        {
            try
            {
                DateTime dtFundingDate;
                if (dayOfMonth.HasValue && DateTime.TryParse(fundingDate, out dtFundingDate))
                {
                    DateTime firstPaymentBusinessDate;
                    BusinessCalendarClient.GetFirstPaymentDate(dtFundingDate, dayOfMonth.Value, out firstPaymentBusinessDate);

                    return new JsonNetResult
                    {
                        Data = new
                        {
                            Success = true,
                            FirstPaymentDate = firstPaymentBusinessDate.ToString("dddd, MMMM d, yyyy")
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
            }

            return new JsonNetResult
            {
                Data = new {
                    Success = false
                }, 
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
    }
}
