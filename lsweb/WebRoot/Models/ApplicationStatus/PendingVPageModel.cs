using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.ServiceModel.Contracts.MessageContracts.CustomerOperations;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.SharedTypes.InterestRate;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using LightStreamWeb.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.ApplicationStatus
{
    public class PendingVPageModel : ApplicationStatusPageModel
    {
        protected LoanApplicationDataSet _loanApplicationDataSet;
        
        #region constructors
        public PendingVPageModel(CustomerUserIdDataSet cuidds, LoanOfferDataSet loanOfferDataSet, LoanApplicationDataSet lads)
            : base(cuidds, loanOfferDataSet)
        {
            _loanApplicationDataSet = lads;
            Populate();
            EnableFileUpload = true;
        }
        #endregion

        public bool HasNothingToShow => !VerificationRequests.AllDocumentsReceivedOrWaived
                && VerificationRequests.Documents.Count == 0
                && !VerificationRequests.CollateralInformation.IsRequired()
                && !VerificationRequests.CollateralInformation.Received
                && !VerificationRequests.SubjectPropertyAddress.IsRequired()
                && !VerificationRequests.SubjectPropertyAddress.Received
                && !VerificationRequests.CustomerIdentification.IsRequired()
                && !VerificationRequests.CustomerIdentification.Received;
    
        private void Populate()
        {
            RateModalDisplay = RateModalDisplayType.ReadOnlyPurposeOfLoan;
            VerificationRequests = new VerificationRequestsModel();

            InterestRates rates = DomainServiceInterestRateOperations.GetApplicationInterestRates(ApplicationId);
            LoanTerms = new LoanTermsModel();

            // approved, prefunding NLTR, or approved NLTR get a single rate displayed
            if (Application.ApplicationStatusType == ApplicationStatusTypeLookup.ApplicationStatusType.Approved ||
                Application.ApplicationStatusType == ApplicationStatusTypeLookup.ApplicationStatusType.PreFundingNLTR || 
                Application.ApplicationStatusType == ApplicationStatusTypeLookup.ApplicationStatusType.ApprovedNLTR)
            {
                if (_loanOfferDataSet != null)
                {
                    var latestLtr = _loanOfferDataSet.LatestLoanTermsRequest;
                    RateModalDisplay = RateModalDisplayType.ApplicationRates;
                    LoanTerms.Populate(latestLtr);

                    IsNLTR = latestLtr.Status == LoanTermsRequestStatusLookup.LoanTermsRequestStatus.PendingV;
                }
            }
            else
            {
                // pending v gets a rate range, but we may have a loan terms request (for secured auto) or not.
                if (_loanOfferDataSet != null && _loanOfferDataSet.LatestLoanTermsRequest != null)
                {
                    LoanTerms.PopulateRateRange(_loanOfferDataSet.LatestLoanTermsRequest, rates);
                }
                else
                {
                    LoanTerms.PopulateRateRange(Application.GetApplicationDetailRows().First(), rates);
                }
            }

            // populate verification requests
            VerificationRequests.Populate(
                    _customerUserIdDataSet,                     
                    ApplicationId,
                    (_loanOfferDataSet != null && _loanOfferDataSet.LatestLoanTermsRequest != null) ? _loanOfferDataSet.LatestLoanTermsRequest.PurposeOfLoan : base.GetPurposeOfLoan(),
                    _loanApplicationDataSet);
        }


        public VerificationRequestsModel VerificationRequests { get; protected set; }

        public LoanTermsModel LoanTerms { get; private set; }
        public bool IsNLTR { get; private set; }

        public override PurposeOfLoanLookup.PurposeOfLoan GetPurposeOfLoan()
        {
            PurposeOfLoanLookup.PurposeOfLoan purposeOfLoan = base.GetPurposeOfLoan();
            if (_loanOfferDataSet != null && _loanOfferDataSet.LatestApprovedLoanTerms != null)
            {
                purposeOfLoan = _loanOfferDataSet.LatestApprovedLoanTerms.PurposeOfLoan;
            }
            if (purposeOfLoan.IsSecured())
            {
                return purposeOfLoan.GetUnsecuredPurpose();
            }

            return purposeOfLoan;
        }

        public string Heading
        {
            get
            {
                if (IsNLTR)
                {
                    return "New Loan Terms Information Request";
                }

                return "Loan Application Information Request";
            }
        }

        public string ProductName
        {
            get
            {
                if (IsNLTR)
                {
                    return "new loan terms request";
                }
                return "loan application";
            }
        }

    }
}