using FirstAgain.Domain.ServiceModel.Contracts.MessageContracts.CustomerOperations;
using LightStreamWeb.App_State;
using System;
using System.Diagnostics.CodeAnalysis;

namespace LightStreamWeb.Tests.Mocks
{
    [ExcludeFromCodeCoverage]
    public class MockUser : ICurrentUser
    {
        public MockUser()
        {
            IPAddress = "1.1.1.1";
            UserAgent = "Mock User Agent";
            AcceptLanguage = "Mock Language";
            FirstAgainCodeTrackingId = 1;
            UniqueCookie = Guid.NewGuid().ToString();
            SessionApplyCookie = Guid.NewGuid().ToString();
        }
        public int? FirstAgainCodeTrackingId { get; set; }
        public int? CMSWebContentId { get; set; }
        public int? CMSRevision { get; set; }
        public string IPAddress { get; set; }
        public string UserAgent { get; set; }
        public string AcceptLanguage { get; set; }
        public string UniqueCookie { get; set; }
        public string SearchTermsCookie { get; set; }
        public string SearchEngineCookie { get; set; }

        public bool IsAccountServices { get; set; }
        public bool AddCoApplicant { get; set; }
        public bool IsGenericPostingPartner { get; set; }

        public string SessionApplyCookie { get; set; }
        public int? SplitTestCookie { get; set; }
        public int? WebRedirectId { get; set; }
        public string SubId { get; set; }
        public string TntId { get; set; }
        public string BRLId { get; set; }
        public string GSLID { get; set; }
        public string AID { get; set; }
        public string EFID { get; set; }
        public string TTID { get; set; }
        public string TAID { get; set; }
        public string TURL { get; set; }
        public string TuneAffiliateName { get; set; }
        public string TransUnionSessionId { get; set; }
        public DateTime FactSetDate { get; set; }
        public DateTime FirstVisitDate { get; set; }
        public MarketingReferrerInfo MarketingReferrerInfo { get; set; }
        public int? ApplicationId { get; set; }
        public string FirstAgainIdReferral { get; set; }
        public void SetMarketingReferrerInfoCookie(MarketingReferrerInfo mri)
        {
            throw new NotImplementedException();
        }
        public GetAccountInfoResponse Refresh()
        {
            throw new NotImplementedException();
        }
        public void SetFACT(int? fact)
        {
            throw new NotImplementedException();
        }
        public void SignOut()
        {
            throw new NotImplementedException();
        }
        public byte[] PrimarySignatureImageBytes { get; set; }
        public byte[] SecondarySignatureImageBytes { get; set; }



        public string PrimarySignatureTimestamp
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string SecondarySignatureTimestamp
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool PrimarySignatureOnFile { get; set; }
        public bool SecondarySignatureOnFile { get; set; }
        public void ResetSignatures() { }


        public bool FoundApplicantSignatureOnFile
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool FoundCoApplicantSignatureOnFile
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        public bool FoundActiveLoanAgreementOnFile
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int? LoanTerm { get; set; }
        public decimal? LoanAmount { get; set; }
        public int? TeammateReferralId { get; private set; }

        public int? SelectedLoanTerm { get; set; }
    }
}
