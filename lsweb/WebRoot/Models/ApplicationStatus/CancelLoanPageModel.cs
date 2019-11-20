using FirstAgain.Common;
using FirstAgain.Common.Extensions;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.ApplicationStatus
{
    public class CancelLoanPageModel : ApplicationStatusPageModel
    {
        public CancelLoanPageModel(CustomerUserIdDataSet cuidds)
            : base(cuidds) 
        {
            BodyClass = "pre-funding";
        }

        internal void CancelApprovedLoan(int applicationId, FirstAgain.Domain.Lookups.FirstLook.WithdrawReasonTypeLookup.WithdrawReasonType withdrawReason, string withdrawReasonDescription)
        {
            var applicationRow = _customerUserIdDataSet.Application.FirstOrDefault(a => a.ApplicationId == applicationId);

            if (applicationRow == null)
            {
                throw new ArgumentNullException(string.Format("applicationRow not found for applicationId {0}", applicationId));
            }
            if (applicationRow.ApplicationStatusType.IsNoneOf(ApplicationStatusTypeLookup.ApplicationStatusType.Approved, ApplicationStatusTypeLookup.ApplicationStatusType.ApprovedNLTR))
            {
                throw new ArgumentNullException(string.Format("ApplicationStatus of {0} was unexpected for applicationId {1}", applicationRow.ApplicationStatusType, applicationId));
            }
            DomainServiceLoanApplicationOperations.ExpireApplication(applicationRow, withdrawReason, withdrawReasonDescription, null);
        }
    }
}