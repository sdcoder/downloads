using System;
using System.ComponentModel;
using FirstAgain.Domain.Lookups.FirstLook;

namespace LightStreamWeb.App_State
{
    public interface ICurrentApplicationData
    {
        [ReadOnly(true)]
        PaymentTypeLookup.PaymentType PaymentType { get; set; }

        [ReadOnly(true)]
        int ApplicationId { get; set; }

        [ReadOnly(true)]
        CreditTierLookup.CreditTier CreditTier { get; set; }

        [ReadOnly(true)]
        ApplicationStatusTypeLookup.ApplicationStatusType ApplicationStatus { get; set; }

        [ReadOnly(true)]
        LoanTermsRequestStatusLookup.LoanTermsRequestStatus LatestLoanTermsRequestStatus { get; set; }

        [ReadOnly(true)]
        bool AddCoApplicantIsEnabled { get; set; }

        [ReadOnly(true)]
        bool ApplicationResultedFromAddCoApplicant { get; set; }

        [ReadOnly(true)]
        bool HasEnotices { get; set; }

        [ReadOnly(true)]
        bool HasDeclineNotice { get; set; }

        [ReadOnly(true)]
        string ApplicantNames { get; set; }
        [ReadOnly(true)]
        PurposeOfLoanLookup.PurposeOfLoan PurposeOfLoan { get; set; }

        [ReadOnly(true)]
        bool IsRegOApplication { get; set; }
        [ReadOnly(true)]
        ApplicationTypeLookup.ApplicationType ApplicationType { get; set; }

        [ReadOnly(true)]
        bool IsAutoApprovalEligible { get; set; }

        [ReadOnly(true)]
        decimal? CounterAmount { get; set; }

        [ReadOnly(true)]
        DateTime? WithdrawDate { get; set; }

    }
}
