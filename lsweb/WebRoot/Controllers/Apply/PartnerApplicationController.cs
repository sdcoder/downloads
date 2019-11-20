using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using FirstAgain.Domain.SharedTypes.LoanApplication.LoanApplicationPostData;
using FirstAgain.Common.Logging;
using LightStreamWeb.App_State;
using LightStreamWeb.Controllers.Shared;
using LightStreamWeb.Filters;
using LightStreamWeb.Models.Apply;
using Newtonsoft.Json;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LightStreamWeb.Controllers
{
    [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
    public class PartnerApplicationController : BaseApplicationController
    {
        [Inject]
        public PartnerApplicationController(ICurrentUser user)
            : base(user)
        {
        }

        [Authorize]
        [RequireApplicationId]
        [InjectLoanApplicationDataSet]
        [InjectCustomerUserIdDataSet]
        public ActionResult LoadApp(LoanApplicationDataSet lads, CustomerUserIdDataSet customerData)
        {
            try
            {
                var model = new InquiryApplicationModel();

                if (customerData != null && lads != null)
                {
                    if (!customerData.Application.Any(a => a.ApplicationId == lads.Application.First().ApplicationId))
                    {
                        return new HttpUnauthorizedResult();
                    }
                }

                model.Populate(lads, customerData);
                model.LoanAmount = WebUser.LoanAmount ?? model.LoanAmount;
                model.LoanTermMonths = WebUser.LoanTerm.HasValue ? (short)WebUser.LoanTerm.Value : (short)0;

                return new JsonNetResult()
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new
                    {
                        Success = true,
                        LoanApp = model.HideSSNs()
                    }
                };
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
                return new JsonNetResult()
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new
                    {
                        Success = false,
                        ErrorMessage = ex.Message
                    }
                };
            }
        }

        // GET: /Apply/Partner
        [HttpGet]
        [RequireApplicationId]
        [InjectLoanApplicationDataSet]
        [RestrictToStatus(ApplicationStatusTypeLookup.ApplicationStatusType.Inquiry)]
        public ActionResult Index(LoanApplicationDataSet lads)
        {
            try
            {
                WebUser.IsGenericPostingPartner = PostingPartnerLookup.GetFilteredBindingSource(PostingPartnerLookup.FilterType.Generic).GetValue(lads.Application[0].GetApplicationDetail().PostingPartner) != null;
                return View("~/Views/Apply/Partner/Index.cshtml", new PartnerApplicationPageModel(lads));
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
                return new RedirectResult("/error/general");
            }
        }

        // GET: /Apply/Partner/Submit
        [HttpGet]
        [RequireApplicationId]
        public ActionResult Submit()
        {
            try
            {
                return View("~/Views/Apply/Partner/Submit.cshtml", new BaseInProcessAppPageModel(null));
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
                return new RedirectResult("/error/general");
            }
        }

        // GET: /Apply/Partner/ThankYou
        [HttpGet]
        [RequireApplicationId]
        [InjectLoanApplicationDataSet]
        public ActionResult ThankYou(LoanApplicationDataSet lads)
        {
            try
            {
                FirstAgain.Web.Cookie.CookieUtility.ExpireApplicationCookies();

                return View("~/Views/Apply/Partner/ThankYou.cshtml", new PartnerApplicationPageModel(lads));
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
                return new RedirectResult("/error/general");
            }
        }

        [HttpPost]
        [RequireApplicationId]
        [InjectLoanApplicationDataSet]
        [RestrictToStatus(ApplicationStatusTypeLookup.ApplicationStatusType.Inquiry)]
        public ActionResult Submit(InquiryApplicationModel model, LoanApplicationDataSet lads)
        {
            QueueApplicationPostModel.QueueApplicationPostResult result = null;

            try
            {
                result = QueueApplicationPostModel.SetDefaultsAndValidate(model);
                if (result.Success)
                {
                    result = QueueApplicationPostModel.ValidateSecurityInfo(model);
                    if (result.Success)
                    {
                        // Generic Partner specific
                        result = model.SubmitGenericPartnerApp(lads, this.WebUser);
                    }
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = "We're sorry, but there was an error submitting your application.";

                LogApplicationPostData(model);
                LightStreamLogger.WriteError(ex);
            }

            return new JsonResult()
            {
                Data = result
            };

        }

        #region Error navigation routes
        public override ActionResult ZipCode()
        {
            return RedirectToAction("Index");
        }

        public override ActionResult ElectronicDisclosures()
        {
            return RedirectToAction("Confirm");
        }

        public override ActionResult LoanInfo()
        {
            return RedirectToAction("Index");
        }

        public override ActionResult BasicInfo()
        {
            return RedirectToAction("Index");
        }

        public override ActionResult GoToStep(int step)
        {
            switch (step)
            {
                case 4:
                    return RedirectToAction("Confirm");
                case 3:
                    return RedirectToAction("SecurityInfo");
                case 2:
                    return RedirectToAction("PersonalInfo");
                default:
                    return RedirectToAction("Index");
            }
        }

        #endregion

        [RequireApplicationId]
        [RestrictToStatus(ApplicationStatusTypeLookup.ApplicationStatusType.Inquiry)]
        public override ActionResult SecurityInfo()
        {
            try
            {
                return View("~/Views/Apply/Partner/SecurityInfo.cshtml", new PartnerApplicationPageModel(null));
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
                return new RedirectResult("/error/general");
            }
        }

        [RequireApplicationId]
        [RestrictToStatus(ApplicationStatusTypeLookup.ApplicationStatusType.Inquiry)]
        public override ActionResult PersonalInfo()
        {
            try
            {
                return View("~/Views/Apply/Partner/PersonalInfo.cshtml", new PartnerApplicationPageModel(null));
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
                return new RedirectResult("/error/general");
            }
        }

        [RequireApplicationId]
        [RestrictToStatus(ApplicationStatusTypeLookup.ApplicationStatusType.Inquiry)]
        public override ActionResult Confirm()
        {
            try
            {
                return View("~/Views/Apply/Partner/Confirm.cshtml", new PartnerApplicationPageModel(null));
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
                return new RedirectResult("/error/general");
            }
        }

    }
}
