using FirstAgain.Common.Extensions;
using FirstAgain.Common.Wcf;
using FirstAgain.Common.Web;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.ServiceModel.Contracts.MessageContracts.CustomerOperations;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.SharedTypes.IDProfile;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using LightStreamWeb.App_State;
using LightStreamWeb.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace LightStreamWeb.Models.ApplicationStatus
{
    public class ApplicationStatusPageModel : BaseLightstreamPageModel
    {
        int? _applicationId = null;
        protected CustomerUserIdDataSet _customerUserIdDataSet = null;
        protected GetAccountInfoResponse _accountInfo = null;
        protected LoanOfferDataSet _loanOfferDataSet = null;
        protected ICurrentApplicationData _currentApplicationData = null;

        #region so many contructors
        public ApplicationStatusPageModel(LoanOfferDataSet loanOfferDataSet)
        {
            BodyClass = "pre-funding counter";
            EnableFileUpload = true;

            _loanOfferDataSet = loanOfferDataSet;
        }

        public ApplicationStatusPageModel(CustomerUserIdDataSet cuidds)
        {
            BodyClass = "pre-funding counter";
            EnableFileUpload = true;

            _customerUserIdDataSet = cuidds;
        }

        public ApplicationStatusPageModel(ICurrentUser webUser, CustomerUserIdDataSet cuidds)
            : base(webUser)
        {
            BodyClass = "pre-funding counter";
            EnableFileUpload = true;

            _customerUserIdDataSet = cuidds;
        }

        public ApplicationStatusPageModel(GetAccountInfoResponse accountInfo)
        {
            BodyClass = "pre-funding counter";
            EnableFileUpload = true;

            _accountInfo = accountInfo;
            _customerUserIdDataSet = _accountInfo.CustomerUserIdDataSet;
        }
        public ApplicationStatusPageModel(GetAccountInfoResponse accountInfo, LoanOfferDataSet loanOfferDataSet)
        {
            EnableFileUpload = true;
            BodyClass = "pre-funding counter";

            _accountInfo = accountInfo;
            _customerUserIdDataSet = accountInfo.CustomerUserIdDataSet;
            _loanOfferDataSet = loanOfferDataSet;
        }

        public ApplicationStatusPageModel(CustomerUserIdDataSet cuidds, LoanOfferDataSet loanOfferDataSet)
        {
            EnableFileUpload = true;
            BodyClass = "pre-funding counter";

            _customerUserIdDataSet = cuidds;
            _loanOfferDataSet = loanOfferDataSet;
        }

        public ApplicationStatusPageModel(ICurrentApplicationData currentApplicationData)
        {
            EnableFileUpload = true;
            BodyClass = "pre-funding counter";

            _currentApplicationData = currentApplicationData;
        }
        #endregion

        public virtual bool DisplayContactUs()
        {
            return false;
        }

        public static void CancelNLTR(ICurrentUser user, CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet)
        {
            var applicationRow = customerData.Application.FirstOrDefault(a => a.ApplicationId == user.ApplicationId);
            var webActivity = WebActivityDataSetHelper.Populate(user, applicationRow.ApplicationStatusType);
            DomainServiceLoanApplicationOperations.CancelLoanTermsRequestWithWebActivity(applicationRow, loanOfferDataSet.LatestLoanTermsRequest, "Customer cancelled request via web site", webActivity);
        }

        public static bool HasExceptionAfterDecisionBeenTriggered(CustomerUserIdDataSet customerData, int applicationID)
        {
            // Feature 85490 - if the active application has the ExceptionAfterDecision IDPA set, redirect to a new page
            var applicationStatus = customerData.AccountInfo.Applications.SingleOrDefault(a => a.ApplicationId == applicationID)?.ApplicationStatusType;
            if (applicationStatus.IsOneOf(
                ApplicationStatusTypeLookup.ApplicationStatusType.Approved,
                ApplicationStatusTypeLookup.ApplicationStatusType.ApprovedNLTR,
                ApplicationStatusTypeLookup.ApplicationStatusType.PreFundingNLTR,
                ApplicationStatusTypeLookup.ApplicationStatusType.Counter,
                ApplicationStatusTypeLookup.ApplicationStatusType.PreFunding
                ))
            {
                var idpads = DomainServiceIDProfileOperations.GetIDProfileAlertsByApplicationId(applicationID);
                if (idpads != null && idpads.IsIDPASet(IdProfileAlertType.ExceptionAfterDecision))
                {
                    return true;
                }
            }

            return false;
        }

    /// <summary>
    /// Returns the applicant names text that can be displayed directly on a web page,
    /// e.g. "Joe Schmo and Jane Schmo"
    /// </summary>
    public string ApplicantNamesText
        {
            get
            {
                if (_currentApplicationData != null)
                {
                    return _currentApplicationData.ApplicantNames;
                }

                StringBuilder sb = new StringBuilder();
                var primary = Application.Applicants.FirstOrDefault(a => a.ApplicantType == ApplicantTypeLookup.ApplicantType.Primary);
                sb.AppendFormat("{0} {1}", primary.FirstName, primary.LastName);
                if (Application.IsJoint)
                {
                    var secondary = Application.Applicants.FirstOrDefault(a => a.ApplicantType == ApplicantTypeLookup.ApplicantType.Secondary);
                    sb.AppendFormat(" and {0} {1}", secondary.FirstName, secondary.LastName);
                }

                return sb.ToString();
            }
        }

        [Obsolete("Phasing this out in 2019. Hopefully. Be warned.", false)]
        protected CustomerUserIdDataSet.ApplicationRow Application
        {
            get
            {
                if (_customerUserIdDataSet == null || _customerUserIdDataSet.Application == null)
                {
                    return null;
                }
                return _customerUserIdDataSet.Application.FirstOrDefault(x => x.ApplicationId == WebUser.ApplicationId);
            }
        }

        public int ApplicationId
        {
            get
            {
                return _applicationId
                    ?? _currentApplicationData?.ApplicationId
                    ?? WebUser.ApplicationId.GetValueOrDefault();
            }
            protected set
            {
                _applicationId = value;
            }
        }

        public string Ctx
        {
            get
            {
                return WebSecurityUtility.Scramble(ApplicationId);
            }
        }

        public virtual PurposeOfLoanLookup.PurposeOfLoan GetPurposeOfLoan()
        {
            if (_currentApplicationData != null)
            {
                return _currentApplicationData.PurposeOfLoan;
            }

            if (Application == null || Application.GetApplicationDetailRows() == null || Application.GetApplicationDetailRows().Length == 0)
            {
                return PurposeOfLoanLookup.PurposeOfLoan.NotSelected;
            }
            return Application.GetApplicationDetailRows()[0].PurposeOfLoan;
        }

        public ApplicationStatusTypeLookup.ApplicationStatusType CurrentStatus
        {
            get
            {
                return _currentApplicationData?.ApplicationStatus
                        ?? Application?.ApplicationStatusType
                        ?? ApplicationStatusTypeLookup.ApplicationStatusType.NotSelected;
            }
        }

        protected string GetPurposeOfLoanText()
        {
            switch (GetPurposeOfLoan())
            {
                case PurposeOfLoanLookup.PurposeOfLoan.NewAutoPurchaseSecured:
                    return "Secured New Auto Loan";
                case PurposeOfLoanLookup.PurposeOfLoan.UsedAutoPurchaseSecured:
                    return "Secured Used Auto Loan";
                case PurposeOfLoanLookup.PurposeOfLoan.AutoRefinancingSecured:
                    return "Secured Auto Refinance Loan";
                case PurposeOfLoanLookup.PurposeOfLoan.LeaseBuyOutSecured:
                    return "Secured Auto Lease Buyout Loan";
                case PurposeOfLoanLookup.PurposeOfLoan.PrivatePartyPurchaseSecured:
                    return "Secured Auto Private Party Purchase Loan";
                default:
                    return PurposeOfLoanLookup.GetCaption(GetPurposeOfLoan());
            }
        }

        public enum RateModalDisplayType
        {
            NotSelected = 0,
            ReadOnlyPurposeOfLoan, // rates for one loan purpose, read only
            ApplicationRates // show rates for an application. The rates service will determine loan purposes and what can be changed
        }

        public RateModalDisplayType RateModalDisplay { get; set; }

        public ApplicationTypeLookup.ApplicationType ApplicationType
        {
            get
            {
                if (_currentApplicationData != null)
                {
                    return _currentApplicationData.ApplicationType;
                }
                return Application.ApplicationType;
            }
        }
        public bool IsAccountServices()
        {
            return WebUser != null && WebUser.IsAccountServices;
        }

        public bool ApplicationResultedFromAddCoApplicant()
        {
            if (_currentApplicationData != null)
            {
                return _currentApplicationData.ApplicationResultedFromAddCoApplicant;
            }
            return _accountInfo != null && _accountInfo.AddCoApplicantInfo.Any(a => a.ApplicationId == Application.ApplicationId);
        }

        public bool IsPrime5
        {
            get
            {
                var isPrime5 = false;

                if (_currentApplicationData.IsNotNull())
                {
                    isPrime5 = _currentApplicationData.CreditTier.IsPrime5();
                }
                else if (_loanOfferDataSet.IsNotNull() && _loanOfferDataSet.LatestLoanTermsRequest.IsNotNull())
                {
                    isPrime5 = _loanOfferDataSet.LatestLoanTermsRequest.CreditTier.IsPrime5();
                }

                return isPrime5;
            }
        }

        public bool IsPrime6
        {
            get
            {
                var isPrime6 = false;

                if (_currentApplicationData.IsNotNull())
                {
                    isPrime6 = _currentApplicationData.CreditTier.IsPrime6();
                } else if (_loanOfferDataSet.IsNotNull() && _loanOfferDataSet.LatestLoanTermsRequest.IsNotNull())
                {
                    isPrime6 = _loanOfferDataSet.LatestLoanTermsRequest.CreditTier.IsPrime6();
                }

                return isPrime6;
            }
        }

        public bool CanRequestNLTR()
        {
            if (IsPrime5 && FeatureSwitch.DisablePrime5NLTRs)
            {
                return false;
            }
            if (IsPrime6 && FeatureSwitch.DisablePrime6NLTRs)
            {
                return false;
            }

            return true;
        }

        public bool AddCoApplicantIsEnabled()
        {
            if (_currentApplicationData != null)
            {
                return _currentApplicationData.AddCoApplicantIsEnabled;
            }

            return Application != null && Application.FlagIsSet(FlagLookup.Flag.AddCoApplicant) && !Application.FlagIsSet(FlagLookup.Flag.IsInAMLReview);
        }

        public virtual bool HasEnotices()
        {
            if (_currentApplicationData != null)
            {
                return _currentApplicationData.HasEnotices;
            }

            if (_customerUserIdDataSet == null || Application == null)
            {
                return false;
            }
            return _customerUserIdDataSet.DocumentStore.Any(a => a.ApplicationId == Application.ApplicationId && a.IsViewable &&
                (a.CorrespondenceCategory == CorrespondenceCategoryLookup.CorrespondenceCategory.DeclineNotice
                || a.CorrespondenceCategory == CorrespondenceCategoryLookup.CorrespondenceCategory.CreditScoreDisclosureHtml));
        }

        public virtual bool HasDeclineNotice()
        {
            if (_currentApplicationData != null)
            {
                return _currentApplicationData.HasDeclineNotice;
            }

            if (_customerUserIdDataSet == null || Application == null)
            {
                return false;
            }
            return _customerUserIdDataSet.DocumentStore
                    .Any(a => a.ApplicationId == Application.ApplicationId && a.IsViewable &&
                         a.CorrespondenceCategory == CorrespondenceCategoryLookup.CorrespondenceCategory.DeclineNotice);
        }

        public List<Tuple<string, string>> GetEmailPreferences()
        {
            SolicitationPreferenceLookup.FilterType which = GetSolicitationPreferenceType();

            return (from x in SolicitationPreferenceLookup.GetFilteredList(which)
                    select new Tuple<string, string>(x.Enumeration.ToString(), x.Caption)).ToList();
        }

        public SolicitationPreferenceLookup.FilterType GetSolicitationPreferenceType()
        {
            if (_currentApplicationData != null)
            {
                return (_currentApplicationData.PaymentType == PaymentTypeLookup.PaymentType.AutoPay) ? SolicitationPreferenceLookup.FilterType.Autopay : SolicitationPreferenceLookup.FilterType.Invoice;
            }

            if (_loanOfferDataSet != null && _loanOfferDataSet.LatestApprovedLoanTerms != null)
            {
                return (_loanOfferDataSet.LatestApprovedLoanTerms.PaymentType == PaymentTypeLookup.PaymentType.AutoPay) ? SolicitationPreferenceLookup.FilterType.Autopay : SolicitationPreferenceLookup.FilterType.Invoice;
            }

            return (Application.GetApplicationDetailRows()[0].PaymentType == PaymentTypeLookup.PaymentType.AutoPay) ? SolicitationPreferenceLookup.FilterType.Autopay : SolicitationPreferenceLookup.FilterType.Invoice;
        }

        /// <summary>
        /// validate that the application id provided is one of the current customer's accounts
        /// </summary>
        protected void DoAccessCheck()
        {
            var application = _customerUserIdDataSet.Application.FirstOrDefault(a => a.ApplicationId == ApplicationId);
            if (application == null)
            {
                throw new HttpException(403, "Access Denied");
            }
        }
    }
}