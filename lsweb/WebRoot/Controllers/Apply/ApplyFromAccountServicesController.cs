using FirstAgain.Common.Logging;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using LightStreamWeb.App_State;
using LightStreamWeb.Controllers.Shared;
using LightStreamWeb.Filters;
using LightStreamWeb.Models.Apply;
using Ninject;
using System;
using System.Linq;
using System.Web.Mvc;
using FirstAgain.Common.Extensions;
using LightStreamWeb.Models.SignIn;

namespace LightStreamWeb.Controllers
{
    [Authorize]
    public class ApplyFromAccountServicesController : BaseApplicationController
    {
        [Inject]
        public ApplyFromAccountServicesController(ICurrentUser user)
            : base(user)
        {
        }

        // on the index.cshtml page, the application type drop-down needs to be wildly different
        // dynamically loads different apps, beased on # of people on the account.... individual, joint, etc....
        [RequireApplicationId]
        public ActionResult Start()
        {
            return View("~/Views/Apply/Shared/LoadExistingApp.cshtml", new LoadExistingAppModel()
                {
                    DataURL = Url.Action("LoadApp"),
                    NextPage = Url.Action("Index") 
                });
        }

        [RequireApplicationId]
        [InjectLoanApplicationDataSet]
        [InjectCustomerUserIdDataSet]
        public ActionResult LoadApp(
            FirstAgain.Web.UI.ReApplyApplicationTypeLookup.ReApplyApplicationType? ReApplyApplicationType, 
            LoanApplicationDataSet lads, 
            CustomerUserIdDataSet customerData,
            string ZipCode, 
            PurposeOfLoanLookup.PurposeOfLoan? PurposeOfLoan,
            decimal? LoanAmount, 
            short? LoanTermMonths, 
            PaymentTypeLookup.PaymentType? LoanPaymentType)
        {
            try
            {
                if (lads.IsNotNull() &&
                    lads.Application.IsNotNull() &&
                    lads.Application.SingleOrDefault()
                        .ApplicationStatusType.IsOneOf(ApplicationStatusTypeLookup.ApplicationStatusType.Inquiry,
                            ApplicationStatusTypeLookup.ApplicationStatusType.Incomplete))
                {
                    base.SignOut();
                    return new JsonNetResult()
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new
                        {
                            IsSignOut = true
                        }
                    };
                }
                var model = new AccountServicesAppModel();
                model.Populate(ReApplyApplicationType, lads, customerData);

                // PBI 8144 - 2014.12.01 - regardless of what they enter (in the useless zip code field) on the calculator - pre-populate the zip code from 
                // the customer's contact info. Do not use what was supplied on the rate calculator.
                //if (!string.IsNullOrEmpty(ZipCode))
                //{
                //    model.ZipCode = ZipCode;
                //}
                if (PurposeOfLoan.HasValue && PurposeOfLoan.Value != PurposeOfLoanLookup.PurposeOfLoan.NotSelected)
                {
                    model.PurposeOfLoan.Type = PurposeOfLoan.Value;
                }
                if (LoanAmount.HasValue)
                {
                    model.LoanAmount = LoanAmount.Value;
                }
                if (LoanTermMonths.HasValue)
                {
                    model.LoanTermMonths = LoanTermMonths.Value;
                }
                if (LoanPaymentType.HasValue && LoanPaymentType.Value != PaymentTypeLookup.PaymentType.NotSelected)
                {
                    model.LoanPaymentType = LoanPaymentType.Value;
                }

                return new JsonNetResult()
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new
                    {
                        Success = true,
                        LoanApp = model
                    }
                };
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
                return new JsonNetResult()
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new
                    {
                        Success = false,
                        ErrorMessage = ex.Message
                    }
                };
            }
        }

        //
        // GET: /Apply/Joint/
        [RedirectToApplyIfTerminated]
        public ActionResult Index()
        {
            return View("~/Views/Apply/AccountServices/Index.cshtml", new AccountServicesApplyPageModel(WebUser));
        }

        public override ActionResult SecurityInfo()
        {
            // a new view is needed for this "tab" - because of different copy
            return View("~/Views/Apply/AccountServices/SecurityInfo.cshtml", new AccountServicesApplyPageModel(WebUser));
        }

        public override ActionResult PersonalInfo()
        {
            return View("~/Views/Apply/AccountServices/PersonalInfo.cshtml", new AccountServicesApplyPageModel(WebUser));
        }

        //
        // GET: /Apply/Joint/Confirm
        public override ActionResult Confirm()
        {
            return View("~/Views/Apply/AccountServices/Confirm.cshtml", new AccountServicesApplyPageModel(WebUser));
        }

        // 
        // GET: /Apply/Joint/ThankYou
        public ActionResult ThankYou()
        {
            AccountServicesApplyPageModel model = new AccountServicesApplyPageModel(WebUser);
            return base.ThankYou(model,"~/Views/Apply/AccountServices/ThankYou.cshtml");
        }

        //
        // POST: /Apply/Submit
        [HttpPost]
        [InjectCustomerUserIdDataSet]
        public ActionResult Submit(AccountServicesAppModel model, CustomerUserIdDataSet customerData)
        {
            LightStreamWeb.Models.Apply.QueueApplicationPostModel.QueueApplicationPostResult result = null;

            try
            {
                result = QueueApplicationPostModel.SetDefaultsAndValidate(model);
                QueueApplicationPostModel.SetMarketingData_AccountServices(model, WebUser);
                if (result.Success)
                {
                    result = QueueApplicationPostModel.ValidateAccountServicesSecurityInfo(model);
                    if (result.Success)
                    {
                        QueueApplicationPostModel.SubmitAccountServicesApp(model, WebUser, customerData);
                    }
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = "We're sorry, but there was an error submitting your application.";

                LogApplicationPostData(model);
                LightStreamLogger.WriteError(ex);
            }

            return base.Submit(model, result);
        }


        public override ActionResult LoanInfo()
        {
            return RedirectToAction("Index");
        }

        public override ActionResult BasicInfo()
        {
            return RedirectToAction("Index");
        }

        public override ActionResult ElectronicDisclosures()
        {
            return RedirectToAction("Confirm");
        }

        public override ActionResult ZipCode()
        {
            return RedirectToAction("Index");
        }

        public override ActionResult GoToStep(int step)
        {
            switch (step)
            {
                case 4:
                    return RedirectToAction("Confirm");
                case 3:
                    return RedirectToAction("SecurityInfo");
                case 2:
                    return RedirectToAction("PersonalInfo");
                default:
                    return RedirectToAction("Index");
            }
        }
    }
}
