using FirstAgain.Common;
using FirstAgain.Common.Logging;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.LoanServicing.SharedTypes;
using LightStreamWeb.App_State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.VehicleInformation
{
    public class SaveVehicleInformationModel
    {
        public string VIN { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public AppraisalTypeLookup.AppraisalType Provider { get; set; }
        public string Description { get; set; }
        public int Value { get; set; }
        public int Year { get; set; }
        public int Mileage { get; set; }
        public decimal PaidToDealer { get; set; }
        public int? ApplicationId { get; set; }
        public bool UserCertified0 { get; set; }
        public bool UserCertified1 { get; set; }

        public SaveVehicleInformationResult Save(CustomerUserIdDataSet customerData, 
                                                ICurrentUser webUser, 
                                                AccountServicesDataSet accountServicesDataSet = null)
        {
            SaveVehicleInformationResult result = new SaveVehicleInformationResult();

            try
            {
                if (!ApplicationId.HasValue)
                {
                    LightStreamLogger.WriteWarning("ApplicationId not supplied to VehicleInformation/Save");
                }
                int applicationId = ApplicationId.GetValueOrDefault(webUser.ApplicationId.GetValueOrDefault());

                if (customerData.Applications.Any(a => a.ApplicationId == applicationId))
                {
                    var application = customerData.Applications.First(a => a.ApplicationId == applicationId);
                    var primary = application.Applicants.First(app => app.ApplicantType == ApplicantTypeLookup.ApplicantType.Primary);

                    // correct short VIN numbers so they can be processed by third party services
                    if (VIN.Length < 10)
                    {
                        VIN = VIN.PadLeft(10, '0');
                    }

                    if (accountServicesDataSet != null && accountServicesDataSet.LoanMaster.Any(a => a.ApplicationId == applicationId))
                    {
                        var loanMaster = accountServicesDataSet.LoanMaster.First(a => a.ApplicationId == applicationId);

                        DomainServiceLoanApplicationOperations.SaveConsumerEnteredVehicleData(applicationId,
                                                                                                VIN.ToUpper(),
                                                                                                Year,
                                                                                                Make,
                                                                                                Model,
                                                                                                Description,
                                                                                                Mileage,
                                                                                                PaidToDealer,
                                                                                                Value,
                                                                                                Provider,
                                                                                                loanMaster.FundingDate,
                                                                                                primary.GetApplicantPostalAddressRows().First().State
                                                                                                );
                    }
                    else
                    {
                        result.AutoSatisfied = DomainServiceLoanApplicationOperations.SaveConsumerEnteredNonCertifiedVehicleData(applicationId,
                                                                        VIN.ToUpper(),
                                                                        Year,
                                                                        Make,
                                                                        Model,
                                                                        Description,
                                                                        Mileage,
                                                                        PaidToDealer,
                                                                        Value,
                                                                        Provider,
                                                                        primary.GetApplicantPostalAddressRows().First().State
                                                                        );

                    }
                    result.Success = true;
                }
                else
                {
                    LightStreamLogger.WriteWarning(string.Format("Application not found for applicationId {0}", applicationId));
                }
            }
            catch (Exception ex)
            {
                ExceptionUtility.AddObjectStateToExceptionData(ex, this);
                LightStreamLogger.WriteError(ex);
            }

            return result;
        }

        public class SaveVehicleInformationResult
        {
            public bool AutoSatisfied { get; set; }
            public bool Success { get; set; }
        }
    }
}