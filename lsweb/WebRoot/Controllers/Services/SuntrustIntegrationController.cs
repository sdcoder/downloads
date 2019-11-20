using FirstAgain.Common;
using FirstAgain.Common.Caching;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.ServiceModel.Contracts.MessageContracts.LoanApplicationOperations;
using FirstAgain.Common.Logging;
using LightStreamWeb.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using FirstAgain.Domain.SharedTypes.LoanApplication;

namespace LightStreamWeb.Controllers.Services
{
    [SessionState(SessionStateBehavior.Disabled)]
    public class SuntrustIntegrationController : Controller
    {
        //
        // GET: /Services/ValidateOfficerId/
        [HttpGet]
        [OutputCache(Duration=60, VaryByParam="*")]
        public ActionResult ValidateOfficerId(string id)
        {
            var officerIds = MachineCache.Get<List<string>>("OfficerIds");
            Guard.AgainstNull<List<string>>(officerIds, "OfficerIds");

            bool success = officerIds != null && officerIds.Contains(id);

            return new JsonResult()
            {
                JsonRequestBehavior = System.Web.Mvc.JsonRequestBehavior.AllowGet, 
                Data = new 
                {
                    Success = success
                }
            };
        }

        //
        // GET: /Services/ValidateBranchCostCenter/
        [HttpGet]
        [OutputCache(Duration = 60, VaryByParam = "*")]
        public ActionResult ValidateBranchCostCenter(string id)
        {
            var costCenters = MachineCache.Get<List<string>>("BranchCostCenters");
            Guard.AgainstNull<List<string>>(costCenters, "BranchCostCenters");

            bool success = costCenters != null && costCenters.Contains(id);

            return new JsonResult()
            {
                JsonRequestBehavior = System.Web.Mvc.JsonRequestBehavior.AllowGet, 
                Data = new 
                {
                    Success = success
                }
            };
        }

        //
        // GET / POST: /Services/ValidateEmployeeId/
        [OutputCache(Duration = 60, VaryByParam = "*")]
        public ActionResult ValidateEmployeeId(string id)
        {
            var result = EmployeeVerificationResultLookup.EmployeeVerificationResult.Error;
            try
            {
                int employeeId;
                if (int.TryParse(id, out employeeId))
                {
                    result = DomainServiceLoanApplicationOperations.ValidateBankEmployeeId(employeeId);
                }
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
            }

            return new JsonNetResult()
            {
                JsonRequestBehavior = System.Web.Mvc.JsonRequestBehavior.AllowGet,
                Data = new
                {
                    Success = result == EmployeeVerificationResultLookup.EmployeeVerificationResult.NotFound || result == EmployeeVerificationResultLookup.EmployeeVerificationResult.Verified,
                    Found = result  == EmployeeVerificationResultLookup.EmployeeVerificationResult.Verified,
                    APIResult = result,
                    EmployeeId = id
                }
            }; 
        }
        
    }
}
