using FirstAgain.Common.Web;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using FirstAgain.Domain.SharedTypes.LoanApplication.LoanApplicationPostData;
using FirstAgain.Common.Logging;
using LightStreamWeb.App_State;
using LightStreamWeb.Controllers.Shared;
using LightStreamWeb.Filters;
using LightStreamWeb.Models.Apply;
using LightStreamWeb.Models.PublicSite;
using Ninject;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FirstAgain.Domain.Lookups.FirstLook;

namespace LightStreamWeb.Controllers
{
    [Authorize]
    [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
    public class IncompleteApplicationController : BaseController
    {
        [Inject]
        public IncompleteApplicationController(ICurrentUser user)
            : base(user)
        {
        }

        // Step 1 - disclosures
        // GET: /Apply/Incomplete/
        [HttpGet]
        [RequireApplicationId]
        [InjectLoanApplicationDataSet]
        [RestrictToStatus(ApplicationStatusTypeLookup.ApplicationStatusType.Incomplete)]
        public ActionResult Index(LoanApplicationDataSet lads)
        {
            try
            {
                return View("~/Views/Apply/Incomplete/Index.cshtml", new BaseInProcessAppPageModel(lads));
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
                return new RedirectResult("/customer-sign-in");
            }
        }

        // Step 1 - disclosures
        // POST: /Apply/Incomplete/
        [HttpPost]
        [RequireApplicationId]
        public ActionResult Index(bool? ElectronicDisclosures)
        {
            try
            {
                if (!ElectronicDisclosures.GetValueOrDefault(false))
                {
                    TempData["Alert"] = "'Electronic Disclosures' is required to proceed.";
                    return RedirectToAction("Index");
                }

                return RedirectToAction("SecurityInfo");
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
                return new RedirectResult("/customer-sign-in");
            }
        }

        // Step 2- Customer id and password
        // GET: /Apply/Incomplete/SecurityInfo
        [HttpGet]
        [RequireApplicationId]
        [InjectLoanApplicationDataSet]
        public ActionResult SecurityInfo(LoanApplicationDataSet lads)
        {
            try
            {
                return View("~/Views/Apply/Incomplete/SecurityInfo.cshtml", new BaseInProcessAppPageModel(lads));
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
                return new RedirectResult("/error/general");
            }
        }

        // Step 2- Customer id and password
        // POST: /Apply/Incomplete/SecurityInfo
        [HttpPost]
        [RequireApplicationId]
        [InjectLoanApplicationDataSet(Refresh=true)]
        public ActionResult SecurityInfo(CustomerUserCredentialsPostData userCredentials, LoanApplicationDataSet lads)
        {
            ViewBag.PostData = userCredentials;

            try
            {
                var model = new BaseInProcessAppPageModel(lads);
                if (!ModelState.IsValid)
                {
                    return View("~/Views/Apply/Incomplete/SecurityInfo.cshtml", model);
                }

                // try to create the user id and complete the application
                var result = model.CompleteLoanApplication(userCredentials, WebUser);
                if (result == CompleteLoanApplicationResultEnum.CustomerUserIdExists)
                {
                    TempData["Alert"] = "That user id is already in use. Please try again";
                    return View("~/Views/Apply/Incomplete/SecurityInfo.cshtml", model);
                }

                // if complete the process.....
                return RedirectToAction("ThankYou");
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
                return new RedirectResult("/error/general");
            }

        }

        // Step 3- Thank you page
        // GET: /Apply/Incomplete/ThankYou
        [HttpGet]
        [RequireApplicationId]
        [InjectLoanApplicationDataSet(Refresh = true)]
        public ActionResult ThankYou(LoanApplicationDataSet lads)
        {
            try
            {
                FirstAgain.Web.Cookie.CookieUtility.ExpireApplicationCookies();

                return View("~/Views/Apply/Incomplete/ThankYou.cshtml", new BaseInProcessAppPageModel(lads));
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
                return new RedirectResult("/error/general");
            }
        }
    }
}
