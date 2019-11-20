using System.Collections.Generic;
using System.Linq;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Contracts.MessageContracts.CustomerOperations;
using FirstAgain.LoanServicing.SharedTypes;

namespace LightStreamWeb.Models.AccountServices
{
    public class AccountPreferenceModelData
    {
        private GetAccountInfoResponse _accountInfo;

        public bool HasCoApplicant { get; private set; }
        public int UserId { get; private set; }
        public bool CanUpdate { get; set; }
        public bool IsAccountLockEnabled { get; private set; }
        public ApplicantPageModel Applicant { get; private set; }
        public ApplicantPageModel CoApplicant { get; set; }
        public IEnumerable<ApplicationSummaryPageModel> Applications { get; set; }
        public IEnumerable<string> SecondaryUnitTypes { get; private set;}
        public IEnumerable<string> States { get; private set;}
        public IEnumerable<SelectListOptionPageModel> SecurityQuestionTypes { get; private set; }
        public string SecurityHint { get; private set; }
        public string PasswordHint { get; private set; }
        public string UserIdHint { get; private set; }
        public string PasswordErrorHint { get; private set; }
        public string PasswordNoMatchHint { get; private set; }

        public void Populate(GetAccountInfoResponse accountInfo, AccountServicesDataSet accountServicesData)
        {
            _accountInfo = accountInfo;

            IsAccountLockEnabled = _accountInfo.CustomerUserIdDataSet.AccountInfo.IsLockedAccountLock;

            SecondaryUnitTypes = PostalAddressSecondaryUnitTypeLookup.GetFilteredList(PostalAddressSecondaryUnitTypeLookup.FilterType.Home)
                                                                     .Select(type => type.Enumeration.ToString());
            States = StateLookup.GetFilteredBindingSource(StateLookup.FilterType.Inbusiness)
                                .Where(state => state.Enumeration.ToString() != StateLookup.State.NotSelected.ToString())
                                .Select(state => state.Enumeration.ToString());
            SecurityQuestionTypes = SecurityQuestionLookup.BindingSource
                                                          .Where(q => q.Enumeration.ToString() != SecurityQuestionLookup.SecurityQuestion.NotSelected.ToString())
                                                          .Select(SelectListOptionPageModel.CreateSelectListOptionFromBindingSource<SecurityQuestionLookup.SecurityQuestion>());
            SecurityHint = Resources.LoanAppErrorMessages.HintSecurityAnswer;
            PasswordHint = Resources.LoanAppErrorMessages.HintPassword;
            UserIdHint = Resources.LoanAppErrorMessages.HintUserId;
            PasswordErrorHint = Resources.LoanAppErrorMessages.ErrorPasswordRegEx;
            PasswordNoMatchHint = Resources.LoanAppErrorMessages.ErrorPasswordNoMatch;

            UserId = _accountInfo.CustomerUserIdDataSet.ContactInfo.UserId;
            CanUpdate = _accountInfo.CustomerUserIdDataSet.ContactInfo.CanUpdate;
            Applications = _accountInfo.CustomerUserIdDataSet.Applications.Where(app => app.ApplicationStatusType.HasBeenFunded())
                                                                          .Select(ApplicationSummaryPageModel.Populate)
                                                                          .OrderByDescending(model => model.FundingDate);
            var applicantInfo = _accountInfo.CustomerUserIdDataSet.ContactInfo.ApplicantInfo;
            var applicantEmailAndPrivacyPreferences = _accountInfo.CustomerUserIdDataSet.GetCustomerEmailAndPrivacyPreferences(applicantInfo.SSN);
            Applicant = ApplicantPageModel.Populate(applicantInfo, applicantEmailAndPrivacyPreferences);

            var coApplicantInfo = _accountInfo.CustomerUserIdDataSet.ContactInfo.CoApplicantInfo;

            if (coApplicantInfo != null)
            {
                HasCoApplicant = true;
                var coApplicantEmailAndPrivacyPreferences = _accountInfo.CustomerUserIdDataSet.GetCustomerEmailAndPrivacyPreferences(coApplicantInfo.SSN);
                CoApplicant = ApplicantPageModel.Populate(coApplicantInfo, coApplicantEmailAndPrivacyPreferences);
            }
        }
    }
}