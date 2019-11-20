using FirstAgain.Domain.ServiceModel.Contracts.MessageContracts.CustomerOperations;
using FirstAgain.LoanServicing.SharedTypes;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Common.Extensions;
using FirstAgain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LightStreamWeb.App_State;
using Ninject;
using FirstAgain.LoanServicing.ServiceModel.Client;
using TransactionHistoryItem = LightStreamWeb.Models.AccountServices.FundedAccountModel.TransactionHistoryItem;
using ShawLookups = FirstAgain.Domain.Lookups.ShawData;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.Common;
using System.Xml;
using FirstAgain.Common.Xml;
using System.Collections;
using FirstAgain.Common.Logging;
using FirstAgain.Correspondence.ServiceModel.Client;
using System.Configuration;
using FirstAgain.Domain.ServiceModel.Client;
using AutoMapper;
using FirstAgain.LoanServicing.SharedTypes.Entities.AmortizationSchedule;

namespace LightStreamWeb.Models.AccountServices
{
    public class AccountServiceModelData
    {
        [Inject]
        public AccountServiceModelData(ICurrentUser webUser)
        {
            _webUser = webUser;
        }

        private GetAccountInfoResponse _accountInfo = null;
        private AccountServicesDataSet _accountServicesData = null;
        private ICurrentUser _webUser = null;

        public int ActiveApplicationId { get; set; }
        public List<FundedAccountModel> Applications { get; set; }
        public Hashtable PayoffDatesViaInvoice { get; set; }
        public Hashtable PayoffDatesViaAutoPay { get; set; }

        public void Populate(GetAccountInfoResponse accountInfo, AccountServicesDataSet accountServicesData, BusinessCalendarDataSet businessCalendar)
        {
            _accountInfo = accountInfo;
            _accountServicesData = accountServicesData;

            if (!_webUser.ApplicationId.HasValue)
            {
                _webUser.ApplicationId = _accountInfo.ApplicationsDates.MostRecentFundedOrViewableApplicationId;
            }

            ActiveApplicationId = _webUser.ApplicationId.Value;

            Applications = new List<FundedAccountModel>();

            if (_accountInfo.ApplicationsDates.ViewableFundedAccounts == null || _accountInfo.ApplicationsDates.ViewableFundedAccounts.Count == 0)
            {
                LightStreamLogger.WriteWarning(string.Format("No ViewableFundedAccounts for {0}", _accountInfo.CustomerUserIdDataSet.CustomerUserId));
                return;
            }

            foreach (var loanMasterRow in _accountServicesData.LoanMaster.SortedByRecentAndActive.Where(a => _accountInfo.ApplicationsDates.ViewableFundedAccounts.Contains(a.ApplicationId)))
            {
                var app = _accountInfo.CustomerUserIdDataSet.AccountInfo.Applications.Single(a => a.ApplicationId == loanMasterRow.ApplicationId);
                var appDetail = app.GetApplicationDetailRows().First();
                var retailLoanMasterRow = _accountServicesData.GetRetailLoanMasterByApplicationId(loanMasterRow.ApplicationId);
                var applicationCollateral = app.GetApplicationCollateralRows().FirstOrDefault();
                var approvedLoanTerms = _accountInfo.CustomerUserIdDataSet.LoanTermsRequest.GetCurrentApprovedLoanTerms(loanMasterRow.ApplicationId);
                var verificationRequests = new ApplicationStatus.VerificationRequestsModel();
                verificationRequests.Populate(_accountInfo.CustomerUserIdDataSet, loanMasterRow.ApplicationId, approvedLoanTerms.PurposeOfLoan);

                Applications.Add(new FundedAccountModel()
                {
                    ApplicationId = loanMasterRow.ApplicationId,
                    Nickname = (appDetail.IsApplicationNickNameNull()) ? string.Empty : appDetail.ApplicationNickName,
                    InformationReqestRequired = (app.GetLoanContractRows().Any(lc => lc.LoanOfferRow.LoanTermsRequestRow.PurposeOfLoan.IsSecuredAuto()) && app.VINEntryRequired) ||
                                                (verificationRequests.Documents.Any() && !verificationRequests.AllDocumentsAreInASatisfiedStatus),
                    IsSecuredAuto = app.GetLoanContractRows().Any(lc => lc.LoanOfferRow.LoanTermsRequestRow.PurposeOfLoan.IsSecuredAuto()),
                    OriginalLoanAmount = loanMasterRow.OriginalLoanAmount,
                    FloridaDocStampTax = approvedLoanTerms.Fees,
                    OriginalInterestRate = loanMasterRow.OriginalInterestRate * 100.0m,
                    OriginalTermMonths = loanMasterRow.OriginalTermMonths,
                    FundingDate = loanMasterRow.FundingDate.ToShortDateString(),
                    LoanBalanceCurrent = retailLoanMasterRow.LoanBalanceCurrent,
                    MonthlyPayment = loanMasterRow.CurrentMonthlyPaymentAmt,
                    PaymentDayOfMonth = app.GetLoanContractRows().First().MonthlyPaymentDate,
                    CurrentInterestRate = retailLoanMasterRow.CurrentInterestRate * 100.0m,
                    NextPaymentDueDate = loanMasterRow.IsUINextPaymentDateNull() ? null : loanMasterRow.UINextPaymentDate.ToShortDateString(),
                    MonthlyPaymentChangeEffectiveDate = GetMonthlyPaymentChangeEffectiveDate(retailLoanMasterRow, businessCalendar),
                    NextPaymentAmount = loanMasterRow.IsUINextPaymentAmountNull() ? (decimal?)null : loanMasterRow.UINextPaymentAmount,
                    IsClosed = loanMasterRow.AccountStatus == FirstAgain.Domain.Lookups.ShawData.AccountStatusLookup.AccountStatus.Closed,
                    EmailPreferences = GetEmailPreferences(loanMasterRow.ApplicationId),
                    Active = loanMasterRow.ApplicationId == ActiveApplicationId,
                    PaymentHistory = GetPaymentHistory(loanMasterRow.ApplicationId),
                    EligibleToViewAmortizationSchedule = GetIsEligibleToViewAmortizationSchedule(appDetail, loanMasterRow,
                                                                _accountServicesData.v_RetailLoanMaster.First(vlm => vlm.ApplicationId == loanMasterRow.ApplicationId).CycleEndDate),
                    CanViewMonthlyPayment = IsBillingCycleActive(_accountServicesData.v_RetailLoanMaster.First(vlm => vlm.ApplicationId == loanMasterRow.ApplicationId).CycleEndDate),
                    AccountStatus = loanMasterRow.AccountStatus,
                    PaymentType = loanMasterRow.PaymentType,
                    AuthorizedSignerList  = Helpers.ApplicationStatusHelper.PopulateAuthorizedSignerList(app),
                    IsJoint = app.ApplicationType  == ApplicationTypeLookup.ApplicationType.Joint,
                    AuthorizedSigner = (app.ApplicationType != ApplicationTypeLookup.ApplicationType.Joint) ? BankAccountHolderNameTypeLookup.BankAccountHolderNameType.PrimaryBorrowerName.ToString() : "",
                    MinimumMonthlyPaymentAmount = loanMasterRow.OriginalMonthlyPaymentAmt,
                    MaximumMonthlyPaymentAmount = retailLoanMasterRow.LoanBalanceCurrent, // default to current balance, will update to payoff quote on secondary pass
                    EInvoicePayDaysEnabled = app.GetEmailPreference(SolicitationPreferenceLookup.SolicitationPreference.EInvoicePayDays),
                    VehicleYear = (applicationCollateral == null || applicationCollateral.IsYearNull()) ? string.Empty : applicationCollateral.Year.ToString(),
                    VehicleMake = (applicationCollateral == null || applicationCollateral.IsMakeNull()) ? string.Empty : applicationCollateral.Make,
                    VehicleModel = (applicationCollateral == null || applicationCollateral.IsModelNull()) ? string.Empty : applicationCollateral.Model,
                    VehicleDescription = (applicationCollateral == null || applicationCollateral.IsDescriptionNull()) ? string.Empty : applicationCollateral.Description,
                    CompanyLegalName = GetCompanyLegalName(app)
                });
            }

            // populate secondary data - that may require extra calls to back end
            foreach (var fundedAccount in Applications)
            {
                // populate scheduled payments before reamortization eligibility is determined
                PopulateSecondaryAccountInformation(accountInfo, accountServicesData, businessCalendar, fundedAccount);
                                
                if (fundedAccount.EligibleToViewAmortizationSchedule)
                {
                    PopulateReamortizationData(fundedAccount, businessCalendar);
                }
            }

            // populate payoff dates 
            PayoffDatesViaInvoice = new Hashtable();
            var fundingDates = LoanServicingOperations.GetPayoffDates(ShawLookups.PaymentTypeLookup.PaymentType.Invoice, businessCalendar, BusinessConstants.Instance.OneTimeACHPaymentCutOffTime);
            foreach (var d in fundingDates)
            {
                PayoffDatesViaInvoice[d.Date.ToString("yyyy-M-d")] = true;
            }
            PayoffDatesViaAutoPay = new Hashtable();
            fundingDates = LoanServicingOperations.GetPayoffDates(ShawLookups.PaymentTypeLookup.PaymentType.AutoPay, businessCalendar, BusinessConstants.Instance.OneTimeACHPaymentCutOffTime);
            foreach (var d in fundingDates)
            {
                PayoffDatesViaAutoPay[d.Date.ToString("yyyy-M-d")] = true;
            }

            // Reamortization. Enabled by default. Set to "false" to disable new reamortization requests
            if (ConfigurationManager.AppSettings["EnableReamortization"] != null && ConfigurationManager.AppSettings["EnableReamortization"] == "false")
            {
                foreach (var account in Applications)
                {
                    if (account.MinimumMonthlyPaymentAmount < account.ContractualMonthlyPaymentAmount)
                    {
                        var lmr= _accountServicesData.LoanMaster.First(a => a.ApplicationId == account.ApplicationId);
                        account.MinimumMonthlyPaymentAmount = lmr.OriginalMonthlyPaymentAmt;
                    }
                    account.MayBeEligibleForReamortization = false;
                    account.ReamortizationDate = null;
                }
            }
        }

        private void PopulateSecondaryAccountInformation(GetAccountInfoResponse accountInfo, AccountServicesDataSet accountServicesData, BusinessCalendarDataSet businessCalendar, FundedAccountModel fundedAccount)
        {
            var batchParms = LoanServicingOperations.GetBatchConfig();

            var debitAccount = _accountServicesData.GetDebitAccountByApplicationId(fundedAccount.ApplicationId);
            if (debitAccount != null)
            {
                fundedAccount.DebitAccountNumber = debitAccount.BankAccountNumber.Trim();
                fundedAccount.DebitRoutingNumber = (debitAccount.IsACHTransactionRoutingNumberNull()) ? string.Empty : debitAccount.ACHTransactionRoutingNumber.Trim();

                // for a totally irrational reason ("that's how it used to work!"), we don't want to default the account type for Invoice accounts, but we do for AutoPay accounts.
                if (fundedAccount.PaymentType == ShawLookups.PaymentTypeLookup.PaymentType.AutoPay)
                {
                    fundedAccount.IsCheckingAccount = debitAccount.BankAccountType == ShawLookups.BankAccountTypeLookup.BankAccountType.CheckingAccount;
                }
            }

            if (fundedAccount.PaymentType == FirstAgain.Domain.Lookups.ShawData.PaymentTypeLookup.PaymentType.AutoPay)
            {
                var servicingExceptions = _accountServicesData.GetServicingExceptions(fundedAccount.ApplicationId);
                if (servicingExceptions.Length > 0 &&
                    servicingExceptions[0].ServicingExceptionType == ShawLookups.ServicingExceptionTypeLookup.ServicingExceptionType.AutoDebitPaymentException &&
                    servicingExceptions[0].ServicingExceptionStatus == ShawLookups.ServicingExceptionStatusLookup.ServicingExceptionStatus.Pending)
                {
                    fundedAccount.PaymentAccountMessage = "We recently experienced a failure in our attempt to debit your designated account for your scheduled monthly payment. <br /><br />An email was sent to you with more details. In part, it asked that you work with your financial institution to resolve the situation. If you have already done so or, once you do resolve the situation, please enter your account information below.";
                    fundedAccount.HasPendingAutoDebitPaymentException = true;
                }
                else
                {
                    fundedAccount.PaymentAccountMessage =
                        string.Format("Changes submitted by the end of business on {0}, will be in effect on {1}. Changes made tomorrow will be in effect on {2}.<br /><br />Please provide the following information for your new account. See example below.",
                        DateTime.Today.ToLongDateString(),
                        businessCalendar.GetBankingDateNBankingDaysFromDate(DateTime.Today, BusinessConstants.Instance.OnlinePmtInfoChgLeadDays).ToLongDateString(),
                        businessCalendar.GetBankingDateNBankingDaysFromDate(DateTime.Today, BusinessConstants.Instance.OnlinePmtInfoChgLeadDays + 1).ToLongDateString());
                }
            }
            else
            {
                // default to "New Account" or 'on file' for payoffs, but only for invoice accounts
                if (fundedAccount.HasPaymentAccount)
                {
                    fundedAccount.PaymentAccountSelection = LightStreamWeb.Models.AccountServices.FundedAccountModel.AccountSelection.PaymentAccount;
                }
                else
                {
                    fundedAccount.PaymentAccountSelection = LightStreamWeb.Models.AccountServices.FundedAccountModel.AccountSelection.NewAccount;
                }

            }

            if (fundedAccount.AccountStatus != ShawLookups.AccountStatusLookup.AccountStatus.Closed)
            {
                if (fundedAccount.LoanBalanceCurrent > 0)
                {
                    fundedAccount.CurrentPayoffAmount = LoanServicingOperations.GetPayoffQuote(fundedAccount.ApplicationId, DateTime.Today);

                    // Reamortization - set the max monthly payment amount to the payoff quote for the next payment date
                    if (fundedAccount.PaymentType == ShawLookups.PaymentTypeLookup.PaymentType.AutoPay)
                    {
                        var loanMasterRow = _accountServicesData.LoanMaster.FirstOrDefault(a => a.ApplicationId == fundedAccount.ApplicationId);
                        if (loanMasterRow != null && !loanMasterRow.IsUINextPaymentDateNull())
                        {
                            fundedAccount.MaximumMonthlyPaymentAmount = LoanServicingOperations.GetPayoffQuote(fundedAccount.ApplicationId, loanMasterRow.UINextPaymentDate);
                        }
                    }
                }
                else
                {
                    fundedAccount.CurrentInterestRate = 0.0m;
                    fundedAccount.NextPaymentDueDate = "N/A";
                    fundedAccount.NextPaymentAmount = 0.0m;
                }

                // Any extra payments?
                var trr = _accountServicesData.GetScheduledPayoffTransactionByApplicationId(fundedAccount.ApplicationId, batchParms.RunDate);
                if (null != trr) // if an ACH payoff is scheduled
                {
                    ParseScheduledPayoffTransaction(businessCalendar, fundedAccount, trr);
                }
                else
                {
                    trr = _accountServicesData.GetExtraACHPaymentTransactionByApplicationId(fundedAccount.ApplicationId, batchParms.RunDate);
                    if (null != trr) // if an extra ACH payment is scheduled
                    {
                        ParseScheduledPaymentTransaction(businessCalendar, fundedAccount, trr);
                    }
                    else
                    {
                        trr = _accountServicesData.OneTimeACHPaymentTransaction(fundedAccount.ApplicationId, batchParms.RunDate);
                        if (null != trr) // if a one time payment is scheduled
                        {
                            ParseScheduledPaymentTransaction(businessCalendar, fundedAccount, trr);
                        }
                    }
                }

                // any payment exceptions?
                if (fundedAccount.PaymentType == ShawLookups.PaymentTypeLookup.PaymentType.AutoPay)
                {
                    CheckForACHExceptions(accountServicesData, fundedAccount);
                }
            }

            // eDocs
            fundedAccount.Documents = new LightStreamWeb.Models.ENotices.ENoticesModalModel(fundedAccount.ApplicationId, accountInfo.CustomerUserIdDataSet, null).Docs.ToList();
            PopulateLoanAgreementRow(fundedAccount);
            PopulateInvoices(fundedAccount);
        }

        protected void PopulateReamortizationData(FundedAccountModel fundedAccount, BusinessCalendarDataSet businessCalendar)
        {
            try
            {
                var response = LoanServicingOperations.GetAmortizationPaymentSchedule(fundedAccount.ApplicationId, null, null, null);
                if (response != null && response.ScheduleLineItems != null && response.ScheduleLineItems.Any())
                {
                    // any pending requests?
                    var reamortizationRequests = _accountServicesData.AccountReamortizationRequestQueue.Where(a => a.ApplicationId == fundedAccount.ApplicationId);
                    if (reamortizationRequests.Any())
                    {
                        fundedAccount.QueuedMonthlyPaymentAmount = reamortizationRequests.First().RequestedMinimumPaymentAmount;
                        fundedAccount.QueuedMonthlyPaymentDate = reamortizationRequests.First().NextEffectivePaymentDueDate;
                    }
                    // map relevant data back to the model for use by the client
                    var model = MapLoanServicingAmortizationScheduleToModel(fundedAccount.ApplicationId, response);
                    fundedAccount.AmortizationSchedule = model.Items;
                    fundedAccount.MinimumMonthlyPaymentAmount = model.MinimumMonthlyPaymentAmount;
                    fundedAccount.ContractualMonthlyPaymentAmount = model.ContractualMonthlyPaymentAmount;
                    fundedAccount.DecreasedMonthlyPaymentEffectiveDate = model.DecreasedMonthlyPaymentEffectiveDate;

                    var result = CheckReamortizationAvailability(fundedAccount);
                    Mapper.Map(result, fundedAccount);
                }
                else
                {
                    // nothing to see here.
                    fundedAccount.EligibleToViewAmortizationSchedule = false;
                }
            }
            catch (Exception ex) 
            {
                // The GetAmortizationPaymentSchedule method has historically been the source of many bugs.
                // Trap any errors so that when it fails, the user is not prevented from viewing the rets of their account information.
                // Thus, the error is logged but not re-thrown, and all amortization features are hidden.
                LightStreamLogger.WriteError(ex);
                fundedAccount.EligibleToViewAmortizationSchedule = false;
            }
        }

        protected static PredictReamortizationAvailabilityResponse CheckReamortizationAvailability(FundedAccountModel fundedAccount)
        {
            var result = LoanServicingOperations.PredictReamortizationAvailability(new FirstAgain.LoanServicing.SharedTypes.Entities.AmortizationSchedule.PredictReamortizationAvailabilityRequest()
                {
                    ApplicationId = fundedAccount.ApplicationId,
                    MinimumMonthlyPaymentAmount = fundedAccount.MinimumMonthlyPaymentAmount,
                    MonthlyPayment = fundedAccount.MonthlyPayment,
                    ScheduledPaymentAmount = fundedAccount.ScheduledPaymentAmount,
                    ScheduledPaymentDate = fundedAccount.ScheduledPaymentDate
                });

            if (result.ProjectedEffectiveDate.HasValue)
            {
                fundedAccount.DecreasedMonthlyPaymentEffectiveDate = result.ProjectedEffectiveDate;
            }
            return Mapper.Map<PredictReamortizationAvailabilityResponse>(result);
        }

        private string GetCompanyLegalName(FirstAgain.Domain.SharedTypes.Customer.CustomerUserIdDataSet.ApplicationRow applicationRow)
        {
            var  addressRow = _accountInfo.CustomerUserIdDataSet.ApplicantPostalAddress
                                                                            .Where(a => a.ApplicantRow.ApplicantType == ApplicantTypeLookup.ApplicantType.Primary
                                                                                   && a.ApplicantRow.ApplicationId == applicationRow.ApplicationId
                                                                                   && a.PostalAddressType == PostalAddressTypeLookup.PostalAddressType.PrimaryResidence).First();
            // Makes call to web sevice, so caller should cache this data!
            var companyNames = CorrespondenceServiceCorrespondenceOperations.GetCompanyLegalNames(addressRow.State);
            return companyNames.CompanyLegalName;

        }
        private DateTime GetMonthlyPaymentChangeEffectiveDate(FirstAgain.LoanServicing.SharedTypes.AccountServicesDataSet.v_RetailLoanMasterRow retailLoanMasterRow, BusinessCalendarDataSet businessCalendar)
        {
            var nextPaymentDate = businessCalendar.GetPaymentDueDate(retailLoanMasterRow.PaymentDueDate);

            if (nextPaymentDate < businessCalendar.GetBankingDateNBankingDaysFromDate(DateTime.Now, BusinessConstants.Instance.OnlinePmtInfoChgLeadDays))
            {
                nextPaymentDate = businessCalendar.GetPaymentDueDate(nextPaymentDate.AddMonths(1));
            }

            return nextPaymentDate;
        }

        private void ParseScheduledPayoffTransaction(BusinessCalendarDataSet businessCalendar, FundedAccountModel fundedAccount, AccountServicesDataSet.TransactionRepositoryRow trr)
        {
            var txnDetail = trr.TransactionDetails;
            var txnDoc = new XmlDocument();
            txnDoc.XmlResolver = null;
            txnDoc.LoadXml(txnDetail);

            var paymentAmountNode = txnDoc.SelectSingleNode(@"/*[local-name()='PaymentInfo' and namespace-uri()='urn:firstagain-com:domain']/@PaymentAmount");
            if (paymentAmountNode != null)
            {
                fundedAccount.ScheduledPaymentAmount = Convert.ToDecimal(paymentAmountNode.InnerText);
            }
            var paymentDateNode = txnDoc.SelectSingleNode(@"/*[local-name()='PaymentInfo' and namespace-uri()='urn:firstagain-com:domain']/@PaymentDate");
            if (paymentDateNode != null)
            {
                fundedAccount.ScheduledPaymentDate = Convert.ToDateTime(paymentDateNode.InnerText);
            }
            var paymentCancelRescheduleDate = businessCalendar.GetPayoffCancelCutoffDateTime(fundedAccount.ScheduledPaymentDate.GetValueOrDefault(), BusinessConstants.Instance.OneTimeACHPaymentCutOffTime);
            fundedAccount.LastDayToCancelScheduledPayment = paymentCancelRescheduleDate;

            if (paymentCancelRescheduleDate > DateTime.Now)
            {
                fundedAccount.IsOkToCancelScheduledPayment = true;
            }

            var loanMasterRow = _accountServicesData.GetLoanMasterByApplicationId(fundedAccount.ApplicationId);
            if (!loanMasterRow.IsUINextPaymentDateNull())
            {
                if ((fundedAccount.ScheduledPaymentDate > loanMasterRow.UINextPaymentDate) && (DateTime.Today.Date <= loanMasterRow.UINextPaymentDate) && (loanMasterRow.PaymentType == ShawLookups.PaymentTypeLookup.PaymentType.AutoPay))
                {
                    var achDebitPrepSettlementDate = LoanServicingOperations.GetACHDebitPrepSettlementDate();

                    fundedAccount.PayOffIncludedACHPayment = (achDebitPrepSettlementDate != null && loanMasterRow.UINextPaymentDate <= achDebitPrepSettlementDate)
                                                            ? loanMasterRow.UINextPaymentAmount
                                                            : loanMasterRow.CurrentMonthlyPaymentAmt;                    

                    fundedAccount.PayOffIncludedACHPaymentDate = loanMasterRow.UINextPaymentDate.ToLongDateString();
                }
            }

            fundedAccount.ExtraPaymentCurrentStep = FundedAccountModel.PaymentPayoffsSteps.HasScheduledPayoff;
        }

        private void PopulateInvoices(FundedAccountModel fundedAccount)
        {
            // get monthly and past-due invoices created within the last month
            CustomerUserIdDataSet.DocumentStoreRow monthlyInvoiceHtml = GetLatestMonthlyInvoiceHtml(fundedAccount);
            CustomerUserIdDataSet.DocumentStoreRow monthlyInvoicePdf = GetLatestMonthlyInvoicePdf(fundedAccount);

            //if there are any invoices to display, set up the invoice section
            if (monthlyInvoiceHtml != null && fundedAccount.PaymentType == ShawLookups.PaymentTypeLookup.PaymentType.Invoice)
            {
                fundedAccount.LatestInvoice = new ENotices.ENoticesModalModel.ENoticeRow
                {
                    ENoticeTitle = "Current payment invoice",
                    EDocId = monthlyInvoiceHtml.EdocId,
                    PdfVersionEDocId = (monthlyInvoicePdf != null) ? monthlyInvoicePdf.EdocId : (long?)null
                };
            }
        }

        private CustomerUserIdDataSet.DocumentStoreRow GetLatestMonthlyInvoicePdf(FundedAccountModel fundedAccount)
        {
            DateTime since = DateTime.Now.AddMonths(-1);

            return _accountInfo.CustomerUserIdDataSet.DocumentStore
                   .Where(a => a.ApplicationId == fundedAccount.ApplicationId &&
                        a.CreatedDate > since &&
                       (a.CorrespondenceCategory == CorrespondenceCategoryLookup.CorrespondenceCategory.InvoiceMonthlyPdf
                       || a.CorrespondenceCategory == CorrespondenceCategoryLookup.CorrespondenceCategory.InvoicePastDuePdf))
                   .OrderByDescending(a => a.CreatedDate).FirstOrDefault();
        }

        private CustomerUserIdDataSet.DocumentStoreRow GetLatestMonthlyInvoiceHtml(FundedAccountModel fundedAccount)
        {
            DateTime since = DateTime.Now.AddMonths(-1);

            return _accountInfo.CustomerUserIdDataSet.DocumentStore
                   .Where(a => a.ApplicationId == fundedAccount.ApplicationId &&
                        a.CreatedDate > since &&
                       (a.CorrespondenceCategory == CorrespondenceCategoryLookup.CorrespondenceCategory.InvoiceMonthly
                       || a.CorrespondenceCategory == CorrespondenceCategoryLookup.CorrespondenceCategory.InvoicePastDue))
                   .OrderByDescending(a => a.CreatedDate).FirstOrDefault();
        }

        private void PopulateLoanAgreementRow(FundedAccountModel fundedAccount)
        {
            CustomerUserIdDataSet.DocumentStoreRow paidInFullHtmlLoanAgreement = GetLatestDoc(fundedAccount, CorrespondenceCategoryLookup.CorrespondenceCategory.LoanAgreementPaidInFullHtml);
            CustomerUserIdDataSet.DocumentStoreRow paidInFullPdfLoanAgreement = GetLatestDoc(fundedAccount, CorrespondenceCategoryLookup.CorrespondenceCategory.LoanAgreementPaidInFull);
            CustomerUserIdDataSet.DocumentStoreRow htmlLoanAgreement = GetLatestDoc(fundedAccount, CorrespondenceCategoryLookup.CorrespondenceCategory.LoanAgreementHtml);
            CustomerUserIdDataSet.DocumentStoreRow pdfLoanAgreement = GetLatestDoc(fundedAccount, CorrespondenceCategoryLookup.CorrespondenceCategory.LoanAgreementPdf);

            if (paidInFullPdfLoanAgreement != null)
            {
                fundedAccount.LoanAgreement = new ENotices.ENoticesModalModel.ENoticeRow()
                {
                    Category = paidInFullPdfLoanAgreement.CorrespondenceCategory,
                    ENoticeTitle = "Paid In Full Loan Agreement",
                    PdfVersionEDocId = paidInFullPdfLoanAgreement.EdocId,
                    EDocId = (paidInFullHtmlLoanAgreement != null) ? paidInFullHtmlLoanAgreement.EdocId : 0
                };
            }
            else if (pdfLoanAgreement != null)
            {
                fundedAccount.LoanAgreement = new ENotices.ENoticesModalModel.ENoticeRow()
                {
                    Category = pdfLoanAgreement.CorrespondenceCategory,
                    ENoticeTitle = "Loan Agreement",
                    PdfVersionEDocId = pdfLoanAgreement.EdocId,
                    EDocId = (htmlLoanAgreement != null) ? htmlLoanAgreement.EdocId : 0
                };
            }

            CustomerUserIdDataSet.DocumentStoreRow htmlDisclosureStatement = GetLatestDoc(fundedAccount, CorrespondenceCategoryLookup.CorrespondenceCategory.DisclosureStatement);
            CustomerUserIdDataSet.DocumentStoreRow pdfDisclosureStatement = GetLatestDoc(fundedAccount, CorrespondenceCategoryLookup.CorrespondenceCategory.DisclosureStatementPdf);
            if (htmlDisclosureStatement != null)
            {
                fundedAccount.DisclosureStatement = new ENotices.ENoticesModalModel.ENoticeRow()
                {
                    Category = htmlDisclosureStatement.CorrespondenceCategory,
                    ENoticeTitle = "Final Disclosures",
                    PdfVersionEDocId = (pdfDisclosureStatement != null) ? pdfDisclosureStatement.EdocId : 0,
                    EDocId = (htmlDisclosureStatement != null) ? htmlDisclosureStatement.EdocId : 0
                };
            }
        }
        private CustomerUserIdDataSet.DocumentStoreRow GetLatestDoc(FundedAccountModel fundedAccount, CorrespondenceCategoryLookup.CorrespondenceCategory category)
        {
            return this._accountInfo.CustomerUserIdDataSet.DocumentStore
                   .Where(a => a.ApplicationId == fundedAccount.ApplicationId && a.IsViewable && a.CorrespondenceCategory == category)
                   .OrderByDescending(a => a.CreatedDate).FirstOrDefault();
        }


        private static void CheckForACHExceptions(AccountServicesDataSet accountServicesData, FundedAccountModel fundedAccount)
        {
            AccountServicesDataSet.ServicingExceptionRow[] servicingExceptionRows = accountServicesData.GetServicingExceptions(fundedAccount.ApplicationId);

            if (servicingExceptionRows.Length > 0 &&
                servicingExceptionRows[0].ServicingExceptionType == ShawLookups.ServicingExceptionTypeLookup.ServicingExceptionType.AutoDebitPaymentException &&
                servicingExceptionRows[0].ServicingExceptionStatus == ShawLookups.ServicingExceptionStatusLookup.ServicingExceptionStatus.Pending)
            {
                var returnCode = XmlUtility.DeserializeFromString<ACHExceptionInfo>(servicingExceptionRows[0].ExceptionDetails).ReturnCode;

                switch (ShawLookups.ACHReturnCodeTypeLookup.GetEnumeration(returnCode))
                {
                    case ShawLookups.ACHReturnCodeTypeLookup.ACHReturnCodeType.R01:
                        break; // allow extra payments and payoffs after reattempt date
                    case ShawLookups.ACHReturnCodeTypeLookup.ACHReturnCodeType.R02:
                        fundedAccount.ReturnCodeType = FundedAccountModel.ACHReturnCodeType.R020304;
                        fundedAccount.ReturnCodeText = "account is closed";
                        fundedAccount.ExtraPaymentCurrentStep = FundedAccountModel.PaymentPayoffsSteps.HasACHPaymentException;
                        break;
                    case ShawLookups.ACHReturnCodeTypeLookup.ACHReturnCodeType.R03:
                    case ShawLookups.ACHReturnCodeTypeLookup.ACHReturnCodeType.R04:
                        fundedAccount.ReturnCodeType = FundedAccountModel.ACHReturnCodeType.R020304;
                        fundedAccount.ReturnCodeText = "account number is invalid";
                        fundedAccount.ExtraPaymentCurrentStep = FundedAccountModel.PaymentPayoffsSteps.HasACHPaymentException;
                        break;
                    default:
                        fundedAccount.ReturnCodeType = FundedAccountModel.ACHReturnCodeType.R05Plus;
                        fundedAccount.ReturnCodeText = string.Format("{0} - {1}", returnCode, ShawLookups.ACHReturnCodeTypeLookup.GetDefinition(returnCode));
                        fundedAccount.ExtraPaymentCurrentStep = FundedAccountModel.PaymentPayoffsSteps.HasACHPaymentException;
                        break;
                }
            }
        }

        private static void ParseScheduledPaymentTransaction(BusinessCalendarDataSet businessCalendar, FundedAccountModel fundedAccount, AccountServicesDataSet.TransactionRepositoryRow trr)
        {
            String txnDetail = trr.TransactionDetails;
            XmlDocument txnDoc = new XmlDocument();
            txnDoc.XmlResolver = null;
            txnDoc.LoadXml(txnDetail);
            fundedAccount.ScheduledPaymentDate = Convert.ToDateTime(txnDoc.SelectSingleNode(@"/*[local-name()='PaymentInfo' and namespace-uri()='urn:firstagain-com:domain']/@PaymentDate").InnerText);
            fundedAccount.LastDayToCancelScheduledPayment = businessCalendar.GetBankingDateNBankingDaysFromDate(fundedAccount.ScheduledPaymentDate.Value, -1 * BusinessConstants.Instance.OnlinePmtInfoChgLeadDays);
            fundedAccount.ScheduledPaymentAmount = Convert.ToDecimal(txnDoc.SelectSingleNode(@"/*[local-name()='PaymentInfo' and namespace-uri()='urn:firstagain-com:domain']/@PaymentAmount").InnerText);
            fundedAccount.ExtraPaymentCurrentStep = FundedAccountModel.PaymentPayoffsSteps.HasScheduledPayment;

            var paymentCancelRescheduleDate = businessCalendar.GetBankingDateNBankingDaysFromDate(fundedAccount.ScheduledPaymentDate.Value, -1 * BusinessConstants.Instance.OnlinePmtInfoChgLeadDays);
            if (paymentCancelRescheduleDate > DateTime.Now)
            {
                fundedAccount.IsOkToCancelScheduledPayment = true;
            }

            // get re-amortization eligibility and date to display
            if (fundedAccount.PaymentType == ShawLookups.PaymentTypeLookup.PaymentType.AutoPay && fundedAccount.EligibleToViewAmortizationSchedule)
            {
                // g.	The new minimum monthly payment amount is $10.00 or less than the current contractual monthly payment. 
                var maxMonthlyPayment = fundedAccount.MonthlyPayment - 10;
                // h.	The new minimum monthly payment amount is => $100.  
                const decimal minMonthlyPayment = 100.0M;


                if (fundedAccount.MinimumMonthlyPaymentAmount < maxMonthlyPayment
                    && fundedAccount.MinimumMonthlyPaymentAmount >= minMonthlyPayment)
                {
                    fundedAccount.MayBeEligibleForReamortization = true;
                    fundedAccount.ReamortizationDate =businessCalendar.GetLargePaymentClearingDate(fundedAccount.ScheduledPaymentDate.Value);
                }
            }
        }

        public static AmortizationScheduleModel GetAmortizationSchedule(string ctx, CustomerUserIdDataSet customerData, decimal? paymentAmount, decimal? extraPaymentAmount = null, DateTime? extraPaymentEffectiveDate = null)
        {
            int? applicationId = FirstAgain.Common.Web.WebSecurityUtility.Descramble(ctx);
            if (applicationId.HasValue)
            {
                if (!customerData.Application.Any(a => a.ApplicationId == applicationId))
                {
                    throw new HttpException(403, "Access Denied");
                }

                return GetAmortizationSchedule(applicationId.Value, paymentAmount, extraPaymentAmount, extraPaymentEffectiveDate);
            }

            return null;
        }

        public static AmortizationScheduleModel GetAmortizationSchedule(int applicationId, decimal? paymentAmount, decimal? extraPaymentAmount = null, DateTime? extraPaymentEffectiveDate = null)
        {
            FirstAgain.LoanServicing.SharedTypes.Entities.AmortizationSchedule.AmortizationSchedule response = null;
            try
            {
                response = LoanServicingOperations.GetAmortizationPaymentSchedule(applicationId, paymentAmount, extraPaymentAmount, extraPaymentEffectiveDate);
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
            }
                 
            if (response == null || response.ScheduleLineItems == null || response.ScheduleLineItems.Count == 0)
            {
                return new AmortizationScheduleModel()
                {
                    ApplicationId = applicationId, 
                    Items = new List<TransactionHistoryItem>()
                };
            }

            return MapLoanServicingAmortizationScheduleToModel(applicationId, response);
        }

        private static AmortizationScheduleModel MapLoanServicingAmortizationScheduleToModel(int applicationId, FirstAgain.LoanServicing.SharedTypes.Entities.AmortizationSchedule.AmortizationSchedule response)
        {
            return new AmortizationScheduleModel()
            {
                ApplicationId = applicationId,
                Items = (from row in response.ScheduleLineItems
                         select new TransactionHistoryItem()
                         {
                             InterestAmount = (double)row.InterestAmount,
                             PrincipalAmount = (double)row.PrincipalAmount,
                             EndingPrincipalBalance = (double)row.EndingBalance,
                             StartingPrincipalBalance = (double)row.StartingPrincipalBalance,
                             TransactionAmount = (double)row.TotalAmount,
                             TransactionDate = row.EffectiveDate.ToShortDateString(),
                             HasBeenPosted = row.HasBeenPosted
                         }).ToList(),
                MinimumMonthlyPaymentAmount = response.DefaultMinimumMonthlyPaymentAmount,
                ContractualMonthlyPaymentAmount = response.CurrentMinimumMonthlyPaymentAmount,
                DecreasedMonthlyPaymentEffectiveDate = response.DefaultMinimumMonthlyPaymentEffectiveDate
            };
        }

        /*
         * PBI 290
        BUSR3
            An Amortization schedule can be viewed for current loan terms. 
        SYSR003.1	The Amortization Schedule is available only when all of the conditions below are met: 
        a.	AutoPay account
        b.	LightStream Portfolio
        c.	Funded status (excluding delinquent, bankrupt, charge-off, and closed accounts)  
         */
        private bool GetIsEligibleToViewAmortizationSchedule(FirstAgain.Domain.SharedTypes.Customer.CustomerUserIdDataSet.ApplicationDetailRow appDetail,
                                                             AccountServicesDataSet.LoanMasterRow loanMasterRow,
                                                             DateTime billingCycleEndDate)
        {
            if (!IsBillingCycleActive(billingCycleEndDate))
                return false;

            return loanMasterRow.PaymentType == ShawLookups.PaymentTypeLookup.PaymentType.AutoPay &&
                   appDetail.OriginationsPortfolio == OriginationsPortfolioLookup.OriginationsPortfolio.SunTrust &&
                   loanMasterRow.AccountStatus.IsNoneOf(FirstAgain.Domain.Lookups.ShawData.AccountStatusLookup.AccountStatus.Delinquent,
                                                        FirstAgain.Domain.Lookups.ShawData.AccountStatusLookup.AccountStatus.ChargedOff,
                                                        FirstAgain.Domain.Lookups.ShawData.AccountStatusLookup.AccountStatus.Closed);

        }



        private static bool IsBillingCycleActive(DateTime billingCycleEndDate)
        {
            return DateTime.Now < new DateTime(billingCycleEndDate.Year, billingCycleEndDate.Month, billingCycleEndDate.Day, 15, 0, 0);
        }

        // email preferences tab
        private List<LightStreamWeb.Models.AccountServices.FundedAccountModel.EmailPreference> GetEmailPreferences(int applicationId)
        {
            var which = SolicitationPreferenceLookup.FilterType.Invoice;

            var loanMaster = _accountServicesData.GetLoanMasterByApplicationId(applicationId);
            var application = _accountInfo.CustomerUserIdDataSet.Application.FindByApplicationId(applicationId);

            if (loanMaster.PaymentType == FirstAgain.Domain.Lookups.ShawData.PaymentTypeLookup.PaymentType.AutoPay)
            {
                which = SolicitationPreferenceLookup.FilterType.Autopay;
            }

            return (from x in SolicitationPreferenceLookup.GetFilteredList(which)
                    select new LightStreamWeb.Models.AccountServices.FundedAccountModel.EmailPreference()
                   {
                       Caption = x.Caption,
                       Enumeration = x.Enumeration,
                       IsSelected = application.GetEmailPreference(x.Enumeration)
                   }).ToList();
        }



        // History tab
        public List<LightStreamWeb.Models.AccountServices.FundedAccountModel.TransactionHistoryItem> GetPaymentHistory(int applicationId)
        {
            List<TransactionHistoryItem> results = new List<TransactionHistoryItem>();
            foreach (System.Data.DataRow row in _accountServicesData.GetWebTransactionHistoryByApplicationId(applicationId).Rows)
            {
                results.Add(new TransactionHistoryItem()
                {
                    TransactionDate = ((DateTime)row["TransactionDate"]).ToShortDateString(),
                    StartingPrincipalBalance = (double)row["StartingPrincipalBalance"],
                    TransactionAmount = (double)row["TransactionAmount"],
                    InterestAmount = (double)row["InterestAmount"],
                    PrincipalAmount = (double)row["PrincipalAmount"],
                    EndingPrincipalBalance = (double)row["EndingPrincipalBalance"]
                });
            }
            return results;
        }



        public static void CancelExtraPayment(AccountServicesDataSet accountServicesData, CustomerUserIdDataSet customerData, ICurrentUser webUser, FundedAccountModel fundedAccount)
        {
            Guard.AgainstNull<CustomerUserIdDataSet>(customerData, "customerData");
            Guard.AgainstNull<AccountServicesDataSet>(accountServicesData, "accountServicesData");
            Guard.AgainstNull<ICurrentUser>(webUser, "webUser");
            Guard.AgainstNull<FundedAccountModel>(fundedAccount, "fundedAccount");

            string note;
            if (accountServicesData.GetExtraACHPaymentTransactionByApplicationId(fundedAccount.ApplicationId) != null)
            {
                LoanServicingOperations.CancelExtraACHPayment(fundedAccount.ApplicationId, accountServicesData,
                        Helpers.EventAuditLogHelper.PopulateEventAuditLogDataSet(customerData, webUser, EventTypeLookup.EventType.CancelExtraACHPayment, "Canceled extra ACH payment.", fundedAccount.ApplicationId));
            }
            else
            {
                note = "Cancelled One-time ACH payment.";
                LoanServicingOperations.CancelOneTimeACHPayment(fundedAccount.ApplicationId, accountServicesData,
                    Helpers.EventAuditLogHelper.PopulateEventAuditLogDataSet(customerData, webUser, EventTypeLookup.EventType.OneTimeACHPaymentCancelled, note, fundedAccount.ApplicationId),
                    FirstAgain.Domain.Lookups.ShawData.TransactionSourceTypeLookup.TransactionSourceType.SOURCE_WEB,
                    EventInitiatorTypeLookup.EventInitiatorType.Web,
                    note);
            }
        }
    }
}