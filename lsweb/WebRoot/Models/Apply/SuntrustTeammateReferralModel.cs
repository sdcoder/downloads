using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.Apply
{
    public class SuntrustTeammateReferralModel : SuntrustLoanApplicationModel
    {
        public string SocialSecurityNumber { get; set; }

        public string ApplicantEmailAddress { get; set; }

        public string ApplicantCIN { get; set; }

        public override void ProtectPersonalData()
        {
            base.ProtectPersonalData();

            this.SocialSecurityNumber = "(hidden)";
        }
    }
}