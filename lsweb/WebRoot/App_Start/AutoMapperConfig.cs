using LightStreamWeb.Models.AccountServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.App_Start
{
    public class AutoMapperConfig
    {
        public static void Initialize()
        {
            AutoMapper.Mapper.Initialize(cfg => 
            {
                cfg.CreateMap<FirstAgain.LoanServicing.SharedTypes.Entities.AmortizationSchedule.PredictReamortizationAvailabilityResponse, FundedAccountModel>();
            });
        }
    }
}