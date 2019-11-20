using FirstAgain.Common.Logging;
using LightStreamWeb.App_State;
using LightStreamWeb.Controllers.Shared;
using LightStreamWeb.Models.Apply;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FirstAgain.Common.Web;
using FirstAgain.Domain.ServiceModel.Client;
using System.Configuration;

namespace LightStreamWeb.Controllers
{
    public class SuntrustChannelOpsController : BaseSuntrustApplicationController
    {
        [Inject]
        public SuntrustChannelOpsController(ICurrentUser user)
            : base(user)
        {

        }

        protected override SunTrustApplyPageModel.IntroPageDisplayMode IntroPageDisplayMode
        {
            get { return SunTrustApplyPageModel.IntroPageDisplayMode.ChannelOps; }
        }


        protected override bool IsBranchApp
        {
            get { return false; }
        }

        protected override int FACTOverride
        {
            get { return FirstAgain.Domain.SharedTypes.LoanApplication.LoanApplicationDataSet.KnownFACTs.SUNTRUST_CHANNEL_OPS; /*SunTrust Channel Ops */ }
            set {  }
        }

        protected override string Tagline
        {
            get 
            {
                return "Client Contact Center Web Link Generator";
            }
        }

        public override ActionResult BasicInfo()
        {
            return new RedirectResult("ReferralForm");
        }

    }
}
