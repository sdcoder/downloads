using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Contracts.MessageContracts.LoanApplicationOperations;
using LightStreamWeb.Models.Middleware;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.Apply
{
    public class ApplyFromReferralModel : GetTeammateReferralResponse
    {
        public ApplyFromReferralModel(GetTeammateReferralResponse response)
        {
            this.TeammateEmployeeId = response.TeammateEmployeeId;
            this.BranchCostCenter = response.BranchCostCenter;
            this.TeammateEmailAddress = response.TeammateEmailAddress;
            this.TeammateIsContractorOrTemporary = response.TeammateIsContractorOrTemporary;
            this.EmployeeVerificationResult = response.EmployeeVerificationResult;
            this.EmailAddress = response.EmailAddress;
            this.SocialSecurityNumber = response.SocialSecurityNumber;
            this.PurposeOfLoan = response.PurposeOfLoan;
            this.AddressLine = response.AddressLine;
            this.SecondaryUnitType = response.SecondaryUnitType;
            this.SecondaryUnitValue = response.SecondaryUnitValue;
            this.City = response.City;
            this.State = response.State;
            this.ZipCode = response.ZipCode;
            this.ApplicationType = response.ApplicationType;
            this.TeammateReferralId = response.TeammateReferralId;
            this.FirstAgainCodeTrackingId = response.FirstAgainCodeTrackingId;
            this.LoanAmount = response.LoanAmount;
            this.BranchEmployeePhoneNumber = response.BranchEmployeePhoneNumber;
        }

        public string CdnBaseUrl { get
            {
                return App_Start.NinjectWebCommon.Bootstrapper.Kernel.Get<IAppSettings>().CdnBaseUrl;
            } }

        public FlagLookup.Flag? Discount { get; set; }
    }
}