using System;
using System.Collections.Generic;
using System.Linq;
using FirstAgain.Domain.Lookups.FirstLook;

namespace LightStreamWeb.Shared.Helpers
{
    public class PurposeOfLoanValue
    {
        public PurposeOfLoanLookup.PurposeOfLoan Enumeration { get; set; }

        public String Caption { get; set; }

        public bool IsHidden { get; set; }
    }

    public static class PurposeOfLoanHelper
    {
        public static IEnumerable<PurposeOfLoanValue> GetFilteredList(PurposeOfLoanLookup.FilterType filter = PurposeOfLoanLookup.FilterType.Public)
        {
            return PurposeOfLoanLookup.GetFilteredList(filter).Select(a => 
                new PurposeOfLoanValue { Enumeration = a.Enumeration, Caption = GetCaption(a) });
        }

        public static PurposeOfLoanLookup.PurposeOfLoan GetEnumerationFromCaption(string caption)
        {
            switch(caption)
            {
                case "Other":
                    return PurposeOfLoanLookup.PurposeOfLoan.Other;
                case "Please select a loan purpose":
                    return PurposeOfLoanLookup.PurposeOfLoan.NotSelected;
                default:
                    return PurposeOfLoanLookup.GetEnumerationFromCaption(caption);
            }
        }

        public static string GetCaption(PurposeOfLoanLookup.PurposeOfLoan purpose)
        {
            return GetCaptionOverride(purpose) ?? PurposeOfLoanLookup.GetCaption(purpose);
        }

        public static string GetCaption(PurposeOfLoanLookup.Value purpose)
        {
            return GetCaptionOverride(purpose.Enumeration) ?? purpose.Caption;
        }

        private static string GetCaptionOverride(PurposeOfLoanLookup.PurposeOfLoan purpose)
        {
            switch (purpose)
            {
                case PurposeOfLoanLookup.PurposeOfLoan.NotSelected:
                    return "Please select a loan purpose";
                case PurposeOfLoanLookup.PurposeOfLoan.Other:
                    return "Other";
                default:
                    return null;
            }
        }
    }
}