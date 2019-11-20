using FirstAgain.Common.Web;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Contracts.MessageContracts.CustomerOperations;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.AccountServices
{
    public class AccountServicesVINPageModel : AccountServicesPageModel
    {
        public int ApplicationId { get; set; }

        public decimal RequestedLoanAmount { get; set; }
        public string LoanNumber { get; set; }
        public List<string> ApplicantNames { get; set; }
        public string VIN { get; set; }
        public decimal? Mileage { get; set; }
        public decimal? TransactionAmount { get; set; }
        public string TransactionDescription { get; set; }
        public int? Year { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public bool VINEntryRequired { get; set; }

        [JsonIgnore]
        public ApplicationStatus.VerificationRequestsModel VerificationRequests { get; protected set; }


        public AccountServicesVINPageModel(GetAccountInfoResponse accountInfo, CustomerUserIdDataSet customerData, string ctx) : base(accountInfo)
        {
            ApplicationId = WebSecurityUtility.Descramble(ctx);
            EnableFileUpload = true;

            var application = customerData.Applications.FirstOrDefault(a => a.ApplicationId == ApplicationId);
            var purposeOfLoan = application.GetLoanContractRows()[0].LoanOfferRow.LoanTermsRequestRow.PurposeOfLoan;

            VINEntryRequired = application.VINEntryRequired;
            if (VINEntryRequired)
            {
                RequestedLoanAmount = application.GetLoanContractRows()[0].LoanOfferRow.LoanTermsRequestRow.AmountMinusFees;
                LoanNumber = application.ApplicationId.ToString();
                ApplicantNames = application.Applicants.Select(a => a.Name).ToList();
                if (application.GetApplicationCollateralRows().Count() > 0)
                {
                    var applicationCollateralRow = application.GetApplicationCollateralRows()[0];

                    VIN = applicationCollateralRow.VIN;
                    Mileage = applicationCollateralRow.IsMileageNull() ? 0 : applicationCollateralRow.Mileage;
                    TransactionAmount = applicationCollateralRow.IsTransactionAmountNull() ? 0 : applicationCollateralRow.TransactionAmount;
                    Year = applicationCollateralRow.IsYearNull() ? (int?)null : applicationCollateralRow.Year;
                    Make = applicationCollateralRow.IsMakeNull() ? null : applicationCollateralRow.Make;
                    Model = applicationCollateralRow.IsModelNull() ? null : applicationCollateralRow.Model;
                }

                switch (purposeOfLoan)
                {
                    case PurposeOfLoanLookup.PurposeOfLoan.NewAutoPurchaseSecured:
                        TransactionDescription = "Total Amount of Loan Proceeds Paid To Dealer";
                        break;
                    case PurposeOfLoanLookup.PurposeOfLoan.UsedAutoPurchaseSecured:
                        TransactionDescription = "Total Amount of Loan Proceeds Paid To Dealer";
                        break;
                    case PurposeOfLoanLookup.PurposeOfLoan.PrivatePartyPurchaseSecured:
                        TransactionDescription = "Selling Price of Vehicle";
                        break;
                    case PurposeOfLoanLookup.PurposeOfLoan.AutoRefinancingSecured:
                        TransactionDescription = "Auto Loan Payoff Amount";
                        break;
                    case PurposeOfLoanLookup.PurposeOfLoan.LeaseBuyOutSecured:
                        TransactionDescription = "Selling Price of Vehicle";
                        break;
                }
            }

            // populate verification requests
            VerificationRequests = new ApplicationStatus.VerificationRequestsModel();
            VerificationRequests.Populate(
                    customerData,
                    ApplicationId,
                    purposeOfLoan);
        }

        public string ToJSON()
        {
            // return Newtonsoft.Json.JsonConvert.SerializeObject(this);
            //Need to escape apostrophe in Name, so can populate JSON obj Bug 17674
            return JsonConvert.SerializeObject(this).Replace("'","\\'"); 
        }


    }
}