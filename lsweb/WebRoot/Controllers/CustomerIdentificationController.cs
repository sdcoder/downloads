using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.SharedTypes.LoanApplication.CustomerIdentification;
using FirstAgain.Domain.ServiceModel.Client;
using LightStreamWeb.ServerState;

using LightStreamWeb.Controllers.Shared;

namespace LightStreamWeb.Controllers.Services
{
    public class CustomerIdentificationController : BaseAccountController
    {
        public CustomerIdentificationController()
            : base(null) { }

        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public JsonResult GetFirstQuestion(ApplicantTypeLookup.ApplicantType? applicantType)
        {
            if (!applicantType.HasValue || (int)applicantType == 0 || applicantType.Value == ApplicantTypeLookup.ApplicantType.NotSelected)
            {
                applicantType = SessionUtility.CustomerIdentificationApplicant.GetValueOrDefault(ApplicantTypeLookup.ApplicantType.Primary);
            }

            var ciResponse = DomainServiceLoanApplicationOperations.GetFirstCustomerIdentificationQuestion(Application.ApplicationId, applicantType.Value);

            if (ciResponse != null)
            {
                handleCustomerIdentificationResponse(ciResponse);
            }

            return Json(ciResponse, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public JsonResult GetNextQuestion(CustomerIdentificationQuestionResponse response)
        {
            CustomerIdentificationRequest request = new CustomerIdentificationRequest(response);
            CustomerIdentificationResponse ciResponse = DomainServiceLoanApplicationOperations.GetNextCustomerIdentificationQuestion(request);
            handleCustomerIdentificationResponse(ciResponse);
            return Json(ciResponse);
        }

        private void handleCustomerIdentificationResponse(CustomerIdentificationResponse response)
        {
            // Set state of verification item to display as submitted so the test cannot be re-started
            CustomerUserIdDataSet.VerificationRequestStatusRow vrsRow = Account.VerificationRequest
                                                                    .Single(vr => vr.VerificationType == VerificationTypeLookup.VerificationType.IdentityVerification
                                                                                && vr.ApplicationId == response.ApplicationId
                                                                                && vr.ApplicantType == response.ApplicantType)
                                                                    .NewRelatedVerificationRequestStatusRow();
            vrsRow.VerificationRequestStatus = VerificationRequestStatusLookup.VerificationRequestStatus.Submitted;
            Account.VerificationRequestStatus.AddVerificationRequestStatusRow(vrsRow);
        }
    }
}
