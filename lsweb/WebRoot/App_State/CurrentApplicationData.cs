using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using FirstAgain.Domain.Lookups.FirstLook;

namespace LightStreamWeb.App_State
{
    [Serializable]
    public class CurrentApplicationData : ICurrentApplicationData
    {
        public CurrentApplicationData()
        {
            PaymentType = PaymentTypeLookup.PaymentType.NotSelected;
        }
        public int ApplicationId { get; set; }

        public PaymentTypeLookup.PaymentType PaymentType { get; set; }

        public CreditTierLookup.CreditTier CreditTier { get; set; }

        public ApplicationStatusTypeLookup.ApplicationStatusType ApplicationStatus { get; set; }

        public bool AddCoApplicantIsEnabled { get; set; }
        public bool ApplicationResultedFromAddCoApplicant { get; set; }

        public bool HasEnotices { get; set; }
        public bool HasDeclineNotice { get; set; }

        public LoanTermsRequestStatusLookup.LoanTermsRequestStatus LatestLoanTermsRequestStatus { get; set; }
        public string ApplicantNames { get; set; }
        public PurposeOfLoanLookup.PurposeOfLoan PurposeOfLoan { get; set; }

        public bool IsRegOApplication { get; set; }

        public ApplicationTypeLookup.ApplicationType ApplicationType { get; set; }

        public bool IsAutoApprovalEligible { get; set; } = false;

        public decimal? CounterAmount { get; set; }

        public DateTime? WithdrawDate { get; set; }
    }
}