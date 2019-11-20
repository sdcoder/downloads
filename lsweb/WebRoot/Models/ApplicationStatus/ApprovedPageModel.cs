using System.Text;
using FirstAgain.Common.Logging;
using FirstAgain.Correspondence.ServiceModel.Client;
using FirstAgain.Domain.Common;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.SharedTypes.InterestRate;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using BusinessCalendarClient = FirstAgain.Domain.ServiceModel.Client.BusinessCalendar;
using LightStreamWeb.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FirstAgain.Common;
using FirstAgain.Correspondence.SharedTypes;
using LightStreamWeb.Helpers;
using LightStreamWeb.ServerState;

namespace LightStreamWeb.Models.ApplicationStatus
{
    public class ApprovedPageModel : ApplicationStatusPageModel, IChangeLoanTermsModel, IFundingAccountModel
    {
        public ApprovedPageModel(CustomerUserIdDataSet cuidds, LoanOfferDataSet loanOfferDataSet)
            : base(cuidds, loanOfferDataSet)
        {
            Populate();
        }

        
        public bool ShowApplicantAcknowledgement { get; private set; }

        public LoanTermsModel LoanTerms { get; private set; }
        public string WelcomeMessage { get; private set; }
        public string SameDayFundingMessage { get; private set; }
        public DateTime OfferCutOffDate { get; private set; }

        public bool CanSwitchToAutoPay()
        {
            var canSwitch = false;

            // can they switch to auto pay? Invoice accounts, that are not SuntrustPremierBanking, will save 0.5%
            if (_loanOfferDataSet.LatestApprovedLoanTerms.PaymentType == PaymentTypeLookup.PaymentType.Invoice)
            {
                var isSuntrustPremierBanking =
                    Application.GetApplicationFlagRows()
                        .Any(r => r.FlagIsOn && r.Flag == FlagLookup.Flag.SuntrustPremierBanking);

                canSwitch = !isSuntrustPremierBanking;
            }
        
            return canSwitch;
        }
        
        public string ChangeLoanTermsMessage { get; private set; }
        public bool HasSignedLoanAgreement { get; private set; }
        public List<SelectListItem> AuthorizedSignerList { get; private set; }
        public bool IsAutoPay { get; private set; }
        public bool ShowWisconsinNonSpouse { get; private set; }
        public bool IsPrimaryWisconsin { get; private set; }
        public bool IsSecondaryWisconsin { get; private set; }
        public bool LastNLTRWasCancelled { get; private set; }
        public bool LastNLTRWasDeclined { get; private set; }
        public bool IsJoint
        {
            get
            {
                return Application != null && Application.IsJoint;
            }
        }

        public bool HasNonApplicantSpouseIncome { get; private set; }

        private FirstAgain.Correspondence.SharedTypes.LoanAgreementHtml _loanAgreementHtml = null;
        private DocumentStoreDataSet _documentStore = null;

        public MvcHtmlString PreSignatureLoanAgreementHtml()
        {
            if (_loanAgreementHtml == null)
            {
                _loanAgreementHtml = CorrespondenceServiceCorrespondenceOperations.GenerateLoanAgreement(ApplicationId);
            }

            return new MvcHtmlString(_loanAgreementHtml.PreSignatureHtml);
        }

        private string _signedLoanAgreementHtml;
        public MvcHtmlString LoanAgreementHtml()
        {
            return new MvcHtmlString(_signedLoanAgreementHtml);
        }

        public MvcHtmlString PostSignatureLoanAgreementHtml()
        {
            if (_loanAgreementHtml == null)
            {
                _loanAgreementHtml = CorrespondenceServiceCorrespondenceOperations.GenerateLoanAgreement(ApplicationId);
            }
            return new MvcHtmlString(_loanAgreementHtml.PostSignatureHtml);
        }

        public override PurposeOfLoanLookup.PurposeOfLoan GetPurposeOfLoan()
        {
            return _loanOfferDataSet.LatestApprovedLoanTerms.PurposeOfLoan;
        }

        private void Populate()
        {
            LoanTerms = new LoanTermsModel();
            RateModalDisplay = RateModalDisplayType.ApplicationRates;

            InterestRates rates = DomainServiceInterestRateOperations.GetApplicationInterestRates(ApplicationId);

            var approvedTerms = _loanOfferDataSet.LatestApprovedLoanTerms;
            var latestLtr = _loanOfferDataSet.LatestLoanTermsRequest;
            _documentStore = CorrespondenceServiceCorrespondenceOperations.GetApplicationDocumentStore(ApplicationId);

            LoanTerms.Populate(approvedTerms);

            // approved loan application or approved NLTR messaging
            if (latestLtr != null && (latestLtr.Status == LoanTermsRequestStatusLookup.LoanTermsRequestStatus.Cancelled || latestLtr.Status == LoanTermsRequestStatusLookup.LoanTermsRequestStatus.Declined))
            {
                if (latestLtr.Status == LoanTermsRequestStatusLookup.LoanTermsRequestStatus.Declined)
                {
                    WelcomeMessage = "We are unable to approve the new loan terms that you requested, however, your previous approved terms are available and ready for your use.  To continue with your previous terms and begin the funding set up of your loan, select <a href=\"#/LoanAgreement\" >Continue with Previous Terms.</a>";
                    LastNLTRWasDeclined = true;
                }
                else if (latestLtr.Status == LoanTermsRequestStatusLookup.LoanTermsRequestStatus.Cancelled)
                {
                    WelcomeMessage = "To accept the loan terms below simply (1) <a href=\"#/LoanAgreement\" >Proceed to Loan Agreement</a> to review and electronically sign your loan agreement; (2) go to Account Set Up and provide us with your preferences - funding date, banking information, monthly payment date, etc. and; (3) complete the final verification process. ";
                    LastNLTRWasCancelled = true;
                }
            }
            else
            {
                if (approvedTerms.IsNewLoanTermsRequest)
                {
                    WelcomeMessage = Resources.FAMessages.LoanOfferNewTermsApproved;
                }
                else 
                {
                    if (GetPurposeOfLoan().IsSecured())
                    {
                        WelcomeMessage = string.Format("Your loan application has been approved for a " + GetPurposeOfLoanText() + "!");
                    }
                    else
                    {
                        WelcomeMessage = Resources.FAMessages.LoanOfferLoanApplicationApproved;
                    }
                }
            }

            // same day funding messaging
            string sameDayCutoff = BusinessConstants.Instance.SameDayFundingCutoffTimeWeb.ToLightStreamTimeString();
            if (LightStreamWeb.Helpers.BusinessCalendarHelper.CanFundToday())
            {
                SameDayFundingMessage = string.Format(Resources.FAMessages.LoanOfferLoanApplicationApprovedParagraph3_SameDay, sameDayCutoff);
            }
            else
            {
                SameDayFundingMessage = string.Format(Resources.FAMessages.LoanOfferLoanApplicationApprovedParagraph3, sameDayCutoff);
            }

            // last date to schedule funding
            TimeSpan window;
            DateTime lastDateToScheduleFunding = BusinessCalendarClient.GetLastDateTimeToScheduleFunding(ApplicationId, out window);
            OfferCutOffDate = lastDateToScheduleFunding.Date.Add(BusinessConstants.Instance.SelectNextDayFundingCutoffTime);

            IsAutoPay = approvedTerms.PaymentType == PaymentTypeLookup.PaymentType.AutoPay;

            // Message displayed on the "change loan terms" page
            ChangeLoanTermsMessage = Resources.FAMessages.LoanTerms_ChangeTerms;
            if (GetPurposeOfLoan().IsSecuredAuto())
            {
                ChangeLoanTermsMessage = Resources.FAMessages.LoanTerms_Approved_SecuredAuto;
            }
            else
            {
                // check for a counter offer to limit the amount if the NLTR
                decimal? counterAmount = null;

                if (this._currentApplicationData != null)
                {
                    counterAmount = _currentApplicationData.CounterAmount;
                }
                else
                {
                    CustomerUserIdDataSet.LoanTermsRequestRow counterData = this._customerUserIdDataSet.LoanTermsRequest.GetCounterLoanTerms(ApplicationId);
                    if (counterData != null)
                    {
                        counterAmount = counterData.Amount;
                    }

                    if (counterAmount != null)
                    {
                        //if current amount is equal to the counter amount
                        if (this._loanOfferDataSet.LatestApprovedLoanTerms.Amount != counterData.Amount)
                        {
                            //current amount must be less than the counter amount
                            ChangeLoanTermsMessage = string.Format(Resources.FAMessages.LoanTerms_Current_Below_Counter, DataConversions.FormatAndGetMoney(counterData.Amount));
                        }
                    }
                }
            }

            // loan agreement 
            if (!CheckForSignedLoanAgreement())
            {
                if (!CheckForPartialLoanAgreement())
                {
                    _loanAgreementHtml = CorrespondenceServiceCorrespondenceOperations.GenerateLoanAgreement(ApplicationId);
                    SessionUtility.SetLoanAgreement(_loanAgreementHtml);
                }
            }

            // account info
            AuthorizedSignerList = LightStreamWeb.Helpers.ApplicationStatusHelper.PopulateAuthorizedSignerList(Application);

            // Wisconsin
            if (Application.Applicants.Any(a =>
                a.GetApplicantPostalAddressRows()
                    .Any(h => h.PostalAddressType == PostalAddressTypeLookup.PostalAddressType.PrimaryResidence && h.State == StateLookup.State.Wisconsin))
                 && Application.RequestedLoanAmount <= 25000m)
            {
                if (Application.PrimaryApplicant.PostalAddresses.Any(
                        d => d.PostalAddressType == PostalAddressTypeLookup.PostalAddressType.PrimaryResidence && d.State == StateLookup.State.Wisconsin))
                    IsPrimaryWisconsin = true;

                if (Application.IsJoint)
                {
                    if (Application.SecondaryApplicant.PostalAddresses.Any(
                            d => d.PostalAddressType == PostalAddressTypeLookup.PostalAddressType.PrimaryResidence && d.State == StateLookup.State.Wisconsin))
                        IsSecondaryWisconsin = true;
                }

                ShowWisconsinNonSpouse = true;
            }

            // Rhode Island
            if (PurposeOfLoanIsSecured() &&
                Application.Applicants.Any(a => 
                    a.GetApplicantPostalAddressRows()
                    .Any(h => h.PostalAddressType == PostalAddressTypeLookup.PostalAddressType.PrimaryResidence && h.State == StateLookup.State.RhodeIsland)))
            {
                ShowApplicantAcknowledgement = true;                  
            }
        }

        internal static bool HasDeclineNotice(CustomerUserIdDataSet customerUserIdDataSet)
        {
            return customerUserIdDataSet != null && customerUserIdDataSet.DocumentStore != null && customerUserIdDataSet.DocumentStore.Any(a => a.CorrespondenceCategory == CorrespondenceCategoryLookup.CorrespondenceCategory.DeclineNotice && a.IsViewable);
        }

        private bool CheckForSignedLoanAgreement()
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


                        HasSignedLoanAgreement = true;

                        byte[] htmlContent = CorrespondenceServiceCorrespondenceOperations.GetEDoc(row.EdocId);
                        string html = FirstAgain.Common.Text.TextDecoder.GetString(htmlContent);
                        _signedLoanAgreementHtml = LoanAgreementHtmlHelper.GetHtmlLoanAgreement(html, ApplicationId, _documentStore, _customerUserIdDataSet);
                    }
                }

            }
            catch (LoanAgreementHtmlHelper.MissingSignatureException ex)
            {
                // if a signature is missing - proceed. Somehoe we have a loan agreement on file without signatures, so they need to sign again
                LightStreamLogger.WriteWarning(ex.Message);
                HasSignedLoanAgreement = false;
            }

            return HasSignedLoanAgreement;
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

        public IEnumerable<SignatureModel> GetSignatureModels()
        {
            long ltrId = _loanOfferDataSet.LatestApprovedLoanTerms.LoanTermsRequestId;

            var existingAgreementRow = _documentStore.DocumentStore.FirstOrDefault(a => 
                a.CorrespondenceCategory == CorrespondenceCategoryLookup.CorrespondenceCategory.LoanAgreementHtml
                && !a.IsLoanTermsRequestIdNull()
                && a.LoanTermsRequestId == ltrId);

            if (existingAgreementRow != null)
            {
                byte[] image = CorrespondenceServiceCorrespondenceOperations.GetEDoc(existingAgreementRow.EdocId);

                var agreement = new LoanAgreementHtml(Encoding.UTF8.GetString(image));

                var appSignature = _documentStore.DocumentStore.SingleOrDefault(a =>
                    a.CorrespondenceCategory == CorrespondenceCategoryLookup.CorrespondenceCategory.LoanAgreementAppSignature
                    && !a.IsLoanTermsRequestIdNull() && a.LoanTermsRequestId == ltrId);

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

            List<SignatureModel> results = new List<SignatureModel>();
            results.Add(new SignatureModel()
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
                results.Add(new SignatureModel()
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
           return results;
        }


        public class SignatureModel
        {
            public string ApplicantName { get; set; }
            public bool IsCoApplicant { get; set; }
            public bool Submitted { get; set; }
            public string TimeStamp { get; set; }
            public string TagName
            {
                get
                {
                    return (IsCoApplicant) ? "CoApplicantSignature" : "ApplicantSignature";
                }
            }
            public string ApplicantFullName { get; set; }

        }

        public bool PurposeOfLoanIsSecured()
        {
            return GetPurposeOfLoan().IsSecured();
        }

        public void SwitchToAutoPay(out bool isAutoApproved)
        {
            var ltr = _loanOfferDataSet.LatestApprovedLoanTerms;
            var loanterms = _loanOfferDataSet.LoanTermsRequest.FindByLoanTermsRequestId(ltr.LoanTermsRequestId);
            InterestRates rates = DomainServiceInterestRateOperations.GetApplicationInterestRates(Application.ApplicationId);
            rates.InterestRateParams.PaymentType = PaymentTypeLookup.PaymentType.AutoPay;

            decimal? interestRate = rates.GetRate(loanterms.TermMonths, loanterms.AmountMinusFees);

            DomainServiceLoanApplicationOperations.CreateNewLoanTermsRequestWithWebActivity(Application, LoanTermsRequestTypeLookup.LoanTermsRequestType.NLTR,
                ltr.ProductRateType,
                ltr.ProductIndexType,
                ltr.ProductPeriodType,
                PaymentTypeLookup.PaymentType.AutoPay,
                ltr.AmountMinusFees,
                ltr.TermMonths,
                interestRate.Value * 100,
                null,
                LightStreamWeb.Helpers.WebActivityDataSetHelper.Populate(WebUser, Application.ApplicationStatusType),
                EventTypeLookup.EventType.NewLoanTermsRequest, out isAutoApproved);

        }

        internal void PersistLoanAgreement()
        {
            new LoanAgreementModel(_customerUserIdDataSet, ApplicationId, WebUser, _loanOfferDataSet.LatestApprovedLoanTerms).PersistLoanAgreement();
        }

        public List<SelectListItem> CCMonthsList
        {
            get
            {
                List<SelectListItem> result = new List<SelectListItem>() { new SelectListItem() };
                for (var i = 1; i <= 12; i++)
                {
                    result.Add(new SelectListItem() { Text = i.ToString() });
                }
                return result;
            }
        }

        public List<SelectListItem> CCYearsList
        {
            get
            {
                List<SelectListItem> result = new List<SelectListItem>() { new SelectListItem() };
                for (var i = DateTime.Now.Year; i <= DateTime.Now.Year + 10; i++)
                {
                    result.Add(new SelectListItem() { Text = i.ToString() });
                }
                return result;
            }
        }

    }
}