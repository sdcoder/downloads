using FirstAgain.Common;
using FirstAgain.Common.Logging;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.LoanServicing.ServiceModel.Client;
using ShawLookups = FirstAgain.Domain.Lookups.ShawData;

using LightStreamWeb.App_State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FirstAgain.LoanServicing.SharedTypes;
using FirstAgain.Domain.Common;
using FirstAgain.Common.Web;
using FirstAgain.Common.Extensions;
using System.Collections;
using System.ComponentModel;
using System.IO;

namespace LightStreamWeb.Models.AccountServices
{
    [Serializable]
    public class FundedAccountModel
    {
        public FundedAccountModel()
        {
            
        }
        public int ApplicationId { get; set; }


        private string _nickname;
        public string Nickname 
        { 
            get
            {
                return (_nickname == null) ? string.Empty : _nickname.Truncate(25, true);
            }
            set
            {
                _nickname = value;
            }
        }

        #region secured auto
        public bool InformationReqestRequired { get; set; }
        public bool IsSecuredAuto { get; set; }
        public string VehicleYear { get; set; }
        public string VehicleMake { get; set; }
        public string VehicleModel { get; set; }
        public string VehicleDescription { get; set; }
        #endregion
        public decimal? FloridaDocStampTax { get; set; }

        public string CompanyLegalName { get; set; }
        public decimal OriginalLoanAmount { get; set; }
        public decimal OriginalInterestRate { get; set; }
        public int OriginalTermMonths { get; set; }
        public string FundingDate { get; set; }
        public decimal LoanBalanceCurrent { get; set; }
        public decimal CurrentInterestRate { get; set; }
        public string NextPaymentDueDate { get; set; }
        public DateTime MonthlyPaymentChangeEffectiveDate { get; set; }
        public decimal? NextPaymentAmount { get; set; }
        public bool IsClosed { get; set; }
        public bool Active { get; set; }
        public bool EligibleToViewAmortizationSchedule { get; set; }
        public bool MayBeEligibleForReamortization { get; set; }
        public DateTime? ReamortizationDate { get; set; }

        public ShawLookups.PaymentTypeLookup.PaymentType PaymentType { get; set; }
        public List<SelectListItem> AuthorizedSignerList { get; set; }
        public bool IsJoint { get; set; }

        public decimal MonthlyPayment { get; set; }
        public int PaymentDayOfMonth { get; set; }
        public string NextNextPayemntDueDate { get; set; }

        #region account info
        public string DebitAccountNumber { get; set; }
        public string DebitRoutingNumber { get; set; }
        public bool HasPaymentAccount
        {
            get
            {
                return !string.IsNullOrEmpty(DebitAccountNumber);
            }
        }
        public string AuthorizedSigner { get; set; }
        public string RoutingNumber { get; set; }
        public string AccountNumber { get; set; }
        public bool? IsCheckingAccount { get; set; }

        public string PaymentAccountMessage { get; set; }
        public AccountSelection? PaymentAccountSelection { get; set; }
        public enum AccountSelection
        {
            PaymentAccount,
            DepositAccount,
            NewAccount
        }
        #endregion

        public decimal? NewMonthlyPaymentAmount { get; set; }
        public decimal MinimumMonthlyPaymentAmount { get; set; }
        public decimal MaximumMonthlyPaymentAmount { get; set; }
        public decimal ContractualMonthlyPaymentAmount { get; set; }
        public decimal MinPaymentForReamortization
        {
            get
            {
                return ContractualMonthlyPaymentAmount - 10.0m;
            }
        }
        public DateTime? DecreasedMonthlyPaymentEffectiveDate { get; set; }
        public decimal? QueuedMonthlyPaymentAmount { get; set; }
        public DateTime? QueuedMonthlyPaymentDate { get; set; }
        public bool HasPendingAutoDebitPaymentException { get; set; }
        public ShawLookups.AccountStatusLookup.AccountStatus AccountStatus { get; set; }
        public decimal CurrentPayoffAmount { get; set; }

        public string Ctx
        {
            get
            {
                return WebSecurityUtility.Scramble(ApplicationId);
            }
        }

        #region payments and payoffs
        public string PayOffReason { get; set; }
        public string PayOffComments { get; set; }
        public DateTime? PayOffByAutoPayDate { get; set; }
        public decimal? ScheduledPaymentAmount { get; set; }
        public DateTime? ScheduledPaymentDate { get; set; }
        public DateTime? LastDayToCancelScheduledPayment { get; set; }
        public bool IsOkToCancelScheduledPayment { get; set; }
        public bool HasScheduledPayment
        {
            get
            {
                return ScheduledPaymentAmount.HasValue;
            }
        }
        public decimal? PayOffIncludedACHPayment { get; set; }
        public string PayOffIncludedACHPaymentDate { get; set; }
        public enum PaymentPayoffsSteps
        {
            HasScheduledPayment,
            HasScheduledPayoff,
            SchedulePayment,
            HasACHPaymentException,
            PaymentByACH,
            PayOffByACH,
            PayOffAutoPayByMail,
            PaymentByInvoice,
            PayOffByInvoice
        }

        public PaymentPayoffsSteps? ExtraPaymentCurrentStep { get; set; }
        public string ReturnCodeText { get; set; }

        public enum ACHReturnCodeType
        {
            R020304,
            R05Plus
        }
        public ACHReturnCodeType ReturnCodeType { get; set; }
        public bool EInvoicePayDaysEnabled { get; set; }
        #endregion

        #region Documents / ENotices / Loan Agreements
        public List<LightStreamWeb.Models.ENotices.ENoticesModalModel.ENoticeRow> Documents { get; set; }
        public LightStreamWeb.Models.ENotices.ENoticesModalModel.ENoticeRow LoanAgreement { get; set; }
        public LightStreamWeb.Models.ENotices.ENoticesModalModel.ENoticeRow LatestInvoice { get; set; }
        public LightStreamWeb.Models.ENotices.ENoticesModalModel.ENoticeRow DisclosureStatement { get; set; }
        #endregion

        public List<EmailPreference> EmailPreferences { get; set; }
        public List<TransactionHistoryItem> PaymentHistory { get; set; }
        public List<TransactionHistoryItem> AmortizationSchedule { get; set; }

        public string MaskedDebitAccountNumber
        {
            get
            {
                if (DebitAccountNumber != null && DebitAccountNumber.Trim().Length >= 4)
                {
                    return "**" + DebitAccountNumber.Substring(DebitAccountNumber.Trim().Length - 4);
                }
                return string.Empty;
            }
        }

        public bool HasNickname
        {
            get
            {
                return !string.IsNullOrEmpty(Nickname);
            }
        }

        public decimal ProjectedMinimumMonthlyPaymentAmount { get; set; }
        public bool CanViewMonthlyPayment { get; set; }

        public class EmailPreference
        {
            public string Caption { get; set; }
            public SolicitationPreferenceLookup.SolicitationPreference Enumeration { get; set; }
            public bool IsSelected { get; set; }
        }

        #region inner classes
        // TransactionHistoryItem
        public class TransactionHistoryItem
        {   
            public string TransactionDate { get; set; }
            public double StartingPrincipalBalance { get; set; }
            public double TransactionAmount { get; set; }
            public double InterestAmount { get; set; }
            public double PrincipalAmount { get; set; }
            public double EndingPrincipalBalance { get; set; }
            public bool HasBeenPosted { get; set; }
        }
        #endregion


        #region Action methods
        // UpdateENotices
        // the EmailPreference and ApplicationId are supplied by the client.
        // Validate the application id is valid for this user - by checking against data set supplied by session
        // update against back end, return true
        public bool UpdateENotices(CustomerUserIdDataSet customerData, ICurrentUser webUser)
        {
            Guard.AgainstNull<CustomerUserIdDataSet>(customerData, "customerData");
            if (ApplicationId == 0)
            {
                LightStreamLogger.WriteWarning("ApplicationId is zero in FundedAccountModel.UpdateENotices");
                return false;
            }

            var applicationRow = customerData.Application.FindByApplicationId(ApplicationId);
            if (applicationRow == null)
            {
                LightStreamLogger.WriteWarning(string.Format("Application {0} not found in customer data set, in method UpdateENotices", ApplicationId));
                throw new HttpException(403, "Access Denied");
            }

            if (EmailPreferences != null)
            {
                foreach (var p in EmailPreferences)
                {
                    applicationRow.SetEmailPreference(p.Enumeration, p.IsSelected);
                }
            }

            DomainServiceCustomerOperations.UpdateCustomerUserIdData(ApplicationId, customerData, Helpers.EventAuditLogHelper.PopulateEventAuditLogDataSet(customerData, webUser, EventTypeLookup.EventType.UpdateENotices, null, applicationId: ApplicationId));
            return true;

        }
        #endregion

        public bool SwitchToAutoPay(AccountServicesDataSet accountServicesData, CustomerUserIdDataSet customerData, ICurrentUser WebUser, out string errorMessage)
        {
            Guard.AgainstNull<CustomerUserIdDataSet>(customerData, "customerData");
            if (ApplicationId == 0)
            {
                LightStreamLogger.WriteWarning("ApplicationId is zero in FundedAccountModel.SwitchToAutoPay");
                errorMessage = Resources.FAMessages.GenericError;
                return false;
            }

            var applicationRow = customerData.Application.FindByApplicationId(ApplicationId);
            if (applicationRow == null)
            {
                LightStreamLogger.WriteWarning(string.Format("Application {0} not found in customer data set, in method SwitchToAutoPay", ApplicationId));
                throw new HttpException(403, "Access Denied");
            }
            
            string error;
            bool changeWasSuccessful = true;
            changeWasSuccessful = LoanServicingOperations.ChangePaymentMethod(
                            ApplicationId,
                            ShawLookups.PaymentTypeLookup.PaymentType.AutoPay,
                            RoutingNumber.Trim(),
                            AccountNumber.Trim(),
                            (IsCheckingAccount.GetValueOrDefault()) ? FirstAgain.Domain.Lookups.ShawData.BankAccountTypeLookup.BankAccountType.CheckingAccount : ShawLookups.BankAccountTypeLookup.BankAccountType.SavingsAccount,
                            ShawLookups.BankAccountHolderNameTypeLookup.GetEnumeration(AuthorizedSigner),
                            accountServicesData,
                            Helpers.EventAuditLogHelper.PopulateEventAuditLogDataSet(customerData, WebUser, EventTypeLookup.EventType.ChangePaymentMethod, null, ApplicationId),
                            out error);

            if (changeWasSuccessful)
            {
                DomainServiceCustomerOperations.ResetApplicationSolicitationPreference(ApplicationId, SolicitationPreferenceLookup.SolicitationPreference.EInvoicePayDays);

                Helpers.EventAuditLogHelper.Submit(customerData, WebUser, EventTypeLookup.EventType.AutoPayAuthorizationAgreed, null, ApplicationId);
                errorMessage = string.Empty;
                return true;
            }
            else
            {
                errorMessage = "The bank account routing number entered is invalid. Please try again";
                return false;
            }
        }

        public DateTime ACHDebitFileHasBeenCreated(AccountServicesDataSet accountServicesData)
        {
            DateTime achDebitPrepDate = LoanServicingOperations.GetACHDebitPrepSettlementDate();
            DateTime nextPaymentDate = Convert.ToDateTime(NextPaymentDueDate);
            DateTime nextNextPaymentBankingDate = DateTime.MinValue;

            if (achDebitPrepDate < nextPaymentDate)
            {
                //Debit file not created
                nextNextPaymentBankingDate = nextPaymentDate;
            }
            else
            { 
                //Debit file has been created show extra text
                var lmRow = accountServicesData.GetLoanMasterByApplicationId(ApplicationId);
                int dayOfTheMonthPaymentDue = Convert.ToInt16(lmRow.OriginalMonthlyDueDate);

                DateTime paymentDatePlusOneMonth = nextPaymentDate.AddMonths(1);
                if (dayOfTheMonthPaymentDue == 99)
                    dayOfTheMonthPaymentDue = DateTime.DaysInMonth(paymentDatePlusOneMonth.Year, paymentDatePlusOneMonth.Month);
                DateTime nextNextPaymentDate = new DateTime(paymentDatePlusOneMonth.Year, paymentDatePlusOneMonth.Month, dayOfTheMonthPaymentDue);
                nextNextPaymentBankingDate = DomainServiceLoanApplicationOperations.GetClosestBankingDay(nextNextPaymentDate);
            }

            return nextNextPaymentBankingDate;
        }

        internal bool UpdatePaymentAccount(AccountServicesDataSet accountServicesData, CustomerUserIdDataSet customerData, BusinessCalendarDataSet businessCalendar, ICurrentUser WebUser, out string errorMessage, out string effectiveDate)
        {
            Guard.AgainstNull<CustomerUserIdDataSet>(customerData, "customerData");
            if (ApplicationId == 0)
            {
                LightStreamLogger.WriteWarning("ApplicationId is zero in FundedAccountModel.SwitchToAutoPay");
                errorMessage = Resources.FAMessages.GenericError;
                effectiveDate = string.Empty;
                return false;
            }

            var applicationRow = customerData.Application.FindByApplicationId(ApplicationId);
            if (applicationRow == null)
            {
                LightStreamLogger.WriteWarning(string.Format("Application {0} not found in customer data set, in method SwitchToAutoPay", ApplicationId));
                throw new HttpException(403, "Access Denied");
            }

            string error;
            bool changeWasSuccessful = true;
            changeWasSuccessful = LoanServicingOperations.UpdateDebitAccountInfo(
                            ApplicationId,
                            RoutingNumber.Trim(),
                            AccountNumber.Trim(),
                            ShawLookups.BankAccountHolderNameTypeLookup.GetEnumeration(AuthorizedSigner),
                            (IsCheckingAccount.GetValueOrDefault()) ? FirstAgain.Domain.Lookups.ShawData.BankAccountTypeLookup.BankAccountType.CheckingAccount : ShawLookups.BankAccountTypeLookup.BankAccountType.SavingsAccount,
                            accountServicesData,
                            Helpers.EventAuditLogHelper.PopulateEventAuditLogDataSet(customerData, WebUser, EventTypeLookup.EventType.ModifiedBankingInfo, null, ApplicationId),
                            out error);

            if (changeWasSuccessful)
            {
                errorMessage = string.Empty;
                effectiveDate = businessCalendar.GetBankingDateNBankingDaysFromDate(DateTime.Today, BusinessConstants.Instance.OnlinePmtInfoChgLeadDays).ToLongDateString();
                return true;
            }
            else
            {
                errorMessage = "The bank account routing number entered is invalid. Please try again";
                effectiveDate = string.Empty;
                return false;
            }
        }

        public bool UpdateMonthlyPayment(AccountServicesDataSet accountServicesData, CustomerUserIdDataSet customerData, BusinessCalendarDataSet businessCalendar, ICurrentUser WebUser, out string effectiveDate)
        {
            Guard.AgainstNull<CustomerUserIdDataSet>(customerData, "customerData");
            if (ApplicationId == 0)
            {
                LightStreamLogger.WriteWarning("ApplicationId is zero in FundedAccountModel.SwitchToAutoPay");
                effectiveDate = string.Empty;
                return false;
            }
            if (NewMonthlyPaymentAmount.IsNull() || (decimal)NewMonthlyPaymentAmount.GetValueOrDefault() < MinimumMonthlyPaymentAmount)
            {
                effectiveDate = string.Empty;
                return false;
            }

            var applicationRow = customerData.Application.FindByApplicationId(ApplicationId);
            if (applicationRow == null)
            {
                LightStreamLogger.WriteWarning(string.Format("Application {0} not found in customer data set, in method SwitchToAutoPay", ApplicationId));
                throw new HttpException(403, "Access Denied");
            }

            string note = string.Format("Changed monthly payment amount to {0:C}.", NewMonthlyPaymentAmount.GetValueOrDefault());

            LoanServicingOperations.ChangeMonthlyPaymentAmount(
                    ApplicationId, 
                    (double)NewMonthlyPaymentAmount.GetValueOrDefault(), 
                    accountServicesData,
                    Helpers.EventAuditLogHelper.PopulateEventAuditLogDataSet(customerData, WebUser, EventTypeLookup.EventType.ChangeMonthlyPaymentAmt, note, ApplicationId));

            var retailLoanMasterRow = accountServicesData.GetRetailLoanMasterByApplicationId(ApplicationId);
            var nextPaymentDate = businessCalendar.GetPaymentDueDate(retailLoanMasterRow.PaymentDueDate);
            if (nextPaymentDate < businessCalendar.GetBankingDateNBankingDaysFromDate(DateTime.Now, BusinessConstants.Instance.OnlinePmtInfoChgLeadDays))
            {
                nextPaymentDate = businessCalendar.GetPaymentDueDate(nextPaymentDate.AddMonths(1));
            }

            // use case where reamortization occurs 
            if (this.MayBeEligibleForReamortization)
            {
                if (this.NewMonthlyPaymentAmount < this.MinPaymentForReamortization)
                {
                    nextPaymentDate = this.DecreasedMonthlyPaymentEffectiveDate.GetValueOrDefault(nextPaymentDate);
                }
            }

            effectiveDate = String.Format("{0: dddd, MMMM d, yyyy}", nextPaymentDate);

            return true;
        }

        internal static bool UpdateNickname(CustomerUserIdDataSet customerData, ICurrentUser webUser, int? ApplicationId, string NewNickname)
        {
            Guard.AgainstNull<CustomerUserIdDataSet>(customerData, "customerData");
            if (ApplicationId.GetValueOrDefault() == 0)
            {
                LightStreamLogger.WriteWarning("ApplicationId is zero in FundedAccountModel.UpdateNickname");
                return false;
            }
            var applicationRow = customerData.Application.FindByApplicationId(ApplicationId.Value);
            if (applicationRow == null)
            {
                LightStreamLogger.WriteWarning(string.Format("Application {0} not found in customer data set, in method UpdateNickname", ApplicationId));
                throw new HttpException(403, "Access Denied");
            }

            //Profanity Check
            var serializer = new Newtonsoft.Json.JsonSerializer();
            using (var sr = new StreamReader(File.OpenRead(HttpContext.Current.Server.MapPath("~/bundles/bad-words.json"))))
            using (var jsonTextReader = new Newtonsoft.Json.JsonTextReader(sr))
            {
                var badwords = Newtonsoft.Json.Linq.JObject.Parse(serializer.Deserialize(jsonTextReader).ToString()).First.First.ToArray();
                if (badwords.Contains(NewNickname.ToLower())) {
                    LightStreamLogger.WriteWarning("User attempted to set account nickname using a word that's not permitted: " + NewNickname);
                    return false;
                }
            }

            applicationRow.GetApplicationDetailRows().First().ApplicationNickName = NewNickname;

            DomainServiceCustomerOperations.UpdateCustomerUserIdData(
                ApplicationId.Value, 
                customerData,
                Helpers.EventAuditLogHelper.PopulateEventAuditLogDataSet(customerData, webUser, EventTypeLookup.EventType.UpdateNickname, null, applicationId: ApplicationId.Value));

            return true;
        }

        internal static PayoffQuoteResult GetPayoffQuote(AccountServicesDataSet accountServicesData, 
                                                            CustomerUserIdDataSet customerData, 
                                                            int? ApplicationId, 
                                                            DateTime? SelectedDate, 
                                                            ShawLookups.PaymentTypeLookup.PaymentType? PaymentType)
        {
            Guard.AgainstNull<CustomerUserIdDataSet>(customerData, "customerData");
            if (ApplicationId.GetValueOrDefault() == 0)
            {
                LightStreamLogger.WriteWarning("ApplicationId is zero in FundedAccountModel.UpdateNickname");
                return new PayoffQuoteResult();
            }
            var applicationRow = customerData.Application.FindByApplicationId(ApplicationId.Value);
            if (applicationRow == null)
            {
                LightStreamLogger.WriteWarning(string.Format("Application {0} not found in customer data set, in method UpdateNickname", ApplicationId));
                throw new HttpException(403, "Access Denied");
            }
            if (!SelectedDate.HasValue || !PaymentType.HasValue)
            {
                return new PayoffQuoteResult();
            }

            var loanMasterRow = accountServicesData.LoanMaster.FirstOrDefault(x => x.ApplicationId == ApplicationId.Value);
            var payoffQuote = LoanServicingOperations.GetPayoffQuoteByPaymentMethod(ApplicationId.Value, SelectedDate.Value, PaymentType.Value);
            var result = new PayoffQuoteResult()
            {
                Success = true,
                PayOffQuote = payoffQuote,
            };
            if (loanMasterRow.PaymentType == ShawLookups.PaymentTypeLookup.PaymentType.AutoPay)
            {
                if (SelectedDate.Value > loanMasterRow.UINextPaymentDate)
                {
                    var achDebitPrepSettlementDate = LoanServicingOperations.GetACHDebitPrepSettlementDate();

                    result.PayOffIncludedACHPayment = (achDebitPrepSettlementDate != null && loanMasterRow.UINextPaymentDate <= achDebitPrepSettlementDate)
                                                            ? loanMasterRow.UINextPaymentAmount
                                                            : loanMasterRow.CurrentMonthlyPaymentAmt;

                    result.PayOffIncludedACHPaymentDate = loanMasterRow.UINextPaymentDate.ToLongDateString();
                }
            }
            return result;
        }

        public class PayoffQuoteResult
        {
            public bool Success { get; set; }
            public decimal? PayOffQuote { get; set; }
            public decimal? PayOffIncludedACHPayment { get; set; }
            public string PayOffIncludedACHPaymentDate { get; set; }
        }

        internal bool PayOffByACH(AccountServicesDataSet accountServicesData, CustomerUserIdDataSet customerData, ICurrentUser webUser, BusinessCalendarDataSet businessCalendar, out string errorMessage)
        {
            Guard.AgainstNull<CustomerUserIdDataSet>(customerData, "customerData");
            Guard.AgainstNull<AccountServicesDataSet>(accountServicesData, "accountServicesData");
            if (ApplicationId == 0)
            {
                LightStreamLogger.WriteWarning("ApplicationId is zero in FundedAccountModel.PayOffByACH");
                errorMessage = string.Empty;
                return false;
            }
            var applicationRow = customerData.Application.FindByApplicationId(ApplicationId);
            if (applicationRow == null)
            {
                LightStreamLogger.WriteWarning(string.Format("Application {0} not found in customer data set, in method PayOffByACH", ApplicationId));
                throw new HttpException(403, "Access Denied");
            }

            string payoffReasonText = ShawLookups.PayoffReasonLookup.GetCaption(PayOffReason);
            string payoffNote = string.Empty;
            decimal payoffAmount = LoanServicingOperations.GetPayoffQuoteByPaymentMethod(ApplicationId, PayOffByAutoPayDate.Value, ShawLookups.PaymentTypeLookup.PaymentType.AutoPay);
            if (!PayOffByAutoPayDate.HasValue || string.IsNullOrEmpty(PayOffReason) || payoffAmount == 0)
            {
                errorMessage = "A payoff date, reason, and amount are required";
                return false;
            }

            if (CheckForScheduledPayment(accountServicesData, ShawLookups.TransactionTypeLookup.TransactionType.ACHLoanPayoff))
            {
                errorMessage = "There is already a scheduled payment for this account. If you recently cancelled an extra payment or payoff, please reload the page and try again.";
                LightStreamLogger.WriteInfo(errorMessage + "Application ID=" + ApplicationId);
                return false;
            }

            if (!ProcessPaymentAccountSelection(accountServicesData, customerData, webUser, out errorMessage))
            {
                return false;
            }

            if (!String.IsNullOrEmpty(PayOffComments))
            {
                payoffNote = String.Format("Payoff date:  {0:d},   Amount: {1:C},  Reason:  {2} ({3})", PayOffByAutoPayDate, payoffAmount, payoffReasonText, PayOffComments.Trim());
            }
            else
            {
                payoffNote = String.Format("Payoff date:  {0:d},   Amount: {1:C},  Reason:  {2}", PayOffByAutoPayDate, payoffAmount, payoffReasonText);
            }

            if (!IsScheduledPaymentDateValid(PayOffByAutoPayDate.Value, businessCalendar, out errorMessage))
            {
                return false;
            }

            LoanServicingOperations.ScheduleAutoPayoff(ApplicationId,
                        payoffAmount,
                        PayOffByAutoPayDate.Value,
                        ShawLookups.PayoffReasonLookup.GetEnumeration(PayOffReason),
                        (PayOffComments ?? "").Trim(),
                        accountServicesData,
                        Helpers.EventAuditLogHelper.PopulateEventAuditLogDataSet(customerData, webUser, FirstAgain.Domain.Lookups.FirstLook.EventTypeLookup.EventType.SchedulePayoff, applicationId: ApplicationId, note: payoffNote));
            errorMessage = string.Empty;
            return true;
        }

        private bool CheckForScheduledPayment(AccountServicesDataSet accountServicesData, FirstAgain.Domain.Lookups.ShawData.TransactionTypeLookup.TransactionType transactionType)
        {
            var pendingTransactions = accountServicesData.TransactionRepository
                                        .Where(y => y.ApplicationId == ApplicationId)
                                        .Where(y => y.ProcessDate.Date >= DateTime.Today)
                                        .Where(y => y.TransactionStatus != FirstAgain.Domain.Lookups.ShawData.TransactionStatusLookup.TransactionStatus.Complete)
                                        .Where(y => y.TransactionType == transactionType)
                                        .ToList();
            return (pendingTransactions != null && pendingTransactions.Count > 0);
        }

        private bool ProcessPaymentAccountSelection(AccountServicesDataSet accountServicesData, CustomerUserIdDataSet customerData, ICurrentUser webUser, out string errorMessage)
        {
            // any account updates?
            if (PaymentAccountSelection.HasValue)
            {
                if (PaymentAccountSelection.Value == AccountSelection.DepositAccount)
                {
                    //set up a debit account with the values of credit account
                    AccountServicesDataSet.BankAccountInfoRow creditAccount = accountServicesData.GetCreditAccountByApplicationId(ApplicationId);
                    bool changeWasSuccessful = LoanServicingOperations.UpdateDebitAccountInfo(ApplicationId,
                                                                creditAccount.ACHTransactionRoutingNumber,
                                                                creditAccount.BankAccountNumber,
                                                                creditAccount.BankAccountHolderNameType,
                                                                ShawLookups.BankAccountTypeLookup.BankAccountType.CheckingAccount,
                                                                accountServicesData,
                                                                Helpers.EventAuditLogHelper.PopulateEventAuditLogDataSet(customerData, webUser, EventTypeLookup.EventType.ModifiedBankingInfo, applicationId: ApplicationId, note: null),
                                                                out errorMessage);

                    if (!changeWasSuccessful)
                    {
                        return false;
                    }
                }

                if (PaymentAccountSelection.Value == AccountSelection.NewAccount)
                {
                    bool changeWasSuccessful = true;
                    changeWasSuccessful = LoanServicingOperations.UpdateDebitAccountInfo(
                                    ApplicationId,
                                    RoutingNumber.Trim(),
                                    AccountNumber.Trim(),
                                    ShawLookups.BankAccountHolderNameTypeLookup.GetEnumeration(AuthorizedSigner),
                                    (IsCheckingAccount.GetValueOrDefault()) ? FirstAgain.Domain.Lookups.ShawData.BankAccountTypeLookup.BankAccountType.CheckingAccount : ShawLookups.BankAccountTypeLookup.BankAccountType.SavingsAccount,
                                    accountServicesData,
                                    Helpers.EventAuditLogHelper.PopulateEventAuditLogDataSet(customerData, webUser, EventTypeLookup.EventType.ModifiedBankingInfo, null, ApplicationId),
                                    out errorMessage);


                    if (!changeWasSuccessful)
                    {
                        return false;
                    }
                }
            }

            errorMessage = string.Empty;
            return true;
        }

        internal bool CancelPayoff(AccountServicesDataSet accountServicesData, CustomerUserIdDataSet customerData, ICurrentUser webUser)
        {
            Guard.AgainstNull<CustomerUserIdDataSet>(customerData, "customerData");
            Guard.AgainstNull<AccountServicesDataSet>(accountServicesData, "accountServicesData");
            if (ApplicationId == 0)
            {
                LightStreamLogger.WriteWarning("ApplicationId is zero in FundedAccountModel.CancelPayoff");
            }
            var applicationRow = customerData.Application.FindByApplicationId(ApplicationId);
            if (applicationRow == null)
            {
                LightStreamLogger.WriteWarning(string.Format("Application {0} not found in customer data set, in method CancelPayoff", ApplicationId));
                throw new HttpException(403, "Access Denied");
            }

            LoanServicingOperations.CancelAutoPayoff(ApplicationId, accountServicesData, Helpers.EventAuditLogHelper.PopulateEventAuditLogDataSet(
                customerData,
                webUser,
                EventTypeLookup.EventType.CancelScheduledPayoff, 
                applicationId: ApplicationId, 
                note: "Cancelled an ACH payoff."));
            return true;
        }

        public bool PaymentByACH(AccountServicesDataSet accountServicesData, CustomerUserIdDataSet customerData, ICurrentUser webUser, BusinessCalendarDataSet businessCalendar, out string errorMessage)
        {
            Guard.AgainstNull<CustomerUserIdDataSet>(customerData, "customerData");
            Guard.AgainstNull<AccountServicesDataSet>(accountServicesData, "accountServicesData");
            if (ApplicationId == 0)
            {
                LightStreamLogger.WriteWarning("ApplicationId is zero in FundedAccountModel.CancelPayoff");
                errorMessage = string.Empty;
                return false;
            }
            var applicationRow = customerData.Application.FindByApplicationId(ApplicationId);
            if (applicationRow == null)
            {
                LightStreamLogger.WriteWarning(string.Format("Application {0} not found in customer data set, in method CancelPayoff", ApplicationId));
                throw new HttpException(403, "Access Denied");
            }
            if (!ScheduledPaymentDate.HasValue)
            {
                errorMessage = "Please enter a payment date";
                LightStreamLogger.WriteWarning("ScheduledPaymentDate is NULL in FundedAccountModel.PaymentByACH");
                return false;
            }
            if (!ScheduledPaymentAmount.HasValue)
            {
                errorMessage = "Please enter a payment amount";
                LightStreamLogger.WriteWarning("ScheduledPaymentAmount is NULL in FundedAccountModel.PaymentByACH");
                return false;
            }

            if (!ProcessPaymentAccountSelection(accountServicesData, customerData, webUser, out errorMessage))
            {
                return false;
            }

            if (CheckForScheduledPayment(accountServicesData, ShawLookups.TransactionTypeLookup.TransactionType.OneTimeACHPayment))
            {
                errorMessage = "There is already a scheduled payment for this account. If you recently cancelled an extra payment or payoff, please reload the page and try again.";
                LightStreamLogger.WriteInfo(errorMessage + "Application ID=" + ApplicationId);
                return false;
            }

            if (!IsScheduledPaymentDateValid(ScheduledPaymentDate.Value, businessCalendar, out errorMessage))
            {
                return false;
            }

            string noteText = String.Format("One-time payment date: {0:d}, One-time payment amount: {1:C}", ScheduledPaymentDate.Value, ScheduledPaymentAmount.Value);
            LoanServicingOperations.ScheduleOneTimeACHPayment(ApplicationId,
                            ScheduledPaymentAmount.Value,
                            ScheduledPaymentDate.Value, 
                            accountServicesData,
                            Helpers.EventAuditLogHelper.PopulateEventAuditLogDataSet(customerData, webUser, EventTypeLookup.EventType.OneTimeACHPaymentScheduled, applicationId: ApplicationId, note: noteText),
                            ShawLookups.TransactionSourceTypeLookup.TransactionSourceType.SOURCE_WEB,
                            EventInitiatorTypeLookup.EventInitiatorType.Web,
                            noteText);
            errorMessage = string.Empty;
            return true;
        }

        private bool IsScheduledPaymentDateValid(DateTime? scheduledPaymentDate,BusinessCalendarDataSet businessCalendar, out string errorMessage)
        {
            DateTime earliestAvailablePmtDate = businessCalendar.GetEffectiveACHDate(DateTime.Now);
            if (scheduledPaymentDate != null && scheduledPaymentDate.Value < earliestAvailablePmtDate)
            {
                errorMessage = $"The payment date you have selected is not available. The earliest available payment date is {earliestAvailablePmtDate.ToShortDateString()}.";
                LightStreamLogger.WriteInfo(errorMessage + "Application ID=" + ApplicationId);
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }
    }
}