using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace LightStreamWeb.App_State
{
    public interface ICurrentUser
    {
        int? FirstAgainCodeTrackingId { get; }

        int? CMSWebContentId { get; set; }

        int? CMSRevision { get; set; }

        [MaxLength(255)]
        string IPAddress { get; set; }

        [MaxLength(255)]
        string UserAgent { get; set; }

        [MaxLength(255)]
        string AcceptLanguage { get; set; }

        [MaxLength(255)]
        string SessionApplyCookie { get; set; }

        [MaxLength(255)]
        string SearchTermsCookie { get; set; }

        [MaxLength(255)]
        string SearchEngineCookie { get; set; }

        int? SplitTestCookie { get; set; }
        int? WebRedirectId { get; set; }

        [MaxLength(255)]
        string SubId { get; set; }

        [MaxLength(255)]
        string TntId { get; set; }

        [MaxLength(255)]
        string AID { get; set; }

        [MaxLength(255)]
        string BRLId { get; set; }

        [MaxLength(255)]
        string GSLID { get; set; }

        [MaxLength(255)]
        string EFID { get; set; }

        [MaxLength(30)]
        string TTID { get; set; }

        [MaxLength(255)]
        string TAID { get; set; }

        [MaxLength(2048)]
        string TURL { get; set; }

        string TuneAffiliateName { get; set; }

        [MaxLength(255)]
        string TransUnionSessionId { get; set; }

        DateTime FactSetDate { get; set; }

        DateTime FirstVisitDate { get; set; }

        [ReadOnly(true)]
        MarketingReferrerInfo MarketingReferrerInfo { get;  }

        int? ApplicationId { get; set; }

        [MaxLength(255)]
        string UniqueCookie { get; }

        void SetMarketingReferrerInfoCookie(MarketingReferrerInfo mri);

        bool IsAccountServices { get; set; }
        bool AddCoApplicant  { get; set; }
        bool IsGenericPostingPartner { get; set; }

        [MaxLength(100)]
        string FirstAgainIdReferral { get; }
        
        void SetFACT(int? fact);
        void SignOut();

        byte[] PrimarySignatureImageBytes { get; set; }
        byte[] SecondarySignatureImageBytes { get; set; }

        [MaxLength(100)]
        string PrimarySignatureTimestamp { get; set; }

        [MaxLength(100)]
        string SecondarySignatureTimestamp { get; set; }

        bool PrimarySignatureOnFile { get; set; }
        bool SecondarySignatureOnFile { get; set; }

        void ResetSignatures();

        bool FoundApplicantSignatureOnFile { get; set; }
        bool FoundCoApplicantSignatureOnFile { get; set; }
        bool FoundActiveLoanAgreementOnFile { get; set; }
        
        decimal? LoanAmount { get; set; }
        int? LoanTerm { get; set; }
        int? TeammateReferralId { get; }

        int? SelectedLoanTerm { get; set; }
    }
}
