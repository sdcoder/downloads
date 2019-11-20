using FirstAgain.Common;
using FirstAgain.Common.Extensions;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using LightStreamWeb.App_State;
using LightStreamWeb.Models.ApplicationStatus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BusinessCalendar = FirstAgain.Domain.ServiceModel.Client.BusinessCalendar;

namespace LightStreamWeb.Models.PreFunding
{
    public class FundingFailedPageModel : AccountInformationChangeModel
    {
        private BankingInfoDataSet _bankingInfoDataSet = null; 

        public FundingFailedPageModel(CustomerUserIdDataSet cuidds, LoanOfferDataSet loanOfferDataSet)
            : base(cuidds, loanOfferDataSet)
        {
            AuthorizedSignerList = LightStreamWeb.Helpers.ApplicationStatusHelper.PopulateAuthorizedSignerList(Application);
            _bankingInfoDataSet = DomainServiceLoanApplicationOperations.GetBankingInfo(ApplicationId);
        }

        [Serializable]
        public class FundingFailedData
        {
            public FundingFailedData()
            {
                FundingAccount = new LoanAcceptanceModel.BankAccountInfo();
                PaymentAccount = new LoanAcceptanceModel.BankAccountInfo();
                BrokerageAccount = new LoanAcceptanceModel.BrokerageAccountInfo();
            }
            public bool? FundingAccountIsBrokerageAccount { get; set; }
            public bool? PaymentAccountSameAsFunding { get; set; }
            public DateTime FundingDate { get; set; }
            public LoanAcceptanceModel.BankAccountInfo FundingAccount { get; set; }
            public LoanAcceptanceModel.BankAccountInfo PaymentAccount { get; set; }
            public LoanAcceptanceModel.BrokerageAccountInfo BrokerageAccount { get; set; }
        }


        internal bool Submit(FundingFailedData data, out string errorMessage)
        {
            FundsTransferTypeLookup.FundsTransferType transferType;
            FundsTransferTypeLookup.FundsTransferType previousTransferType;
            errorMessage = string.Empty;
            if (!Validate(data, out errorMessage, out transferType))
            {
                return false;
            }

            // validate + populate deposit account?
            var fundingBank = DomainServiceLoanApplicationOperations.GetBankingInstitution(data.FundingAccount.RoutingNumber, BankAccountActionTypeLookup.BankAccountActionType.Credit, ApplicationId);
            if (fundingBank == null)
            {
                errorMessage = "Deposit account is invalid";
                return false;
            }

            // grab current funding info
            var fundingRow = _bankingInfoDataSet.BankAccountInfo.FirstOrDefault(x => x.BankAccountActionType == BankAccountActionTypeLookup.BankAccountActionType.Credit);

            // grab previous funding info
            previousTransferType = _bankingInfoDataSet.LoanContract[0].FundsTransferType;

            // validate that a change is being made, either acct number or transfer type
            if (data.FundingAccount.RoutingNumber.Equals(fundingRow.ACHTransactionRoutingNumber.Trim()) && 
                data.FundingAccount.AccountNumber.Equals(fundingRow.BankAccountNumber.Trim()) && 
                (transferType == previousTransferType))
            {
                errorMessage = "The account information that you entered is the same one that recently failed.  Please enter a different account number or routing number.  If you have any concerns regarding which numbers to use, contact your financial institution.  Thank you.";
                return false;
            }

            // validate + populate payment account?
            if (IsAutoPay() && IsPaymentAccountSameAsFundingAccount() && data.PaymentAccount != null)
            {
                var paymentBank = DomainServiceLoanApplicationOperations.GetBankingInstitution(data.PaymentAccount.RoutingNumber, BankAccountActionTypeLookup.BankAccountActionType.Debit, ApplicationId);
                if (paymentBank == null)
                {
                    errorMessage = "Payment account is invalid";
                    return false;
                }

                var debitRow = _bankingInfoDataSet.BankAccountInfo.FirstOrDefault(x => x.BankAccountActionType == BankAccountActionTypeLookup.BankAccountActionType.Debit);
                MapToBankingInfoRow(data.PaymentAccount, paymentBank, debitRow);
            }

            // map new data to existing row
            MapToBankingInfoRow(data.FundingAccount, fundingBank, fundingRow);

            if (data.FundingAccountIsBrokerageAccount.GetValueOrDefault() && data.BrokerageAccount != null)
            {
                fundingRow.IsBrokerageAccount = true;

                fundingRow.BeneficiaryBankName = data.BrokerageAccount.BeneficiaryBankName;
                fundingRow.BeneficiaryBankABANumber = data.BrokerageAccount.BeneficiaryRoutingNumber;
                fundingRow.BeneficiaryAccountNumber = data.BrokerageAccount.BeneficiaryAccountNumber;

                fundingRow.IntermediaryBankName = data.BrokerageAccount.IntermediaryBankName;
                fundingRow.IntermediaryBankABANumber = data.BrokerageAccount.IntermediaryRoutingNumber;
                fundingRow.IntermediaryBankAccountNumber = data.BrokerageAccount.IntermediaryAccountNumber;
            }
            else
            {
                fundingRow.IsBrokerageAccount = false;

                fundingRow.SetBeneficiaryBankNameNull();
                fundingRow.SetBeneficiaryBankABANumberNull();
                fundingRow.SetBeneficiaryAccountNumberNull();

                fundingRow.SetIntermediaryBankNameNull();
                fundingRow.SetIntermediaryBankABANumberNull();
                fundingRow.SetIntermediaryBankAccountNumberNull();
            }

            // funding failed specific
            DateTime processDate = BusinessCalendar.GetFundingProcessDate(data.FundingDate, transferType);
            DateTime firstPaymentBusinessDate;
            DateTime firstPaymentDate = BusinessCalendar.GetFirstPaymentDate(data.FundingDate, _loanOfferDataSet.LoanContract[0].MonthlyPaymentDate, out firstPaymentBusinessDate);
            _bankingInfoDataSet.LoanContract[0].FundingDate = data.FundingDate;
            _bankingInfoDataSet.LoanContract[0].FundsTransferType = transferType;
            _bankingInfoDataSet.LoanContract[0].InitialPaymentDate = firstPaymentDate;
            _bankingInfoDataSet.QueueShawExport[0].ProcessDate = processDate;
            _bankingInfoDataSet.QueueShawExport[0].QueueItemStatus = QueueItemStatusTypeLookup.QueueItemStatusType.Ready;

            DomainServiceLoanApplicationOperations.UpdateBankingInfo(_bankingInfoDataSet, Helpers.EventAuditLogHelper.PopulateEventAuditLogDataSet(_customerUserIdDataSet, WebUser, EventTypeLookup.EventType.ModifiedBankingInfo, string.Empty));

            return true;
        }

        public bool IsFundingAccountBrokerageAccount()
        {
            var fundingRow = _bankingInfoDataSet.BankAccountInfo.FirstOrDefault(x => x.BankAccountActionType == BankAccountActionTypeLookup.BankAccountActionType.Credit);
            return fundingRow != null && fundingRow.IsBrokerageAccount;
        }

        public bool IsPaymentAccountSameAsFundingAccount()
        {
            if (IsAutoPay())
            {
                var fundingRow = _bankingInfoDataSet.BankAccountInfo.FirstOrDefault(x => x.BankAccountActionType == BankAccountActionTypeLookup.BankAccountActionType.Credit);
                var debitRow = _bankingInfoDataSet.BankAccountInfo.FirstOrDefault(x => x.BankAccountActionType == BankAccountActionTypeLookup.BankAccountActionType.Debit);
                if (fundingRow != null && debitRow != null)
                {
                    return fundingRow.MICRRoutingNumber.Equals(debitRow.MICRRoutingNumber) && fundingRow.BankAccountNumber.Equals(debitRow.BankAccountNumber);
                }
            }

            return false;
        }

        public bool IsPreviousFundingWire()
        {
            var previousFundingType = _bankingInfoDataSet.LoanContract.FirstOrDefault(f => f.FundsTransferType == FundsTransferTypeLookup.FundsTransferType.ACHTransaction);
            return previousFundingType == null; //If transfer type isn't an ACH, has to be wire.
        }

        private bool Validate(FundingFailedData data, out string errorMessage, out FundsTransferTypeLookup.FundsTransferType transferType)
        {
            Guard.AgainstNull<ICurrentUser>(WebUser, "_webUser");
            Guard.AgainstNull<CustomerUserIdDataSet>(_customerUserIdDataSet, "_customerUserIdDataSet");
            Guard.AgainstNull<CustomerUserIdDataSet.ApplicationRow>(Application, "_applicationRow");

            DateTime firstPaymentBusinessDate;
            BankingInstitution fundingBankingInstitution = null;
            BankingInstitution paymentBankingInstitution = null;

            var newFundingDate = BusinessCalendar.GetPossibleFundingDates(ApplicationId, true).FindByDate(data.FundingDate);
            var firstPaymentDate = BusinessCalendar.GetFirstPaymentDate(data.FundingDate, PaymentDayOfMonth, out firstPaymentBusinessDate);
            if (newFundingDate == null || newFundingDate.HasExpired)
            {
                transferType = FundsTransferTypeLookup.FundsTransferType.NotSelected;
                errorMessage = Resources.LoanAppErrorMessages.FundingDateNotAvailable;
                return false;
            }
            transferType = newFundingDate.FundsTransferType;
            if (newFundingDate.HasEscaltedFundsTransferType && !IsWire)
            {
                errorMessage = Resources.LoanAppErrorMessages.ACHFundingDateExpired;
                return false;
            }

            bool creditAccountFailed = false;
            bool debitAccountFailed = false;

            if (data.FundingAccount == null || data.FundingAccount.AccountNumber.IsNullOrEmpty())
            {
                errorMessage = "FundingAccount is required";
                return false;
            }

            // If we're submitting a payment account, validate it
            if (IsAutoPay() && IsPaymentAccountSameAsFundingAccount())
            {
                if (data.PaymentAccountSameAsFunding.GetValueOrDefault())
                {
                    data.PaymentAccount = data.FundingAccount;
                }
                else if (data.PaymentAccount == null || data.PaymentAccount.AccountNumber.IsNullOrEmpty())
                {
                    errorMessage = "PaymentAccount is required";
                    return false;
                }

                paymentBankingInstitution = DomainServiceLoanApplicationOperations.GetBankingInstitution(data.PaymentAccount.RoutingNumber, BankAccountActionTypeLookup.BankAccountActionType.Debit, ApplicationId);
                if (paymentBankingInstitution == null) debitAccountFailed = true;
            }


            fundingBankingInstitution = DomainServiceLoanApplicationOperations.GetBankingInstitution(data.FundingAccount.RoutingNumber, BankAccountActionTypeLookup.BankAccountActionType.Credit, ApplicationId);
            if (fundingBankingInstitution == null) creditAccountFailed = true;

            //if credit does not allow WireTransfers return error
            if (transferType.FundingByWire() && !fundingBankingInstitution.WireRoutingNumberIsSupportedByUSBank)
            {
                errorMessage = "The 9 digit routing number entered is invalid, please correct the information below and resubmit.  Thank You.";
                return false;
            }
            //if MICR number is not a valid ACH routing number return error back to customer
            else if (transferType.FundingByACH() && !fundingBankingInstitution.ACHRoutingNumberIsSupportedByFedFile)
            {
                errorMessage = "The 9 digit routing number entered is invalid, please correct the information below and resubmit.  Thank You.";
                return false;
            }

            if (creditAccountFailed)
            {
                errorMessage = "The 9 digit routing number entered for your deposit account is invalid, please correct the information below and resubmit.  Thank You.";
                return false;
            }
            if (debitAccountFailed)
            {
                errorMessage = "The 9 digit routing number entered for your payment account is invalid, please correct the information below and resubmit.  Thank You.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

    }
}