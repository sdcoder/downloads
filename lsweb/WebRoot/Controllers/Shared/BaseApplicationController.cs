using System;
using System.Web.Mvc;
using Newtonsoft.Json;
using Ninject;

using FirstAgain.Domain.SharedTypes.LoanApplication.LoanApplicationPostData;
using FirstAgain.Common.Logging;

using LightStreamWeb.App_State;
using LightStreamWeb.Models.Apply;
using FirstAgain.Web.Cookie;
using System.Configuration;
using System.Web.Security.AntiXss;
using FirstAgain.Domain.Common;
using LightStreamWeb.Models.Shared;

namespace LightStreamWeb.Controllers.Shared
{
    public abstract class BaseApplicationController : BaseController
    {
        [Inject]
        public BaseApplicationController(ICurrentUser user)
            : base(user)
        {
        }

        // Different versions of the App (Inquiry, Incomplete, Native) have different navigation
        // But otherwise the logic is identical. Navigation logic resides in the individual controllers
        // for each app, whereas data validation is handled by the shared model
        public abstract ActionResult SecurityInfo();
        public abstract ActionResult LoanInfo();
        public abstract ActionResult BasicInfo();
        public abstract ActionResult PersonalInfo();
        public abstract ActionResult Confirm();
        public abstract ActionResult ElectronicDisclosures();
        public abstract ActionResult ZipCode();
        public abstract ActionResult GoToStep(int step);

        protected ActionResult Submit(NativeLoanApplicationPostData loanAppPostData, QueueApplicationPostModel.QueueApplicationPostResult result)
        {
            // set cookies required by mbox marketing code on "thank you" page
            // excessive null checks, because apps should never fail over setting cookies
            if (loanAppPostData != null)
            {
                CookieUtility.SessionApplyLoanAmount = loanAppPostData.LoanAmount.ToString();
                if (loanAppPostData.PurposeOfLoan != null)
                {
                    CookieUtility.SessionApplyPurposeOfLoan = loanAppPostData.PurposeOfLoan.Type.ToString();
                }
            }

            // temporary additional logging of iOS issues
            if (ConfigurationManager.AppSettings["LogiOSErrors"] == "true" &&
                !result.Success &&
                (Request.UserAgent.Contains("iPad") || Request.UserAgent.Contains("iPhone")))
            {
                LogApplicationPostData(loanAppPostData);
            }
            return new JsonResult() { Data = result };
        }

        protected ActionResult ThankYou(ApplyPageModel model, string view)
        {
            ViewBag.ImpactRadiusJsFilePath = model.GetImpactRadiusJsFilePath();

            ViewBag.SessionApplyCookie = AntiXssEncoder.HtmlEncode(CookieUtility.SessionApplyCookie, false);
            ViewBag.SessionApplyLoanAmount = AntiXssEncoder.HtmlEncode(CookieUtility.SessionApplyLoanAmount, false);
            ViewBag.SessionApplyPurposeOfLoan = AntiXssEncoder.HtmlEncode(CookieUtility.SessionApplyPurposeOfLoan, false);      

            CookieUtility.ExpireApplicationCookies();

            return View(view, model);
        }

        // these are generally the same, but can be overridden in derived classes
        public virtual ActionResult ApplicantInfo()
        {
            return new RedirectResult(Url.Action("PersonalInfo") + "#ApplicantSection");
        }

        public virtual ActionResult CoApplicantInfo()
        {
            return new RedirectResult(Url.Action("PersonalInfo") + "#CoApplicantSection");
        }

        public virtual ActionResult ErrorMessage()
        {
            return new RedirectResult(Url.Action("PersonalInfo") + "#ErrorMessage");
        }

        public virtual ActionResult HMDA()
        {
            return new RedirectResult(Url.Action("PersonalInfo") + "#HMDA");
        }

        public virtual ActionResult SubjectProperty()
        {
            return new RedirectResult(Url.Action("PersonalInfo") + "#SubjectProperty");
        }

        [HttpPost]
        public ActionResult ReserveUserId(CustomerUserCredentialsPostData userCredentials)
        {
            try
            {
                var model = new ReserveUserIdModel();
                model.ReserveUserId(userCredentials);
                if (!model.Success)
                {
                    LightStreamLogger.WriteWarning(string.Format("ReserveUserId failed for {0} ({2}), {1}", userCredentials.UserId, model.ErrorMessage, model.UserId));
                }
                return new JsonResult()
                {
                    Data = new
                    {
                        Success = model.Success,
                        UserId = model.UserId,
                        TemporaryUserName = model.TemporaryUserName,
                        ErrorMessage = model.ErrorMessage
                    }
                };
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
            }

            return new JSONSuccessResult(false);
        }

        protected void LogApplicationPostData(NativeLoanApplicationPostData applicationPostData)
        {
            applicationPostData.ProtectPersonalData();
            LightStreamLogger.WriteError(JsonConvert.SerializeObject(applicationPostData));
            applicationPostData.UnProtectPersonalData();
        }
    }
}
