using FirstAgain.Common;
using FirstAgain.Domain.Common;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.SharedTypes.InterestRate;
using FirstAgain.Domain.SharedTypes.LoanApplication;

namespace LightStreamWeb.Models.Shared
{
    public class LoanTermsModel
    {
        public PurposeOfLoanLookup.PurposeOfLoan PurposeOfLoan { get; set; }
        public LoanTermsRequestTypeLookup.LoanTermsRequestType LoanTermsRequestType { get; set; }
        public decimal LoanAmount { get; set; }
        public decimal LoanAmountMinusFees { get; set; }
        public int LoanTerm { get; set; }
        public int LoanTermsRequestId { get; set; }
        public PaymentTypeLookup.PaymentType PaymentMethod { get; set; }
        public decimal InterestRateMin { get; set; }
        public decimal InterestRateMax { get; set; }
        public string LoanOfferDisclaimer { get; private set; }
        public decimal? FloridaDocStampTax { get; set; }

        public bool IsSecured()
        {
            return PurposeOfLoan.IsSecured();
        }
        public bool IsAuto()
        {
            return PurposeOfLoan.IsAuto();
        }

        public bool IsHomeImprovement()
        {
            return PurposeOfLoan.Equals(PurposeOfLoanLookup.PurposeOfLoan.HomeImprovement);
        }

        public bool IsCreditCardConsolidation()
        {
            return PurposeOfLoan.Equals(PurposeOfLoanLookup.PurposeOfLoan.CreditCardConsolidation);
        }

        /// <summary>
        /// Populate the model from a LoanTermsRequestRow
        /// </summary>
        /// <param name="loanTermsRequestRow"></param>
        public void Populate(ILoanTerms loanTermsRequestRow)
        {
            LoanTermsRequestId = loanTermsRequestRow.LoanTermsRequestId;
            PurposeOfLoan = loanTermsRequestRow.PurposeOfLoan;
            LoanTermsRequestType = loanTermsRequestRow.LoanTermsRequestType;
            LoanAmount = loanTermsRequestRow.Amount;
            LoanAmountMinusFees = loanTermsRequestRow.AmountMinusFees;
            PaymentMethod = (PaymentTypeLookup.PaymentType)loanTermsRequestRow.PaymentType;
            InterestRateMin = loanTermsRequestRow.InterestRate / 100m;
            InterestRateMax = loanTermsRequestRow.InterestRate / 100m;
            LoanTerm = loanTermsRequestRow.TermMonths;
            if (loanTermsRequestRow.FloridaDocStampFee > 0)
            {
                FloridaDocStampTax = loanTermsRequestRow.FloridaDocStampFee;
            }
            PopulateLoanOfferDisclaimer();
        }


        /// <summary>
        /// Populate the model from a LoanTermsRequestRow, and current InterestRates for the application
        /// </summary>
        /// <param name="loanTermsRequestRow"></param>
        public void PopulateSingleRate(ILoanTerms loanTermsRequestRow, InterestRates rates)
        {
            Populate(loanTermsRequestRow);
            decimal? rate = rates.GetRate();
            if (rate == null)
                throw new FirstAgainException("The interest rate for application ID " + loanTermsRequestRow.ApplicationId + " is null.");

            if (rates.InterestRateParams.PurposeOfLoan != loanTermsRequestRow.PurposeOfLoan || rates.InterestRateParams.LoanAmount != loanTermsRequestRow.AmountMinusFees
                || rates.InterestRateParams.LoanTerm != loanTermsRequestRow.TermMonths)
            {
                throw new FirstAgainException("The counter offer terms do not match the interest rate terms.");
            }
            InterestRateMin = InterestRateMax = rate.Value;
        }

        /// <summary>
        /// Populate the model from a LoanTermsRequestRow, and current InterestRates for the application
        /// </summary>
        /// <param name="loanTermsRequestRow"></param>
        public void PopulateRateRange(ILoanTerms loanTermsRequestRow, InterestRates rates)
        {
            Populate(loanTermsRequestRow);
            DecimalRange rateRange = rates.GetTieredRateRange(loanTermsRequestRow.TermMonths, loanTermsRequestRow.AmountMinusFees);
            if (rateRange == null)
                throw new FirstAgainException("The interest rate for application ID " + loanTermsRequestRow.ApplicationId + " is null.");

            if (rates.InterestRateParams.PurposeOfLoan != loanTermsRequestRow.PurposeOfLoan || rates.InterestRateParams.LoanAmount != loanTermsRequestRow.AmountMinusFees
                || rates.InterestRateParams.LoanTerm != loanTermsRequestRow.TermMonths)
            {
                throw new FirstAgainException("The counter offer terms do not match the interest rate terms.");
            }
            InterestRateMin = rateRange.Min;
            InterestRateMax = rateRange.Max;
        }

        private void PopulateLoanOfferDisclaimer()
        {
            const string LEINHOLDER_DISCLAIMER = "<p>You must ensure that LightStream is listed as the lienholder " + 
                                                         "on the vehicle title. We will provide you with instructions to give to the dealer once you have set up your loan " +
                                                         "funding. LightStream will be listed as the lienholder until the loan is paid in full.</p>";
            if (PurposeOfLoan.IsSecured())
            {
                switch (PurposeOfLoan)
                {
                    case PurposeOfLoanLookup.PurposeOfLoan.NewAutoPurchaseSecured:
                    case PurposeOfLoanLookup.PurposeOfLoan.UsedAutoPurchaseSecured:
                        LoanOfferDisclaimer = "<p>Please note that your automobile must be purchased from a licensed automobile dealer and not from an individual. ";
                        LoanOfferDisclaimer += "You may purchase only 1 vehicle with this loan.</p>";
                        LoanOfferDisclaimer += LEINHOLDER_DISCLAIMER;
                        break;
                    case PurposeOfLoanLookup.PurposeOfLoan.PrivatePartyPurchaseSecured:
                        LoanOfferDisclaimer = "<p>Please note that your loan proceeds must be used to purchase an automobile from a private party. ";
                        LoanOfferDisclaimer += "The vehicle cannot have an existing lien against it. You may purchase only 1 vehicle with this loan.</p>";
                        LoanOfferDisclaimer += LEINHOLDER_DISCLAIMER;
                        break;
                    case PurposeOfLoanLookup.PurposeOfLoan.AutoRefinancingSecured:
                        LoanOfferDisclaimer = "<p>Please note that your loan proceeds must be used to pay off an outstanding auto loan with an existing lender other "; 
                        LoanOfferDisclaimer += "than LightStream. You may refinance only 1 vehicle with this loan.</p>";
                        LoanOfferDisclaimer += LEINHOLDER_DISCLAIMER;

                        break;
                    case PurposeOfLoanLookup.PurposeOfLoan.LeaseBuyOutSecured:
                        LoanOfferDisclaimer = "<p>Please note that your loan proceeds must be used to purchase an automobile presently being leased from a leasing " + 
                                              "company or an automobile dealer. You may purchase only 1 vehicle with this loan.</p>";
                        LoanOfferDisclaimer += LEINHOLDER_DISCLAIMER;
                        break;
                }
            }
        }

        public string ProductName
        {
            get
            {
                return PurposeOfLoanLookup.GetCaption(PurposeOfLoan);
            }
        }

        public decimal MonthlyPaymentMin
        {
            get
            {
                if (LoanAmount > 0 && LoanTerm > 0 && InterestRateMin > 0)
                {
                    return MonthlyPaymentCalculator.CalculatePayment(LoanAmount, LoanTerm, InterestRateMin);
                }
                return 0;
            }
        }
        public decimal MonthlyPaymentMax
        {
            get
            {
                if (LoanAmount > 0 && LoanTerm > 0 && InterestRateMin > 0)
                {
                    return MonthlyPaymentCalculator.CalculatePayment(LoanAmount, LoanTerm, InterestRateMax);
                }
                return 0;
            }
        }

        public string MonthlyPayment
        {
            get
            {
                if (MonthlyPaymentMin == MonthlyPaymentMax || MonthlyPaymentMax == 0)
                {
                    return DataConversions.FormatAndGetMoney(MonthlyPaymentMin);
                }

                return DataConversions.FormatAndGetMoney(MonthlyPaymentMin) + " - " + DataConversions.FormatAndGetMoney(MonthlyPaymentMax);
            }
        }

        public string InterestRate
        {
            get
            {
                if (InterestRateMin == InterestRateMax || InterestRateMax == 0)
                {
                    return DataConversions.FormatAndGetPercent(InterestRateMin);
                }

                return DataConversions.FormatAndGetPercent(InterestRateMin) + " - " + DataConversions.FormatAndGetPercent(InterestRateMax);

            }
        }

    }
}