using LightStreamWeb.App_State;
using LightStreamWeb.Controllers.Shared;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LightStreamWeb.Models.Apply;
using System.Configuration;

namespace LightStreamWeb.Controllers
{
    public class SuntrustBranchApplicationController : BaseSuntrustApplicationController
    {
        [Inject]
        public SuntrustBranchApplicationController(ICurrentUser user)
            : base(user)
        {
        }
        protected override SunTrustApplyPageModel.IntroPageDisplayMode IntroPageDisplayMode
        {
            get 
            {
                return SunTrustApplyPageModel.IntroPageDisplayMode.ReferralForm; 
            }
        }

        protected override bool IsBranchApp
        {
            get { return true; }
        }

        protected override int FACTOverride
        {
            get { return FirstAgain.Domain.SharedTypes.LoanApplication.LoanApplicationDataSet.KnownFACTs.SUNTRUST_BRANCH; /* SunTrust Branch */; }
            set { }

        }

        protected override string Tagline
        {
            get 
            {
                return "Branch Web Link Generator";
            }
        }

        public override ActionResult PersonalInfo()
        {
            return new HttpNotFoundResult();
        }

        public override ActionResult BasicInfo()
        {
            if (IntroPageDisplayMode == SunTrustApplyPageModel.IntroPageDisplayMode.ReferralForm)
            {
                return View(viewName: "~/Views/Apply/Suntrust/ReferralForm.cshtml", model: new SunTrustApplyPageModel(WebUser)
                {
                    Tagline = Tagline
                });
            }

            return base.BasicInfo();
        }

        public ActionResult referral()
        {
            return RedirectToAction("ReferralForm");
        }
    }
}
