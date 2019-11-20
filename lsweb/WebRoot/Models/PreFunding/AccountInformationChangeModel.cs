using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using FirstAgain.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LightStreamWeb.Models.ApplicationStatus;
using System.Web.Mvc;
using FirstAgain.Common;
using LightStreamWeb.App_State;
using FirstAgain.Domain.ServiceModel.Client;
using BusinessCalendar = FirstAgain.Domain.ServiceModel.Client.BusinessCalendar;
using BankAccountInfo = LightStreamWeb.Models.ApplicationStatus.LoanAcceptanceModel.BankAccountInfo;
using FirstAgain.Domain.SharedTypes.IDProfile;

namespace LightStreamWeb.Models.PreFunding
{
    public class AccountInformationChangeModel : PreFundingPageModel, IFundingAccountModel
    {
        public AccountInformationChangeModel(CustomerUserIdDataSet cuidds, LoanOfferDataSet loanOfferDataSet)
            : base(cuidds, loanOfferDataSet)
        {
            AuthorizedSignerList = LightStreamWeb.Helpers.ApplicationStatusHelper.PopulateAuthorizedSignerList(Application);
            BankingInfoDataSet ds = DomainServiceLoanApplicationOperations.GetBankingInfo(ApplicationId);
        }

        public string GetSecurityQuestion()
        {
            return SecurityQuestionLookup.GetCaption(this._customerUserIdDataSet.AccountInfo.SecurityQuestion);
        }

        public string LastFourDigitsOfDebitAccountNumber
        {
            get
            {
                if (PaymentAccountNumber.IsNullOrEmpty())
                {
                    return string.Empty;
                }
                if (PaymentAccountNumber.Length > 4)
                {
                    return PaymentAccountNumber.Substring(PaymentAccountNumber.Length - 4);
                }
                return PaymentAccountNumber;
            }
        }
        public List<SelectListItem> AuthorizedSignerList { get; set; }
        public string GetAuthorizedSigner()
        {
            if (AuthorizedSignerList.Any(x => x.Selected))
            {
                return AuthorizedSignerList.First(x => x.Selected).Value;
            }

            return "";
        }

        public bool ValidateSecurityAnswer(string answer)
        {
            return answer.IsNotNull() && answer.Equals(_customerUserIdDataSet.AccountInfo.SecurityAnswer, StringComparison.InvariantCultureIgnoreCase);
        }

        public class AccountUpdateData
        {
            public AccountUpdateData()
            {
                FundingAccount = new BankAccountInfo();
                PaymentAccount = new BankAccountInfo();
                BrokerageAccount = new LoanAcceptanceModel.BrokerageAccountInfo();
            }
            public AccountUpdateAction AccountAction { get; set; }
            public bool? FundingAccountIsBrokerageAccount { get; set; }
            public BankAccountInfo FundingAccount { get; set; }
            public BankAccountInfo PaymentAccount { get; set; }
            public LoanAcceptanceModel.BrokerageAccountInfo BrokerageAccount { get; set; }
            public string SecurityQuestionAnswer { get; set; }

            public enum AccountUpdateAction
            {
                StepDeposit,
                StepPayment,
                StepBothSame,
                StepBothDifferent
            }
        }

        internal bool Update(AccountUpdateData data, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (!ValidateBankingInfo(data, out errorMessage))
            {
                return false;
            }

            BankingInfoDataSet ds = DomainServiceLoanApplicationOperations.GetBankingInfo(ApplicationId);

            // validate + populate deposit account?
            if (data.AccountAction.IsOneOf(AccountUpdateData.AccountUpdateAction.StepDeposit, AccountUpdateData.AccountUpdateAction.StepBothSame, AccountUpdateData.AccountUpdateAction.StepBothDifferent))
            {
                var fundingBank = DomainServiceLoanApplicationOperations.GetBankingInstitution(data.FundingAccount.RoutingNumber, BankAccountActionTypeLookup.BankAccountActionType.Credit, ApplicationId);
                if (fundingBank == null)
                {
                    errorMessage = "Deposit account is invalid";
                    return false;
                }

                var fundingRow = ds.BankAccountInfo.FirstOrDefault(x => x.BankAccountActionType == BankAccountActionTypeLookup.BankAccountActionType.Credit);
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
            }

            if (data.AccountAction == AccountUpdateData.AccountUpdateAction.StepBothSame)
            {
                data.PaymentAccount = data.FundingAccount;
            }

            // validate + populate payment account?
            if (data.AccountAction.IsOneOf(AccountUpdateData.AccountUpdateAction.StepPayment, AccountUpdateData.AccountUpdateAction.StepBothSame, AccountUpdateData.AccountUpdateAction.StepBothDifferent))
            {
                var paymentBank = DomainServiceLoanApplicationOperations.GetBankingInstitution(data.PaymentAccount.RoutingNumber, BankAccountActionTypeLookup.BankAccountActionType.Debit, ApplicationId);
                if (paymentBank == null)
                {
                    errorMessage = "Payment account is invalid";
                    return false;
                }

                var debitRow = ds.BankAccountInfo.FirstOrDefault(x => x.BankAccountActionType == BankAccountActionTypeLookup.BankAccountActionType.Debit);
                MapToBankingInfoRow(data.PaymentAccount, paymentBank, debitRow);
            }

            DomainServiceLoanApplicationOperations.UpdateBankingInfo(ds, Helpers.EventAuditLogHelper.PopulateEventAuditLogDataSet(_customerUserIdDataSet, WebUser, EventTypeLookup.EventType.ModifiedBankingInfo, string.Empty));

            return true;
        }

        protected static void MapToBankingInfoRow(BankAccountInfo modelData, BankingInstitution bankingInfo, BankingInfoDataSet.BankAccountInfoRow row)
        {
            row.BankAccountHolderNameTypeId = (short)modelData.AuthorizedSigner;
            row.BankAccountNumber = modelData.AccountNumber;
            row.ACHTransactionRoutingNumber = bankingInfo.ACHRoutingNumber;
            if (string.IsNullOrEmpty(bankingInfo.WireRoutingNumber))
            {
                row.SetWireTransferRoutingNumberNull();
            }
            else
            {
                row.WireTransferRoutingNumber = bankingInfo.WireRoutingNumber;
            }
            if (string.IsNullOrEmpty(bankingInfo.CorrespondentBankRoutingNumber))
            {
                row.SetCorrespondentBankRoutingNumberNull();
            }
            else
            {
                row.CorrespondentBankRoutingNumber = bankingInfo.CorrespondentBankRoutingNumber;
            }
            row.MICRRoutingNumber = bankingInfo.MICRRoutingNumber;
            row.IsWireAble = bankingInfo.WireRoutingNumberIsSupportedByUSBank;
            if (string.IsNullOrEmpty(bankingInfo.WireRoutingNumberUSBankShortName))
            {
                row.SetWireTransferRoutingNumberShortNameNull();
            }
            else
            {
                row.WireTransferRoutingNumberShortName = bankingInfo.WireRoutingNumberUSBankShortName;
            }
            if (string.IsNullOrEmpty(bankingInfo.CorrespondentBankUSBankShortName))
            {
                row.SetCorrespondentBankRoutingNumberShortNameNull();
            }
            else
            {
                row.CorrespondentBankRoutingNumberShortName = bankingInfo.CorrespondentBankUSBankShortName;
            }
            row.BankAccountType = modelData.BankAccountType;
        }

        protected PossibleFundingDate _possibleFundingDate = null;
        protected PossibleFundingDate PossibleFundingDate
        {
            get
            {
                if (_possibleFundingDate != null)
                {
                    return _possibleFundingDate;
                }

                var possibleFundingDates = BusinessCalendar.GetPossibleFundingDates(ApplicationId, true);
                _possibleFundingDate = possibleFundingDates.FindByDate(FundingDate);

                return _possibleFundingDate;
            }
        }


        private bool ValidateBankingInfo(AccountUpdateData data, out string errorMessage)
        {
            Guard.AgainstNull<ICurrentUser>(WebUser, "_webUser");
            Guard.AgainstNull<CustomerUserIdDataSet>(_customerUserIdDataSet, "_customerUserIdDataSet");
            Guard.AgainstNull<CustomerUserIdDataSet.ApplicationRow>(Application, "_applicationRow");

            DateTime firstPaymentBusinessDate;
            BankingInstitution fundingBankingInstitution = null;
            BankingInstitution paymentBankingInstitution = null;

            var firstPaymentDate = BusinessCalendar.GetFirstPaymentDate(FundingDate, PaymentDayOfMonth, out firstPaymentBusinessDate);
            if (PossibleFundingDate == null || PossibleFundingDate.HasExpired)
            {
                errorMessage = Resources.LoanAppErrorMessages.FundingDateNotAvailable;
                return false;
            }
            if (PossibleFundingDate.HasEscaltedFundsTransferType && !IsWire)
            {
                errorMessage = Resources.LoanAppErrorMessages.ACHFundingDateExpired;
                return false;
            }

            bool creditAccountFailed = false;
            bool debitAccountFailed = false;

            // If we're submitting a payment account, validate it
            if (IsAutoPay())
            {
                if (data.AccountAction.IsOneOf(AccountUpdateData.AccountUpdateAction.StepPayment, AccountUpdateData.AccountUpdateAction.StepBothSame, AccountUpdateData.AccountUpdateAction.StepBothDifferent))
                {
                    if (data.AccountAction == AccountUpdateData.AccountUpdateAction.StepBothSame)
                    {
                        data.PaymentAccount = data.FundingAccount;
                    }
                    if (data.PaymentAccount == null || data.PaymentAccount.AccountNumber.IsNullOrEmpty())
                    {
                        errorMessage = "PaymentAccount is required";
                        return false;
                    }

                    paymentBankingInstitution = DomainServiceLoanApplicationOperations.GetBankingInstitution(data.PaymentAccount.RoutingNumber, BankAccountActionTypeLookup.BankAccountActionType.Debit, ApplicationId);
                    if (paymentBankingInstitution == null) debitAccountFailed = true;
                }

            }

            // if we're changing a deposit account, validate it
            if (data.AccountAction.IsOneOf(AccountUpdateData.AccountUpdateAction.StepDeposit, AccountUpdateData.AccountUpdateAction.StepBothSame, AccountUpdateData.AccountUpdateAction.StepBothDifferent))
            {
                if (data.FundingAccount == null || data.FundingAccount.AccountNumber.IsNullOrEmpty())
                {
                    errorMessage = "FundingAccount is required";
                    return false;
                }

                fundingBankingInstitution = DomainServiceLoanApplicationOperations.GetBankingInstitution(data.FundingAccount.RoutingNumber, BankAccountActionTypeLookup.BankAccountActionType.Credit, ApplicationId);
                if (fundingBankingInstitution == null) creditAccountFailed = true;

                //if credit does not allow WireTransfers return error
                if (PossibleFundingDate.FundsTransferType.FundingByWire() && !fundingBankingInstitution.WireRoutingNumberIsSupportedByUSBank)
                {
                    errorMessage = "The 9 digit routing number entered is invalid, please correct the information below and resubmit.  Thank You.";
                    return false;
                }
                //if MICR number is not a valid ACH routing number return error back to customer
                else if (PossibleFundingDate.FundsTransferType.FundingByACH() && !fundingBankingInstitution.ACHRoutingNumberIsSupportedByFedFile)
                {
                    errorMessage = "The 9 digit routing number entered is invalid, please correct the information below and resubmit.  Thank You.";
                    return false;
                }
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