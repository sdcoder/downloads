using System;
using System.Web.Mvc;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Common.Logging;
using LightStreamWeb.App_State;
using LightStreamWeb.Filters;
using LightStreamWeb.Controllers.Shared;
using FirstAgain.Domain.SharedTypes.LoanApplication.LoanApplicationPostData;
using LightStreamWeb.Models.ApplicationStatus;
using LightStreamWeb.ServerState;

namespace LightStreamWeb.Controllers.Services
{
    public class SubjectPropertyAddressController : BaseController
    {
        [Ninject.Inject]
        public SubjectPropertyAddressController(ICurrentUser user)
            : base(user)
        {
        }

        // POST: /SubjectPropertyAddress/Save
        [HttpPost]
        public ActionResult Save(AddressPostData address, bool sameAsResidenceAddress)
        {
            bool success = false;

            try
            {
                int applicationId = WebUser.ApplicationId.Value;

                DomainServiceLoanApplicationOperations.UpdateImprovedPropertyAddress(applicationId, address, sameAsResidenceAddress, isVerificationUpload: true);

                SessionUtility.ReloadApplicationData(applicationId);
                SessionUtility.RefreshAccountInfo();

                success = true;
            }
            catch (Exception e)
            {
                LightStreamLogger.WriteError(e);
            }

            return new JsonNetResult()
            {
                Data = new
                {
                    Success = success,
                    AddressLine1 = VerificationRequestsModel.SubjectPropertyAddressModel.GetFullAddressLine1(address),
                    AddressLine2 = VerificationRequestsModel.SubjectPropertyAddressModel.GetFullAddressLine2(address)
                }
            };    
        }        
    }
}