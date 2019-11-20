using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using FirstAgain.Common.Extensions;
using FirstAgain.Correspondence.ServiceModel.Client;
using FirstAgain.Domain.Common;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using LightStreamWeb.Helpers;
using LightStreamWeb.Models.Shared;

using FundsTransferType = FirstAgain.Domain.Lookups.FirstLook.FundsTransferTypeLookup.FundsTransferType;
using LoanTermsRequestStatusType = FirstAgain.Domain.Lookups.FirstLook.LoanTermsRequestStatusLookup.LoanTermsRequestStatus;
using LightStreamWeb.ServerState;
using FirstAgain.Domain.SharedTypes.IDProfile;

namespace LightStreamWeb.Models.PreFunding
{
    public class PreFundingNLTRPageModel : PreFundingPageModel
    {
        #region constructors
        public PreFundingNLTRPageModel(CustomerUserIdDataSet cuidds, LoanOfferDataSet loanOfferDataSet)
            : base(cuidds, loanOfferDataSet)
        {
            Statements = new List<string>();
            AuthorizedSignerList = LightStreamWeb.Helpers.ApplicationStatusHelper.PopulateAuthorizedSignerList(Application);
        }
        #endregion

        public bool CanCancelRequest
        { get; protected set;            
        }

        public string Heading { get; protected set; }
        public List<string> Statements { get; protected set; }
        public LoanTermsModel LoanTerms { get; protected set; }
        public NextSteps NextStep { get; protected set; }
        public List<SelectListItem> AuthorizedSignerList { get; private set; }
        public string AuthorizedSignerInitialValue
        {
            get
            {
                if (AuthorizedSignerList != null && AuthorizedSignerList.Any(x => x.Selected))
                {
                    return AuthorizedSignerList.First(x => x.Selected).Value;
                }

                return "NotSelected";
            }
        }
        public bool HasCheckingAccount()
        {
            return _loanOfferDataSet != null && _loanOfferDataSet.HaveCheckingAccountInfo();
        }

        public PrefundingNLTRStatus GetNLTRStatus()
        {
            PrefundingNLTRStatus result = PrefundingNLTRStatus.Pending;
            var latestLtr = _loanOfferDataSet.LatestLoanTermsRequest;
            var approvedTerms = _loanOfferDataSet.LatestApprovedLoanTerms;

            var eal = DomainServiceLoanApplicationOperations.GetEventAuditLogByApplicationId(Application.ApplicationId);
            if (SessionUtility.AcceptedPreviousTerms)
            {
                latestLtr = _loanOfferDataSet.LatestActiveLoanOffer.LoanTermsRequestRow;
            }
            switch (latestLtr.Status)
            {
                case LoanTermsRequestStatusLookup.LoanTermsRequestStatus.Approved:
                    result = PrefundingNLTRStatus.Approved;
                    if (!latestLtr.GetLoanOfferRows().Single().IsLoanAgreementEdocIdNull())
                    {
                        if (IsAfterFundingDropDeadDateTime() || IsAfterDropDeadDateTime())
                        {
                            result = PrefundingNLTRStatus.ApprovedAfterDropDeadDateTime;
                        }
                        else if (_loanOfferDataSet.SwitchedFromInvoiceToAutoPayPostSign())
                        {
                            result = PrefundingNLTRStatus.SwitchedFromInvoiceToAutoPayPostSign;
                        }
                    }
                    Heading = "Approved New Loan Terms Request";
                    if(latestLtr.LoanTermsRequestType == LoanTermsRequestTypeLookup.LoanTermsRequestType.ApplicationUpdate || eal.EventAuditLog.Any(x => x.EventType == EventTypeLookup.EventType.InvalidateLoanAgreement))
                    {
                        DisplayApprovedAddressChanged(latestLtr);
                    }
                    else
                    {
                        DisplayApproved(latestLtr);
                    }
                    break;
                case LoanTermsRequestStatusLookup.LoanTermsRequestStatus.Cancelled:
                    result = PrefundingNLTRStatus.Cancelled;
                    Heading = "New Loan Terms Request Cancelled";
                    DisplayCancelled(approvedTerms);
                    break;
                case LoanTermsRequestStatusLookup.LoanTermsRequestStatus.Declined:
                    result = PrefundingNLTRStatus.Declined;
                    Heading = "New Loan Terms Request";
                    DisplayDeclined(approvedTerms);
                    break;
                case LoanTermsRequestStatusLookup.LoanTermsRequestStatus.Pending:
                    result = PrefundingNLTRStatus.Pending;
                    Heading = "New Loan Terms Request in Process";
                    DisplayPending();
                    break;
                case LoanTermsRequestStatusLookup.LoanTermsRequestStatus.PendingQ:
                    result = PrefundingNLTRStatus.PendingQ;
                    Heading = "New Loan Terms Request Pending";
                    DisplayPendingQ();
                    break;
                case LoanTermsRequestStatusLookup.LoanTermsRequestStatus.PendingV:
                    result = PrefundingNLTRStatus.PendingV;
                    break;
                case LoanTermsRequestStatusLookup.LoanTermsRequestStatus.Counter:
                    result = PrefundingNLTRStatus.Counter;
                    break;
                case LoanTermsRequestStatusLookup.LoanTermsRequestStatus.CounterV:
                    result = PrefundingNLTRStatus.CounterV;
                    break;
            }

            return result;

        }

        private void DisplayApproved(LoanOfferDataSet.LoanTermsRequestRow latestLtr)
        {
            Statements.Add("<h3>" + this.ApplicantNamesText + ",</h3");
            Statements.Add("Your new loan terms request has been approved. Please review the new loan terms below before you proceed to sign your loan agreement.");
            LoanTerms = new LoanTermsModel();
            LoanTerms.Populate(latestLtr);

            CanCancelRequest = (LoanTerms.LoanTermsRequestType != FirstAgain.Domain.Lookups.FirstLook.LoanTermsRequestTypeLookup.LoanTermsRequestType.ApplicationUpdate);
            NextStep = NextSteps.ProceedToLoanAgreement;
        }

        private void DisplayApprovedAddressChanged(LoanOfferDataSet.LoanTermsRequestRow latestLtr)
        {
            CanCancelRequest = false;
            Statements.Add("<h3>" + this.ApplicantNamesText + ",</h3");
            Statements.Add("Your loan agreement has been updated.  Please review your loan terms below and proceed to sign your loan agreement.");
            LoanTerms = new LoanTermsModel();
            LoanTerms.Populate(latestLtr);
            NextStep = NextSteps.ProceedToLoanAgreement;
        }

        private void DisplayDeclined(LoanOfferDataSet.LoanTermsRequestRow approvedTerms)
        {
            Statements.Add("<h3>" + this.ApplicantNamesText + ",</h3");
            Statements.Add("We are sorry but we are unable to approve your new loan terms request. To view your decline notice information, please click on the 'View Decline Notice' link.");

            if (_loanOfferDataSet.LoanOffer[0]["LoanAgreementEdocId"] != DBNull.Value)
            {
                Statements.Add("If you would like to continue with your previous loan terms, select Continue with Previous Terms below.");
                NextStep = NextSteps.ContinueWithPreviousTerms;
            }
            else
            {
                Statements.Add("If you would like to continue with your previous loan terms, select Proceed to Loan Agreement below.");
                NextStep = NextSteps.ProceedToLoanAgreement;
            }

            LoanTerms = new LoanTermsModel();
            LoanTerms.Populate(approvedTerms);
        }

        private void DisplayCancelled(LoanOfferDataSet.LoanTermsRequestRow approvedTerms)
        {

            if (_loanOfferDataSet.LoanOffer[0]["LoanAgreementEdocId"] != DBNull.Value)
            {
                Statements.Add("As per your request your new loan terms request has been cancelled. To continue with your previous loan terms, select Continue with Previous Terms below.");
                NextStep = NextSteps.ContinueWithPreviousTerms;
            }
            else
            {
                Statements.Add("As per your request your new loan terms request has been cancelled. To continue with your previous loan terms, select Proceed to Loan Agreement below.");
                Statements.Add("If you would like to continue with your previous loan terms, select Proceed to Loan Agreement below.");
                NextStep = NextSteps.ProceedToLoanAgreement;
            }

            Statements.Add("As per your request your new loan terms request has been cancelled. To continue with your previous loan terms, select Continue with Previous Terms below.");
            LoanTerms = new LoanTermsModel();
            LoanTerms.Populate(approvedTerms);
        }

        protected void DisplayPending()
        {
            Statements.Add("<h3>" + this.ApplicantNamesText + ",</h3");
            Statements.Add("The new loan terms request you recently submitted is currently in process.  We will send you an email " + DateUtility.CustomerResponseTimeFrameText + " regarding our credit decision, or questions that we may have.");
            Statements.Add("Thank you.");
        }

        protected void DisplayPendingQ()
        {
            Statements.Add("<h3>" + this.ApplicantNamesText + ",</h3");
            Statements.Add(string.Format("Thank you for your recently submitted new loan terms request (Reference #{0}).  After reviewing your information we have a few questions.", Application.ApplicationId));
            Statements.Add(string.Format("Please contact us at <a href='tel:{0}'>{0}</a>.", GetUnderwritingPhoneNumber()));
            Statements.Add("Thank you.");
        }

        protected string GetUnderwritingPhoneNumber()
        {
            if (_user.ApplicationId.HasValue)
            {
                var uphone = CorrespondenceServiceCorrespondenceOperations.GetUnderwriterPhoneLogic(_user.ApplicationId.Value);
                if (uphone.HasAlert)
                {
                    // #9689 - Don't show fraud # on website.
                    return BusinessConstants.Instance.PhoneNumberMain;
                }
                if (uphone.IsSenior)
                {
                    if (String.IsNullOrEmpty(uphone.NLTRPendingQPhoneNumber) || uphone.NLTRPendingQPhoneNumber.Trim().Equals(BusinessConstants.Instance.PhoneNumberSeniorUnderwriter))
                        return BusinessConstants.Instance.PhoneNumberSeniorUnderwriter;

                    else return uphone.NLTRPendingQPhoneNumber;
                }

                if (uphone.NLTRPendingQPhoneNumber != null)
                    return uphone.NLTRPendingQPhoneNumber;
            }
            return BusinessConstants.Instance.PhoneNumberUnderwriting;
        }


        public bool IsAfterDropDeadDateTime()
        {
            DateTime cutoff;
            DateTime processDate = BusinessCalendar.GetFundingProcessDate(_loanOfferDataSet.LoanContract[0].FundingDate, _loanOfferDataSet.LoanContract[0].FundsTransferType);

            switch (_loanOfferDataSet.LoanContract[0].FundsTransferType)
            {
                case FundsTransferType.WireTransferSameDay:
                    cutoff = new DateTime(processDate.Year,
                                          processDate.Month,
                                          processDate.Day,
                                          BusinessConstants.Instance.SameDayFundingCutoffTimeWeb.Hours,
                                          BusinessConstants.Instance.SameDayFundingCutoffTimeWeb.Minutes,
                                          0);
                    break;
                case FundsTransferType.WireTransfer:
                case FundsTransferType.ACHSameDay:
                    cutoff = new DateTime(processDate.Year,
                                          processDate.Month,
                                          processDate.Day,
                                          BusinessConstants.Instance.SameDayFundingCutoffTimeWeb.Hours - 3,
                                          BusinessConstants.Instance.SameDayFundingCutoffTimeWeb.Minutes,
                                          0);
                    break;
                case FundsTransferType.ACHTransaction:
                    cutoff = new DateTime(processDate.Year,
                                          processDate.Month,
                                          processDate.Day,
                                          BusinessConstants.Instance.SelectNextDayFundingCutoffTime.Hours,
                                          BusinessConstants.Instance.SelectNextDayFundingCutoffTime.Minutes,
                                          0);
                    break;
                default:
                    throw new Exception("Funding Transfer Type Not Supported");
            }
            if (DateTime.Now > cutoff)
                return true;

            return false;
        }

        public bool IsAfterFundingDropDeadDateTime()
        {
            Boolean IsAfterFundingDropDeadDateTime = BusinessCalendar.GetIsAfterFundingProcessDate(_loanOfferDataSet.LoanContract[0].FundingDate, _loanOfferDataSet.LoanContract[0].FundsTransferType);

            return IsAfterFundingDropDeadDateTime;
        }

        public enum PrefundingNLTRStatus
        {
            Approved,
            Pending,
            PendingQ,
            PendingV,
            Counter,
            CounterV,
            Cancelled,
            Declined,
            SwitchedFromInvoiceToAutoPayPostSign,
            ApprovedAfterDropDeadDateTime,
        }

        public enum NextSteps
        {
            ContinueWithPreviousTerms,
            ProceedToLoanAgreement
        }

        internal void AcceptPreviousTerms()
        {
            DomainServiceLoanApplicationOperations.PreFundingAcceptPreviousTerms(Application);
        }

        public string ExpirationDate
        {
            get
            {
                TimeSpan window;
                return BusinessCalendar.GetLastDateTimeToScheduleFunding(_loanOfferDataSet.LoanContract[0].ApplicationId, out window).ToLongDateString();
            }
        }

        internal void CancelNLTR()
        {
            WebActivityDataSet webActivity = LightStreamWeb.Helpers.WebActivityDataSetHelper.Populate(_user, CurrentStatus);
            DomainServiceLoanApplicationOperations.CancelLoanTermsRequestWithWebActivity(Application, _loanOfferDataSet.LatestApprovedLoanTerms, "Customer cancelled request via web site", webActivity);
        }

        public void UpdateEmailPreferences(List<SolicitationPreferenceLookup.SolicitationPreference> preferences)
        {
            UpdatePreferences(preferences);

            SubmitNLTREmailPreferences();
        }

        internal void UpdateNLTRCheckingAccountInfo(GetNLTRCheckingAccountModel data)
        {
            UpdatePreferences(data.EmailPreferences);

            SubmitNLTRCheckingAccountInfo(data);
        }

        private void SubmitNLTREmailPreferences()
        {
            EventTypeLookup.EventType eventType = EventTypeLookup.EventType.AcceptedNewLoanTerms;
            if (_loanOfferDataSet.LatestLoanTermsRequest.LoanTermsRequestType == LoanTermsRequestTypeLookup.LoanTermsRequestType.NLTRCounter)
            {
                eventType = EventTypeLookup.EventType.AcceptedCounterOfferTerms;
            }

            DomainServiceLoanApplicationOperations.CompletePreFundingNLTRWithWebActivity(Application, eventType, WebActivityDataSetHelper.Populate(WebUser));
        }

        private void SubmitNLTRCheckingAccountInfo(GetNLTRCheckingAccountModel data)
        {
            if (data.AccountAction == GetNLTRCheckingAccountModel.AccountActionType.SameAsChecking)
            {
                DomainServiceLoanApplicationOperations.CompletePreFundingNLTR(Application, EventTypeLookup.EventType.AcceptedNewLoanTerms);
                return;
            }

            if (data.AccountAction == GetNLTRCheckingAccountModel.AccountActionType.SameAsFunding)
            {
                CopyFundingAccountToDebitAccount();
                DomainServiceLoanApplicationOperations.SubmitLoanContractWithWebActivity(_loanOfferDataSet, WebActivityDataSetHelper.Populate(WebUser, CurrentStatus), EventTypeLookup.EventType.NewLoanTermsRequest, false);
                return;
            }

            if (data.AccountAction == GetNLTRCheckingAccountModel.AccountActionType.DifferentAccount)
            {
                PopulateDebitAccount(data);
                DomainServiceLoanApplicationOperations.SubmitLoanContractWithWebActivity(_loanOfferDataSet, WebActivityDataSetHelper.Populate(WebUser, CurrentStatus), EventTypeLookup.EventType.NewLoanTermsRequest, false);
                return;
            }

        }

        private void PopulateDebitAccount(GetNLTRCheckingAccountModel data)
        {
            LoanOfferDataSet.BankAccountInfoRow debitAccount = _loanOfferDataSet.BankAccountInfo.FirstOrDefault(x => x.BankAccountActionType == BankAccountActionTypeLookup.BankAccountActionType.Debit);
            if (debitAccount == null)
            {
                debitAccount = _loanOfferDataSet.BankAccountInfo.NewBankAccountInfoRow();
                debitAccount.ApplicationId = Application.ApplicationId;
                debitAccount.BankAccountActionType = BankAccountActionTypeLookup.BankAccountActionType.Debit;
                debitAccount.IsBrokerageAccount = false;
                debitAccount.SetBeneficiaryAccountNumberNull();
                debitAccount.SetIntermediaryBankABANumberNull();
                debitAccount.SetIntermediaryBankAccountNumberNull();
                debitAccount.SetIntermediaryBankNameNull();
            }

            var bankingInfo = DomainServiceLoanApplicationOperations.GetBankingInstitution(data.RoutingNumber, BankAccountActionTypeLookup.BankAccountActionType.Debit, ApplicationId);

            if (this.Application.IsJoint)
            {
                debitAccount.BankAccountHolderNameTypeId = (short)data.AuthorizedSigner;
            }
            else
            {
                debitAccount.BankAccountHolderNameTypeId = (short)BankAccountHolderNameTypeLookup.BankAccountHolderNameType.PrimaryBorrowerName;
            }
            debitAccount.ApplicationId = Application.ApplicationId;
            debitAccount.BankAccountNumber = data.AccountNumber;
            debitAccount.BankAccountType = BankAccountTypeLookup.BankAccountType.CheckingAccount;
            debitAccount.BankAccountActionType = BankAccountActionTypeLookup.BankAccountActionType.Debit;
            debitAccount.ACHTransactionRoutingNumber = bankingInfo.ACHRoutingNumber;
            if (string.IsNullOrEmpty(bankingInfo.WireRoutingNumber))
            {
                debitAccount.SetWireTransferRoutingNumberNull();
            }
            else
            {
                debitAccount.WireTransferRoutingNumber = bankingInfo.WireRoutingNumber;
            }
            if (string.IsNullOrEmpty(bankingInfo.CorrespondentBankRoutingNumber))
            {
                debitAccount.SetCorrespondentBankRoutingNumberNull();
            }
            else
            {
                debitAccount.CorrespondentBankRoutingNumber = bankingInfo.CorrespondentBankRoutingNumber;
            }
            debitAccount.MICRRoutingNumber = bankingInfo.MICRRoutingNumber;
            debitAccount.IsWireAble = bankingInfo.ACHRoutingNumberIsSupportedByFedFile;
            if (string.IsNullOrEmpty(bankingInfo.WireRoutingNumberUSBankShortName))
            {
                debitAccount.SetWireTransferRoutingNumberShortNameNull();
            }
            else
            {
                debitAccount.WireTransferRoutingNumberShortName = bankingInfo.WireRoutingNumberUSBankShortName;
            }
            if (string.IsNullOrEmpty(bankingInfo.CorrespondentBankUSBankShortName))
            {
                debitAccount.SetCorrespondentBankRoutingNumberShortNameNull();
            }
            else
            {
                debitAccount.CorrespondentBankRoutingNumberShortName = bankingInfo.CorrespondentBankUSBankShortName;
            }

            debitAccount.BankAccountNumber = data.AccountNumber;
            if (debitAccount.RowState == System.Data.DataRowState.Detached)
            {
                _loanOfferDataSet.BankAccountInfo.AddBankAccountInfoRow(debitAccount);
            }
        }

        private FirstAgain.Domain.SharedTypes.IDProfile.BankingInfoXml DoIBankCheck(string depositAccountNum, string withdrawalAccountNum, out bool creditAccountFailed, out bool debitAccountFailed)
        {
            DateTime ibankStart = DateTime.Now;
            string responseXML = DomainServiceIDProfileOperations.CheckingAccountBankVerification(_loanOfferDataSet.ApplicationId, depositAccountNum, withdrawalAccountNum, out creditAccountFailed, out debitAccountFailed);
            DateTime ibankEnd = DateTime.Now;
            TimeSpan iBankTime = ibankEnd - ibankStart;
            //LightStreamLogger.WriteDebug(string.Format("UserId: {0}, IBank: {1}", Account.AccountInfo.CustomerUserId, iBankTime.TotalMilliseconds.ToString()));

            return new FirstAgain.Domain.SharedTypes.IDProfile.BankingInfoXml(responseXML);
        }

        private void CopyFundingAccountToDebitAccount()
        {
            LoanOfferDataSet.BankAccountInfoRow debitAccount = _loanOfferDataSet.BankAccountInfo.FirstOrDefault(x => x.BankAccountActionType == BankAccountActionTypeLookup.BankAccountActionType.Debit);
            if (debitAccount == null)
            {
                debitAccount = _loanOfferDataSet.BankAccountInfo.NewBankAccountInfoRow();
                debitAccount.ApplicationId = Application.ApplicationId;
                debitAccount.BankAccountActionType = BankAccountActionTypeLookup.BankAccountActionType.Debit;
                debitAccount.IsBrokerageAccount = false;
                debitAccount.SetBeneficiaryAccountNumberNull();
                debitAccount.SetIntermediaryBankABANumberNull();
                debitAccount.SetIntermediaryBankAccountNumberNull();
                debitAccount.SetIntermediaryBankNameNull();
            }
            var fundingAccount = _loanOfferDataSet.BankAccountInfo.Single(x => x.BankAccountActionType == BankAccountActionTypeLookup.BankAccountActionType.Credit);
            debitAccount.BankAccountNumber = fundingAccount.BankAccountNumber;
            debitAccount.BankAccountType = fundingAccount.BankAccountType;
            debitAccount.ACHTransactionRoutingNumber = fundingAccount.ACHTransactionRoutingNumber;
            debitAccount.WireTransferRoutingNumber = fundingAccount.WireTransferRoutingNumber;
            debitAccount.MICRRoutingNumber = fundingAccount.MICRRoutingNumber;
            debitAccount.IsWireAble = fundingAccount.IsWireAble;
            debitAccount.WireTransferRoutingNumberShortName = fundingAccount.WireTransferRoutingNumberShortName;
            debitAccount.BankAccountHolderNameType = fundingAccount.BankAccountHolderNameType;
            if (debitAccount.RowState == System.Data.DataRowState.Detached)
            {
                _loanOfferDataSet.BankAccountInfo.AddBankAccountInfoRow(debitAccount);
            }
        }

        private void UpdatePreferences(List<SolicitationPreferenceLookup.SolicitationPreference> preferences)
        {
            CustomerUserIdDataSet.ApplicationSolicitationPreferenceRow[] solPrefRows = Application.GetApplicationSolicitationPreferenceRows();

            // Delete application solicitation prefs that do not apply for the payment type selected
            var paymentType = _loanOfferDataSet.LatestApprovedLoanTerms.PaymentType;
            SolicitationPreferenceLookup.ValueList vl = _getSolicitationPreferenceListForPaymentType(paymentType);
            foreach (CustomerUserIdDataSet.ApplicationSolicitationPreferenceRow row in solPrefRows)
            {
                bool deleteRow = true;
                // For some reason a foreach iterator does not iterate over the entire collection
                for (int i = 0; i < vl.Count; i++)
                {
                    if (row.SolicitationPreference == (SolicitationPreferenceLookup.SolicitationPreference)vl[i].Enumeration)
                    {
                        deleteRow = false;
                        break;
                    }
                }
                if (deleteRow)
                    row.Delete();
            }

            if (preferences != null)
            {
                foreach (var p in preferences)
                {
                    Application.SetEmailPreference(p, true);
                }
            }

            if (paymentType == PaymentTypeLookup.PaymentType.Invoice)
            {
                Application.SetEmailPreference
                (SolicitationPreferenceLookup.SolicitationPreference.EInvoicePayDays, true);

                Application.SetEmailPreference
                (SolicitationPreferenceLookup.SolicitationPreference.PaymentReminder10Days, true);
            }

            DomainServiceCustomerOperations.UpdateCustomerUserIdData(ApplicationId, _customerUserIdDataSet, EventAuditLogHelper.PopulateEventAuditLogDataSet(_customerUserIdDataSet, WebUser, EventTypeLookup.EventType.UpdateENotices, null));
        }

        private SolicitationPreferenceLookup.ValueList _getSolicitationPreferenceListForPaymentType(PaymentTypeLookup.PaymentType paymentType)
        {
            SolicitationPreferenceLookup.ValueList prefList = null;
            if (paymentType == PaymentTypeLookup.PaymentType.Invoice)
            {
                prefList = SolicitationPreferenceLookup.
                    GetFilteredBindingSource(SolicitationPreferenceLookup.FilterType.Invoice);
            }
            else
            {
                prefList = SolicitationPreferenceLookup.
                    GetFilteredBindingSource(SolicitationPreferenceLookup.FilterType.Autopay);
            }
            return prefList;
        }

        private int? LatestDeclinedLoanRequestId
        {
            get
            {
                int? loanRequestId = null;

                if (_customerUserIdDataSet.LoanTermsRequestStatus.Where(s => s.LoanTermsRequestStatus == LoanTermsRequestStatusType.Declined).Any())
                {
                    loanRequestId = _customerUserIdDataSet
                                    .LoanTermsRequestStatus.Where(s => s.LoanTermsRequestStatus == LoanTermsRequestStatusType.Declined)
                                    .OrderByDescending(lr => lr.LoanTermsRequestId).First().LoanTermsRequestId;
                }

                return loanRequestId;
            }
        }

        public bool WaitForLatestDeclineNotice(CustomerUserIdDataSet cuids)
        {
            if (cuids == null || Application == null)
            {
                return false;
            }

            if (LatestDeclinedLoanRequestId.IsNotNull())
            {
                // Need to wait for latest notice if it's yet not available.
                return !(cuids.DocumentStore.Any(a => a.ApplicationId == ApplicationId &&
                                                      a.IsViewable &&
                                                      a.CorrespondenceCategory == CorrespondenceCategoryLookup.CorrespondenceCategory.DeclineNotice &&
                                                      a.LoanTermsRequestId == LatestDeclinedLoanRequestId));
            }

            return false;

        }
    }
}