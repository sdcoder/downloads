using FirstAgain.Common.Caching;
using FirstAgain.Common.Logging;
using FirstAgain.Common.Web;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.SharedTypes.GenericPartner;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using FirstAgain.Domain.SharedTypes.LoanApplication.LoanApplicationPostData;
using LightStreamWeb.App_State;
using LightStreamWeb.Controllers.Shared;
using LightStreamWeb.Models.Apply;
using Ninject;
using System;
using System.Web.Mvc;
using System.Linq;
using KnownFACTs = FirstAgain.Domain.SharedTypes.LoanApplication.LoanApplicationDataSet.KnownFACTs;
using LightStreamWeb.ServerState;
using FirstAgain.Common.Extensions;

namespace LightStreamWeb.Controllers
{
    public class ApplyController : BaseApplicationController
    {
        ContentManager _content = new ContentManager();
        [Inject]
        public ApplyController(ICurrentUser user)
            : base(user)
        {
        }

        //
        // GET: /Apply/
        public virtual ActionResult Index(PurposeOfLoanLookup.PurposeOfLoan? purposeOfLoan, int? id, int? revision)
        {
            if (WebUser.IsGenericPostingPartner)
                return RedirectToAction("Index", "PartnerApplication");

            return View(new ApplyPageModel(WebUser, _content)
            {
                Canonical = "https://www.lightstream.com/apply"
            });

        }

        //
        // GET: /Apply/PersonalInfo
        public override ActionResult PersonalInfo()
        {
            if (WebUser.IsGenericPostingPartner)
                return RedirectToAction("PersonalInfo", "PartnerApplication");

            return View(new ApplyPageModel(WebUser, _content)
            {
                Canonical = "https://www.lightstream.com/apply"
            });
        }

        //
        // GET: /Apply/SecurityInfo
        public override ActionResult SecurityInfo()
        {
            if (WebUser.IsGenericPostingPartner)
                return RedirectToAction("SecurityInfo", "PartnerApplication");

            return View(new ApplyPageModel(WebUser, _content)
            {
                Canonical = "https://www.lightstream.com/apply"
            });
        }

        //
        // GET: /Apply/Confirm
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public override ActionResult Confirm()
        {
            if (WebUser.IsGenericPostingPartner)
                return RedirectToAction("Confirm", "PartnerApplication");

            return View(new ApplyPageModel(WebUser, _content)
            {
                Canonical = "https://www.lightstream.com/apply"
            });
        }

        // 
        // GET: /Apply/ThankYou
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult ThankYou()
        {
            ApplyPageModel model = new ApplyPageModel(WebUser, _content);
            FirstAgain.Web.Cookie.CookieUtility.SetApplicationSubmittedCookie();

            return base.ThankYou(model, "ThankYou");
        }

        //
        // GET: /Apply/Test
        [HttpGet]
        public ActionResult Test()
        {
            return View(new ApplyPageModel(WebUser));
        }

        //
        // POST: /Apply/Submit
        [HttpPost]
        public ActionResult Submit(NativeLoanApplicationModel model)
        {
            LightStreamLogger.WriteInfo("Method: {MethodName}, Entering method", "/Apply/Submit");

            // prevent double-submission
            if (FirstAgain.Web.Cookie.CookieUtility.IsApplicationSubmittedCookieSet() || SessionUtility.IsDuplicateSubmission(model, WebUser.UniqueCookie))
            {
                FirstAgain.Web.Cookie.CookieUtility.ClearApplicationSubmittedCookie();

                LightStreamLogger.WriteDebug("Duplicate application received for {UserName}", model.UserCredentials.UserName);

                return new JsonResult()
                {
                    Data = LightStreamWeb.Models.Apply.QueueApplicationPostModel.QueueApplicationPostResult.ReturnError("", QueueApplicationPostModel.QueueApplicationPostResult.RedirectTo.ThankYou)
                };
            }

            DateTime utcNow = DateTime.UtcNow;
            if (model.FACTHistory != null)
            {
                model.FACTHistory = (from m in model.FACTHistory
                                     where m.ExpirationDate >= utcNow
                                     select new FACTHistoryPostData()
                                     {
                                         ID = m.ID,
                                         Timestamp = m.Timestamp.ToLocalTime(),
                                         ExpirationDate = m.ExpirationDate.ToLocalTime()
                                     }).ToList();
            }

            LightStreamWeb.Models.Apply.QueueApplicationPostModel.QueueApplicationPostResult result = null;

            try
            {
                result = QueueApplicationPostModel.SetDefaultsAndValidate(model);
                if (result.Success)
                {
                    QueueApplicationPostModel.SetMarketingData_Native(model, WebUser, null);
                    result = QueueApplicationPostModel.ValidateSecurityInfo(model);
                    if (result.Success)
                    {
                        result = QueueApplicationPostModel.ValidateUserIdReservation(model);
                        if (result.Success)
                        {
                            LightStreamLogger.WriteInfo("Method: {MethodName}, Entering method", "SubmitNativeApp");
                            QueueApplicationPostModel.SubmitNativeApp(model, this.WebUser);
                            LightStreamLogger.WriteInfo("Method: {MethodName}, Finished method", "SubmitNativeApp");
                        }
                        else
                        {
                            LightStreamLogger.WriteError("Method: {MethodName}, Result: {result}, Redirect: {redirect}, ErrorMessage: {errorMsg}",
                            "ValidateUserIdReservation", result.Success, result.Redirect, result.ErrorMessage);
                        }
                    }
                    else
                    {
                        LightStreamLogger.WriteError("Method: {MethodName}, Result: {result}, Redirect: {redirect}, ErrorMessage: {errorMsg}",
                        "ValidateSecurityInfo", result.Success, result.Redirect, result.ErrorMessage);
                    }
                }
                else
                {
                    LightStreamLogger.WriteError("Method: {MethodName}, Result: {result}, Redirect: {redirect}, ErrorMessage: {errorMsg}", 
                        "SetDefaultsAndValidate", result.Success, result.Redirect, result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                SessionUtility.FailAppSubmission(model, WebUser.UniqueCookie);

                if (result == null)
                {
                    result = new LightStreamWeb.Models.Apply.QueueApplicationPostModel.QueueApplicationPostResult();
                }
                result.Success = false;
                result.ErrorMessage = "We're sorry, but there was an error submitting your application.";

                LogApplicationPostData(model);
                LightStreamLogger.WriteError(ex);
            }
            LightStreamLogger.WriteInfo("Method: {MethodName}, Finished method", "/Apply/Submit");

            return base.Submit(model, result);
        }

        //
        // GET: /Apply/Continue
        public ActionResult Continue(string id)
        {
            if (MaintenanceConfiguration.IsInMaintenanceMode || string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index");
            }
            try
            {
                var referralId = WebSecurityUtility.Descramble(id);

                // https://dweb03.firstagain.local/apply/continue?id=c6cXHlfWn91r9kA61tQmTA%3d%3d
                var referral = DomainServiceLoanApplicationOperations.GetTeammateReferral("", "", "", "", referralId);
                if (referral == null || referral.IsNull())
                {
                    return RedirectToAction("Index");
                }
                if (Request.QueryString["Fact"].IsNotNull())
                {
                    int fact;
                    if (int.TryParse(Request.QueryString["Fact"], out fact))
                    {
                        referral.FirstAgainCodeTrackingId = fact;
                    }
                }
                if (Request.QueryString["PurposeOfLoan"].IsNotNull())
                {
                    referral.PurposeOfLoan = (PurposeOfLoanLookup.PurposeOfLoan)Enum.Parse(typeof(PurposeOfLoanLookup.PurposeOfLoan), Request.QueryString["PurposeOfLoan"]);
                }
                if (Request.QueryString["LoanAmount"].IsNotNull())
                {
                    decimal loanAmount;
                    if (decimal.TryParse(Request.QueryString["LoanAmount"], out loanAmount))
                    {
                        referral.LoanAmount = loanAmount;
                    }
                }

                var model = new ApplyFromReferralModel(referral);
                model.ZipCode = referral.ZipCode.Trim();
                WebUser.SetFACT(referral.FirstAgainCodeTrackingId);

                if (referral.FirstAgainCodeTrackingId == KnownFACTs.SUNTRUST_PRIVATE_WEALTH_CONCIERCE ||
                    referral.FirstAgainCodeTrackingId == KnownFACTs.SUNTRUST_PRIVATE_WEALTH_REFERAL)
                {
                    model.Discount = FlagLookup.Flag.SuntrustPrivateWealth;
                }
                FirstAgain.Web.Cookie.CookieUtility.SetTeammateReferralCookie(referralId);

                return View("~/Views/Apply/Continue.cshtml", model);
            }
            catch (Exception ex)
            {
                // log the error, but do not stop the user from continuing to the application process
                LightStreamLogger.WriteError(ex);
                return RedirectToAction("Index");
            }
        }


        // POST: /Apply/Api
        [HttpPost]
        public ActionResult Api(WebApplicationAPIModel model)
        {
            GenericPartnerDataSet gpds = MachineCache.Get<GenericPartnerDataSet>("GenericPartnerList");
            model.Sanitize(gpds);

            if (model.FACT.HasValue)
            {
                WebUser.SetFACT(model.FACT.Value);
            }
            return View(model);
        }

        #region Error navigation routes
        public override ActionResult ZipCode()
        {
            return RedirectToAction("Index");
        }

        public override ActionResult ElectronicDisclosures()
        {
            return RedirectToAction("Confirm");
        }

        public override ActionResult LoanInfo()
        {
            return RedirectToAction("Index");
        }

        public override ActionResult BasicInfo()
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

        #endregion

    }
}
