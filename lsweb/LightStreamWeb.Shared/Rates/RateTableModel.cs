using FirstAgain.Common;
using FirstAgain.Common.Extensions;
using FirstAgain.Common.Logging;
using FirstAgain.Domain.Common;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.SharedTypes.ContentManagement;
using FirstAgain.Domain.SharedTypes.InterestRate;
using LightStreamWeb.Shared.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LightStreamWeb.Shared.Rates
{
    public class RateTableModel
    {
        #region private members
        protected ICMSRatesPage _cmsRatePageContent;
        protected InterestRates _rates;
        private DateTime _rateLockDate = DateTime.Now;
        // There are no requirements for a specific rate lock period, only the concept that the rate will not change
        // once the customer starts to apply on the website.  Limit the rate lock period here to be no more than the
        // rate lock period for inquiries.  The rate lock duration should be kept shorter than the number of days an 
        // application could spend in the process pipeline, the sunset period for obsoleted loan products.
        private const int RateLockDateMaxDaysAgo = 28;
        #endregion

        #region constructors
        public RateTableModel()
        {

        }

        public RateTableModel(ICMSRatesPage cmsRatesPage)
        {
            PurposeOfLoan = PurposeOfLoanLookup.PurposeOfLoan.NotSelected;
            PaymentMethod = PaymentTypeLookup.PaymentType.AutoPay;
            _cmsRatePageContent = cmsRatesPage;
        }
        public RateTableModel(ICMSRatesPage cmsRatesPage, InterestRates rates)
        {
            PurposeOfLoan = PurposeOfLoanLookup.PurposeOfLoan.NotSelected;
            PaymentMethod = PaymentTypeLookup.PaymentType.AutoPay;
            _cmsRatePageContent = cmsRatesPage;
            _rates = rates;
        }
        public RateTableModel(ICMSRatesPage cmsRatesPage, InterestRates rates, PaymentTypeLookup.PaymentType paymentType)
        {
            PurposeOfLoan = PurposeOfLoanLookup.PurposeOfLoan.NotSelected;
            PaymentMethod = paymentType;
            _cmsRatePageContent = cmsRatesPage;
            _rates = rates;
        }
        #endregion

        public string ZipCode { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PurposeOfLoanLookup.PurposeOfLoan PurposeOfLoan { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PaymentTypeLookup.PaymentType? PaymentMethod { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public StateLookup.State? State { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public StateLookup.State? CoApplicantState { get; set; }
        public decimal? LoanAmount { get; set; }
        public int? LoanTermMonths { get; set; }
        public int? ApplicationId { get; set; }
        public ApplicationStatusTypeLookup.ApplicationStatusType ApplicationStatus { get; set; }
        public bool DisableLoanPurposeChange { get; set; }
        public int? FirstAgainCodeTrackingId { get; set; }
        public List<RateTableRow> RateTable { get; private set; }
        public bool[] RateTableRows = new bool[10];
        public DecimalRange EstimatedMonthlyPayment { get; private set; }
        public DecimalRange Rate { get; private set; }
        public LandingPageContent.RateTableCalculatorType TypeOfCalculator { get; set; }

        public bool IsAddCoApp { get; set; }

        public decimal? MinRate
        {
            get
            {
                return RateTable?.Min(t => t.Rates.Min(r => r.Min));
            }
        }

        public decimal? MaxRate
        {
            get
            {
                return RateTable?.Max(t => t.Rates.Max(r => r.Max));
            }
        }

        public FlagLookup.Flag? Discount { get; set; }
        public bool IsSuntrustApplication { get; set; }

        // the ProductRate may differ from the rate range, for secured auto products. 
        // the ProductRate will be the super-prime rate for the product specified
        public decimal? ProductRate { get; private set; }
        public decimal? ProductMonthlyPayment { get; private set; }
        public string RateLockDate
        {
            get
            {
                return _rateLockDate.ToString();
            }
            set
            {
                if (value.IsNotNullOrEmpty())
                {
                    DateTime dt;
                    if (DateTime.TryParse(value, out dt))
                    {
                        _rateLockDate = dt;
                    }
                    else
                    {
                        LightStreamLogger.WriteDebug(string.Format("Could not parse RateLockDate supplied by client: {0}", dt));
                    }
                }
            }
        }

        // for validation
        public int MinTerm { get; private set; }
        public int MaxTerm { get; private set; }
        public decimal MinLoanAmount { get; private set; }
        public decimal MaxLoanAmount { get; private set; }

        public decimal MinLoanAmountHint { get; private set; }
        public decimal MaxLoanAmountHint { get; private set; }


        // for UI elemeents
        public List<LoanPurposeItem> LoanPurposes { get; protected set; }

        public string LoanTermsHeader
        {
            get
            {
                return (_cmsRatePageContent != null) ? _cmsRatePageContent.Header : "Loan Terms";
            }
        }

        public RatesDisclosureContent CustomRateDisclosureContent { get; set; }

        public List<string> LoanTermDisclosures
        {
            get
            {
                var hasLandingPageTerms = CustomRateDisclosureContent.IsNotNull() &&
                                          CustomRateDisclosureContent.Terms.IsNotNull() &&
                                          CustomRateDisclosureContent.Terms.Any();

                var termsToUse = hasLandingPageTerms ? CustomRateDisclosureContent.Terms : _cmsRatePageContent.Terms;

                var cmsTerms = new List<string>();
                var ratesQuoted = "Rates quoted with AutoPay option. Invoicing option is 0.50 points higher.";

                var paymentMethod = PaymentMethod.GetValueOrDefault();
                var discount = Discount.GetValueOrDefault();

                if(discount.IsNoneOf(FlagLookup.Flag.SuntrustPremierBanking, FlagLookup.Flag.SuntrustPrivateWealth))
                {
                    cmsTerms.Add(ratesQuoted);
                }

                cmsTerms.AddRange(termsToUse.Where(a => a.DisplayCondition == RatesDisclosureContent.LoanTermDisplayCondition.ShowAlways
                    || (a.DisplayCondition == RatesDisclosureContent.LoanTermDisplayCondition.SHowIfNotInProcess && !ApplicationStatus.IsActiveApplication())
                    || (a.DisplayCondition == RatesDisclosureContent.LoanTermDisplayCondition.ShowIfIsActiveApplication && ApplicationStatus.IsActiveApplication()))
                    .Select(a => a.LoanTermCopy));

                // PBI 8420. "Rates quoted with AutoPay option. Invoicing option is 0.50 points higher" is currently in CMS, which is wrong.
                // this disclosure should only be displayed if rates are actually quoted with auto pay, and if invoice would be 0.5 higher
                cmsTerms.Remove(ratesQuoted);

                //PBI 32763
                //http://team03:8080/tfs/LightStream/LoanSystem/_workitems?_a=edit&id=32763
                if (cmsTerms.Any())
                {
                    cmsTerms = cmsTerms.Where(term => !string.IsNullOrWhiteSpace(term)).ToList();

                    for (int i = 0; i < cmsTerms.Count; i++)
                    {
                        if (cmsTerms[i] == "Fixed rate, simple interest installment loans, no fees or prepayment penalties")
                        {
                            cmsTerms[i] = "Fixed rate, simple interest fully amortizing installment loans, no fees or prepayment penalties.";
                        }

                        //If no period at end of disclosure, append a period.
                        var lastChar = cmsTerms[i][cmsTerms[i].Length - 1];
                        if (lastChar != '.')
                        {
                            cmsTerms[i] += '.';
                        }
                    }
                }

                if (discount == FlagLookup.Flag.SuntrustPremierBanking && paymentMethod == PaymentTypeLookup.PaymentType.Invoice)
                    cmsTerms.Add("Rates quoted with Premier Banking Discount.");

                return cmsTerms.ToList();
            }
        }

        public string Disclosure
        {
            get
            {
                if (PurposeOfLoan == PurposeOfLoanLookup.PurposeOfLoan.NotSelected)
                {
                    return string.Empty;
                }

                if (IsAuto())
                {
                    return string.Format(@"Rates for each loan amount and term combination below are shown in a minimum to maximum range because LightStream rates vary based on your credit profile. The lowest rate in each range is for LightStream's unsecured auto loan product and requires that you have an <a href=""/partial/cms/excellent-credit"" data-popup=""true"">excellent credit</a> profile. If your application is approved, your credit profile will determine your rate. N/A means that LightStream loans are not available for that loan amount and term combination.");
                }

                return string.Format(@"Rates for each loan amount and term combination below are shown in a minimum to maximum range because LightStream rates and terms vary based on your credit profile. The lowest rate in each range requires that you have an <a href=""/partial/cms/excellent-credit"" data-popup=""true"">excellent credit profile.</a> N/A means that LightStream loans are not available for that loan amount and term combination.");
            }
        }

        private string GetUnsecuredLoanDescription()
        {
            switch (PurposeOfLoan)
            {
                case PurposeOfLoanLookup.PurposeOfLoan.UsedAutoPurchase:
                    return "a used auto";
                case PurposeOfLoanLookup.PurposeOfLoan.NewAutoPurchase:
                    return "a new auto";
                case PurposeOfLoanLookup.PurposeOfLoan.PrivatePartyPurchase:
                    return "a private party purchase";
                case PurposeOfLoanLookup.PurposeOfLoan.AutoRefinancing:
                    return "an auto refinance";
                case PurposeOfLoanLookup.PurposeOfLoan.LeaseBuyOut:
                    return "a lease buyout";
            }
            return string.Empty;
        }


        public List<IntRange> LoanTerms { get; private set; }
        private List<DecimalRange> LoanAmounts { get; set; }

        public string RatesDate
        {
            get
            {
                return DateTime.Now.ToString("MMMM d, yyyy");
            }
        }

        private void FixPurposeOfLoan(PurposeOfLoanLookup.PurposeOfLoan purpose)
        {
            if (purpose.IsSecured() && !PurposeOfLoan.IsSecured() && PurposeOfLoan.HasSecuredPurpose())
            {
                PurposeOfLoan = PurposeOfLoan.GetSecuredPurpose();
            }
            else if(!purpose.IsSecured() && PurposeOfLoan.IsSecured() && PurposeOfLoan.HasUnsecuredPurpose())
            {
                PurposeOfLoan = PurposeOfLoan.GetUnsecuredPurpose();
            }
        }

        private HashSet<PurposeOfLoanLookup.PurposeOfLoan> GetPurposeRange() => PurposeOfLoan == PurposeOfLoanLookup.PurposeOfLoan.NotSelected
                ? new HashSet<PurposeOfLoanLookup.PurposeOfLoan>(LoanPurposes.Select(a => (PurposeOfLoanLookup.PurposeOfLoan)Enum.Parse(typeof(PurposeOfLoanLookup.PurposeOfLoan), a.Value)))
                : new HashSet<PurposeOfLoanLookup.PurposeOfLoan>(new[] { PurposeOfLoan });

        public virtual void Populate()
        {
            List<InterestAdjustmentDate> filteredBaseRates;

            if (ApplicationId.HasValue && ApplicationStatus.IsActiveApplicationOrInquiry() && !IsAddCoApp)
            {
                FixPurposeOfLoan(_rates.InterestRateParams.PurposeOfLoan);

                PopulatePurposeOfLoan();

                var purposes = GetPurposeRange();

                if (PaymentMethod != null && PaymentMethod.Value != PaymentTypeLookup.PaymentType.NotSelected)
                {
                    _rates.InterestRateParams.PaymentType = PaymentMethod.Value;
                }

                if (PurposeOfLoan != PurposeOfLoanLookup.PurposeOfLoan.NotSelected)
                {
                    _rates.InterestRateParams.PurposeOfLoan = PurposeOfLoan;
                }

                filteredBaseRates = _rates.GetBaseRates(a =>
                      a.DateRange.Overlaps(_rates.InterestRateParams.DateRange) &&
                      a.Qualifies(_rates.InterestRateParams.CreditTier) &&
                      a.Qualifies(_rates.InterestRateParams.Flags) &&
                      purposes.Contains(a.PurposeOfLoan)
                      );
            }
            else
            {
                PopulatePurposeOfLoan();

                var purposes = GetPurposeRange();

                // Add private wealth flag
                if (Discount.HasValue && Discount != 0 && !_rates.InterestRateParams.Flags.Contains(Discount.Value))
                {
                    _rates.InterestRateParams.Flags.Add(Discount.Value);
                }
                var range = new DateRange(_rateLockDate, _rateLockDate); // Perhaps we should be using a date range here, but the posted data has start date = end date.

                // Constrain the rate lock date so that it's not too far in the past.  
                var minRateLockDate = DateTime.Now.AddDays(0 - RateLockDateMaxDaysAgo);
                if (_rateLockDate < minRateLockDate)
                {
                    _rateLockDate = minRateLockDate;
                }

                // Bug #88585 The rate should be pegged to the rate lock date, so  use the rate lock date here to prevent 
                // application post errors when there's a rate change. 
                _rates.InterestRateParams.StartDate = _rateLockDate;
                _rates.InterestRateParams.EndDate = _rateLockDate;

                // The rates contain a few days of history, so the rate lock date can be in the past (see interest rate cache setting in web.config).
                filteredBaseRates = _rates.GetBaseRates(a =>
                    a.DateRange.Overlaps(range) &&
                    a.Qualifies(_rates.InterestRateParams.Flags) &&
                    purposes.Contains(a.PurposeOfLoan));
            }

            // Filter the rates to get the term range (if amount is specified we get the terms just for that amount)...
            var filteredRatesByAmount = LoanAmount.HasValue ? filteredBaseRates.Where(a => a.Qualifies(LoanAmount.Value)) : filteredBaseRates;

            // Get amount ranges across all rates (across all available loan purposes if the purpose is not selected).
            DecimalRange amountRange = filteredBaseRates.GetAmountRange();

            MinLoanAmount = amountRange?.Min ?? 10000m;
            MaxLoanAmount = amountRange?.Max ?? 100000m;

            if (LoanTermMonths.HasValue &&
                LoanTermMonths >= _rates.GetMinTerm() &&
                LoanTermMonths <= _rates.GetMaxTerm())
            {
                var termsAmountRange = _rates.GetAmountRange(LoanTermMonths.Value, _rates.InterestRateParams);
                MinLoanAmountHint = termsAmountRange.Min;
                MaxLoanAmountHint = termsAmountRange.Max;
                MinLoanAmount = termsAmountRange.Min;
                MaxLoanAmount = termsAmountRange.Max;
            }
            else
            {
                MinLoanAmountHint = amountRange?.Min ?? 10000m;
                MaxLoanAmountHint = amountRange?.Max ?? 100000m;
            }

            // Get term ranges across all rates for (across all loan amounts if loan amount is not specified and across all available loan purposes if the purpose is not selected).
            var termRange = filteredRatesByAmount.GetTermRange() ?? filteredBaseRates.GetTermRange();


            MinTerm = termRange?.Min ?? 24;
            MaxTerm = termRange?.Max ?? 84;

            LoanAmounts = filteredBaseRates.GetAmounts();
            LoanTerms = filteredBaseRates.GetTerms();

            bool shouldNotDisplayRange = ApplicationStatus.IsOneOf(ApplicationStatusTypeLookup.ApplicationStatusType.Counter,
                                                                    ApplicationStatusTypeLookup.ApplicationStatusType.CounterV,
                                                                    ApplicationStatusTypeLookup.ApplicationStatusType.Approved,
                                                                    ApplicationStatusTypeLookup.ApplicationStatusType.PreFunding,
                                                                    ApplicationStatusTypeLookup.ApplicationStatusType.ApprovedNLTR,
                                                                    ApplicationStatusTypeLookup.ApplicationStatusType.PreFundingNLTR);

            PopulateRateTable(shouldNotDisplayRange);

            CalculateMonthlyPayment();

            if (ApplicationId.HasValue && ApplicationStatus.IsActiveApplication())
            {
                DisableLoanPurposeChange = !PurposeOfLoan.IsSecured(); 
            }
        }

        private void PopulateRateTable(bool shouldNotDisplayRange)
        {
            // act as if they have already 
            RateTable = new List<RateTableRow>();

            if (PurposeOfLoan == PurposeOfLoanLookup.PurposeOfLoan.NotSelected)
            {
                foreach (var range in LoanAmounts)
                {
                    var row = new RateTableRow()
                    {
                        LoanAmount = range,
                        Rates = new List<RateRange>()
                    };
                    foreach (var term in LoanTerms)
                    {
                        var rates = MinMaxRatesForAllPurposes(term.Min, range.Min);
                        if (rates != null) row.Rates.Add(new RateRange(rates.Min, rates.Max, term.Min, term.Max));
                    }
                    RateTable.Add(row);
                }
            }
            else
            {
                foreach (var range in LoanAmounts)
                {
                    var row = new RateTableRow()
                    {
                        LoanAmount = range,
                        Rates = new List<RateRange>()
                    };
                    foreach (var term in LoanTerms)
                    {
                        if (shouldNotDisplayRange)
                        {
                            var rate = _rates.GetRate(term: term.Min, amount: range.Min);
                            if (rate != null)
                            {
                                row.Rates.Add(new RateRange(min: rate.Value, max: rate.Value, minTerm: term.Min, maxTerm: term.Max));
                            }
                        }
                        else
                        {
                            var r = _rates.GetTieredRateRange(term.Min, range.Min);
                            if (r != null)
                            {
                                row.Rates.Add(new RateRange(min: r.Min, max: r.Max, minTerm: term.Min, maxTerm: term.Max));
                            }
                        }
                    }
                    RateTable.Add(row);
                }
            }
            if (RateTableRows.Count() == 0 || RateTable.Count() == 0)
                return;
            RateTableRows[RateTable.SelectItemWithIndex().OrderBy(row => row.Item.Rates.Min(r => r.Min)).First().Index] = true;
        }

        private RateRange MinMaxRatesForAllPurposes(int minTerm, decimal minAmount)
        {
            if (this.LoanPurposes == null || this.LoanPurposes.Count == 0) PopulatePurposeOfLoan();
            var originalPurpose = _rates.InterestRateParams.PurposeOfLoan;
            var result = new RateRange(min: 1, max: 0, minTerm: minTerm, maxTerm: 0);

            foreach (var purpose in LoanPurposes)
            {
                PurposeOfLoanLookup.PurposeOfLoan purposeParsed;
                if (Enum.TryParse(purpose.Value, out purposeParsed))
                {
                    _rates.InterestRateParams.PurposeOfLoan = purposeParsed;
                    var rateRange = _rates.GetTieredRateRange(term: minTerm, amount: minAmount);
                    if (rateRange != null)
                    {
                        if (rateRange.Min < result.Min) result.Min = rateRange.Min;
                        if (rateRange.Max > result.Max) result.Max = rateRange.Max;
                    }
                    var amountRange = _rates.GetAmountRange();
                }
            }
            _rates.InterestRateParams.PurposeOfLoan = originalPurpose;
            if (result.Min == 1 | result.Max == 0) return null;
            return result;
        }

        private void PopulatePurposeOfLoan()
        {
            PurposeOfLoanLookup.FilterType filter = (ApplicationStatus.IsActiveApplication() && PurposeOfLoan.IsSecured()) ? PurposeOfLoanLookup.FilterType.Secured : PurposeOfLoanLookup.FilterType.Public;

            bool isAutoOrMotorcycleOnly = TypeOfCalculator == LandingPageContent.RateTableCalculatorType.Auto;

            LoanPurposes = PurposeOfLoanLookup.GetFilteredBindingSource(filter)
                .Cast<PurposeOfLoanLookup.Value>()
                .Where(a => 
                    !isAutoOrMotorcycleOnly || 
                    (a.Enumeration.IsAuto() || a.Enumeration.IsOneOf(PurposeOfLoanLookup.PurposeOfLoan.MotorcyclePurchase, 
                                                                     PurposeOfLoanLookup.PurposeOfLoan.NotSelected)))
                .Select(a => new LoanPurposeItem()
            {
                Value = a.Enumeration.ToString(),
                Caption = PurposeOfLoanHelper.GetCaption(a)
            }).Where(x => filter == PurposeOfLoanLookup.FilterType.Unsecured || filter == PurposeOfLoanLookup.FilterType.Public || x.Value != "NotSelected").ToList(); // This last predicate seems fishy.
        }

        private void CalculateMonthlyPayment()
        {
            Rate = _rates.GetTieredRateRange(LoanTermMonths.GetValueOrDefault(), LoanAmount.GetValueOrDefault());

            // get the super prime rate 
            ProductRate = _rates.GetRate(LoanTermMonths.GetValueOrDefault(), LoanAmount.GetValueOrDefault());
            if (ProductRate.HasValue && LoanTermMonths.GetValueOrDefault() > 0)
            {
                ProductMonthlyPayment = MonthlyPaymentCalculator.CalculatePayment(LoanAmount.GetValueOrDefault(), LoanTermMonths.GetValueOrDefault(), ProductRate.Value);
            }

            if (Rate != null && Rate.IsValid)
            {
                EstimatedMonthlyPayment = new DecimalRange();
                if (LoanAmount.HasValue && LoanTermMonths.GetValueOrDefault() > 0)
                {
                    EstimatedMonthlyPayment.Min = MonthlyPaymentCalculator.CalculatePayment(LoanAmount.GetValueOrDefault(), LoanTermMonths.GetValueOrDefault(), Rate.Min);
                    EstimatedMonthlyPayment.Max = MonthlyPaymentCalculator.CalculatePayment(LoanAmount.GetValueOrDefault(), LoanTermMonths.GetValueOrDefault(), Rate.Max);
                }
            }
        }

        #region helper functions
        private bool IsSecuredAuto()
        {
            return PurposeOfLoan.IsSecuredAuto();
        }

        private bool IsUnSecuredAuto()
        {
            return PurposeOfLoan.IsUnSecuredAuto();
        }

        private bool IsAuto()
        {
            return PurposeOfLoan.IsAuto();
        }

        public static string GetPurposeOfLoanDisclosureDescription(FirstAgain.Domain.Lookups.FirstLook.PurposeOfLoanLookup.PurposeOfLoan purposeOfLoan)
        {
            switch (purposeOfLoan)
            {
                case PurposeOfLoanLookup.PurposeOfLoan.TimeSharePurchase:
                    return "a timeshare or fractional purchase";
                case PurposeOfLoanLookup.PurposeOfLoan.HomeImprovement:
                case PurposeOfLoanLookup.PurposeOfLoan.CommunityReinvestmentAct:
                    return "a home improvement, pool, or solar system";
                case PurposeOfLoanLookup.PurposeOfLoan.EducationalExpenses:
                    return "a PreK-12 education";
                case PurposeOfLoanLookup.PurposeOfLoan.Other:
                    return "miscellaneous/other purpose";
                case PurposeOfLoanLookup.PurposeOfLoan.BoatRvPurchase:
                    return "a boat, RV or aircraft";
                case PurposeOfLoanLookup.PurposeOfLoan.MotorcyclePurchase:
                    return "a motorcycle";
                case PurposeOfLoanLookup.PurposeOfLoan.MedicalExpense:
                    return "medical or adoption expenses";
                case PurposeOfLoanLookup.PurposeOfLoan.CreditCardConsolidation:
                    return "credit card consolidation";
                case PurposeOfLoanLookup.PurposeOfLoan.AutoRefinancing:
                    return "an auto refinance";
                case PurposeOfLoanLookup.PurposeOfLoan.NewAutoPurchase:
                    return "a new auto";
                case PurposeOfLoanLookup.PurposeOfLoan.UsedAutoPurchase:
                    return "a dealer used auto";
                case PurposeOfLoanLookup.PurposeOfLoan.PrivatePartyPurchase:
                    return "a private party used auto";
                default:
                    return PurposeOfLoanLookup.GetCaption(purposeOfLoan);
            }
        }
        #endregion

        #region inner classes
        public class LoanPurposeItem
        {
            public string Caption { get; set; }
            public string Value { get; set; }
        }
        public class RateTableRow
        {
            public DecimalRange LoanAmount { get; set; }
            public List<RateRange> Rates { get; set; }
        }

        public class RateRange
        {
            public RateRange() { }
            public RateRange(decimal? min, decimal? max, int minTerm, int maxTerm)
            {
                this.Min = min;
                this.Max = max;
                this.MinTerm = minTerm;
                this.MaxTerm = maxTerm;
            }
            public decimal? Min { get; set; }
            public decimal? Max { get; set; }
            public int MinTerm { get; set; }
            public int MaxTerm { get; set; }
        }
        #endregion
    }

}