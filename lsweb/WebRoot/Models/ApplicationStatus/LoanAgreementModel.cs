using FirstAgain.Correspondence.ServiceModel.Client;
using FirstAgain.Correspondence.SharedTypes;
using FirstAgain.Domain.SharedTypes.Customer;
using LightStreamWeb.ServerState;
using FirstAgain.Common.Extensions;
using FirstAgain.Common.Logging;
using LightStreamWeb.App_State;
using FirstAgain.Domain.ServiceModel.Client;
using LightStreamWeb.Helpers;
using System.Linq;

namespace LightStreamWeb.Models.ApplicationStatus
{
    public class LoanAgreementModel
    {
        private CustomerUserIdDataSet.ApplicationRow _application  = null;
        private FirstAgain.Domain.SharedTypes.LoanApplication.LoanOfferDataSet.LoanTermsRequestRow _latestApprovedLoanTerms = null;
        int? _applicationId = null;
        ICurrentUser _webUser = null;
        CustomerUserIdDataSet _customerDataSet = null;

        public LoanAgreementModel(CustomerUserIdDataSet customerDataSet, 
                                  int applicationId,
                                  ICurrentUser webUser,
                                  FirstAgain.Domain.SharedTypes.LoanApplication.LoanOfferDataSet.LoanTermsRequestRow latestApprovedLoanTerms)
        {
            _latestApprovedLoanTerms = latestApprovedLoanTerms;
            _applicationId = applicationId;
            _application = customerDataSet.Application.Single(a => a.ApplicationId == applicationId);
            _webUser = webUser;
            _customerDataSet = customerDataSet;
        }

        public void PersistLoanAgreement(LoanAgreementHtml loanAgreementHtml = null)
        {
            if (SessionUtility.FoundActiveLoanAgreementOnFile)
            {
                LightStreamLogger.WriteWarning("Attempt to save the loan agreement twice.");
                return;                
            }

            byte[] applicantSignature = _webUser.FoundApplicantSignatureOnFile ? null : _webUser.PrimarySignatureImageBytes;
            byte[] coApplicantSignature = _webUser.FoundCoApplicantSignatureOnFile ? null : _webUser.SecondarySignatureImageBytes;

            if (applicantSignature != null || coApplicantSignature != null)
            {
                LoanAgreementHtml agreement = loanAgreementHtml ?? SessionUtility.GetLoanAgreement();

                try
                {
                    AddSignatures(agreement, true);

                    bool isComplete = CorrespondenceServiceCorrespondenceOperations.SaveLoanAgreement(_application.ApplicationId,
                                                            _latestApprovedLoanTerms.LoanTermsRequestId,
                                                            agreement,
                                                            applicantSignature,
                                                            coApplicantSignature);

                    if (!isComplete)
                    {
                        EventAuditLogHelper.Submit(_customerDataSet, _webUser, FirstAgain.Domain.Lookups.FirstLook.EventTypeLookup.EventType.PartialSignature, null, _applicationId);
                    }
                    if (applicantSignature != null) // Prevent double persist.
                    {
                        _webUser.FoundApplicantSignatureOnFile = true;
                    }
                    if (coApplicantSignature != null) // Prevent double persist.
                    {
                        _webUser.FoundCoApplicantSignatureOnFile = true;
                    }
                    if (isComplete)
                    {
                        _webUser.FoundActiveLoanAgreementOnFile = true;// prevent persisting this again within the active session
                    }
                }
                catch(System.Exception ex)
                {
                    LightStreamLogger.WriteError(ex, "There was a problem persisting Loan Agreement for application {ApplicationId} with Applicant Address {ApplicantAddress}.", _application.ApplicationId, agreement.LoanContractDetails.ApplicantAddress );
                }
            }
        }

        public void AddSignatures(LoanAgreementHtml agreement, bool isComplete)
        {
            if (_application.IsJoint)
            {
                if (isComplete)
                {
                    agreement.Signature1Text = SessionUtility.GetSignatureText(false);
                    if (agreement.Signature1Text.IsNullOrEmpty())
                    {
                        agreement.Signature1Text = new CurrentUser().PrimarySignatureTimestamp;
                    }
                    agreement.Signature2Text = SessionUtility.GetSignatureText(true);
                    if (agreement.Signature2Text.IsNullOrEmpty())
                    {
                        agreement.Signature2Text = new CurrentUser().SecondarySignatureTimestamp;
                    }

                    agreement.Signature1Visible = true;
                    agreement.Signature2Visible = true;
                }
                else
                {
                    if (SessionUtility.PrimarySignatureImageBytes != null && !SessionUtility.FoundApplicantSignatureOnFile)
                    {
                        agreement.Signature1Text = SessionUtility.GetSignatureText(false);
                        if (SessionUtility.FoundCoApplicantSignatureOnFile)
                        {
                            agreement.Signature1Visible = true;
                            agreement.Signature2Visible = true;
                        }
                    }
                    else if (SessionUtility.SecondarySignatureImageBytes != null && !SessionUtility.FoundCoApplicantSignatureOnFile)
                    {
                        agreement.Signature2Text = SessionUtility.GetSignatureText(true);
                        if (SessionUtility.FoundApplicantSignatureOnFile)
                        {
                            agreement.Signature1Visible = true;
                            agreement.Signature2Visible = true;
                        }
                    }
                }
            }
            else // individual applicant
            {
                agreement.Signature1Text = SessionUtility.GetSignatureText(false);
                agreement.Signature1Visible = true;
            }
        }

    }
}