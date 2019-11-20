using FirstAgain.Common.Logging;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Client;
using LightStreamWeb.Models.SignIn;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;

namespace LightStreamWeb.Controllers
{
    [SessionState(SessionStateBehavior.Disabled)]
    public class PingController : Controller
    {
        // GET: Ping
        public ActionResult Index()
        {
            try
            {
                if (MaintenanceConfiguration.IsInMaintenanceMode)
                {
                    return new ContentResult()
                    {
                        Content = "IsInMaintenanceMode"
                    };
                }

                DomainServiceLoanApplicationOperations.GetApplicationFlagStatus(0, FlagLookup.Flag.NotSelected);
                return new ContentResult()
                {
                    Content = "OK"
                };
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }

        // GET: Ping/Login
        public ActionResult Login()
        {
            // return ok if in maintenance mode
            if (MaintenanceConfiguration.IsInMaintenanceMode)
            {
                return new ContentResult()
                {
                    Content = "IsInMaintenanceMode"
                };
            }

            // return ok if not configured
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["PingUserId"]) || string.IsNullOrEmpty(ConfigurationManager.AppSettings["PingUserPassword"]))
            {
                return new HttpStatusCodeResult(400, "Login Ping is not configured");
            }

            var model = new SignInModel()
            {
                UserId = ConfigurationManager.AppSettings["PingUserId"],
                UserPassword = ConfigurationManager.AppSettings["PingUserPassword"],
            };

            var result = model.Login(new FirstAgain.Domain.ServiceModel.Client.FirstAgainMembershipProvider());
            if (result == LightStreamWeb.Models.SignIn.SignInModel.LoginResult.Success)
            {
                var info = model.GetAccountStatus(model.UserId);
                return new ContentResult()
                {
                    Content = "OK"
                };
            }

            return new HttpStatusCodeResult(400, result.ToString());
        }

        // GET: Ping/AllServices
        public ActionResult AllServices()
        {
            try
            {
                if (MaintenanceConfiguration.IsInMaintenanceMode)
                {
                    return new ContentResult()
                    {
                        Content = "IsInMaintenanceMode"
                    };
                }

                DomainServiceUtilityOperations.PingAllServices(true);

                return new ContentResult()
                {
                    Content = "OK"
                };
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }

        }

        // GET: Ping/RateCheck
        public ActionResult RateCheck()
        {
            try
            {
                var rates = DomainServiceInterestRateOperations.GetCachedFixedInterestRates(
                                StateLookup.State.NotSelected,
                                StateLookup.State.NotSelected,
                                PaymentTypeLookup.PaymentType.AutoPay,
                                PurposeOfLoanLookup.PurposeOfLoan.NewAutoPurchase,
                                1);

                var rateInfo = rates.GetRateInfo(36, 25000.0m);
                if (rateInfo != null && rateInfo.Rate > 0)
                {
                    return new ContentResult()
                    {
                        Content = rateInfo.Rate.ToString()
                    };
                }

            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
                return new HttpStatusCodeResult(500, ex.Message);
            }


            return new HttpStatusCodeResult(500, "rate not found");
        }
    }
}