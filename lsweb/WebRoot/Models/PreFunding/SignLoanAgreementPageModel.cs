using System.Text;
using FirstAgain.Correspondence.ServiceModel.Client;
using FirstAgain.Correspondence.SharedTypes;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using FirstAgain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SignatureModel = LightStreamWeb.Models.ApplicationStatus.ApprovedPageModel.SignatureModel;
using LightStreamWeb.Models.ApplicationStatus;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Common.Logging;
using LightStreamWeb.Helpers;
using LightStreamWeb.ServerState;

namespace LightStreamWeb.Models.PreFunding
{
    public class SignLoanAgreementPageModel : PreFundingNLTRPageModel
    {
        #region constructors
        public SignLoanAgreementPageModel(CustomerUserIdDataSet cuidds, LoanOfferDataSet loanOfferDataSet, int? applicationId)
            : base(cuidds, loanOfferDataSet)
        {
            Title = "Sign New Loan Agreement";
            ApplicationId = applicationId.GetValueOrDefault(WebUser.ApplicationId.Value);
            LoanTermsRequestId = loanOfferDataSet.LatestAliveLoanTerms.LoanTermsRequestId;
        }
        #endregion

        private string _signedLoanAgreementHtml;
        public MvcHtmlString LoanAgreementHtml()
        {
            return new MvcHtmlString(_signedLoanAgreementHtml);
        }
        private LoanAgreementHtml _loanAgreementHtml;
        private DocumentStoreDataSet _documentStore;

        List<SignatureModel> _signatureModels = new List<SignatureModel>();
        public bool ShowApplicantAcknowledgement { get; private set; }
        public int LoanTermsRequestId { get; private set; }


        public MvcHtmlString PreSignatureLoanAgreementHtml()
        {
            if (_loanAgreementHtml == null)
            {
                _loanAgreementHtml = CorrespondenceServiceCorrespondenceOperations.GenerateLoanAgreement(ApplicationId);
            }

            return new MvcHtmlString(_loanAgreementHtml.PreSignatureHtml);
        }

        public List<SignatureModel> Signatures
        {
            get
            {
                return _signatureModels;
            }
        }
        public MvcHtmlString PostSignatureLoanAgreementHtml()
        {
            if (_loanAgreementHtml == null)
            {
                _loanAgreementHtml = CorrespondenceServiceCorrespondenceOperations.GenerateLoanAgreement(ApplicationId);
            }
            return new MvcHtmlString(_loanAgreementHtml.PostSignatureHtml);
        }

        public bool IsApplicantSignatureSubmitted()
        {
            return _signatureModels != null &&
                   _signatureModels.Any(x => x.IsCoApplicant == false) &&
                   _signatureModels.Single(x => x.IsCoApplicant == false).Submitted;
        }
        public bool IsCoApplicantSignatureSubmitted()
        {
            return _signatureModels != null &&
                   _signatureModels.Any(x => x.IsCoApplicant == true) &&
                   _signatureModels.Single(x => x.IsCoApplicant == true).Submitted;
        }

        public override void Populate()
        {
            DoAccessCheck();
            base.Populate();
            _documentStore = CorrespondenceServiceCorrespondenceOperations.GetApplicationDocumentStore(ApplicationId);
            if (!HasSignedLoanAgreement())
            {
                if (!CheckForPartialLoanAgreement())
                {
                    _loanAgreementHtml = CorrespondenceServiceCorrespondenceOperations.GenerateLoanAgreement(ApplicationId);
                    SessionUtility.SetLoanAgreement(_loanAgreementHtml);
                }
            }
            GetSignatureModels();

            // Rhode Island
            if (PurposeOfLoanIsSecured() &&
                Application.Applicants.Any(a =>
                    a.GetApplicantPostalAddressRows()
                    .Any(h => h.PostalAddressType == PostalAddressTypeLookup.PostalAddressType.PrimaryResidence && h.State == StateLookup.State.RhodeIsland)))
            {
                ShowApplicantAcknowledgement = true;
            }
        }

        private bool CheckForPartialLoanAgreement()
        {
            if (!Application.IsJoint)
            {
                return false;
            }

            var latestApprovedTerms = _loanOfferDataSet.LatestApprovedLoanTerms;

            var existingAgreementRow = latestApprovedTerms == null ? null : _documentStore.DocumentStore.FirstOrDefault(a =>
                a.CorrespondenceCategory == CorrespondenceCategoryLookup.CorrespondenceCategory.LoanAgreementHtml
                && !a.IsLoanTermsRequestIdNull()
                && a.LoanTermsRequestId == latestApprovedTerms.LoanTermsRequestId);

            if (existingAgreementRow != null)
            {
                byte[] image = CorrespondenceServiceCorrespondenceOperations.GetEDoc(existingAgreementRow.EdocId);

                _loanAgreementHtml = new LoanAgreementHtml(Encoding.UTF8.GetString(image));

                SessionUtility.SetLoanAgreement(_loanAgreementHtml);
                SessionUtility.FoundPartiallySignedLoanAgreementOnFile = true;
                return true;
            }
            return false;
        }

        private void GetSignatureModels()
        {
            _signatureModels = new List<SignatureModel>();

            long ltrId = _loanOfferDataSet.LatestApprovedLoanTerms.LoanTermsRequestId;

            var appSignature = _documentStore.DocumentStore.SingleOrDefault(a =>
              a.CorrespondenceCategory == CorrespondenceCategoryLookup.CorrespondenceCategory.LoanAgreementAppSignature
              && !a.IsLoanTermsRequestIdNull() && a.LoanTermsRequestId == ltrId);

            var existingAgreementRow = _documentStore.DocumentStore.FirstOrDefault(a =>
                a.CorrespondenceCategory == CorrespondenceCategoryLookup.CorrespondenceCategory.LoanAgreementHtml
                && !a.IsLoanTermsRequestIdNull()
                && a.LoanTermsRequestId == ltrId);

            if (existingAgreementRow != null)
            {
                byte[] image = CorrespondenceServiceCorrespondenceOperations.GetEDoc(existingAgreementRow.EdocId);

                var agreement = new LoanAgreementHtml(Encoding.UTF8.GetString(image));

                if (appSignature != null)
                {
                    WebUser.PrimarySignatureImageBytes = CorrespondenceServiceCorrespondenceOperations.GetEDoc(appSignature.EdocId);
                    WebUser.PrimarySignatureTimestamp = agreement.Signature1Text;
                    WebUser.PrimarySignatureOnFile = true;
                }

                if (Application.IsJoint)
                {
                    var coAppSignature = _documentStore.DocumentStore.SingleOrDefault(a =>
                     a.CorrespondenceCategory == CorrespondenceCategoryLookup.CorrespondenceCategory.LoanAgreementCoAppSignature
                     && !a.IsLoanTermsRequestIdNull() && a.LoanTermsRequestId == ltrId);

                    if (coAppSignature != null)
                    {
                        WebUser.SecondarySignatureImageBytes = CorrespondenceServiceCorrespondenceOperations.GetEDoc(coAppSignature.EdocId);
                        WebUser.SecondarySignatureTimestamp = agreement.Signature2Text;
                        WebUser.SecondarySignatureOnFile = true;
                    }
                }
            }

            _signatureModels.Add(new SignatureModel()
            {
                ApplicantName = string.Format("{0} {1}", Application.PrimaryApplicant.FirstName, Application.PrimaryApplicant.LastName),
                ApplicantFullName = Application.PrimaryApplicant.IsMiddleInitialNull()
                    ? string.Format("{0} {1}", Application.PrimaryApplicant.FirstName, Application.PrimaryApplicant.LastName)
                    : string.Format("{0} {1} {2}", Application.PrimaryApplicant.FirstName, Application.PrimaryApplicant.MiddleInitial, Application.PrimaryApplicant.LastName),
                IsCoApplicant = false,
                Submitted = WebUser.PrimarySignatureImageBytes != null,
                TimeStamp = WebUser.PrimarySignatureTimestamp
            });
            if (Application.IsJoint)
            {
                _signatureModels.Add(new SignatureModel()
                {
                    ApplicantName = string.Format("{0} {1}", Application.SecondaryApplicant.FirstName, Application.SecondaryApplicant.LastName),
                    ApplicantFullName = Application.SecondaryApplicant.IsMiddleInitialNull()
                        ? string.Format("{0} {1}", Application.SecondaryApplicant.FirstName, Application.SecondaryApplicant.LastName)
                        : string.Format("{0} {1} {2}", Application.SecondaryApplicant.FirstName, Application.SecondaryApplicant.MiddleInitial, Application.SecondaryApplicant.LastName),
                    IsCoApplicant = true,
                    Submitted = WebUser.SecondarySignatureImageBytes != null,
                    TimeStamp = WebUser.SecondarySignatureTimestamp
                });
            }
        }

        internal PersistLoanAgreementNextStep GetNextStep()
        {
            var result = PersistLoanAgreementNextStep.IsComplete;

            if (IsAfterFundingDropDeadDateTime() || IsAfterDropDeadDateTime())
            {
                result = PersistLoanAgreementNextStep.NLTRRescheduleFundingDate;
            }
            else if (_loanOfferDataSet.SwitchedFromInvoiceToAutoPayPreSign()
                || (!_loanOfferDataSet.HasDebitAccountInfo && _loanOfferDataSet.CurrentLoanTerms.PaymentType == PaymentTypeLookup.PaymentType.AutoPay))
            {
                result = PersistLoanAgreementNextStep.GetNLTRCheckingAccountInfo;
            }
            else if (_loanOfferDataSet.SwitchedFromAutoPayToInvoicePreSign())
            {
                result = PersistLoanAgreementNextStep.SelectEmailPreferences;
            }
            else
            {
                result = PersistLoanAgreementNextStep.IsComplete;
            }
            return result;
        }

        internal PersistLoanAgreementNextStep PersistLoanAgreement()
        {
            new LoanAgreementModel(_customerUserIdDataSet, ApplicationId, WebUser, _loanOfferDataSet.LatestApprovedLoanTerms).PersistLoanAgreement(_loanAgreementHtml);

            EventTypeLookup.EventType eventType = EventTypeLookup.EventType.AcceptedNewLoanTerms;
            if (_loanOfferDataSet.LatestLoanTermsRequest.LoanTermsRequestType == LoanTermsRequestTypeLookup.LoanTermsRequestType.NLTRCounter)
            {
                eventType = EventTypeLookup.EventType.AcceptedCounterOfferTerms;
            }
            var nextStep = GetNextStep();
            if (nextStep == PersistLoanAgreementNextStep.IsComplete)
            {
                DomainServiceLoanApplicationOperations.CompletePreFundingNLTRWithWebActivity(Application, eventType, WebActivityDataSetHelper.Populate(WebUser, Application.ApplicationStatusType));
            }

            return nextStep;
        }

        public enum PersistLoanAgreementNextStep
        {
            IsComplete,
            NLTRRescheduleFundingDate,
            GetNLTRCheckingAccountInfo,
            SelectEmailPreferences
        }

        internal bool HasSignedLoanAgreement()
        {
            try
            {
                if (((Application.ApplicationStatusType == ApplicationStatusTypeLookup.ApplicationStatusType.Approved) || (Application.ApplicationStatusType == ApplicationStatusTypeLookup.ApplicationStatusType.PreFunding))
                    && _loanOfferDataSet.LatestApprovedLoanTerms.Status == LoanTermsRequestStatusLookup.LoanTermsRequestStatus.Approved
                    && !_loanOfferDataSet.LoanOffer[0].IsLoanAgreementEdocIdNull())
                {
                    var row = this._documentStore.DocumentStore
                             .Where(a => a.ApplicationId == ApplicationId && a.CorrespondenceCategory == CorrespondenceCategoryLookup.CorrespondenceCategory.LoanAgreementHtml)
                             .OrderByDescending(a => a.CreatedDate).FirstOrDefault();
                    if (row != null)
                    {
                        // validate that all signatures are ok.
                        byte[] htmlContent = CorrespondenceServiceCorrespondenceOperations.GetEDoc(row.EdocId);
                        string html = FirstAgain.Common.Text.TextDecoder.GetString(htmlContent);
                        _signedLoanAgreementHtml = LoanAgreementHtmlHelper.GetHtmlLoanAgreement(html, ApplicationId, _documentStore, _customerUserIdDataSet);

                        return true;
                    }
                }
            }
            catch (LoanAgreementHtmlHelper.MissingSignatureException ex)
            {
                // if a signature is missing - proceed. Somehow we have a loan agreement on file without signatures, so they need to sign again
                LightStreamLogger.WriteWarning(ex.Message);
            }

            return false;
        }
    }
}