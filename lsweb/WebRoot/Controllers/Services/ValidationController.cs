using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using FirstAgain.Common.Extensions;
using FirstAgain.Common.Logging;
using LightStream.Service.CustomerDataVerification.Client;
using LightStream.Service.CustomerDataVerification.Entities;
using LightStream.Service.CustomerDataVerification.Enums;
using LightStreamWeb.App_State;
using LightStreamWeb.Controllers.Shared;
using LightStreamWeb.Helpers;
using LightStreamWeb.Models.Middleware;
using Ninject;

namespace LightStreamWeb.Controllers.Services
{
    public class ValidationController : BaseController
    {
        IVersionInfo VersionInfo;
        IEndpoints EndPoints;
        IAppSettings AppSettings;

        [Inject]
        public ValidationController(ICurrentUser user, IVersionInfo versionInfo, IEndpoints endPoints, IAppSettings appSettings) : base(user)
        {
            VersionInfo = versionInfo;
            EndPoints = endPoints;
            AppSettings = appSettings;
        }

        // GET: Validation
        [HttpPost]
        [Route("services/validate-email")]
        public ActionResult ValidateEmail(string email)
        {
            try
            {
                var request = new EmailAddressVerificationRequest
                {
                    EmailAddress = email,
                    SourceInfo = new NameVersionInfo { Name = VersionInfo.Name, Version = VersionInfo.Version },
                };

                var client = new EmailAddressVerificationClient(EndPoints.CustomerDataVerificationUrl,
                                                                cancelTimeoutMilliseconds: AppSettings.CustomerDataVerificationClientTimeoutMilliseconds);

                var response = client.ValidateEmailAddress(request);

                return new JsonDotNetResult()
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new
                    {
                        isEmailValid = response.CertaintyType == CertaintyTypes.Verified,
                        isUnknown = response.CertaintyType.IsOneOf(CertaintyTypes.Unknown, CertaintyTypes.Timeout, CertaintyTypes.AcceptAll, CertaintyTypes.RelayDenied),
                        isSuccessful = response.IsSuccessful
                    }
                };
            }
            catch (Exception ex)
            {
                if (ex is TaskCanceledException)
                    LightStreamLogger.WriteWarning(ex);
                else
                    LightStreamLogger.WriteError(ex);

                return new JsonDotNetResult()
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { isEmailValid = false, isSuccessful = false }
                };
            }
        }
    }
}