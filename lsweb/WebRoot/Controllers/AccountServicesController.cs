using FirstAgain.Common.Logging;
using FirstAgain.Common.Web;
using FirstAgain.Correspondence.ServiceModel.Client;
using FirstAgain.Domain.ServiceModel.Contracts.MessageContracts.CustomerOperations;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.LoanServicing.SharedTypes;
using LightStreamWeb.App_State;
using LightStreamWeb.Controllers.Shared;
using LightStreamWeb.Filters;
using LightStreamWeb.Models.AccountServices;
using LightStreamWeb.Models.PublicSite;
using LightStreamWeb.Models.Shared;
using LightStreamWeb.ServerState;
using Microsoft.Security.Application;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LightStreamWeb.Controllers
{
    [Authorize]
    [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
    public class AccountServicesController : BaseController
    {
        [Inject]
        public AccountServicesController(ICurrentUser user) : base(user) { }

        #region pages

        //
        // GET: /Account/Index
        [InjectAccountInfo]
        [InjectAccountServicesDataSet]
        public ActionResult Index(GetAccountInfoResponse accountInfo, AccountServicesDataSet accountServicesData)
        {
            // Once logged into account services override any FACT code still lingering in the cookies
            WebUser.SetFACT(FirstAgain.Domain.SharedTypes.LoanApplication.LoanApplicationDataSet.KnownFACTs.LIGHTSTREAM_NATIVETRAFFIC_ACCOUNTSERVICES);
            return View("~/Views/AccountServices/Index.cshtml", new AccountServicesHomePageModel(accountInfo, accountServicesData));
        }

        //
        // GET: /Account/ContactUs
        [InjectAccountInfo]
        public ActionResult ContactUs(GetAccountInfoResponse accountInfo)
        {
            return View("~/Views/AccountServices/ContactUs.cshtml", new AccountServicesPageModel(accountInfo)
            {
                Heading = "Contact Us"
            });
        }

        //
        // GET: /AccountServices/Rates
        [InjectCustomerUserIdDataSet]
        public ActionResult Rates(CustomerUserIdDataSet customerData)
        {
            var model = new RatesPageModel();
            if (customerData != null && customerData.ApplicationFlag.Any(af => af.Flag == FirstAgain.Domain.Lookups.FirstLook.FlagLookup.Flag.SuntrustPrivateWealth && af.FlagIsOn))
            {
                model.Calculator.Discount = FirstAgain.Domain.Lookups.FirstLook.FlagLookup.Flag.SuntrustPrivateWealth;
            }
            return View("~/Views/AccountServices/Rates.cshtml", model);
        }


        //
        // GET: /Account/PrivacySecurity
        public ActionResult PrivacySecurity()
        {
            return View(new PrivacySecurityModel()
                {
                    BodyClass = "privacy"
                });
        }

        // 
        // GET: /Account/DisplayExtraPaymentInvoice
        [InjectCustomerUserIdDataSet]
        public ActionResult DisplayExtraPaymentInvoice(CustomerUserIdDataSet customerData, string Ctx, decimal? Amount)
        {
            if (!Amount.HasValue)
            {
                return new ContentResult()
                {
                    Content = "Amount is required"
                };
            }

            var model = new AccountServicesEDocPageModel();
            model.GetExtraPaymentInvoice(System.Web.Security.AntiXss.AntiXssEncoder.HtmlEncode(Ctx, false), Amount.Value, customerData);
            return new ContentResult()
            {
                Content = Sanitizer.GetSafeHtml(model.Html)
            };
        }

        // 
        // GET: /Account/DisplayPayOffInvoice
        [InjectCustomerUserIdDataSet]
        public ActionResult DisplayPayOffInvoice(CustomerUserIdDataSet customerData, string Ctx, string SelectedDate)
        {
            DateTime date;
            if (!DateTime.TryParse(SelectedDate, out date))
            {
                return new ContentResult()
                {
                    Content = "Date is required"
                };
            }

            var model = new AccountServicesEDocPageModel();
            model.DisplayPayOffInvoice(Ctx, date, customerData);
            return new ContentResult()
            {
                Content = Sanitizer.GetSafeHtml(model.Html)
            };
        }

        //
        // POST: /Account/PrintAmortizationSchedule
        [HttpPost]
#pragma warning disable SCS0017 // Request validation is disabled
        [ValidateInput(false)]
#pragma warning restore SCS0017 // Request validation is disabled
        public ActionResult PrintAmortizationSchedule(AmortizationScheduleModel model)
        {
            if (model == null)
            {
                return new HttpStatusCodeResult(400);
            }
            if (model.Format == "pdf")
            {
                ViewData.Model = model;
                using (var sw = new System.IO.StringWriter())
                {
                    var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, "PrintAmortizationSchedule");
                    var viewContext = new ViewContext(ControllerContext, viewResult.View,
                                                 ViewData, TempData, sw);
                    viewResult.View.Render(viewContext, sw);
                    viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                    var html = sw.GetStringBuilder().ToString();
                    var bytes = CorrespondenceServiceCorrespondenceOperations.ConvertHtmlToPdf(html);
                    return new ContentResult()
                    {
                        Content = Convert.ToBase64String(bytes)
                    };
                }
            }

            return View(model);
        }

        // 
        // GET: /Account/InformationRequest
        [InjectAccountInfo]
        [InjectCustomerUserIdDataSet]
        public ActionResult InformationRequest(GetAccountInfoResponse accountInfo, CustomerUserIdDataSet customerData, string Ctx)
        {
            // Once logged into account services override any FACT code still lingering in the cookies
            WebUser.SetFACT(FirstAgain.Domain.SharedTypes.LoanApplication.LoanApplicationDataSet.KnownFACTs.LIGHTSTREAM_NATIVETRAFFIC_ACCOUNTSERVICES);
            return View(new AccountServicesVINPageModel(accountInfo, customerData, Ctx));
        }

        // 
        // GET: /Account/SaveVehicleInformation
        [InjectAccountInfo(Refresh = true)]
        [InjectAccountServicesDataSet(Refresh =true)]
        public ActionResult SaveVehicleInformation(GetAccountInfoResponse accountInfo, AccountServicesDataSet customerData)
        {
            // This alert shows up nowhere.
            TempData["Alert"] = "Thank you for submitting your vehicle information";
            return View("~/Views/AccountServices/Index.cshtml", new AccountServicesHomePageModel(accountInfo, customerData));
        }
        // 
        // GET: /Account/Apply
        public ActionResult Apply()
        {
            return new RedirectResult("/Apply/AccountServices");
        }
        #endregion

        #region JSON methods
        // 
        // POST: /Account/PaymentByACH
        [HttpPost]
        [InjectAccountServicesDataSet(Refresh=true)]
        [InjectCustomerUserIdDataSet]
        public ActionResult PaymentByACH(AccountServicesDataSet accountServicesData, CustomerUserIdDataSet customerData, FundedAccountModel model, BusinessCalendarDataSet businessCalendar)
        {
            string errorMessage;
            var result = model.PaymentByACH(accountServicesData, customerData, WebUser, businessCalendar, out errorMessage);
            return new JSONSuccessResult(result, errorMessage);
        }

        //
        // POST: /Account/CancelPayoff
        [InjectAccountServicesDataSet]
        [InjectCustomerUserIdDataSet]
        public ActionResult CancelPayoff(AccountServicesDataSet accountServicesData, CustomerUserIdDataSet customerData, FundedAccountModel model)
        {
            var result = model.CancelPayoff(accountServicesData, customerData, WebUser);
            return new JSONSuccessResult(result);
        }
        // 
        // POST: /Account/PayOffByACH
        [InjectAccountServicesDataSet(Refresh = true)]
        [InjectCustomerUserIdDataSet]
        public ActionResult PayOffByACH(AccountServicesDataSet accountServicesData, CustomerUserIdDataSet customerData, FundedAccountModel model, BusinessCalendarDataSet businessCalendar)
        {
            string errorMessage;
            var result = model.PayOffByACH(accountServicesData, customerData, WebUser, businessCalendar, out errorMessage);
            return new JSONSuccessResult(result, errorMessage);
        }

        // 
        // POST: /Account/GetPayoffQuote
        [InjectAccountServicesDataSet]
        [InjectCustomerUserIdDataSet(Refresh = true)]
        public ActionResult GetPayoffQuote(AccountServicesDataSet accountServicesData, CustomerUserIdDataSet customerData, int? ApplicationId, DateTime? SelectedDate, FirstAgain.Domain.Lookups.ShawData.PaymentTypeLookup.PaymentType? PaymentType)
        {
            return new JsonResult()
            {
                Data = FundedAccountModel.GetPayoffQuote(accountServicesData, customerData, ApplicationId, SelectedDate, PaymentType)
            };
        }

        //
        // POST: /Account/UpdateNickname
        [InjectCustomerUserIdDataSet(Refresh=true)]
        public ActionResult UpdateNickname(CustomerUserIdDataSet customerData, int? ApplicationId, string NewNickname)
        {
            bool result = FundedAccountModel.UpdateNickname(customerData, WebUser, ApplicationId, NewNickname);
            return new JSONSuccessResult(result);
        }

        //
        // POST: /Account/Load
        [HttpPost]
        [InjectAccountInfo]
        [InjectAccountServicesDataSet]
        public ActionResult Load(GetAccountInfoResponse accountInfo, AccountServicesDataSet accountServicesData, BusinessCalendarDataSet businessCalendar)
        {
            try
            {
                if (accountServicesData == null)
                {
                    SessionUtility.SetUpAccountServicesData();
                    accountServicesData = SessionUtility.AccountServicesData;
                    businessCalendar = SessionUtility.BusinessCalendar;
                }
                var model = new AccountServiceModelData(WebUser);
                model.Populate(accountInfo, accountServicesData, businessCalendar);
                return new JsonNetResult()
                {
                    Data = model
                };
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
                return new JSONSuccessResult(false);
            }
        }

        //
        // POST: /Account/SubmitENotices
        [HttpPost]
        [InjectCustomerUserIdDataSet(Refresh = true)]
        public ActionResult SubmitENotices(CustomerUserIdDataSet customerData, FundedAccountModel model)
        {
            bool result = model.UpdateENotices(customerData, WebUser);
            return new JSONSuccessResult(result);
        }

        // 
        // POST: /Account/UpdatePaymentAccount
        [HttpPost]
        [InjectCustomerUserIdDataSet(Refresh = true)]
        [InjectAccountServicesDataSet(Refresh=true)]
        public ActionResult UpdatePaymentAccount(AccountServicesDataSet accountServicesData, CustomerUserIdDataSet customerData, BusinessCalendarDataSet businessCalendar, FundedAccountModel model)
        {
            bool result;
            string errorMessage, effectiveDate;

            result = model.UpdatePaymentAccount(accountServicesData, customerData, businessCalendar, WebUser, out errorMessage, out effectiveDate);
            if (result)
            {
                SessionUtility.SetUpAccountServicesData();
            }
            return new JsonResult()
            {
                Data = new
                {
                    Success = result,
                    ErrorMessage = errorMessage,
                    EffectiveDate = effectiveDate
                }
            };
        }

        // 
        // POST: /Account/UpdateMonthlyPayment
        [HttpPost]
        [InjectCustomerUserIdDataSet]
        [InjectAccountServicesDataSet]
        public ActionResult UpdateMonthlyPayment(AccountServicesDataSet accountServicesData, CustomerUserIdDataSet customerData, BusinessCalendarDataSet businessCalendar, FundedAccountModel model)
        {
            bool result;
            string effectiveDate;

            result = model.UpdateMonthlyPayment(accountServicesData, customerData, businessCalendar, WebUser, out effectiveDate);
            if (result)
            {
                SessionUtility.SetUpAccountServicesData();
            }
            return new JsonResult()
            {
                Data = new
                {
                    Success = result,
                    EffectiveDate = effectiveDate
                }
            };
        }

        //
        // POST: /Account/SwitchToAutoPay
        [HttpPost]
        [InjectCustomerUserIdDataSet]
        [InjectAccountServicesDataSet]
        public ActionResult SwitchToAutoPay(AccountServicesDataSet accountServicesData, CustomerUserIdDataSet customerData, FundedAccountModel model)
        {
            bool result;
            bool missedACHCutoff;
            string errorMessage;
            string displayDate;

            result = model.SwitchToAutoPay(accountServicesData, customerData, WebUser, out errorMessage);
            if (result)
            {
                SessionUtility.SetUpAccountServicesData();
            }

            DateTime nextAvailablePaymentDate = model.ACHDebitFileHasBeenCreated(accountServicesData);
            missedACHCutoff = nextAvailablePaymentDate.ToShortDateString() != model.NextPaymentDueDate;
            displayDate = nextAvailablePaymentDate.ToLongDateString();
            return new JsonResult()
            {
                Data = new
                {
                    Success = result,
                    DisplayACHMessage = missedACHCutoff,
                    NextPaymentDate = displayDate,
                    ErrorMessage = errorMessage
                }
            };
        }

        // 
        // POST: /Account/RefreshAccount
        [HttpPost]
        [InjectAccountInfo]
        [InjectAccountServicesDataSet(Refresh = true)]
        public ActionResult RefreshAccount(GetAccountInfoResponse accountInfo, AccountServicesDataSet accountServicesData, FundedAccountModel account, BusinessCalendarDataSet businessCalendar)
        {
            var model = new AccountServiceModelData(WebUser);
            model.Populate(accountInfo, accountServicesData, businessCalendar);
            return new JsonNetResult()
            {
                Data = new
                {
                    Success = true,
                    FundedAccount = model.Applications.FirstOrDefault(x => x.ApplicationId == account.ApplicationId)
                }
            };
        }

        // 
        // POST: /Account/CancelExtraPayment
        [HttpPost]
        [InjectCustomerUserIdDataSet]
        [InjectAccountServicesDataSet]
        public ActionResult CancelExtraPayment(AccountServicesDataSet accountServicesData, CustomerUserIdDataSet customerData, FundedAccountModel account)
        {
            AccountServiceModelData.CancelExtraPayment(accountServicesData, customerData, WebUser, account);
            return new JSONSuccessResult();
        }

        //
        // GET: /Account/GetAmortizationSchedule
        [HttpGet]
        [InjectCustomerUserIdDataSet]
        public ActionResult GetAmortizationSchedule(CustomerUserIdDataSet customerData, string ctx, decimal? paymentAmount, decimal? extraPaymentAmount = null, DateTime? extraPaymentEffectiveDate = null)
        {
            return new JsonResult()
            {
                Data = AccountServiceModelData.GetAmortizationSchedule(ctx, customerData, paymentAmount, extraPaymentAmount, extraPaymentEffectiveDate)
            };
        }

        #endregion

        // GET: /Account/GoTo
        [HttpGet]
        [InjectCustomerUserIdDataSet]
        public ActionResult GoTo(CustomerUserIdDataSet customerData, int? id)
        {
            if (!id.HasValue || customerData == null || !customerData.Application.Any(a => a.ApplicationId == id.GetValueOrDefault()))
            {
                return new RedirectResult("Index");
            }
            return new RedirectResult(FirstAgain.Web.UI.StatusRedirect.GetRedirectBasedOnStatus(customerData, id.Value));
        }

        #region partials
        // POST / GET: /Account/AmortizationSchedule
        [InjectCustomerUserIdDataSet]
        public ActionResult AmortizationSchedule(string ctx, CustomerUserIdDataSet customerData, decimal? paymentAmount, decimal? extraPaymentAmount = null, DateTime? extraPaymentEffectiveDate = null)
        {
            return View(AccountServiceModelData.GetAmortizationSchedule(ctx, customerData, paymentAmount, extraPaymentAmount, extraPaymentEffectiveDate));
        }


        // GET: /modals/account-services-contact-us
        [AllowAnonymous] // so that we can redirect to the normal contact-us when session times out
        [HttpGet]
        [RedirectIfNotLoggedIn("/modals/contact-us")]
        [InjectCustomerUserIdDataSet]
        [InjectCustomerApplicationsDates]
        public ActionResult ContactUsPopup(CustomerUserIdDataSet customerData, CustomerApplicationsDates applicationsDates)
        {
            return PartialView("~/Views/AccountServices/Modals/ContactUs.cshtml", new AccountServicesContactUsModel(customerData, applicationsDates, WebUser));
        }

        // POST: /modals/account-services-contact-us
        [HttpPost]
        [AllowAnonymous] // so that we can redirect to the normal contact-us when session times out
        [RedirectIfNotLoggedIn("/modals/contact-us")]
        public ActionResult ContactUsPopup(AccountServicesContactUsModel model)
        {
            if (ModelState.IsValid)
            {
                model.Send();
                return new JSONSuccessResult();
            }

            return new JsonResult()
            {
                Data = new  { 
                    Success =  ModelState.IsValid,
                    ErrorMessage = ModelState.Values.SelectMany(x => x.Errors).First().ErrorMessage
                }
            };
        }
        #endregion
    }
}
