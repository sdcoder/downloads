using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.SharedTypes.InterestRate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.Shared
{
    public class CounterOfferTermsModel
    {
        public LoanTermsModel Requested { get; set; }
        public LoanTermsModel Offered { get; set; }

        public static CounterOfferTermsModel Populate(CustomerUserIdDataSet customerData, int applicationId)
        {
            var terms = new CounterOfferTermsModel()
            {
                Requested = new LoanTermsModel(),
                Offered = new LoanTermsModel()
            };

            // Select the most recent declined and approved LTRs            
            CustomerUserIdDataSet.LoanTermsRequestRow offered = customerData.LoanTermsRequest.GetCurrentApprovedOrCounterLoanTerms(applicationId);
            InterestRates rates = DomainServiceInterestRateOperations.GetApplicationInterestRates(applicationId);
            // per PBI 1887, counter displays a single rate, not a range
            terms.Offered.PopulateSingleRate(offered, rates);

            CustomerUserIdDataSet.LoanTermsRequestRow declined = customerData.LoanTermsRequest.GetLastDeclinedRequestedLoanTerms(applicationId);
            terms.Requested.Populate(declined);

            return terms;
        }
    }
}