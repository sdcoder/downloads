using FirstAgain.Domain.SharedTypes.LoanApplication.LoanApplicationPostData;
using FirstAgain.Common.Logging;
using LightStreamWeb.App_State;
using LightStreamWeb.Filters;
using LightStreamWeb.Models.Apply;
using LightStreamWeb.Models.PublicSite;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace LightStreamWeb.Controllers.Shared
{
    [RequireSuntrustTeamMateLogin]
    public abstract class BaseSuntrustApplicationController : BaseApplicationController
    {
        [Inject]
        public BaseSuntrustApplicationController(ICurrentUser user)
            : base(user)
        {
            // override the FACT here, if needed
        }

        protected abstract int FACTOverride
        {
            get;
            set;
        }
        protected abstract bool IsBranchApp
        {
            get;
        }
        protected abstract string Tagline
        {
            get;
        }
        protected abstract LightStreamWeb.Models.Apply.SunTrustApplyPageModel.IntroPageDisplayMode IntroPageDisplayMode
        {
            get;
        }

        public virtual ActionResult Index()
        {
            return View(viewName: "~/Views/Apply/Suntrust/Intro.cshtml", model: new SunTrustApplyPageModel(WebUser)
            {
                IsLandingPage = true,
                Tagline = Tagline,
                DisplayMode = IntroPageDisplayMode
            });
        }

        // 
        // GET: /ChangeLoanAppZipCode/
        // first tab may vary, so this always returns a redirect
        public override ActionResult ZipCode()
        {
            return RedirectToAction("LoanInfo");
        }

        // GET: /Apply
        // redirect to LoanInfo tab. Typically happens on the rate calculator
        public virtual ActionResult Apply()
        {
            return RedirectToAction("BasicInfo");
        }

        // LoanInfo
        public override ActionResult LoanInfo()
        {
            return View(viewName: "~/Views/Apply/Suntrust/LoanInfo.cshtml", model: new SunTrustApplyPageModel(WebUser)
            {
                Tagline = Tagline,
                DisplayMode = this.IntroPageDisplayMode
            });
        }
        //
        // PersonalInfo
        public override ActionResult PersonalInfo()
        {
            return View(viewName: "~/Views/Apply/Suntrust/PersonalInfo.cshtml", model: new SunTrustApplyPageModel(WebUser)
            {
                Tagline = Tagline,
                DisplayMode = this.IntroPageDisplayMode
            });
        }
        //
        // BasicInfo
        public override ActionResult BasicInfo()
        {
            return View(viewName: "~/Views/Apply/Suntrust/BasicInfo.cshtml", model: new SunTrustApplyPageModel(WebUser)
            {
                Tagline = Tagline,
                DisplayMode = this.IntroPageDisplayMode
            });
        }

        // GET: /Apply/ThankYou
        public ActionResult ThankYou()
        {
            return View(viewName: "~/Views/Apply/Suntrust/ThankYou.cshtml", model: new SunTrustApplyPageModel(WebUser)
            {
                Tagline = Tagline
            });
        }


        //
        // ConfirmSubmit
        public override ActionResult Confirm()
        {
            return View(viewName: "~/Views/Apply/Suntrust/Confirm.cshtml", model: new SunTrustApplyPageModel(WebUser)
            {
                Tagline = Tagline
            });
        }

        //
        // POST: /Apply/Submit
        [HttpPost]
        public ActionResult Submit(SuntrustLoanApplicationModel model)
        {
            LightStreamWeb.Models.Apply.QueueApplicationPostModel.QueueApplicationPostResult result = new QueueApplicationPostModel.QueueApplicationPostResult();

            try
            {
                result = QueueSuntrustApplicationPostModel.SetDefaultsAndValidateSuntrustApp(model, WebUser, FACTOverride);
                if (result.Success)
                {
                    QueueSuntrustApplicationPostModel.SubmitSuntrustAppApp(model, this.WebUser);
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = "We're sorry, but there was an error submitting your application.";
                LogApplicationPostData(model);
                LightStreamLogger.WriteError(ex);
            }

            return new JsonResult()
            {
                Data = result
            };

        }

        public override ActionResult SecurityInfo()
        {
            throw new NotImplementedException();
        }

        public override ActionResult ElectronicDisclosures()
        {
            return RedirectToAction("Confirm");
        }

        public override ActionResult GoToStep(int step)
        {
            switch (step)
            {
                case 4:
                    return RedirectToAction("Confirm");
                case 3:
                    return RedirectToAction("PersonalInfo");
                case 2:
                    return RedirectToAction("LoanInfo");
                default:
                    return RedirectToAction("BasicInfo");
            }
        }

        #region teammate referral
        [HttpPost]
        public virtual ActionResult SubmitReferral(SuntrustTeammateReferralModel model)
        {
            LightStreamWeb.Models.Apply.QueueApplicationPostModel.QueueApplicationPostResult result = new QueueApplicationPostModel.QueueApplicationPostResult();

            try
            {
                result = QueueSuntrustApplicationPostModel.SubmitSuntrustReferral(model, this.WebUser, this.FACTOverride);

                FirstAgain.Web.Cookie.CookieUtility.TeammateEmailAddress = null;
                string teammateEmail = QueueSuntrustApplicationPostModel.GetExistingReferralInfo(model.SocialSecurityNumber, "", model.ApplicantEmailAddress, "", 0);
                if(!string.IsNullOrEmpty(teammateEmail))
                {
                    FirstAgain.Web.Cookie.CookieUtility.TeammateEmailAddress = teammateEmail;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = "We're sorry, but there was an error submitting the client referral.";
                LogApplicationPostData(model);
                LightStreamLogger.WriteError(ex);
            }

            return new JsonResult()
            {
                Data = result
            };

        }
        
        public virtual ActionResult ReferralThankYou()
        {
            var suntrustApply = new SunTrustApplyPageModel(WebUser);
            suntrustApply.Tagline = Tagline;

            suntrustApply.TeammateEmailAddress = FirstAgain.Web.Cookie.CookieUtility.TeammateEmailAddress;

            return View(viewName: "~/Views/Apply/Suntrust/ReferralThankYou.cshtml", model: suntrustApply);
        }


        public virtual ActionResult ReferralForm()
        {
            return View(viewName: "~/Views/Apply/Suntrust/ReferralForm.cshtml", model: new SunTrustApplyPageModel(WebUser)
            {
                Tagline = Tagline,
                DisplayMode = IntroPageDisplayMode
                
            });
        }
        #endregion
    }
}
