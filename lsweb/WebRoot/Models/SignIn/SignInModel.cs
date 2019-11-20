using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.ServiceModel.Contracts.MessageContracts.CustomerOperations;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Common.Extensions;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Security;
using System.Data;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using System.ComponentModel;
using FirstAgain.Domain.SharedTypes.IDProfile;

namespace LightStreamWeb.Models.SignIn
{
    public class SignInModel : BaseCoBrandPageModel
    {
        [Required]
        [MaxLength(50)]
        public string UserId
        {
            get;
            set;
        }

        [Required]
        [MaxLength(50)]
        public string UserPassword { get; set; }

        public bool isTimeout { get; set; } = false;

        public bool IsSignOut { get; set; }
        private bool _InProcess;
        public bool InProcess
        {
            get
            {
                return _InProcess;
            }
            set
            {
                if (value)
                {
                    BodyClass += " in-process";
                }
                _InProcess = value;
            }
        }

        public SignInModel()
        {
            DisplayPasswordSSNPrompt = false;
            DisplayRecoveredUserIdText = false;
            UserId = string.Empty;
            UserPassword = string.Empty;
            BodyClass = "sign-in";
            NgApp = "LightStreamApp";
            IsTempUserId = false;
        }

        public SignInModel(bool? inProcess) : this()
        {
            InProcess = inProcess.GetValueOrDefault();
        }

        public bool IsTempUserId { get; set; }

        [ReadOnly(true)]
        public bool DisplayPasswordSSNPrompt { get; set; }

        [ReadOnly(true)]
        public bool DisplayRecoveredUserIdText { get; set; }

        /// <summary>
        /// validates the user name and password using the membership provider supplied.
        /// Membership provider is passed as a parameter, for mocking / unit testing
        /// </summary>
        /// <param name="membershipProvider"></param>
        /// <returns></returns>
        public LoginResult Login(MembershipProvider membershipProvider)
        {
            if (string.IsNullOrWhiteSpace(UserId))
            {
                return LoginResult.UserIdRequired;
            }
            if (string.IsNullOrWhiteSpace(UserPassword))
            {
                return LoginResult.UserPasswordRequired;
            }

            UserId = UserId.Trim();
            UserPassword = UserPassword.Trim();

            if (!membershipProvider.ValidateUser(UserId, UserPassword))
            {
                return LoginResult.InvalidUserNameOrPassword;
            }

            return LoginResult.Success;
        }

        public enum LoginAccountStatusType
        {
            Fraud,
            Freeze,
            BoardingFailed,
            PassCodeLock,
            AccountLock,
            EmailLock,
            ConfirmEmail,
            InformationReqestRequired,
            HasFundedAccount,
            HasInactiveFundedAccount,
            ActiveApplication,
            ExceptionAfterDecision,
            Failed
        }

        public class LoginAccountStatusResult
        {
            public LoginAccountStatusType Result { get; set; }
            public GetAccountInfoResponse AccountInfo { get; set; }
            public LoanOfferDataSet LoanOfferDataSet { get; set; }
            [ReadOnly(true)]
            public string CustomerMessage { get; set; }
            public int ApplicationId { get; set; }

            public CustomerApplicationsDates ApplicationsDates { get { return AccountInfo == null ? null : AccountInfo.ApplicationsDates; } }

            public CustomerUserIdDataSet CustomerData
            {
                get { return AccountInfo == null ? null : AccountInfo.CustomerUserIdDataSet; }
            }
        }

        public LoginAccountStatusResult GetAccountStatus(string username)
        {
            var accountData = new LoginAccountStatusResult();

            accountData.AccountInfo = DomainServiceCustomerOperations.GetAccountInfoByCustomerUserId(username);

            //PBI74154 AC4.9 still display the page with Fraud flag if app is cancelled (for incomplete and inquiry only)
            if (IsFraudFlagSet(accountData.CustomerData) && IsCancelled(accountData.CustomerData))
            {
                return new LoginAccountStatusResult() { Result = LoginAccountStatusType.Fraud, AccountInfo = accountData.AccountInfo };
            }

            // We have to re-evaluate when we disallow the customer to view decline/credit score disclosure notices.
            // There appear to be some compliance violations here.
            // Here we bypass the freeze and loan boarding failed IDPA conditions for applications where we've already screwed the pooch.
            bool force = accountData.ApplicationsDates != null && accountData.ApplicationsDates.ApplicationsDates.Any(ad => ad.AdverseActionNoticesRetroactivelyAvailable);

            //PBI74154 AC4.9 still display the page with Freeze flag if app is in incomplete and inquiry status, or got terminated.
            if (!force 
                && IsFreezeFlagSet(accountData.CustomerData) 
                && (IsAwaitingCompletion(accountData.CustomerData)|| IsTerminated(accountData.CustomerData))
                )
            {
                return new LoginAccountStatusResult() { Result = LoginAccountStatusType.Freeze, AccountInfo = accountData.AccountInfo };
            }

            if (!force && IsBoardingFailedAlertOn(accountData.CustomerData) && !(IsDeclined(accountData.CustomerData) || IsExpired(accountData.CustomerData)))
            {
                return new LoginAccountStatusResult() { Result = LoginAccountStatusType.BoardingFailed, AccountInfo = accountData.AccountInfo };
            }

            if (accountData.CustomerData.AccountInfo.IsLockedAccountLock)
            {
                accountData.ApplicationId = accountData.CustomerData.AccountInfo.FundedOrClosedAppId;

                if (accountData.CustomerData.AccountInfo.PassCode.IsNotNull()
                        && (accountData.CustomerData.AccountInfo.PassCode.Length > 0)
                        && (accountData.CustomerData.AccountInfo.HasExpiredPassCode == false))
                {
                    accountData.Result = LoginAccountStatusType.AccountLock;
                    return accountData;
                }

                accountData.Result = LoginAccountStatusType.EmailLock;
                return accountData;
            }

            if (accountData.ApplicationsDates != null && !accountData.ApplicationsDates.ApplicationsDates.Any(ad => ad.IsViewable))
            {
                // except... if the status is terminated with a decline notice, this app may have been a 
                // soft pull declined app, in which case we need to allow access to the decline notice
                var declinedTerminatedSoftPullApps = accountData.CustomerData.Application
                    .Where(
                    a => a.ApplicationStatusType == ApplicationStatusTypeLookup.ApplicationStatusType.Terminated &&
                         DateTime.Now.Subtract(a.SubmittedDate).TotalDays <= 150 &&
                         a.GetDocumentStoreRows().Any(d => d.ApplicationId == a.ApplicationId && d.CorrespondenceCategory == CorrespondenceCategoryLookup.CorrespondenceCategory.DeclineNotice));
                if (declinedTerminatedSoftPullApps.Any())
                {
                    return new LoginAccountStatusResult()
                    {
                        Result = LoginAccountStatusType.ActiveApplication,
                        ApplicationId = declinedTerminatedSoftPullApps.OrderByDescending(a => a.CreatedDate).First().ApplicationId,
                        AccountInfo = accountData.AccountInfo
                    };
                }

                return new LoginAccountStatusResult()
                {
                    CustomerMessage = accountData.ApplicationsDates.GetAccountInfoNotice(),
                    Result = LoginAccountStatusType.Failed,
                    AccountInfo = accountData.AccountInfo,
                };
            }

            if (IsEmailConfirmationNeeded(accountData.CustomerData))
            {
                accountData.ApplicationId = accountData.ApplicationsDates.MostRecentFundedOrViewableApplicationId;
                accountData.Result = LoginAccountStatusType.ConfirmEmail;
                return accountData;
            }

            if (accountData.CustomerData.AccountInfo.Applications.Any(a => a.ApplicationStatusType.IsInactiveFunded()))
            {
                accountData.ApplicationId = accountData.CustomerData.Application.Where(y => y.ApplicationStatusType.IsInactiveFunded()).OrderByDescending(y => y.LastModifiedDate).First().ApplicationId;
                accountData.Result = LoginAccountStatusType.HasInactiveFundedAccount;
            }
            else if (accountData.CustomerData.AccountInfo.Applications.Any(a => a.ApplicationStatusType.HasBeenFunded()))
            {
                // SecuredAuto: if VIN is required on any app, redirect there first
                if (accountData.CustomerData.AccountInfo.Applications.Any(a => a.VINEntryRequired))
                {
                    var vinReqApp = accountData.CustomerData.AccountInfo.Applications.FirstOrDefault(a => a.VINEntryRequired);
                    bool supressVINCertProcess = DomainServiceLoanApplicationOperations.SuppressVINEntry(vinReqApp.ApplicationId);
                    if (!supressVINCertProcess)
                    {
                        var viewableAppIds = accountData.ApplicationsDates.ApplicationsDates.Where(ad => ad.IsViewable).Select(ad => ad.ApplicationId);
                        var app = accountData.CustomerData.AccountInfo.Applications.FirstOrDefault(a => viewableAppIds.Contains(a.ApplicationId) && a.VINEntryRequired);
                        if (app != null)
                        {
                            accountData.ApplicationId = app.ApplicationId;
                            accountData.Result = LoginAccountStatusType.InformationReqestRequired;
                            return accountData;
                        }
                    }
                }

                // funded account verification requests
                foreach (var account in accountData.CustomerData.AccountInfo.Applications.Where(a => a.ApplicationStatusType.HasBeenFunded()))
                {
                    var verificationRequests = new ApplicationStatus.VerificationRequestsModel();
                    verificationRequests.Populate(accountData.CustomerData, account.ApplicationId, account.Detail.PurposeOfLoan);

                    if (verificationRequests.Documents.Any() && !verificationRequests.AllDocumentsAreInASatisfiedStatus)
                    {
                        accountData.ApplicationId = account.ApplicationId;
                        accountData.Result = LoginAccountStatusType.InformationReqestRequired;
                        return accountData;
                    }
                }

                accountData.ApplicationId = accountData.ApplicationsDates.MostRecentFundedOrViewableApplicationId;
                accountData.Result = LoginAccountStatusType.HasFundedAccount;
            }
            else if (accountData.CustomerData.AccountInfo.Applications.Any(a => a.ApplicationStatusType.IsApprovedOrPrefunding()))
            {
                accountData.ApplicationId = accountData.CustomerData.Application.Where(y => y.ApplicationStatusType.IsApprovedOrPrefunding()).OrderByDescending(y => y.LastModifiedDate).First().ApplicationId;
                accountData.Result = LoginAccountStatusType.ActiveApplication;
            }
            else if (accountData.CustomerData.AccountInfo.Applications.Any(a => a.ApplicationStatusType.IsActiveApplication()))
            {
                accountData.ApplicationId = accountData.CustomerData.Application.Where(y => y.ApplicationStatusType.IsActiveApplication()).OrderByDescending(y => y.LastModifiedDate).First().ApplicationId;
                accountData.Result = LoginAccountStatusType.ActiveApplication;
            }
            else
            {
                //Display the application with the most recent status modification
                accountData.ApplicationId = accountData.CustomerData.Application.OrderByDescending(y => y.LastModifiedDate).First().ApplicationId;
                accountData.Result = LoginAccountStatusType.ActiveApplication;
            }

            // Feature 85490 - if the active application has the ExceptionAfterDecision IDPA set, redirect to a new page
            var applicationStatus = accountData.CustomerData.AccountInfo.Applications.SingleOrDefault(a => a.ApplicationId == accountData.ApplicationId)?.ApplicationStatusType;
            if (applicationStatus.IsOneOf(
                ApplicationStatusTypeLookup.ApplicationStatusType.Approved,
                ApplicationStatusTypeLookup.ApplicationStatusType.Counter,
                ApplicationStatusTypeLookup.ApplicationStatusType.PreFunding
                ))
            {
                var idpads = DomainServiceIDProfileOperations.GetIDProfileAlertsByApplicationId(accountData.ApplicationId);
                if (idpads != null && idpads.IsIDPASet(IdProfileAlertType.ExceptionAfterDecision))
                {
                    accountData.Result = LoginAccountStatusType.ExceptionAfterDecision;
                }
            }

            return accountData;
        }

        private bool IsEmailConfirmationNeeded(CustomerUserIdDataSet account)
        {
            bool isApplicantEmailConfirmed = account.ContactInfo.ApplicantInfo.HomeEmailAddress.IsConfirmed;

            bool isCoApplicantEmailConfirmed = true;
            if (account.ContactInfo.CoApplicantInfo.IsNotNull())
            {
                isCoApplicantEmailConfirmed = account.ContactInfo.CoApplicantInfo.HomeEmailAddress.IsConfirmed;
            }

            if ((!isApplicantEmailConfirmed
                    || !isCoApplicantEmailConfirmed)
                && account.AccountInfo.Applications.Any(a => a.ApplicationStatusType.HasBeenFunded() || a.ApplicationStatusType.IsActiveApplication()))
            {
                return true;
            }

            return false;
        }

        public string MaintenanceMessage()
        {
            if (MaintenanceConfiguration.IsTemporarilyUnavailable)
            {
                return MaintenanceConfiguration.TemporarilyUnavailableMessage;
            }
            else if (MaintenanceConfiguration.ScheduledMaintenanceStartTime.HasValue)
            {
                return string.Format(MaintenanceConfiguration.ScheduledMaintenanceMessage, MaintenanceConfiguration.ScheduledMaintenanceStartTime.Value.ToShortTimeString());
            }
            else
            {
                return MaintenanceConfiguration.SignInMaintenanceMessage;
            }
        }

        public static bool IsInMaintenanceMode()
        {
            if (MaintenanceConfiguration.IsInMaintenanceMode)
            {
                return true;
            }

            return false;
        }
        public enum LoginResult
        {
            UserIdRequired,
            UserPasswordRequired,
            InvalidUserNameOrPassword,
            Success
        }

        private bool IsBoardingFailedAlertOn(CustomerUserIdDataSet account)
        {
            return account.IdProfileAlert.Any(y => y.AlertIsOn && y.IdProfileAlertTypeId == (short)FirstAgain.Domain.SharedTypes.IDProfile.IdProfileAlertType.Loan_Boarding_Failed);
        }

        private bool IsFreezeFlagSet(CustomerUserIdDataSet account)
        {
            DataRow[] rows = account
                                .ApplicationFlag
                                .Select("FlagId = {0} AND FlagIsOn = 1".FormatWith((short)FirstAgain.Domain.Lookups.FirstLook.FlagLookup.Flag.FreezeAccount));

            return rows.Length > 0;
        }

        private bool IsFraudFlagSet(CustomerUserIdDataSet account)
        {
            DataRow[] rows = account
                                .ApplicationFlag
                                .Select("FlagId = {0} AND FlagIsOn = 1".FormatWith((short)FirstAgain.Domain.Lookups.FirstLook.FlagLookup.Flag.Fraud));

            return rows.Length > 0;
        }

        private bool IsCancelled(CustomerUserIdDataSet account)
        {
            return account.AccountInfo.Applications.Any(a => a.ApplicationStatusType == ApplicationStatusTypeLookup.ApplicationStatusType.Cancelled);
        }

        private bool IsTerminated(CustomerUserIdDataSet account)
        {
            return account.AccountInfo.Applications.Any(a => a.ApplicationStatusType == ApplicationStatusTypeLookup.ApplicationStatusType.Terminated);
        }

        private bool IsAwaitingCompletion(CustomerUserIdDataSet account)
        {
            return account.AccountInfo.Applications.Any(a => a.ApplicationStatusType.IsAwaitingCompletion());
        }

        private bool IsDeclined(CustomerUserIdDataSet account)
        {
            return account.AccountInfo.Applications.Any(a => a.ApplicationStatusType == ApplicationStatusTypeLookup.ApplicationStatusType.Declined);
        }

        private bool IsExpired(CustomerUserIdDataSet account)
        {
            return account.AccountInfo.Applications.Any(a => a.ApplicationStatusType == ApplicationStatusTypeLookup.ApplicationStatusType.Expired);
        }

        public string ToJSON()
        {
            var simpleObject = new
            {
                UserId = UserId,
                IsTempUserId = IsTempUserId,
                IsSignOut = IsSignOut
            };

            //Need to escape apostrophe in Name, so can populate JSON obj Bug 17674
            return Newtonsoft.Json.JsonConvert.SerializeObject(simpleObject).Replace("'", "\\'");
        }
    }
}