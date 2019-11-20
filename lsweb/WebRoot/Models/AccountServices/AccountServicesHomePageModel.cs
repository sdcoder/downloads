using FirstAgain.Domain.ServiceModel.Contracts.MessageContracts.CustomerOperations;
using FirstAgain.LoanServicing.SharedTypes;
using FirstAgain.Domain.Lookups.FirstLook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirstAgain.Domain.ServiceModel.Client;
using System.Web.Mvc;
using FirstAgain.Common;
using System.Configuration;

namespace LightStreamWeb.Models.AccountServices
{
    public class AccountServicesHomePageModel : AccountServicesPageModel
    {
        [Obsolete("GetAccountInfoResponse is required", error: true)]
        public AccountServicesHomePageModel() {}

        private AccountServicesDataSet _accountServicesData = null;
        public AccountServicesHomePageModel(GetAccountInfoResponse accountInfo, AccountServicesDataSet accountServicesData) : base(accountInfo)
        {
            _accountServicesData = accountServicesData;
            Heading = "Account Services";
        }

        public IEnumerable<LoanSummaryItem> GetViewableLoans()
        {
            List <LoanSummaryItem> results = new List<LoanSummaryItem>();
            foreach (var loanMasterRow in _accountServicesData.LoanMaster.SortedByRecentAndActive.Where(a => _accountInfo.ApplicationsDates.ViewableFundedAccounts.Contains(a.ApplicationId)))
            {
                var app = _accountInfo.CustomerUserIdDataSet.AccountInfo.Applications.Single(a => a.ApplicationId == loanMasterRow.ApplicationId);
                var appDetail = app.GetApplicationDetailRows().First();
                var retailLoanMasterRow = _accountServicesData.GetRetailLoanMasterByApplicationId(loanMasterRow.ApplicationId);
                var verificationRequests = new ApplicationStatus.VerificationRequestsModel();
                verificationRequests.Populate(_accountInfo.CustomerUserIdDataSet, appDetail.ApplicationId, appDetail.PurposeOfLoan);

                results.Add(new LoanSummaryItem()
                {
                    ApplicationId = loanMasterRow.ApplicationId,
                    Nickname = (appDetail.IsApplicationNickNameNull()) ? string.Empty : appDetail.ApplicationNickName,
                    InformationReqestRequired = (app.GetLoanContractRows().Any(lc => lc.LoanOfferRow.LoanTermsRequestRow.PurposeOfLoan.IsSecuredAuto()) && app.VINEntryRequired) ||
                                                (verificationRequests.Documents.Any() && !verificationRequests.AllDocumentsAreInASatisfiedStatus),
                    OriginalLoanAmount = loanMasterRow.OriginalLoanAmount,
                    OriginalInterestRate = loanMasterRow.OriginalInterestRate,
                    OriginalTermMonths = loanMasterRow.OriginalTermMonths,
                    FundingDate = loanMasterRow.FundingDate,
                    LoanBalanceCurrent = retailLoanMasterRow.LoanBalanceCurrent,
                    CurrentInterestRate = retailLoanMasterRow.CurrentInterestRate,
                    NextPaymentDueDate = loanMasterRow.IsUINextPaymentDateNull() ? "N/A" : loanMasterRow.UINextPaymentDate.ToShortDateString(),
                    NextPaymentAmount = loanMasterRow.IsUINextPaymentAmountNull() ? (decimal?)null : loanMasterRow.UINextPaymentAmount,
                    IsClosed = loanMasterRow.AccountStatus == FirstAgain.Domain.Lookups.ShawData.AccountStatusLookup.AccountStatus.Closed
                });
            }
            return results;
        }

        public string GetEmailsGreeting()
        {
            //email addresses
            if (_accountInfo.ApplicationsDates.NoViewableFundedAccounts() == false)
            {
                var contactInfo = _accountInfo.CustomerUserIdDataSet.ContactInfo;
                var coApp = contactInfo.CoApplicantInfo != null;

                var appEmail = contactInfo.ApplicantInfo.HomeEmailAddress.EmailAddress;
                var coAppEmail = coApp ? contactInfo.CoApplicantInfo.HomeEmailAddress.EmailAddress : "";

                var addrAddrs = coApp ? "addresses" : "address";
                var isAre = coApp ? "are" : "is";
                var thisThese = coApp ? "these" : "this";
                var itThem = coApp ? "them" : "it";
                var emailEmails = coApp ? "EMAILS" : "EMAIL";
                var email = coApp ? $"<b>{appEmail}</b> and <b>{coAppEmail}</b>" : $"<b>{appEmail}</b>";

                return $"Your current email {addrAddrs} {isAre} {email}. If {thisThese} {isAre} not correct, please update {itThem} now. Thank you. " +
                    $"<br/><a href=\"/profile?/#/contactInformation/updateEmail\" id=\"updateEmailLink\" class=\"account-services-blue-button\">UPDATE {emailEmails}</a>";
            }

            return string.Empty;
        }

        public bool GetDisplayCreditBureauAccountServicesMessage()
        {
            return ConfigurationManager.AppSettings["DisplayCreditBureauAccountServicesMessage"] != null && ConfigurationManager.AppSettings["DisplayCreditBureauAccountServicesMessage"] == "true";

        }

        public string GetEmails()
        {
            //email addresses
            if (_accountInfo.ApplicationsDates.NoViewableFundedAccounts() == false)
            {
                var contactInfo = _accountInfo.CustomerUserIdDataSet.ContactInfo;

                // individual app
                if (contactInfo.CoApplicantInfo == null)
                {
                    return contactInfo.ApplicantInfo.HomeEmailAddress.EmailAddress;
                }
                //joint app
                else
                {
                    return String.Format("{0} and {1}", contactInfo.ApplicantInfo.HomeEmailAddress.EmailAddress, contactInfo.CoApplicantInfo.HomeEmailAddress.EmailAddress);
                }
            }

            return string.Empty;
        }

        public string GetOtherApplicationDisplayText(ApplicationStatusTypeLookup.ApplicationStatusType status)
        {
            switch (status)
            {
                case ApplicationStatusTypeLookup.ApplicationStatusType.Expired:
                    return $"Your loan application has {status.ToString().ToLower()}";

                case ApplicationStatusTypeLookup.ApplicationStatusType.Withdrawn:
                case ApplicationStatusTypeLookup.ApplicationStatusType.Cancelled:
                case ApplicationStatusTypeLookup.ApplicationStatusType.Declined:
                    return $"Your loan application has been {status.ToString().ToLower()}";

                default:
                    return "Your application in progress";
            }
        }

        // email preferences tab
        public IEnumerable<EmailPreference> GetEmailPreferences(int applicationId)
        {
            var which = SolicitationPreferenceLookup.FilterType.Invoice;

            var loanMaster = _accountServicesData.GetLoanMasterByApplicationId(applicationId);
            var application = _accountInfo.CustomerUserIdDataSet.Application.FindByApplicationId(applicationId);

            if (loanMaster.PaymentType == FirstAgain.Domain.Lookups.ShawData.PaymentTypeLookup.PaymentType.AutoPay)
            {
                which = SolicitationPreferenceLookup.FilterType.Autopay;
            }

            return from x in SolicitationPreferenceLookup.GetFilteredList(which)
                    select new EmailPreference()
                    {
                        Caption = x.Caption,
                        Enumeration = x.Enumeration,
                        IsSelected = application.GetEmailPreference(x.Enumeration)
                    };
        }

        public class EmailPreference
        {
            public string Caption { get; set; }
            public FirstAgain.Domain.Lookups.FirstLook.SolicitationPreferenceLookup.SolicitationPreference Enumeration { get; set; }
            public bool IsSelected { get; set; }
        }

        public List<ActiveApplication> GetOtherApplications()
        {
            List<ActiveApplication> apps = new List<ActiveApplication>();

            foreach (var app in _accountInfo
                                    .CustomerUserIdDataSet
                                    .AccountInfo
                                    .Applications
                                    .Where(y => _accountInfo.ApplicationsDates.ViewableApplications.Contains(y.ApplicationId)))
            {
                if (_accountInfo.ApplicationsDates.IsAppViewable(app.ApplicationId) && !app.ApplicationStatusType.HasBeenFunded())
                {
                    decimal amount;
                    var lods = DomainServiceLoanApplicationOperations.GetLoanOffer(app.ApplicationId);
                    var ltr = lods.LatestApprovedLoanTerms;
                    if (ltr == null)
                    {
                        amount = app.Detail.AmountMinusFees;
                    }
                    else
                    {
                        amount = ltr.Amount;
                        if (app.ApplicationStatusType != ApplicationStatusTypeLookup.ApplicationStatusType.PreFunding &&
                            app.ApplicationStatusType != ApplicationStatusTypeLookup.ApplicationStatusType.PreFundingNLTR)
                        {
                            amount = ltr.AmountMinusFees;
                        }

                    }
                    apps.Add(new ActiveApplication()
                    {
                        ApplicationId = app.ApplicationId,
                        SubmittedDate = app.SubmittedDate,
                        LoanAmount = amount,
                        CurrentStatus = app.ApplicationStatusType,
                        PurposeOfLoan = app.GetApplicationDetailRows()[0].PurposeOfLoan
                    });
                }
            }

            return apps;
        }

        public class ActiveApplication
        {
            public int ApplicationId { get; set; }
            public DateTime SubmittedDate { get; set; }
            public decimal LoanAmount { get; set; }
            public ApplicationStatusTypeLookup.ApplicationStatusType CurrentStatus { get; set; }
            public PurposeOfLoanLookup.PurposeOfLoan PurposeOfLoan { get; set; }
        }


        public class LoanSummaryItem
        {
            public int ApplicationId { get; set; }
            public string Nickname { get; set; }
            public bool InformationReqestRequired { get; set; }
            public decimal OriginalLoanAmount { get; set; }
            public decimal OriginalInterestRate { get; set; }
            public int OriginalTermMonths { get; set; }
            public DateTime FundingDate { get; set; }
            public decimal LoanBalanceCurrent { get; set; }
            public decimal CurrentInterestRate { get; set; }
            public string NextPaymentDueDate { get; set; }
            public decimal? NextPaymentAmount { get; set; }
            public bool IsClosed { get; set; }

            public bool HasNickname
            {
                get
                {
                    return !string.IsNullOrEmpty(Nickname);
                }
            }
        }
    }
}