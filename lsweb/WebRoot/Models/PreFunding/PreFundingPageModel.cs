using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using FundsTransferType=FirstAgain.Domain.Lookups.FirstLook.FundsTransferTypeLookup.FundsTransferType;
using FirstAgain.Common;
using LightStreamWeb.Models.ApplicationStatus;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using BusinessCalendarClient = FirstAgain.Domain.ServiceModel.Client.BusinessCalendar;
using FirstAgain.Domain.ServiceModel.Client;
using System.Web.Mvc;
using Resources;
using LightStreamWeb.Helpers;
using Newtonsoft.Json;
using FirstAgain.Correspondence.SharedTypes;
using FirstAgain.Correspondence.ServiceModel.Client;
using FirstAgain.Common.Extensions;
using Ninject;
using LightStreamWeb.Models.Middleware;

namespace LightStreamWeb.Models.PreFunding
{
    public class PreFundingPageModel : ApplicationStatusPageModel
    {
        #region constructors

        public PreFundingPageModel(CustomerUserIdDataSet cuidds, LoanOfferDataSet loanOfferDataSet)
            : base(cuidds, loanOfferDataSet)
        {
            RateModalDisplay = RateModalDisplayType.ApplicationRates;
            BodyClass = "pre-funding counter";

            Populate();
        }

        public PreFundingPageModel(CustomerUserIdDataSet cuidds)
            : base(cuidds)
        {
            RateModalDisplay = RateModalDisplayType.ApplicationRates;
            BodyClass = "pre-funding counter";

            Populate();
        }
        #endregion

        #region public properies
        public long PdfLoanAgreementEdocId { get; private set; }
        public long HtmlLoanAgreementEdocId { get; private set; }
        public string ImportantMessage { get; private set; }
        public decimal FundingAmount { get; private set; }
        public string FundingDateText { get; private set; }
        public DateTime FundingDate { get; private set; }
        public string FundingAccountNumber { get; private set; }
        public string FundingRoutingNumber { get; private set; }
        public string PaymentAccountNumber { get; private set; }
        public string FirstPaymentDateText { get; private set; }
        public string LastChangeDateText { get; private set; }
        public DateTime LastChangeDate { get; private set; }
        public double SecondsUntilLastChangeDate
        {
            get
            {
                return Math.Floor((LastChangeDate - DateTime.Now).TotalSeconds);
            }
        }
        public bool IsPastLockOut { get { return SecondsUntilLastChangeDate <= 0; } }
        public int PaymentDayOfMonth { get; private set; }
        public System.Collections.Hashtable CalendarFundingDates { get; private set; }

        public string PaymentDayOfMonthText
        {
            get
            {
                switch (PaymentDayOfMonth)
                {
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                    case 10:
                    case 11:
                    case 12:
                    case 13:
                    case 14:
                    case 15:
                    case 16:
                    case 17:
                    case 18:
                    case 19:
                    case 20:
                    case 24:
                    case 25:
                    case 26:
                    case 27:
                    case 28:
                    case 29:
                    case 30:
                        return PaymentDayOfMonth.ToString() + "th";
                    case 1:
                    case 21:
                    case 31:
                        return PaymentDayOfMonth.ToString() + "st";
                    case 2:
                    case 22:
                        return PaymentDayOfMonth.ToString() + "nd";
                    case 3:
                    case 23:
                        return PaymentDayOfMonth.ToString() + "rd";
                    case 99:
                        return "last day";
                    default:
                        return PaymentDayOfMonth.ToString();
                }
            }
        }

        public override bool DisplayContactUs()
        {
            return true;
        }

        public override PurposeOfLoanLookup.PurposeOfLoan GetPurposeOfLoan()
        {
            if (_loanOfferDataSet != null && _loanOfferDataSet.LatestApprovedLoanTerms != null)
            {
                return _loanOfferDataSet.LatestApprovedLoanTerms.PurposeOfLoan;
            }
            return base.GetPurposeOfLoan();
        }

        public bool CanMakeChanges
        {
            get
            {
                return !IsPastLockOut;
            }
        }
        public bool IsFundingToday
        {
            get
            {
                if (_loanOfferDataSet != null && this._loanOfferDataSet.LoanContract[0].FundingDate.Date.Equals(DateTime.Now.Date))
                    return true;
                else
                    return false;
            }
        }

        public bool IsWire
        {
            get
            {
                if (_loanOfferDataSet != null)
                {
                    return _loanOfferDataSet.LoanContract[0].FundsTransferType == FundsTransferType.WireTransfer || _loanOfferDataSet.LoanContract[0].FundsTransferType == FundsTransferType.WireTransferSameDay;
                }
                else
                {
                    return false;
                }
            }
        }

        public StateLookup.State ApplicantState
        {
            get
            {
                var applicant = _customerUserIdDataSet.Application.FirstOrDefault(x => x.ApplicationId == ApplicationId);
                return applicant.PrimaryApplicant.GetApplicantPostalAddressRows().Single(x => x.PostalAddressType == PostalAddressTypeLookup.PostalAddressType.PrimaryResidence).State;
            }
        }

        public bool IsJoint
        {
            get
            {
                return Application != null && Application.IsJoint;
            }
        }

        public bool PurposeOfLoanIsSecured()
        {
            return GetPurposeOfLoan().IsSecured();
        }
        public bool IsAutoPay()
        {
            return GetPaymentMethod() == PaymentTypeLookup.PaymentType.AutoPay;
        }
        public PaymentTypeLookup.PaymentType GetPaymentMethod()
        {
            return _loanOfferDataSet.LatestApprovedLoanTerms.PaymentType;
        }

        public bool ShouldDisplayFundingAccountNumber()
        {
            return FundingAccountNumber.Length >= 6;
        }
        public bool ShouldDisplayPaymentAccountNumber()
        {
            return PaymentAccountNumber.IsNotNull() && PaymentAccountNumber.Length >= 6 && PaymentAccountNumber != FundingAccountNumber;
        }
        public string GetCalendarFundingDatesJson()
        {
            return JsonConvert.SerializeObject(CalendarFundingDates);
        }
        #endregion

        public virtual void Populate()
        {
            if (_loanOfferDataSet != null)
            {
                LoanOfferDataSet.LoanTermsRequestRow ltr = _loanOfferDataSet.LatestApprovedLoanTerms;
                FundingAmount = ltr.AmountMinusFees;
            }

            CalendarFundingDates = BusinessCalendarHelper.GetCalendarFundingDates(ApplicationId);
            CultureInfo cI = new CultureInfo("en-US", false);
            cI.DateTimeFormat.PMDesignator = "p.m.";
            cI.DateTimeFormat.AMDesignator = "a.m.";
            TimeSpan window;
            QueueItemStatusTypeLookup.QueueItemStatusType status;
            DateTime preFundingLockoutDateTime = BusinessCalendar.GetPrefundingLockoutDateTime(Application.ApplicationId, out window, out status);
            LastChangeDate = preFundingLockoutDateTime.AddMinutes(-30);           

            if (_loanOfferDataSet != null)
            {
                FundingDate = _loanOfferDataSet.LoanContract[0].FundingDate;
                if (IsFundingToday)
                {
                    FundingDateText = String.Format("{0}", _loanOfferDataSet.LoanContract[0].FundingDate.ToString("MMMM d, yyyy"));
                    LastChangeDateText = string.Format("{0}, today, {1:MMMM d, yyyy}", LastChangeDate.TimeOfDay.ToLightStreamTimeString(FirstAgain.Common.TimeZoneUS.EasternStandardTime), LastChangeDate);
                }
                else
                {
                    FundingDateText = String.Format("{0}", _loanOfferDataSet.LoanContract[0].FundingDate.ToString("dddd, MMMM d, yyyy"));
                    LastChangeDateText = LastChangeDate.ToLightStreamLongDateTimeString(FirstAgain.Common.TimeZoneUS.EasternStandardTime);
                }

                for (int i = 0; i < _loanOfferDataSet.BankAccountInfo.Rows.Count; i++)
                {
                    switch (_loanOfferDataSet.BankAccountInfo[i].BankAccountActionTypeId)
                    {
                        case (short)BankAccountActionTypeLookup.BankAccountActionType.Credit:
                            FundingAccountNumber = _loanOfferDataSet.BankAccountInfo[i].BankAccountNumber.Trim();
                            FundingRoutingNumber = _loanOfferDataSet.BankAccountInfo[i].IsACHTransactionRoutingNumberNull() ? null : _loanOfferDataSet.BankAccountInfo[i].ACHTransactionRoutingNumber.Trim();
                            break;
                        case (short)BankAccountActionTypeLookup.BankAccountActionType.Debit:
                            PaymentAccountNumber = _loanOfferDataSet.BankAccountInfo[i].BankAccountNumber.Trim();
                            break;
                    }
                }

                PaymentDayOfMonth = _loanOfferDataSet.LoanContract[0].MonthlyPaymentDate;
                FirstPaymentDateText = BusinessCalendar.GetClosestBankingDay(_loanOfferDataSet.LoanContract[0].InitialPaymentDate).ToString("dddd, MMMM d, yyyy");

                DocumentStoreDataSet documentStore = CorrespondenceServiceCorrespondenceOperations.GetApplicationDocumentStore(Application.ApplicationId);

                var html = documentStore.DocumentStore
                    .Where(a => a.ApplicationId == this.Application.ApplicationId && a.CorrespondenceCategory == CorrespondenceCategoryLookup.CorrespondenceCategory.LoanAgreementHtml)
                    .OrderByDescending(a => a.CreatedDate).FirstOrDefault();
                var pdf = documentStore.DocumentStore
                    .Where(a => a.ApplicationId == this.Application.ApplicationId && a.CorrespondenceCategory == CorrespondenceCategoryLookup.CorrespondenceCategory.LoanAgreementPdf)
                    .OrderByDescending(a => a.CreatedDate).FirstOrDefault();
                if (html != null)
                {
                    HtmlLoanAgreementEdocId = html.EdocId;
                }
                if (pdf != null)
                {
                    PdfLoanAgreementEdocId = pdf.EdocId;
                }

                PopulateImportantMessage();
            }

        }


        internal void CancelPreFundingLoan(int applicationId, FirstAgain.Domain.Lookups.FirstLook.WithdrawReasonTypeLookup.WithdrawReasonType withdrawReason, string withdrawReasonDescription)
        {
            var applicationRow = _customerUserIdDataSet.Application.FirstOrDefault(a => a.ApplicationId == applicationId);
            if (IsPastLockOut)
            {
                throw new ArgumentNullException(string.Format("Application cannot be cancelled past the lock out date {0}", applicationId));
            }
            if (applicationRow == null)
            {
                throw new ArgumentNullException(string.Format("applicationRow not found for applicationId {0}", applicationId));
            }
            if (applicationRow.ApplicationStatusType.IsNoneOf(ApplicationStatusTypeLookup.ApplicationStatusType.PreFunding, ApplicationStatusTypeLookup.ApplicationStatusType.PreFundingNLTR))
            {
                throw new ArgumentNullException(string.Format("ApplicationStatus of {0} was unexpected for applicationId {1}", applicationRow.ApplicationStatusType, applicationId));
            }

            DomainServiceLoanApplicationOperations.ExpireApplication(applicationRow, withdrawReason, withdrawReasonDescription, null);
        }

        private void PopulateImportantMessage()
        {
            if ((_loanOfferDataSet.LatestApprovedLoanTerms.PurposeOfLoan == PurposeOfLoanLookup.PurposeOfLoan.NewAutoPurchase) ||
                (_loanOfferDataSet.LatestApprovedLoanTerms.PurposeOfLoan == PurposeOfLoanLookup.PurposeOfLoan.UsedAutoPurchase) ||
                (_loanOfferDataSet.LatestApprovedLoanTerms.PurposeOfLoan == PurposeOfLoanLookup.PurposeOfLoan.AutoRefinancing) ||
                (_loanOfferDataSet.LatestApprovedLoanTerms.PurposeOfLoan == PurposeOfLoanLookup.PurposeOfLoan.MotorcyclePurchase) ||
                (_loanOfferDataSet.LatestApprovedLoanTerms.PurposeOfLoan == PurposeOfLoanLookup.PurposeOfLoan.BoatRvPurchase))
            {
                string vehicleDesc;
                switch (_loanOfferDataSet.LatestApprovedLoanTerms.PurposeOfLoan)
                {
                    case PurposeOfLoanLookup.PurposeOfLoan.NewAutoPurchase:
                    case PurposeOfLoanLookup.PurposeOfLoan.UsedAutoPurchase:
                        vehicleDesc = "the vehicle seller or your";
                        break;
                    case PurposeOfLoanLookup.PurposeOfLoan.AutoRefinancing:
                        vehicleDesc = "your current lender or";
                        break;
                    case PurposeOfLoanLookup.PurposeOfLoan.BoatRvPurchase:
                    case PurposeOfLoanLookup.PurposeOfLoan.MotorcyclePurchase:
                        vehicleDesc = "the seller, current lender, or your";
                        break;
                    default:
                        vehicleDesc = "the vehicle seller or your";
                        break;
                }

                ImportantMessage = string.Format(FAMessages.PreFundingInfo_LoanPurpose_Message, vehicleDesc);
            }
        }

        #region Action methods
        public void ChangePaymentDate(int dayOfMonth)
        {
            Guard.AgainstNull<LoanOfferDataSet>(_loanOfferDataSet, "_loanOfferDataSet");

            // If identical data submitted (client validation error, double-post, or shenanigans, then there is nothing to do)
            if (_loanOfferDataSet.LoanContract[0].MonthlyPaymentDate != (byte)dayOfMonth)
            {
                // update the new payment day of month
                _loanOfferDataSet.LoanContract[0].MonthlyPaymentDate = (byte)dayOfMonth;

                // re-calculate and update the first payment date
                DateTime firstPaymentBusinessDate;
                DateTime firstPaymentDate = BusinessCalendar.GetFirstPaymentDate(_loanOfferDataSet.LoanContract[0].FundingDate, dayOfMonth, out firstPaymentBusinessDate);
                _loanOfferDataSet.LoanContract[0].InitialPaymentDate = firstPaymentDate;

                // submit the combined updates to the middle tier
                DomainServiceLoanApplicationOperations.SubmitLoanContractWithWebActivity(_loanOfferDataSet,
                                                                                         WebActivityDataSetHelper.Populate(WebUser, CurrentStatus),
                                                                                         EventTypeLookup.EventType.ChangedPaymentDate,
                                                                                         false);
            }

        }

        internal bool RescheduleFundingDate(DateTime requestedFundingDate, out string errorMessage, bool isNLTR = false)
        {
            var availableFundingDates = BusinessCalendarClient.GetPossibleFundingDates(ApplicationId, true);
            PossibleFundingDate fundingDate = availableFundingDates.FindByDate(requestedFundingDate); ;

            if (fundingDate != null && fundingDate.HasExpired)
            {
                errorMessage = Resources.LoanAppErrorMessages.FundingDateNotAvailable;
                return false;
            }
            else if (fundingDate != null && fundingDate.HasEscaltedFundsTransferType &&
                fundingDate.OriginalValue.FundsTransferType == FirstAgain.Domain.Lookups.FirstLook.FundsTransferTypeLookup.FundsTransferType.ACHTransaction)
            {
                errorMessage = Resources.LoanAppErrorMessages.ACHFundingDateExpired;
                return false;
            }
            else
            {
                if (fundingDate == null)
                {
                    errorMessage = "Funding date is required";
                    return false;
                }
                else if (BankSupportsChange(fundingDate, out errorMessage))
                {
                    UpdateFundingInfo(fundingDate);
                    errorMessage = string.Empty;

                    if (isNLTR)
                    {
                        if (!NeedsBankingInfo())
                        {
                            DomainServiceLoanApplicationOperations.CompletePreFundingNLTR(Application, EventTypeLookup.EventType.UpdateFundedDate);
                        }
                    }
                    return true;
                }
                else
                {
                    errorMessage = Resources.LoanAppErrorMessages.BankDoesNotSupportWire;
                    return false;
                }
            }
        }

        private bool NeedsBankingInfo()
        {
            return _loanOfferDataSet.SwitchedFromInvoiceToAutoPayPreSign()
                   || (!_loanOfferDataSet.HasDebitAccountInfo && _loanOfferDataSet.CurrentLoanTerms.PaymentType == PaymentTypeLookup.PaymentType.AutoPay);
        }

        private void UpdateFundingInfo(PossibleFundingDate fundingDate)
        {
            var transferType = fundingDate.FundsTransferType;
            DateTime firstPaymentBusinessDate;
            DateTime firstPaymentDate = BusinessCalendarClient.GetFirstPaymentDate(fundingDate, _loanOfferDataSet.LoanContract[0].MonthlyPaymentDate, out firstPaymentBusinessDate);

            BankingInfoDataSet ds = DomainServiceLoanApplicationOperations.GetBankingInfo(Application.ApplicationId);

            ds.LoanContract[0].FundingDate = fundingDate;
            ds.LoanContract[0].FundsTransferType = transferType;
            ds.LoanContract[0].InitialPaymentDate = firstPaymentDate;

            DomainServiceLoanApplicationOperations.UpdateBankingInfo(ds, EventAuditLogHelper.PopulateEventAuditLogDataSet(_customerUserIdDataSet, WebUser, EventTypeLookup.EventType.UpdateFundedDate, null));
        }
        
        private bool BankSupportsChange(PossibleFundingDate fundingDate, out string errorMessage)
        {
            FundsTransferTypeLookup.FundsTransferType tranferType = fundingDate.FundsTransferType;
            bool isWire = ((tranferType == FundsTransferType.WireTransfer) || (tranferType == FundsTransferType.WireTransferSameDay));
            if (!isWire)
            {
                BankingInfoDataSet ds = DomainServiceLoanApplicationOperations.GetBankingInfo(Application.ApplicationId);

                var creditRow = _loanOfferDataSet.BankAccountInfo.FirstOrDefault(r => r.BankAccountActionType == BankAccountActionTypeLookup.BankAccountActionType.Credit);
                if (creditRow.IsACHTransactionRoutingNumberNull() && !creditRow.IsWireTransferRoutingNumberNull())
                {
                    errorMessage = "Unfortunately, the banking information you provided only supports wire transfers, please select another wire transfer date from the calendar.";
                    return false;
                }
                else if (creditRow.IsACHTransactionRoutingNumberNull() && creditRow.IsWireTransferRoutingNumberNull())
                {
                    errorMessage = "The 9 digit routing number entered is invalid, please correct the information below and resubmit.  Thank You.";
                    return false;
                }
                else
                {
                    errorMessage = string.Empty;
                    return true; // In this case, the customer doesn't want a wire date, so just move on
                }
            }
            if (isWire && _loanOfferDataSet.FundingAccountSupportsWire())
            {
                errorMessage = string.Empty;
                return true;
            }
            else
            {
                errorMessage = "Unfortunately, your banking institution does not support wire transfers, please select another eligible date from the calendar.  Thank you.";
                return false;
            }
        }

        #endregion


        #region HTML / view specific properies
        public MvcHtmlString FundingDataLink(string text, string tooltipText, string href = null, bool readOnly = false)
        {
            if (!CanMakeChanges || readOnly)
            {
                return new MvcHtmlString(string.Format("<strong class='brand'>{0}</strong>", text));
            }
            return new MvcHtmlString(string.Format("<a href=\"{2}\" data-tooltip aria-haspopup=\"true\" data-options=\"disable_for_touch:true\" class=\"no-track has-tip\" title=\"{1}\">{0}</a>", text, tooltipText, href ?? "#"));
        }
        #endregion


    }
}