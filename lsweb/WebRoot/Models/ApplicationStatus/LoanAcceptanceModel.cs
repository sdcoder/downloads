using FirstAgain.Common;
using FirstAgain.Common.Extensions;
using FirstAgain.Correspondence.ServiceModel.Client;
using FirstAgain.Domain.Common;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.ServiceModel.Contracts.MessageContracts.IDProfileOperations;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.SharedTypes.IDProfile;
using FirstAgain.Domain.SharedTypes.InterestRate;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using FirstAgain.Common.Logging;
using LightStreamWeb.App_State;
using LightStreamWeb.Controllers.Shared;
using LightStreamWeb.Helpers;
using LightStreamWeb.Models.LoanAcceptance;
using LightStreamWeb.Models.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BusinessCalendar = FirstAgain.Domain.ServiceModel.Client.BusinessCalendar;
using LightStreamWeb.ServerState;

namespace LightStreamWeb.Models.ApplicationStatus
{
    [Serializable]
    public class LoanAcceptanceModel
    {
        public LoanAcceptanceModel()
        {
            EmailPreferences = new List<SolicitationPreferenceLookup.SolicitationPreference>();
            LoanTerms = new LoanTermsModel();
            FundingAccount = new BankAccountInfo();
            PaymentAccount = new BankAccountInfo();
            BrokerageAccount = new BrokerageAccountInfo();
            Verification = new AccountVerificationModel();
            NonAppSpouseData = new NonApplicantSpouseModel();
        }

        // public properties that can be set via the client
        public LoanTermsModel ChangeLoanTerms { get; set; }
        public bool IsSigned { get; set; }
        public bool IsPartiallySigned { get; set; }
        public SignatureModel ApplicantSignature { get; set; }
        public SignatureModel CoApplicantSignature { get; set; }
        public List<SolicitationPreferenceLookup.SolicitationPreference> EmailPreferences { get; set; }
        public bool? FundingAccountIsBrokerageAccount { get; set; }
        public int NumberOfAttempts { get; set; }

        // public / private properties can only be set by the server
        public LoanTermsModel LoanTerms { get; set; }
        public int ApplicationId { get; set; }
        public StateLookup.State State { get; set; }
        public bool IsEligibleForFloridaDocStampTax { get; set; }
        public bool RequiresAcknowledgements { get; set; }
        public bool IsSecured { get; set; }
        public bool IsJoint { get; set; }
        public System.Collections.Hashtable CalendarFundingDates { get; set; }
        public DateTime? FirstPaymentDate { get; set; }
        public DateTime RateLockDate { get; set; }

        // internals
        private CustomerUserIdDataSet.ApplicationRow _applicationRow = null;
        private ICurrentUser _webUser = null;
        private CustomerUserIdDataSet _customerData = null;
        private LoanOfferDataSet _loanOfferDataSet = null;
        private PossibleFundingDate _possibleFundingDate = null;
        private PossibleFundingDate PossibleFundingDate
        {
            get
            {
                if (_possibleFundingDate != null)
                {
                    return _possibleFundingDate;
                }

                if (FundingDate.HasValue)
                {
                    var possibleFundingDates = BusinessCalendar.GetPossibleFundingDates(ApplicationId, true);
                    _possibleFundingDate = possibleFundingDates.FindByDate(FundingDate.Value);
                }

                return _possibleFundingDate;
            }
        }
        BankingInstitution _fundingBankingInstitution = null;
        BankingInstitution _paymentBankingInstitution = null;

        private bool IsAutoPay()
        {
            if (_loanOfferDataSet != null && _loanOfferDataSet.LatestApprovedLoanTerms != null)
            {
                return _loanOfferDataSet.LatestApprovedLoanTerms.PaymentType == PaymentTypeLookup.PaymentType.AutoPay;
            }

            return _applicationRow != null && _applicationRow.GetApplicationDetailRows()[0].PaymentType == PaymentTypeLookup.PaymentType.AutoPay;
        }

        #region Signatures
        public class SignatureModel
        {
            public string Name { get; set; }
            public bool UsingScriptFont { get; set; }
            public bool Submitted { get; set; }
            public string Data { get; set; }
        }
        #endregion

        #region NLTR
        public class SubmitChangeLoanTermsRequestResult
        {
            public bool Success { get; set; }
            public bool IsAutoApproved { get; set; }
            public bool IsAutoDeclined { get; set; }
            public bool IsStale { get; set; }
            public string ErrorMessage { get; set; }
            public decimal MaxLoanAmount { get; set; }
            public decimal MinLoanAmount { get; set; }

            public static SubmitChangeLoanTermsRequestResult Failure(string errorMessage)
            {
                return new SubmitChangeLoanTermsRequestResult()
                {
                    Success = false,
                    ErrorMessage = errorMessage
                };
            }
            public static SubmitChangeLoanTermsRequestResult Ok(bool isAutoApproved, bool isAutoDeclined = false)
            {
                return new SubmitChangeLoanTermsRequestResult()
                {
                    Success = true,
                    IsAutoApproved = isAutoApproved,
                    IsAutoDeclined = isAutoDeclined
                };
            }
        }

        public SubmitChangeLoanTermsRequestResult ConfirmChangeLoanTermsRequest()
        {
            Guard.AgainstNull<LoanTermsModel>(LoanTerms, "LoanTerms");

            if (ChangeLoanTerms == null)
            {
                return SubmitChangeLoanTermsRequestResult.Failure("New loan terms request was not found.");
            }
            if (LoanTerms.LoanAmountMinusFees == ChangeLoanTerms.LoanAmountMinusFees && LoanTerms.LoanTerm == ChangeLoanTerms.LoanTerm && LoanTerms.PaymentMethod == ChangeLoanTerms.PaymentMethod)
            {
                return SubmitChangeLoanTermsRequestResult.Failure("If you wish to submit a change, please modify your existing terms.");
            }

            string errorMessage;
            InterestRates matrix = null;
            decimal maxLoanAmount;
            if (ValidateAmountAndTerm(out matrix, out errorMessage, out maxLoanAmount))
            {
                SubmitNewLoanTermsRequestToBackEnd();

                return SubmitChangeLoanTermsRequestResult.Ok(true, false);
            }
            else
            {
                return SubmitChangeLoanTermsRequestResult.Failure(errorMessage);
            }
        }

        public SubmitChangeLoanTermsRequestResult SubmitChangeLoanTermsRequest()
        {
            Guard.AgainstNull<LoanTermsModel>(LoanTerms, "LoanTerms");

            if (ChangeLoanTerms == null)
            {
                return SubmitChangeLoanTermsRequestResult.Failure("New loan terms request was not found.");
            }
            if (LoanTerms.LoanAmountMinusFees == ChangeLoanTerms.LoanAmountMinusFees && LoanTerms.LoanTerm == ChangeLoanTerms.LoanTerm && LoanTerms.PaymentMethod == ChangeLoanTerms.PaymentMethod)
            {
                return SubmitChangeLoanTermsRequestResult.Failure("If you wish to submit a change, please modify your existing terms.");
            }

            string errorMessage;
            InterestRates matrix = null;
            decimal maxLoanAmount;
            if (ValidateAmountAndTerm(out matrix, out errorMessage, out maxLoanAmount))
            {
                if (DomainServiceLoanApplicationOperations.CanAutoDecisionNewLoanTermsRequest(ApplicationId, ChangeLoanTerms.PaymentMethod, ChangeLoanTerms.LoanAmountMinusFees, ChangeLoanTerms.LoanTerm))
                {
                    if (!SubmitNewLoanTermsRequestToBackEnd())
                    {
                        return SubmitChangeLoanTermsRequestResult.Ok(false, true);
                    }

                    // clear out signatures
                    // TODO e3p0 - remove SessionUtility dependency
                    SessionUtility.ResetLoanAgreementSignature();

                    return SubmitChangeLoanTermsRequestResult.Ok(true);
                }
                else
                {
                    // return ok
                    return SubmitChangeLoanTermsRequestResult.Ok(false);
                }
            }
            else
            {
                return new SubmitChangeLoanTermsRequestResult()
                {
                    Success = false,
                    ErrorMessage = errorMessage,
                    MaxLoanAmount = maxLoanAmount,
                    MinLoanAmount = matrix.GetMinAmount().Value
                };
            }
        }

        private bool ValidateAmountAndTerm(out InterestRates matrix, out string errorMessage, out decimal maxLoanAmount)
        {
            // TODO: populate ChangeLoanTerms with interest rate

            matrix = DomainServiceInterestRateOperations.GetApplicationInterestRates(ApplicationId);

            matrix.InterestRateParams.PaymentType = ChangeLoanTerms.PaymentMethod;

            decimal minLoanAmount = matrix.GetMinAmount().Value;
            maxLoanAmount = matrix.GetMaxAmount().Value;

            int minLoanTerm = matrix.GetMinTerm().Value;
            int maxLoanTerm = matrix.GetMaxTerm().Value;

            matrix.InterestRateParams.LoanAmount = ChangeLoanTerms.LoanAmountMinusFees;
            matrix.InterestRateParams.LoanTerm = ChangeLoanTerms.LoanTerm;

            DecimalRange dRange = matrix.GetAmountRange(term: ChangeLoanTerms.LoanTerm);
            IntRange tRange = matrix.GetTermRange(amount: ChangeLoanTerms.LoanAmountMinusFees);

            if (tRange == null)
            {
                errorMessage = string.Format(Resources.FAMessages.LoanTerms_InvalidAmount, DataConversions.FormatAndGetMoney(minLoanAmount), DataConversions.FormatAndGetMoney(maxLoanAmount));
                return false;
            }

            if (dRange == null)
            {
                errorMessage = string.Format(Resources.FAMessages.LoanTerms_InvalidTerm, minLoanTerm, maxLoanTerm);
                return false;
            }

            if (!ValidateAmount(matrix, dRange, ChangeLoanTerms.LoanAmountMinusFees, ChangeLoanTerms.LoanTerm, out errorMessage, out maxLoanAmount))
            {
                return false;
            }

            if(!ValidateRestrictedEmployeeAmount(ChangeLoanTerms.LoanAmountMinusFees, dRange.Min, out errorMessage))
            {
                return false;
            }

            if (!ValidateTerm(matrix, tRange, ChangeLoanTerms.LoanAmountMinusFees, ChangeLoanTerms.LoanTerm, out errorMessage))
            {
                return false;
            }

            // all good, ensure the interest rate and doc stamp tax in the model matches the computed value, not the submitted value (to prevent shenanigaans)
            ChangeLoanTerms.InterestRateMax = ChangeLoanTerms.InterestRateMin = matrix.GetRate(ChangeLoanTerms.LoanTerm, ChangeLoanTerms.LoanAmountMinusFees).Value;
            ChangeLoanTerms.FloridaDocStampTax = _customerData.IsEligibleForFloridaDocStampTax() ? FloridaDocumentaryStampTaxCalculator.CalculateTaxWhenTaxIsFinanced(ChangeLoanTerms.LoanAmountMinusFees) : 0M;

            return true;
        }

        private bool ValidateAmount(InterestRates matrix, DecimalRange range, decimal newRequestedLoanAmount, int newRequestedLoanTerm, out string errorMessage, out decimal maxLoanAmount)
        {
            // check for a counter offer to limit the amount if the NLTR
            CustomerUserIdDataSet.LoanTermsRequestRow counterData = _customerData.LoanTermsRequest.GetCounterLoanTerms(ApplicationId);

            string min = DataConversions.FormatAndGetMoney(range.Min);
            string max = DataConversions.FormatAndGetMoney(range.Max);
            maxLoanAmount = range.Max;

            StateRestriction restriction = matrix.GetImpingingStateRestriction(newRequestedLoanTerm, newRequestedLoanAmount);

            if (counterData != null)
            {
                if (newRequestedLoanAmount < range.Min || newRequestedLoanAmount > counterData.Amount)
                {
                    maxLoanAmount = counterData.AmountMinusFees;
                    if (maxLoanAmount == range.Min)
                    {
                        errorMessage = string.Format("New loan amount cannot exceed the Counter Offer amount of {0}.", DataConversions.FormatAndGetMoney(counterData.AmountMinusFees));
                    }
                    else
                    {
                        errorMessage = string.Format("New loan amount cannot exceed the Counter Offer amount of {0} and must be greater than {1}", DataConversions.FormatAndGetMoney(counterData.AmountMinusFees), min);
                    }
                    return false;
                }
            }
            else if (restriction != null && restriction.StateRestrictionType == StateRestrictionTypeEnum.MaxTerm)
            {
                errorMessage = matrix.GetStateRestrictionReason(restriction);
                return false;
            }
            else if (newRequestedLoanAmount < range.Min)
            {
                if (restriction != null && restriction.StateRestrictionType == StateRestrictionTypeEnum.MinAmount)
                    errorMessage = matrix.GetStateRestrictionReason(restriction);
                else
                    errorMessage = string.Format(Resources.FAMessages.LoanTerms_InvalidAmount, min, max);
                return false;
            }
            else if (newRequestedLoanAmount > range.Max)
            {
                if (restriction != null && restriction.StateRestrictionType == StateRestrictionTypeEnum.MaxAmount)
                    errorMessage = matrix.GetStateRestrictionReason(restriction);
                else
                    errorMessage = string.Format(Resources.FAMessages.LoanTerms_InvalidAmount, min, max);
                return false;
            }
            errorMessage = string.Empty;
            return true;
        }


        private bool ValidateRestrictedEmployeeAmount(decimal requestedAmount, decimal min, out string errorMessage)
        {
            bool valid = true;
            errorMessage = string.Empty;

            if (_customerData.ApplicationFlag.HasSunTrustOfficerFlagOn)
            {
                var originalRequest = _loanOfferDataSet.HstApplicationDetail
                                                       .OrderBy(h => h.EventAuditLogId).First()
                                                       .Amount;

                var originalRequestMinusFees = _customerData.IsEligibleForFloridaDocStampTax() ?
                                               FloridaDocumentaryStampTaxCalculator.CalculateLoanAmountMinusFees(originalRequest) :
                                               originalRequest;

                valid = requestedAmount <= originalRequestMinusFees;

                if (!valid)
                {
                    errorMessage = string.Format("New loan amount cannot exceed the original loan amount of {0} and must be greater than {1}.",
                                                  DataConversions.FormatAndGetMoney(originalRequestMinusFees), 
                                                  DataConversions.FormatAndGetMoney(min));
                }
            }

            return valid;
        }

        private bool ValidateTerm(InterestRates matrix, IntRange range, decimal newRequestedLoanAmount, int newRequestedLoanTerm, out string errorMessage)
        {
            StateRestriction restriction = matrix.GetImpingingStateRestriction(newRequestedLoanTerm, newRequestedLoanAmount);
            if (newRequestedLoanTerm < range.Min)
            {
                errorMessage = string.Format(Resources.FAMessages.LoanTerms_InvalidTerm, range.Min, range.Max);
                return false;
            }
            else if (newRequestedLoanTerm > range.Max)
            {
                if (restriction != null && restriction.StateRestrictionType == StateRestrictionTypeEnum.MaxTerm)
                {
                    errorMessage = string.Format(Resources.FAMessages.LoanTerms_InvalidTermStateMax, StateLookup.GetCaption(restriction.State), range.Max, range.Min, range.Max);
                }
                else
                {
                    errorMessage = string.Format(Resources.FAMessages.LoanTerms_InvalidTerm, range.Min, range.Max);
                }
                return false;
            }
            errorMessage = string.Empty;
            return true;
        }

        private bool SubmitNewLoanTermsRequestToBackEnd()
        {
            bool isAutoApproved;
            DomainServiceLoanApplicationOperations.CreateNewLoanTermsRequestWithWebActivity(_applicationRow,
                LoanTermsRequestTypeLookup.LoanTermsRequestType.NLTR,
                ProductRateTypeLookup.ProductRateType.FixedRate,
                ProductIndexTypeLookup.ProductIndexType.NA,
                ProductPeriodTypeLookup.ProductPeriodType.NA,
                ChangeLoanTerms.PaymentMethod,
                ChangeLoanTerms.LoanAmountMinusFees,
                ChangeLoanTerms.LoanTerm,
                ChangeLoanTerms.InterestRateMin * 100,
                null,
                LightStreamWeb.Helpers.WebActivityDataSetHelper.Populate(_webUser, _applicationRow.ApplicationStatusType),
                EventTypeLookup.EventType.NewLoanTermsRequest,
                out isAutoApproved);

            return isAutoApproved;
        }

        #endregion

        #region Account Setup
        public DateTime? FundingDate { get; set; }
        public BankAccountInfo FundingAccount { get; set; }
        public BankAccountInfo PaymentAccount { get; set; }
        public BrokerageAccountInfo BrokerageAccount { get; set; }
        public bool? PaymentIsSameAsFunding { get; set; }
        public bool FundingRoutingNumberInvalidForDebits { get; set; }
        public int? PaymentDayOfMonth { get; set; }
        public bool PaymentReminder5Days { get; set; }
        public bool PaymentConfirmationReminder { get; set; }
        public bool IsWire { get; set; }
        public NonApplicantSpouseModel NonAppSpouseData { get; set; }

        public AccountVerificationModel Verification { get; set; }

        #endregion

        public class ApprovedApplicationNotFoundException : Exception
        {
            public ApprovedApplicationNotFoundException() : base("The application requested could not be located. Your session may have timed out. Please log out and try again")
            {
            }
        }

        public void Populate(ICurrentUser webUser, LoanApplicationDataSet lads, CustomerUserIdDataSet cuidds, LoanOfferDataSet loanOfferDataSet)
        {
            Guard.AgainstNull<CustomerUserIdDataSet>(cuidds, "cuidds");
            Guard.AgainstNull<LoanOfferDataSet>(loanOfferDataSet, "loanOfferDataSet");
            Guard.AgainstNull<ICurrentUser>(webUser, "webUser");
            Guard.AgainstNull<int>(webUser.ApplicationId, "webUser.ApplicationId");

            var approvedTerms = loanOfferDataSet.LatestApprovedLoanTerms;

            LoanTerms.Populate(approvedTerms);

            _webUser = webUser;
            _customerData = cuidds;
            _loanOfferDataSet = loanOfferDataSet;
            ApplicationId = webUser.ApplicationId.Value;
            _applicationRow = cuidds.Application.FirstOrDefault(x => x.ApplicationId == ApplicationId);
            if (_applicationRow == null)
            {
                throw new ApprovedApplicationNotFoundException();
            }
            if (_applicationRow.ApplicationStatusType.IsNoneOf(ApplicationStatusTypeLookup.ApplicationStatusType.Approved, ApplicationStatusTypeLookup.ApplicationStatusType.ApprovedNLTR, ApplicationStatusTypeLookup.ApplicationStatusType.PreFunding, ApplicationStatusTypeLookup.ApplicationStatusType.PreFundingNLTR))
            {
                throw new ApprovedApplicationNotFoundException();
            }
            State = _applicationRow.PrimaryApplicant.GetApplicantPostalAddressRows().Single(x => x.PostalAddressType == PostalAddressTypeLookup.PostalAddressType.PrimaryResidence).State;

            IsEligibleForFloridaDocStampTax = _applicationRow.Applicants.Any(a => a.GetApplicantPostalAddressRows().Any(h => h.PostalAddressType
                .IsOneOf(PostalAddressTypeLookup.PostalAddressType.PrimaryResidence, PostalAddressTypeLookup.PostalAddressType.SecondaryResidence) && h.State == StateLookup.State.Florida));

            RequiresAcknowledgements = approvedTerms.PurposeOfLoan.IsSecured() &&
              _applicationRow.Applicants.Any(a => a.GetApplicantPostalAddressRows().Any(h => h.PostalAddressType == PostalAddressTypeLookup.PostalAddressType.PrimaryResidence
                && h.State == StateLookup.State.RhodeIsland));

            IsSecured = approvedTerms.PurposeOfLoan.IsSecured();
            IsJoint = _applicationRow.IsJoint;
            IsSigned = (_applicationRow.IsJoint && _webUser.PrimarySignatureImageBytes != null && _webUser.SecondarySignatureImageBytes != null) || (!_applicationRow.IsJoint && _webUser.PrimarySignatureImageBytes != null);
            if (!IsSigned)
            {
                IsPartiallySigned = _webUser.PrimarySignatureImageBytes != null || _webUser.SecondarySignatureImageBytes != null;
            }

            var primary = _applicationRow.Applicants.First(a => a.ApplicantType == ApplicantTypeLookup.ApplicantType.Primary);
            ApplicantSignature = new SignatureModel()
            {
                Name = string.Format("{0} {1}", primary.FirstName, primary.LastName),
                Submitted = _webUser.PrimarySignatureImageBytes != null
            };
            var secondary = _applicationRow.Applicants.FirstOrDefault(a => a.ApplicantType == ApplicantTypeLookup.ApplicantType.Secondary);
            CoApplicantSignature = new SignatureModel()
            {
                Name = (secondary == null) ? string.Empty : string.Format("{0} {1}", secondary.FirstName, secondary.LastName),
                Submitted = _webUser.SecondarySignatureImageBytes != null
            };

            // funding dates
            CalendarFundingDates = BusinessCalendarHelper.GetCalendarFundingDates(ApplicationId);

            if (!_applicationRow.IsJoint)
            {
                Verification.CardholderName = BankAccountHolderNameTypeLookup.BankAccountHolderNameType.PrimaryBorrowerName;
                PaymentAccount.AuthorizedSigner = BankAccountHolderNameTypeLookup.BankAccountHolderNameType.PrimaryBorrowerName;
                FundingAccount.AuthorizedSigner = BankAccountHolderNameTypeLookup.BankAccountHolderNameType.PrimaryBorrowerName;
            }

            Verification.Populate(_applicationRow.PrimaryApplicant,
                                           (_applicationRow.IsJoint) ? _applicationRow.SecondaryApplicant : null);

            RateLockDate = cuidds.ApplicationDetail.Single(ad => ad.ApplicationId == ApplicationId).RateLockDate;

            if (lads != null)
            {
                if (NonAppSpouseData == null)
                {
                    NonAppSpouseData = new NonApplicantSpouseModel();
                }
                NonApplicantSpouseModel.Populate(lads, NonAppSpouseData);
                // PBI 5830 - 6.	The system will default the question “Does {Applicant name} have a spouse that is not on this application?” to yes, when the Application type = Individual and the application has the other income type of Non Applicant spouse income set on the file.
                if (_applicationRow.ApplicationType == ApplicationTypeLookup.ApplicationType.Individual && NonAppSpouseData != null && NonAppSpouseData.Primary != null && NonAppSpouseData.HasNonApplicantSpouseIncome)
                {
                    NonAppSpouseData.Primary.HasNonApplicantSpouse = true;
                }
            }
        }


        public SubmitLoanContractResult SubmitLoanContract()
        {
            // first, re-run the banking info validation, in case they have been sitting around for days
            var bankInfoResult = ValidateBankingInfo();
            if (!bankInfoResult.Success)
            {
                return SubmitLoanContractResult.Fail(bankInfoResult, "/Account");
            }

            DefaultApplicationSolicitationPreferences(false);

            foreach (var p in EmailPreferences)
            {
                SetSolicitationPreference(p, true);
            }

            if (!ValidateLoanAgreementIsSigned())
            {
                return SubmitLoanContractResult.Fail(bankInfoResult, "/LoanAgreement");
            }

            // invoice accounts automatically assigned this preference
            if (!IsAutoPay())
            {
                SetSolicitationPreference(SolicitationPreferenceLookup.SolicitationPreference.EInvoicePayDays, true);
            }
            else
            {
                EventAuditLogHelper.Submit(_customerData, _webUser, EventTypeLookup.EventType.AutoPayAuthorizationAgreed, null);
            }

            if (!FundingAccountIsBrokerageAccount.GetValueOrDefault())
            {
                BrokerageAccount = new BrokerageAccountInfo();
            }

            if (PaymentIsSameAsFunding.HasValue && PaymentIsSameAsFunding.Value)
            {
                PaymentAccount = FundingAccount;
            }
            DateTime fundingProcessDate = BusinessCalendar.GetFundingProcessDate(FundingDate.Value, PossibleFundingDate.FundsTransferType);
            if (_paymentBankingInstitution == null && PaymentIsSameAsFunding.HasValue && PaymentIsSameAsFunding.Value)
            {
                _paymentBankingInstitution = _fundingBankingInstitution;
            }

            _loanOfferDataSet.SetLoanContract(
                FundingDate.Value,
                FirstPaymentDate.Value,
                (byte)PaymentDayOfMonth,
                PaymentAccount.RoutingNumber,
                PaymentAccount.AccountNumber,
                PaymentAccount.AuthorizedSigner.GetValueOrDefault(BankAccountHolderNameTypeLookup.BankAccountHolderNameType.PrimaryBorrowerName),
                FundingAccount.RoutingNumber,
                FundingAccount.AccountNumber,
                FundingAccount.AuthorizedSigner.Value,
                _fundingBankingInstitution.ACHRoutingNumber, //bankingInfo.CreditBankingInfo.ACHRoutingNumber,
                _fundingBankingInstitution.WireRoutingNumber,//bankingInfo.CreditBankingInfo.WireRoutingNumber,
                _fundingBankingInstitution.CorrespondentBankRoutingNumber,//bankingInfo.CreditBankingInfo.CorrespondentBankRoutingNumber,
                _fundingBankingInstitution.WireRoutingNumberIsSupportedByUSBank,//bankingInfo.CreditBankingInfo.WireIndicator,
                _fundingBankingInstitution.WireRoutingNumberUSBankShortName,//bankingInfo.CreditBankingInfo.USBankShortname,
                _fundingBankingInstitution.CorrespondentBankUSBankShortName,//bankingInfo.CreditBankingInfo.USBankShortnameForCorrespondentBank,
                _paymentBankingInstitution.ACHRoutingNumber,//bankingInfo.DebitBankingInfo.ACHRoutingNumber,
                _paymentBankingInstitution.WireRoutingNumber, //bankingInfo.DebitBankingInfo.WireRoutingNumber,
                _paymentBankingInstitution.CorrespondentBankRoutingNumber,
                _paymentBankingInstitution.WireRoutingNumberIsSupportedByUSBank,
                _paymentBankingInstitution.WireRoutingNumberUSBankShortName,
                _paymentBankingInstitution.CorrespondentBankUSBankShortName,
                ApplicationId,
                _loanOfferDataSet.LoanOffer[0].LoanOfferId,
                fundingProcessDate,
                PossibleFundingDate.FundsTransferType,
                FundingAccountIsBrokerageAccount.GetValueOrDefault(),
                BrokerageAccount.BeneficiaryBankName,
                BrokerageAccount.BeneficiaryRoutingNumber,
                BrokerageAccount.BeneficiaryAccountNumber,
                BrokerageAccount.IntermediaryBankName,
                BrokerageAccount.IntermediaryRoutingNumber,
                BrokerageAccount.IntermediaryAccountNumber,
                FundingAccount.BankAccountType,
                PaymentAccount.BankAccountType
                );

            if (!_applicationRow.FlagIsSet(FlagLookup.Flag.BypassCreditCard))
            {
                //Accept VISA Or MasterCard Only, after mod10 check passes
                if (!ValidateCreditCardNumber())
                {
                    return SubmitLoanContractResult.Fail("/Verify", Resources.FAMessages.LoanOfferInvalidMCOrVisaNumber, "Verification.CreditCardNumber");
                }
            }


            CreditCardVerificationRequest ccr = new CreditCardVerificationRequest();
            if (!_applicationRow.FlagIsSet(FlagLookup.Flag.BypassCreditCard))
            {
                ccr.CreditCardType = Verification.CreditCardNumber.StartsWith("4") ? "VISA" : "MC";
                ccr.AttemptNumber = this.NumberOfAttempts;

                ccr.CardBillingAddressLine1 = Verification.Address.BillingAddress;

                if (Verification.Address.SecondaryUnitType != PostalAddressSecondaryUnitTypeLookup.PostalAddressSecondaryUnitType.NotSelected && !string.IsNullOrEmpty(Verification.Address.SecondaryUnitValue))
                {
                    ccr.CardBillingAddressLine1 += " " + Verification.Address.SecondaryUnitType.ToString() + " " + Verification.Address.SecondaryUnitValue;
                }

                ccr.CardBillingAddressNumber = RetrieveNumberFromAddress(Verification.Address.BillingAddress);
                ccr.CardBillingAddressCity = Verification.Address.City;
                ccr.CardBillingAddressState = StateLookup.GetCaption(Verification.Address.State);
                ccr.CardBillingAddressZip5 = Verification.Address.ZipCode;
                ccr.CardExpirationMonth = Verification.ExpirationMonth.GetValueOrDefault().ToString();
                ccr.CardExpirationYear = Verification.ExpirationYear.GetValueOrDefault().ToString(); //(int.Parse(ddlExpirationDate.Text) - 2000).ToString("00");
                ccr.CreditCardNumber = Verification.CreditCardNumber;
                ccr.CardCode = string.Empty;
            }

            string applicantType = "Applicant";
            if (_applicationRow.IsJoint)
            {
                if (Verification.CardholderName == BankAccountHolderNameTypeLookup.BankAccountHolderNameType.SecondaryBorrowerName)
                {
                    applicantType = "CoApplicant";
                }
            }
            ccr.CreditCardBelongsTo = applicantType;
            ccr.EditLevel = 1;
            ccr.PreAuthAmount = 1.00m;
            ccr.ApplicationId = ApplicationId;
            ccr.RequestedBy = "PublicSite";

            if (NonAppSpouseData != null && _applicationRow.Applicants.Any(a => a.GetApplicantPostalAddressRows().Any(h => h.State == StateLookup.State.Wisconsin)))
            {
                if (NonAppSpouseData.Primary != null)
                {
                    _loanOfferDataSet.SetNonApplicantSpouseData(
                        ApplicantTypeLookup.ApplicantType.Primary,
                        NonAppSpouseData.Primary.HasNonApplicantSpouse,
                        NonAppSpouseData.Primary.SpouseNotInState,
                        NonAppSpouseData.Primary.Name.First,
                        NonAppSpouseData.Primary.Name.MI,
                        NonAppSpouseData.Primary.Name.Last,
                        NonAppSpouseData.Primary.Address.AddressLine,
                        NonAppSpouseData.Primary.Address.City,
                        NonAppSpouseData.Primary.Address.State,
                        NonAppSpouseData.Primary.Address.SecondaryUnitType,
                        NonAppSpouseData.Primary.Address.SecondaryUnitValue,
                        NonAppSpouseData.Primary.Address.ZipCode);
                }
                if (_applicationRow.IsJoint && NonAppSpouseData.Secondary != null)
                {
                    _loanOfferDataSet.SetNonApplicantSpouseData(
                        ApplicantTypeLookup.ApplicantType.Secondary,
                        NonAppSpouseData.Secondary.HasNonApplicantSpouse,
                        NonAppSpouseData.Secondary.SpouseNotInState,
                        NonAppSpouseData.Secondary.Name.First,
                        NonAppSpouseData.Secondary.Name.MI,
                        NonAppSpouseData.Secondary.Name.Last,
                        NonAppSpouseData.Secondary.Address.AddressLine,
                        NonAppSpouseData.Secondary.Address.City,
                        NonAppSpouseData.Secondary.Address.State,
                        NonAppSpouseData.Secondary.Address.SecondaryUnitType,
                        NonAppSpouseData.Secondary.Address.SecondaryUnitValue,
                        NonAppSpouseData.Secondary.Address.ZipCode);
                }
            }

            try
            {
                if (!_applicationRow.FlagIsSet(FlagLookup.Flag.BypassCreditCard))
                {
                    DomainServiceIDProfileOperations.CreditCardVerification(ccr);
                }

                DomainServiceLoanApplicationOperations.SubmitLoanContractWithWebActivity(_loanOfferDataSet, Helpers.WebActivityDataSetHelper.Populate(_webUser, _applicationRow.ApplicationStatusType), EventTypeLookup.EventType.LoanAcceptance, _applicationRow.FlagIsSet(FlagLookup.Flag.BypassCreditCard));
                return SubmitLoanContractResult.Ok();
            }
            catch (Exception err)
            {
                LightStreamLogger.WriteWarning(err, "CC Verification Exception failed for {ApplicationId}", ApplicationId);
                return SubmitLoanContractResult.Fail(tab: "/Verify", errorMessage: "Unfortunately, due to a system error we are unable to process your credit card. Please check back with us shortly to complete the funding of your loan.<br />Thank you.");
            }
        }


        private bool ValidateCreditCardNumber()
        {
            if (Verification == null || Verification.CreditCardNumber.IsNull())
            {
                return false;
            }
            //Accept VISA Or MasterCard Only, after mod10 check passes
            string ccNumber = Verification.CreditCardNumber.Trim();
            bool mod10CheckPassed = CreditCardValidator.Mod10Check(ccNumber);
            bool isVisa = !string.IsNullOrWhiteSpace(ccNumber) && ccNumber.StartsWith("4");
            bool isMasterCard = !string.IsNullOrWhiteSpace(ccNumber) && ccNumber.StartsWith("5");
            return mod10CheckPassed && (isVisa || isMasterCard);
        }

        /// <summary>
        /// Assumption, First number in the address is the one that they want
        /// 150 W 51 apt 1826 would return 150
        /// Eight 7th street would return 7
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public string RetrieveNumberFromAddress(string address)
        {
            string numberResult = "";
            bool containsNumber = false;
            foreach (char number in address)
            {
                if (DataConversions.IsInt(number))
                {
                    numberResult = numberResult + number;
                    containsNumber = true;
                }
                if (number.Equals(" "))
                {
                    break;
                }
            }
            //return min or the number
            if (containsNumber)
            {
                return numberResult;
            }

            return string.Empty;
        }


        private bool ValidateLoanAgreementIsSigned()
        {
            var documentStore = CorrespondenceServiceCorrespondenceOperations.GetApplicationDocumentStore(ApplicationId);
            var row = documentStore.DocumentStore
                        .Where(a => a.ApplicationId == ApplicationId && a.CorrespondenceCategory == CorrespondenceCategoryLookup.CorrespondenceCategory.LoanAgreementHtml)
                        .OrderByDescending(a => a.CreatedDate).FirstOrDefault();
            if (row != null)
            {
                if (_applicationRow.IsJoint)
                {
                    if (!documentStore.DocumentStore
                        .Any(a => a.ApplicationId == ApplicationId && a.CorrespondenceCategory == CorrespondenceCategoryLookup.CorrespondenceCategory.LoanAgreementCoAppSignature))
                    {
                        return false;
                    }
                }
                if (!documentStore.DocumentStore
                    .Any(a => a.ApplicationId == ApplicationId && a.CorrespondenceCategory == CorrespondenceCategoryLookup.CorrespondenceCategory.LoanAgreementAppSignature))
                {
                    return false;
                }
                if (!documentStore.DocumentStore
                        .Any(a => a.ApplicationId == ApplicationId
                            && a.CorrespondenceCategory == CorrespondenceCategoryLookup.CorrespondenceCategory.LoanAgreementHtml
                            && !a.IsLoanTermsRequestIdNull()
                            && a.LoanTermsRequestId == _loanOfferDataSet.LatestActiveLoanOffer.LoanTermsRequestId))
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        private void SetSolicitationPreference(SolicitationPreferenceLookup.SolicitationPreference preference,
                                              bool IsPreferred)
        {
            bool addnew = false;
            LoanOfferDataSet.ApplicationSolicitationPreferenceRow solicitationRow = _loanOfferDataSet.ApplicationSolicitationPreference.FindByApplicationIdSolicitationPreferenceIdSolicitationPreferenceCategoryId(ApplicationId, (short)preference, (short)SolicitationPreferenceCategoryLookup.SolicitationPreferenceCategory.Email);
            if (null == solicitationRow)
            {
                solicitationRow = _loanOfferDataSet.ApplicationSolicitationPreference.NewApplicationSolicitationPreferenceRow();
                addnew = true;
            }
            solicitationRow.ApplicationId = ApplicationId;
            solicitationRow.SolicitationPreference = preference;
            solicitationRow.IsPreferred = IsPreferred;
            solicitationRow.SolicitationPreferenceCategory = SolicitationPreferenceCategoryLookup.SolicitationPreferenceCategory.Email;
            if (addnew)
            {
                _loanOfferDataSet.ApplicationSolicitationPreference.AddApplicationSolicitationPreferenceRow(solicitationRow);
            }
        }

        private void DefaultApplicationSolicitationPreferences(bool IsPreferred)
        {
            if (IsAutoPay())
            {
                SetSolicitationPreference(SolicitationPreferenceLookup.SolicitationPreference.PaymentReminder5Days, IsPreferred);
            }
            SetSolicitationPreference(SolicitationPreferenceLookup.SolicitationPreference.EmailPaymentConfirmation, IsPreferred);
        }

        private bool ValidateRequiredBankingFields(out string errorMessage, out string errorField)
        {
            if (!FundingDate.HasValue)
            {
                errorField = "LoanAcceptance.FundingDate";
                errorMessage = "Funding date is required";
                return false;
            }
            if (!PaymentDayOfMonth.HasValue || PaymentDayOfMonth.GetValueOrDefault() <= 0)
            {
                errorField = "PaymentDayOfMonth";
                errorMessage = "Payment day of month is required";
                return false;
            }
            if (FundingAccount == null || FundingAccount.RoutingNumber.IsNullOrEmpty())
            {
                errorField = "FundingAccount.RoutingNumber";
                errorMessage = "Funding account routing number is required.";
                return false;
            }
            errorField = string.Empty;
            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Server-side validation for all banking info. Client should prevent user from proceeding to CC validation page if the banking info isn't valid
        /// </summary>
        /// <returns></returns>
        public ValidateBankingInfoResult ValidateBankingInfo()
        {
            Guard.AgainstNull<ICurrentUser>(_webUser, "_webUser");
            Guard.AgainstNull<CustomerUserIdDataSet>(_customerData, "_customerData");
            Guard.AgainstNull<CustomerUserIdDataSet.ApplicationRow>(_applicationRow, "_applicationRow");

            DateTime firstPaymentBusinessDate;
            string errorMessage, errorField;
            if (!ValidateRequiredBankingFields(out errorMessage, out errorField))
            {
                return ValidateBankingInfoResult.Fail(errorMessage, errorField);
            }

            if (_loanOfferDataSet.LatestApprovedLoanTerms.PaymentType == PaymentTypeLookup.PaymentType.Invoice)
            {
                PaymentAccount = new BankAccountInfo();
            }

            FirstPaymentDate = BusinessCalendar.GetFirstPaymentDate(FundingDate.Value, PaymentDayOfMonth.Value, out firstPaymentBusinessDate);
            if (PossibleFundingDate == null || PossibleFundingDate.HasExpired)
            {
                return ValidateBankingInfoResult.Fail(Resources.LoanAppErrorMessages.FundingDateNotAvailable);
            }
            if (PossibleFundingDate.HasEscaltedFundsTransferType && !IsWire)
            {
                return ValidateBankingInfoResult.Fail(Resources.LoanAppErrorMessages.ACHFundingDateExpired);
            }

            bool creditAccountFailed = false;
            bool debitAccountFailed = false;

            if (PaymentAccount == null)
            {
                _fundingBankingInstitution = DomainServiceLoanApplicationOperations.GetBankingInstitution(FundingAccount.RoutingNumber, BankAccountActionTypeLookup.BankAccountActionType.Credit, ApplicationId);
                if (_fundingBankingInstitution == null) creditAccountFailed = true;
            }
            else
            {
                BankingInstitution[] banks = DomainServiceLoanApplicationOperations.GetBankingInstitutions(FundingAccount.RoutingNumber, PaymentAccount.RoutingNumber, ApplicationId);
                _fundingBankingInstitution = banks[0];
                _paymentBankingInstitution = banks[1];
                if (_fundingBankingInstitution == null) creditAccountFailed = true;
                if (_paymentBankingInstitution == null 
                    || _paymentBankingInstitution.RoutingNumberIsNotValidForACHDebits 
                    || IsInvalidAutoPayPaymentInformation())
                    debitAccountFailed = true;
            }

            if (creditAccountFailed)
            {
                return ValidateBankingInfoResult.Fail("The 9 digit routing number entered is invalid, please correct the information below and resubmit.  Thank You.", "FundingAccount.RoutingNumber");
            }
            if (debitAccountFailed)
            {
                return ValidateBankingInfoResult.Fail("The 9 digit routing number entered is invalid, please correct the information below and resubmit.  Thank You.", "PaymentAccount.RoutingNumber");
            }

            //if credit does not allow WireTransfers return error
            if (PossibleFundingDate.FundsTransferType.FundingByWire() && !_fundingBankingInstitution.WireRoutingNumberIsSupportedByUSBank)
            {
                return ValidateBankingInfoResult.Fail("The 9 digit routing number entered is invalid, please correct the information below and resubmit.  Thank You.", "FundingAccount.RoutingNumber");
            }
            //if MICR number is not a valid ACH routing number return error back to customer
            else if (PossibleFundingDate.FundsTransferType.FundingByACH() && !_fundingBankingInstitution.ACHRoutingNumberIsSupportedByFedFile)
            {
                return ValidateBankingInfoResult.Fail("The 9 digit routing number entered is invalid, please correct the information below and resubmit.  Thank You.", "FundingAccount.RoutingNumber");
            }

            return ValidateBankingInfoResult.Ok(_applicationRow.FlagIsSet(FlagLookup.Flag.BypassCreditCard));
        }

        private bool IsInvalidAutoPayPaymentInformation()
        {
            return (_loanOfferDataSet.LatestApprovedLoanTerms.PaymentType == PaymentTypeLookup.PaymentType.AutoPay 
                    &&
                    (PaymentAccount.RoutingNumber == null 
                     || PaymentAccount.AccountNumber == null 
                     || PaymentAccount.ConfirmAccountNumber == null
                     || PaymentAccount.BankAccountType == BankAccountTypeLookup.BankAccountType.NotSelected));
        }

        #region inner classes for all kinds of data binding goodness
        public class SubmitLoanContractResult : ValidateBankingInfoResult
        {
            public string Redirect { get; set; }
            public bool SignOut { get; set; }

            public static SubmitLoanContractResult Fail(string tab, string errorMessage, string errorValue = null)
            {
                return new SubmitLoanContractResult()
                {
                    Redirect = tab,
                    ErrorMessage = errorMessage,
                    ErrorValue = errorValue,
                    Success = false
                };
            }

            public new static SubmitLoanContractResult Ok(bool skipCreditCardValidation = false)
            {
                return new SubmitLoanContractResult()
                {
                    Success = true
                };
            }

            internal static SubmitLoanContractResult Fail(ValidateBankingInfoResult bankInfoResult, string redirect)
            {
                return new SubmitLoanContractResult()
                {
                    Success = false,
                    Redirect = redirect,
                    ErrorMessage = bankInfoResult.ErrorMessage,
                    ErrorValue = bankInfoResult.ErrorValue
                };
            }
        }

        public class ValidateBankingInfoResult : GenericJsonSuccessModel
        {
            public bool SkipCreditCardVerification { get; set; }

            public new static ValidateBankingInfoResult Fail(string errorMessage, string errorValue = null)
            {
                return new ValidateBankingInfoResult()
                {
                    ErrorValue = errorValue,
                    ErrorMessage = errorMessage,
                    Success = false
                };
            }

            public static ValidateBankingInfoResult Ok(bool skipCreditCardValidation = false)
            {
                return new ValidateBankingInfoResult()
                {
                    Success = true,
                    SkipCreditCardVerification = skipCreditCardValidation
                };
            }
        }

        [Serializable]
        public class BankAccountInfo
        {
            public BankAccountHolderNameTypeLookup.BankAccountHolderNameType? AuthorizedSigner { get; set; }
            public string RoutingNumber { get; set; }
            public bool IsRoutingNumberInvalid { get; set; }
            public string AccountNumber { get; set; }

            [JsonConverter(typeof(StringEnumConverter))]
            public string ConfirmAccountNumber { get; set; }

            public bool? IsCheckingAccount { get; set; }

            [JsonIgnore]
            public BankAccountTypeLookup.BankAccountType BankAccountType
            {
                get
                {
                    if (!IsCheckingAccount.HasValue)
                    {
                        return BankAccountTypeLookup.BankAccountType.NotSelected;
                    }

                    return (IsCheckingAccount.Value) ? BankAccountTypeLookup.BankAccountType.CheckingAccount : BankAccountTypeLookup.BankAccountType.SavingsAccount;
                }
            }
        }

        [Serializable]
        public class BrokerageAccountInfo
        {
            public string BeneficiaryBankName { get; set; }
            public string BeneficiaryRoutingNumber { get; set; }
            public string BeneficiaryAccountNumber { get; set; }
            public string BeneficiaryConfirmAccountNumber { get; set; }

            public string IntermediaryBankName { get; set; }
            public string IntermediaryRoutingNumber { get; set; }
            public string IntermediaryAccountNumber { get; set; }
        }

        [Serializable]
        public class AccountVerificationModel
        {
            public BankAccountHolderNameTypeLookup.BankAccountHolderNameType CardholderName { get; set; }

            public AccountVerificationModelAddress Address { get; set; }
            public string CreditCardNumber { get; set; }
            public int? ExpirationMonth { get; set; }
            public int? ExpirationYear { get; set; }

            public AccountVerificationModelAddress PrimaryAddress { get; set; }
            public AccountVerificationModelAddress SecondaryAddress { get; set; }

            public class AccountVerificationModelAddress
            {
                public string CardholderName { get; set; }
                public string BillingAddress { get; set; }
                public PostalAddressSecondaryUnitTypeLookup.PostalAddressSecondaryUnitType SecondaryUnitType { get; set; }
                public string SecondaryUnitValue { get; set; }
                public string City { get; set; }
                public StateLookup.State State { get; set; }
                public string ZipCode { get; set; }
            }

            public void Populate(FirstAgain.Domain.SharedTypes.Customer.CustomerUserIdDataSet.ApplicantRow primary,
                                          FirstAgain.Domain.SharedTypes.Customer.CustomerUserIdDataSet.ApplicantRow secondary = null)
            {
                Guard.AgainstNull<FirstAgain.Domain.SharedTypes.Customer.CustomerUserIdDataSet.ApplicantRow>(primary, "primary");

                var applicantAddressRow = primary.PostalAddresses.Single(a => a.PostalAddressType == PostalAddressTypeLookup.PostalAddressType.PrimaryResidence);
                PrimaryAddress = PopulateAddress(primary.PostalAddresses.Single(a => a.PostalAddressType == PostalAddressTypeLookup.PostalAddressType.PrimaryResidence));
                PrimaryAddress.CardholderName = primary.Name;
                SecondaryAddress = new AccountVerificationModelAddress();
                if (secondary != null)
                {
                    SecondaryAddress.CardholderName = secondary.Name;
                    SecondaryAddress = PopulateAddress(secondary.PostalAddresses.Single(a => a.PostalAddressType == PostalAddressTypeLookup.PostalAddressType.PrimaryResidence));
                }
            }

            private AccountVerificationModelAddress PopulateAddress(FirstAgain.Domain.SharedTypes.Customer.CustomerUserIdDataSet.ApplicantPostalAddressRow addressRow)
            {
                return new AccountVerificationModelAddress()
                {
                    BillingAddress = addressRow.AddressLine1,
                    SecondaryUnitType = addressRow.SecondaryUnitType,
                    SecondaryUnitValue = (addressRow.IsSecondaryUnitValueNull()) ? string.Empty : addressRow.SecondaryUnitValue,
                    City = addressRow.City,
                    State = addressRow.State,
                    ZipCode = addressRow.ZipCode
                };
            }
        }

        #endregion


    }


}