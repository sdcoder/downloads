using FirstAgain.Domain.ServiceModel.Client;
using LightStreamWeb.App_State;
using LightStreamWeb.Controllers.Shared;
using LightStreamWeb.Helpers;
using LightStreamWeb.Models.Apply;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LightStreamWeb.Controllers.Apply
{
    public class SuntrustPrivateWealthApplicationController : BaseSuntrustApplicationController
    {
        [Inject]
        public SuntrustPrivateWealthApplicationController(ICurrentUser user)
            : base(user)
        {
            FACTOverride = FirstAgain.Domain.SharedTypes.LoanApplication.LoanApplicationDataSet.KnownFACTs.SUNTRUST_PRIVATE_WEALTH_CONCIERCE; /*PWM Concierge*/ 
        }

        protected override SunTrustApplyPageModel.IntroPageDisplayMode IntroPageDisplayMode
        {
            get { return SunTrustApplyPageModel.IntroPageDisplayMode.PrivateWealth; }
        }

        protected override bool IsBranchApp
        {
            get { return false; }
        }

        protected override int FACTOverride { get; set; }

        protected override string Tagline
        {
            get 
            {
                return "Private Wealth Management Concierge Application";
            }
        }

        public override ActionResult Index()
        {
            return View(viewName: "~/Views/Apply/Suntrust/Intro.cshtml", model: new SunTrustApplyPageModel(WebUser)
            {
                IsLandingPage = true,
                Tagline = "Private Wealth Management LightStream Loan Page",
                DisplayMode = IntroPageDisplayMode
            });
        }
        public override ActionResult ReferralForm()
        {
            return View(viewName: "~/Views/Apply/Suntrust/ReferralForm.cshtml", model: new SunTrustApplyPageModel(WebUser)
            {
                Tagline = "Private Wealth Management Web Link Generator",
                DisplayMode = IntroPageDisplayMode
            });
        }

        public override ActionResult SubmitReferral(SuntrustTeammateReferralModel model)
        {
            this.FACTOverride = FirstAgain.Domain.SharedTypes.LoanApplication.LoanApplicationDataSet.KnownFACTs.SUNTRUST_PRIVATE_WEALTH_REFERAL; // PWM Referral
            return base.SubmitReferral(model);
        }

        public override ActionResult ReferralThankYou()
        {
            var suntrustApply = new SunTrustApplyPageModel(WebUser);
            suntrustApply.Tagline = "Private Wealth Management Web Link Generator";

            suntrustApply.TeammateEmailAddress = FirstAgain.Web.Cookie.CookieUtility.TeammateEmailAddress;
            
            return View(viewName: "~/Views/Apply/Suntrust/ReferralThankYou.cshtml", model: suntrustApply);
        }

        public ActionResult Collateral()
        {
            return View(viewName: "~/Views/Apply/Suntrust/Collateral.cshtml", model: new PWMCollateralPageModel(WebUser)
            {
                IsLandingPage = true,
                Tagline = "Private Wealth Management LightStream Loan Page",
                DisplayMode = IntroPageDisplayMode
            });
        }


        // GET: /Apply
        // redirect to LoanInfo tab. Typically happens on the rate calculator
        public override ActionResult Apply()
        {
            return RedirectToAction("ReferralForm");
        }
    }
}