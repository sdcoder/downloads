using FirstAgain.Domain.Lookups.FirstLook;
using System.Collections.Generic;
using FirstAgain.Domain.SharedTypes.ContentManagement;

namespace LightStreamWeb.Models.Components
{
    public class RateCalculatorModel
    {
        public RateCalculatorModel()
        {
            DisplayCalculator = true;
            DisplayRateMatch = false;
        }

        public PurposeOfLoanLookup.PurposeOfLoan PurposeOfLoan { get; set; }
        public bool DisplayCalculator { get; set; }
        public bool DisplayRateMatch { get; set; }
        public int? FirstAgainCodeTrackingId { get; set; }

        public FlagLookup.Flag Discount { get; set; }
        public bool IsSuntrustApplication { get; set; }
        public RatesDisclosureContent CustomRateDisclosureContent { get; set; }
        public bool DisplayItsEasyToFindYourRate { get; set; } = false;
        public string RateCalculatorTitle { get; set; }
        public string RateCalcTitle { get; set; }
        public LandingPageContent.RateTableCalculatorType TypeOfCalculator { get; set; }
        public LandingPageContent.RateTableCalculatorDisplayType TypeOfDisplay { get; set; }
    }
}