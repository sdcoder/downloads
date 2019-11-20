using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.PreFunding
{
    public class RescheduleFundingDateModel : PreFundingPageModel
    {
        public RescheduleFundingDateModel(CustomerUserIdDataSet cuidds, LoanOfferDataSet loanOfferDataSet)
            : base(cuidds, loanOfferDataSet) {}

    }
}