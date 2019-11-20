using FirstAgain.Common;
using FirstAgain.Domain.Lookups.FirstLook;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Diagnostics.CodeAnalysis;

namespace LightStreamWeb.Models.Rates
{
    [ExcludeFromCodeCoverage]
    public class WidgetModel
    {
        #region inputs
        [JsonConverter(typeof(StringEnumConverter))]
        public PurposeOfLoanLookup.PurposeOfLoan PurposeOfLoan { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PaymentTypeLookup.PaymentType? PaymentMethod { get; set; }

        public decimal? LoanAmount { get; set; }

        public int? LoanTermMonths { get; set; }

        public int? FirstAgainCodeTrackingId { get; set; }
        #endregion

        #region outputs
        public DecimalRange EstimatedMonthlyPayment { get; set; }
        public DecimalRange Rate { get; set; }
        public DecimalRange LoanAmounts { get; set; }
        #endregion
    }
}