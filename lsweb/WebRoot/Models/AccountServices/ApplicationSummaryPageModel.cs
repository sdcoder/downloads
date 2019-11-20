using FirstAgain.Domain.SharedTypes.Customer;

namespace LightStreamWeb.Models.AccountServices
{
    public class ApplicationSummaryPageModel
    {
        public int ApplicationId { get; set; }
        public string LoanAmount { get; set; }
        public string FundingDate { get; set; }

        public static ApplicationSummaryPageModel Populate(CustomerUserIdDataSet.ApplicationRow applicationRow)
        {
            return new ApplicationSummaryPageModel
            {
                ApplicationId = applicationRow.ApplicationId,
                LoanAmount = applicationRow.LoanAmount.ToString("C"),
                FundingDate = applicationRow.FundingDate.HasValue ? applicationRow.FundingDate.Value.ToString("MM/dd/yy") : "N/A"
            };
        }
    }
}