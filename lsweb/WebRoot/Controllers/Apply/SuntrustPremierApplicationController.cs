using LightStreamWeb.App_State;
using LightStreamWeb.Controllers.Shared;
using LightStreamWeb.Models.Apply;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LightStreamWeb.Controllers.Apply
{
    public class SuntrustPremierApplicationController : BaseSuntrustApplicationController
    {
        [Inject]
        public SuntrustPremierApplicationController(ICurrentUser user)
            : base(user)
        {
            FACTOverride = FirstAgain.Domain.SharedTypes.LoanApplication.LoanApplicationDataSet.KnownFACTs.SUNTRUST_PREMIER_CONCIERCE; /*Premier Concierge*/ 
        }

        protected override SunTrustApplyPageModel.IntroPageDisplayMode IntroPageDisplayMode
        {
            get { return SunTrustApplyPageModel.IntroPageDisplayMode.PremierBanking; }
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
                return "Premier Banking - Concierge Application";
            }
        }

        public override ActionResult Index()
        {
            return View(viewName: "~/Views/Apply/Suntrust/Intro.cshtml", model: new SunTrustApplyPageModel(WebUser)
            {
                IsLandingPage = true,
                Tagline = "Premier Banking LightStream Loan Page",
                DisplayMode = IntroPageDisplayMode
            });
        }
        public override ActionResult ReferralForm()
        {
            return View(viewName: "~/Views/Apply/Suntrust/ReferralForm.cshtml", model: new SunTrustApplyPageModel(WebUser)
            {
                Tagline = "Premier Banking – Web Link Generator",
                DisplayMode = SunTrustApplyPageModel.IntroPageDisplayMode.PremierBanking
            });
        }

        public override ActionResult SubmitReferral(SuntrustTeammateReferralModel model)
        {
            this.FACTOverride = FirstAgain.Domain.SharedTypes.LoanApplication.LoanApplicationDataSet.KnownFACTs.SUNTRUST_PREMIER_REFERAL; // Premier Referral
            return base.SubmitReferral(model);
        }

        public override ActionResult ReferralThankYou()
        {
            var suntrustApply = new SunTrustApplyPageModel(WebUser);
            suntrustApply.Tagline = "Premier Banking – Web Link Generator";

            suntrustApply.TeammateEmailAddress = FirstAgain.Web.Cookie.CookieUtility.TeammateEmailAddress;

            return View(viewName: "~/Views/Apply/Suntrust/ReferralThankYou.cshtml", model: suntrustApply);
        }

        // GET: /Apply
        // redirect to LoanInfo tab. Typically happens on the rate calculator
        public override ActionResult Apply()
        {
            return RedirectToAction("ReferralForm");
        }
    }
}
