using FirstAgain.Common;
using FirstAgain.Common.Extensions;
using FirstAgain.Domain.Common;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.SharedTypes.InterestRate;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace LightStreamWeb.Shared.Rates
{
    public class DisplayRateModel
    {
        #region constructors
        public DisplayRateModel(IInterestRates cachedInterestRates)
        {
            _cachedInterestRates = cachedInterestRates;
        }
        #endregion

        #region private memebrs
        private IInterestRates _cachedInterestRates;
        #endregion

        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public int MinTerm { get; set; }
        public int MaxTerm { get; set; }
        public PurposeOfLoanLookup.PurposeOfLoan PurposeOfLoan { get; set; }
        public decimal InvoicePenalty { get; set; }

        public decimal Rate { get; set; }
        public decimal BaseRate { get; set; }
        public decimal Base144Rate { get; set; }
        public bool IsDiscountedRate { get; set; }
        public string DiscountDescription { get; set; }

        public decimal MinRate { get; set; }
        public decimal MaxRate { get; set; }
        [ReadOnly(true)]
        public string RateRangePretty
        {
            get
            {
                var min = (MinRate * 100).ToString("0.00");
                var max = (MaxRate * 100).ToString("0.00");
                return $"{min}% – {max}%";
            }
        }
        public string OverallRateRangePretty
        {
            get
            {
                var subtractedInvoiceRate = 0.5M;
                var min = (MinRate * 100).ToString("0.00");
                var max = ((OverallMaxRate * 100) - subtractedInvoiceRate).ToString("0.00");
                return $"{min}% – {max}%";
            }
        }
        public decimal OverallMaxRate { get; set; }

        public string SampleTermCaption { get; set; }
        public decimal SampleAmountMin { get; set; }
        public decimal SampleAmountMax { get; set; }
        public int SampleNumberOfPayments { get; set; }
        public decimal SampleMonthlyPayment
        {
            get
            {
                return MonthlyPaymentCalculator.CalculatePayment(SampleAmountMin, SampleNumberOfPayments, Rate);
            }
        }

        [ReadOnly(true)]
        public string PurposeOfLoanCaption
        {
            get
            {
                return PurposeOfLoanLookup.GetCaption(PurposeOfLoan);
            }
        }
        public bool DisplaySecuredAutoDisclosure
        {
            get
            {
                return PurposeOfLoan.HasSecuredPurpose();
            }
        }

        public DisplayRateModel GetLowestRate(int? advertisingSourceId = null)
        {
            var interestRates = GetInterestRates(_cachedInterestRates, PurposeOfLoanLookup.PurposeOfLoan.Other, advertisingSourceId);
            var sampleRate = GetSampleRate(interestRates, true);
            var interestRateRange = interestRates.GetCachedRateRangeForAllTermsAmountsPurposesAndTiers(PaymentTypeLookup.PaymentType.AutoPay, advertisingSourceId);

            if (interestRateRange != null)
            {
                PopulateModel(sampleRate, new DecimalRange()
                {
                    Max = interestRateRange.MaxRate.Rate,
                    Min = interestRateRange.MinRate.Rate
                });
            }

            return this;
        }

        public DisplayRateModel GetForPurposeOfLoan(PurposeOfLoanLookup.PurposeOfLoan purposeOfLoan, int? advertisingSourceId = null)
        {
            this.PurposeOfLoan = purposeOfLoan;

            var interestRates = GetInterestRates(_cachedInterestRates, purposeOfLoan, advertisingSourceId);

            ProductInterestRate sampleRate = new ProductInterestRate();
            sampleRate = GetSampleRate(interestRates, false);
            var interestRateRange = interestRates.GetTieredRateRangeForAllTermsAndAmounts();

            if (purposeOfLoan == PurposeOfLoanLookup.PurposeOfLoan.HomeImprovement)
            {
                var sampleRate144HI = GetSampleRateFor144HI(interestRates, purposeOfLoan);
                var Base144Rate = sampleRate.RateInfo.BaseRate.InterestRate;
                PopulateModel(sampleRate144HI, interestRateRange, Base144Rate);
            } else
            {
                PopulateModel(sampleRate, interestRateRange);
            }

            return this;
        }

        private void PopulateModel(ProductInterestRate sampleRate, DecimalRange range, decimal base144Rate = 0)
        {
            MinAmount = _cachedInterestRates.GetAmountRange().Min;
            MaxAmount = _cachedInterestRates.GetAmountRange().Max;
            InvoicePenalty = _cachedInterestRates.InvoicePenalty;
            Rate = sampleRate.Rate;
            IsDiscountedRate = HasDiscount(sampleRate);
            DiscountDescription = GetMarketingPartnerName(sampleRate);
            BaseRate = sampleRate.RateInfo.BaseRate.InterestRate;
            Base144Rate = base144Rate;
            MinTerm = sampleRate.TermRange.Min;
            MaxTerm = sampleRate.TermRange.Max;
            PurposeOfLoan = sampleRate.PurposeOfLoan;
            SampleAmountMin = Math.Floor(sampleRate.AmountRange.Min);
            SampleAmountMax = Math.Floor(sampleRate.AmountRange.Max);
            SampleTermCaption = sampleRate.TermRange.Max % 12 == 0 ? string.Format("{0} years", sampleRate.TermRange.Max / 12) : string.Format("{0} months", sampleRate.TermRange.Max);
            SampleNumberOfPayments = sampleRate.TermRange.Max;
            MinRate = range.Min;
            MaxRate = range.Max;
            OverallMaxRate = GetOverallMaxRate(_cachedInterestRates);
        }


        protected static string GetMarketingPartnerName(ProductInterestRate sampleRate)
        {
            if (!HasDiscount(sampleRate))
            {
                return string.Empty;
            }

            var adjustment = sampleRate.RateInfo.Adjustments.FirstOrDefault(a => a.InterestGroups.Any(i => i.InterestGroupType == InterestGroupTypeLookup.InterestGroupType.AdvertisingSourceID));
            return (adjustment != null) ? adjustment.CustomerDescription : string.Empty;
        }

        private static bool HasDiscount(ProductInterestRate sampleRate)
        {
            var result = sampleRate.RateInfo.Adjustments.Any(a => a.InterestGroups.Any(i => i.InterestGroupType == InterestGroupTypeLookup.InterestGroupType.AdvertisingSourceID));
            return result;
        }

        private static decimal GetOverallMaxRate(IInterestRates interestRates)
        {
            return interestRates.GetCachedRateRangeForAllTermsAmountsPurposesAndTiers(PaymentTypeLookup.PaymentType.Invoice, null).MaxRate.Rate;
        }

        private static ProductInterestRate GetSampleRate(IInterestRates interestRates, bool allLoanPurposes)
        {
            // The same low rate may apply across multiple loan purposes.  If that's the case, we select one
            // randomly for the rate disclosure text.
            List<ProductInterestRate> lowRates = interestRates.GetLowestRates(allLoanPurposes);

            if (lowRates.Any(r => r.PurposeOfLoan == PurposeOfLoanLookup.PurposeOfLoan.AutoRefinancing && lowRates.Count > 1))
            {
                lowRates = lowRates.Where(r => r.PurposeOfLoan != PurposeOfLoanLookup.PurposeOfLoan.AutoRefinancing).ToList();
            }
            if (lowRates != null && lowRates.Count > 0)
            {
                return GetPriorityPurposeForLowRateTieBreaker(lowRates);
            }

            return null;
        }

        private static ProductInterestRate GetSampleRateFor144HI(IInterestRates interestRates, PurposeOfLoanLookup.PurposeOfLoan purposeOfLoan)
        {
            ProductInterestRate sampleRate = new ProductInterestRate
            {
                PurposeOfLoan = purposeOfLoan,
                CreditTier = CreditTierLookup.CreditTier.SuperPrime
            };

            //get the possible max loan term range. eg.:for Home improvement Loan = 84-144
            sampleRate.TermRange = interestRates.GetTerms().Select(r => r).OrderByDescending(r => r.Max).FirstOrDefault();
            interestRates.InterestRateParams.LoanTerm = sampleRate.TermRange.Max;

            //get possible smallest amount for that loan. eg.for Home improvement Loan = $25,000 to $49,999
            sampleRate.AmountRange = interestRates.GetAmounts().Select(a => a).OrderBy(a => a.Min).Where(a => interestRates.IsAmountValid(a.Min, sampleRate.TermRange.Max)).FirstOrDefault();
            interestRates.InterestRateParams.LoanAmount = sampleRate.AmountRange.Min;

            //get sample rate info
            sampleRate.RateInfo = interestRates.GetTieredRates().OrderBy(x => x.Rate).FirstOrDefault();
            sampleRate.Rate = sampleRate.RateInfo.Rate;


            return sampleRate;
        }
        

        private static IInterestRates GetInterestRates(IInterestRates cachedInterestRates, PurposeOfLoanLookup.PurposeOfLoan purposeOfLoan, int? advertisingSourceId)
        {
            cachedInterestRates.InterestRateParams = InterestRateParams.FixedRateDefault;
            cachedInterestRates.InterestRateParams.AdvertisingSourceId = advertisingSourceId;
            cachedInterestRates.InterestRateParams.PurposeOfLoan = purposeOfLoan;
            
            return cachedInterestRates;
        }

        public static ProductInterestRate GetPriorityPurposeForLowRateTieBreaker(List<ProductInterestRate> lowRates)
        {

            //this priority list was provided from the business 
            List<PurposeOfLoanLookup.PurposeOfLoan> orderOfPurpose = new List<PurposeOfLoanLookup.PurposeOfLoan>
             {
                PurposeOfLoanLookup.PurposeOfLoan.UsedAutoPurchase,
                PurposeOfLoanLookup.PurposeOfLoan.HomeImprovement,
                PurposeOfLoanLookup.PurposeOfLoan.CreditCardConsolidation,
                PurposeOfLoanLookup.PurposeOfLoan.PrivatePartyPurchase,
                PurposeOfLoanLookup.PurposeOfLoan.AutoRefinancing,
                PurposeOfLoanLookup.PurposeOfLoan.NewAutoPurchase,
                PurposeOfLoanLookup.PurposeOfLoan.BoatRvPurchase,
                PurposeOfLoanLookup.PurposeOfLoan.Other,
                PurposeOfLoanLookup.PurposeOfLoan.TimeSharePurchase,
                PurposeOfLoanLookup.PurposeOfLoan.MedicalExpense,
                PurposeOfLoanLookup.PurposeOfLoan.MotorcyclePurchase,
                PurposeOfLoanLookup.PurposeOfLoan.LeaseBuyOut,
                PurposeOfLoanLookup.PurposeOfLoan.EducationalExpenses
            };
            var sortedList = lowRates.OrderBy(d => orderOfPurpose.IndexOf(d.PurposeOfLoan)).ToList();
            return sortedList.FirstOrDefault();
        }
    }
}