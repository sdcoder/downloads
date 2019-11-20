using FirstAgain.Domain.Common;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.ApplicationStatus
{
    public class FaxCoverPageModel : ApplicationStatusPageModel
    {
        public FaxCoverPageModel(CustomerUserIdDataSet cuidds, LoanOfferDataSet loanOfferDataSet)
            : base(cuidds, loanOfferDataSet)
        {
            Populate();
        }

        public List<VerificationRequest> Documents { get; set; }
        public string AccountNumber { get; set; }
        public string BarcodeValue { get; set; }

        private void Populate()
        {
            var vr = new VerificationRequestsModel();
            vr.Populate(_customerUserIdDataSet, ApplicationId, GetPurposeOfLoan());
            Documents = vr.Documents.Items().ToList();

            AccountNumber = ApplicationId.ToString();
            BarcodeValue = AppIDWithChecksum.IntEncrypt(ApplicationId);
        }
    }
}