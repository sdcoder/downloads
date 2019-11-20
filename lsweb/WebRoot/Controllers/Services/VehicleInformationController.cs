using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using LightStreamWeb.Models.VehicleInformation;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.SharedTypes.LoanApplication.VehicleInformation;
using FirstAgain.Common.Logging;
using LightStreamWeb.ServerState;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using LightStreamWeb.App_State;
using LightStreamWeb.Filters;
using FirstAgain.Domain.SharedTypes.Customer;
using LightStreamWeb.Controllers.Shared;
using FirstAgain.LoanServicing.SharedTypes;

namespace LightStreamWeb.Controllers
{
    [Obsolete("secured auto is going way, but there are still pipeline apps", false)]
    public class VehicleInformationController : BaseController
    {
        [Ninject.Inject]
        public VehicleInformationController(ICurrentUser user)
            : base(user)
        {
        }

        //
        // GET: /VehicleInformation/Validate
        [HttpGet]
        public ActionResult Validate(string vin)
        {
            if (string.IsNullOrEmpty(vin))
            {
                return new EmptyResult();
            }
            return new JsonResult()
            {
                Data =  VehicleIdentificationNumber.ValidateChecksum(vin.ToUpper()),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        //
        // GET or POST: /VehicleInformation/Search
        public ActionResult Search(string vin, int? mileage, int? applicationId)
        {
            DateTime fundingDate = DateTime.Now;
            if (applicationId.HasValue && SessionUtility.AccountServicesData != null && SessionUtility.AccountServicesData.LoanMaster.Any(a => a.ApplicationId == applicationId))
            {
                var loanMaster = SessionUtility.AccountServicesData.LoanMaster.First(a => a.ApplicationId == applicationId);
                fundingDate = loanMaster.FundingDate;
            }

            /*SYSR007
                1.	Timeouts will be treated as a no match found condition.   
                2.	When no response has been returned within 1 minute of the query, it will be treated as no match condition.  
                3.	System down response will be treated as a no match found condition. 
             * 
             * so, treat all errors as warnings and let the user proceed
             */
            List<VehicleData> vehicles = null;
            try
            {
                // old VIN, return empty result
                if (string.IsNullOrEmpty(vin) || vin.Length < 17)
                {
                    vehicles = new List<VehicleData>();
                }
                else
                {
                    vehicles = DomainServiceLoanApplicationOperations.GetVehicleDataByVIN(vin.ToUpper(), mileage.GetValueOrDefault(), fundingDate);
                }
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteWarning(ex);
            }

            // return Json result
            return new JsonResult()
            {
                Data = vehicles,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }

        // POST: /VehicleInformation/VeracodeTest_Save
        [HttpPost]
        public ActionResult VeracodeTest_Save(
            [Bind(Include = "VIN,Make,Model,Provider,Description,Value,Year,Mileage,PaidToDealer,ApplicationId,UserCertified0,UserCertified1")]SaveVehicleInformationModel vehicle
            )
        {
            throw new NotImplementedException();
        }

        // POST: /VehicleInformation/Save
        [HttpPost]
        [InjectCustomerUserIdDataSet]
        [InjectAccountServicesDataSet]
        public ActionResult Save(
                [Bind(Include = "VIN,Make,Model,Provider,Description,Value,Year,Mileage,PaidToDealer,ApplicationId,UserCertified0,UserCertified1")]SaveVehicleInformationModel vehicle, 
                CustomerUserIdDataSet customerData, 
                AccountServicesDataSet accountServicesData)
        {
            return new JsonResult()
            {
                Data = vehicle.Save(customerData, WebUser, accountServicesData)
            };
        }

        // POST: /VehicleInformation/SaveCertified
        [HttpPost]
        [InjectCustomerUserIdDataSet]
        [InjectAccountServicesDataSet]
        public ActionResult SaveCertified(
            [Bind(Include = "VIN,Make,Model,Provider,Description,Value,Year,Mileage,PaidToDealer,ApplicationId,UserCertified0,UserCertified1")]SaveVehicleInformationModel vehicle, 
            CustomerUserIdDataSet customerData, 
            AccountServicesDataSet accountServicesData)
        {
            if (accountServicesData == null)
            {
                return new JsonResult
                {
                    Data = new SaveVehicleInformationModel.SaveVehicleInformationResult { Success = false }
                };
            }
            return new JsonResult()
            {
                Data = vehicle.Save(customerData, WebUser, accountServicesData)
            };
        }
    }
}
