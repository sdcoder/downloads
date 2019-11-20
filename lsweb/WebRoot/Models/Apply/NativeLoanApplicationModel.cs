using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.SharedTypes.LoanApplication.LoanApplicationPostData;

namespace LightStreamWeb.Models.Apply
{
    [Serializable]
    [XmlRoot("NativeLoanApplication")]
    public class NativeLoanApplicationModel : NativeLoanApplicationPostData
    {
        private bool? _declineReferralOptIn = null;
        public bool DeclineReferralOptIn
        {
            get
            {
                if (_declineReferralOptIn.HasValue)
                {
                    return _declineReferralOptIn.Value;
                }
            
                return ApplicationFlags != null && ApplicationFlags.Any(af => af.FlagType == FlagLookup.Flag.DeclineReferralOptIn && af.FlagIsOn);
            }
            set
            {
                if (ApplicationFlags == null)
                    ApplicationFlags = new List<ApplicationFlagPostData>();
                ApplicationFlagPostData declineReferralFlagData = ApplicationFlags.Where(af => af.FlagType == FlagLookup.Flag.DeclineReferralOptIn).FirstOrDefault();
                if (declineReferralFlagData != null)
                {
                    declineReferralFlagData.FlagIsOn = value;
                }
                else
                {
                    ApplicationFlags.Add(new ApplicationFlagPostData() { FlagType = FlagLookup.Flag.DeclineReferralOptIn, FlagIsOn = value });
                }

                _declineReferralOptIn = value;
            }
        }

        internal object HideSSNs()
        {
            foreach (var app in Applicants)
            {
                app.SocialSecurityNumber = string.Empty;
            }
            this.MaskedSSN = true;
            return this;
        }

    }
}