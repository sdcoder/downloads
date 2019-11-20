using FirstAgain.Common.Logging;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using LightStreamWeb.App_State;
using LightStreamWeb.Controllers.Shared;
using LightStreamWeb.Filters;
using LightStreamWeb.Models.Apply;
using Ninject;
using System;
using System.Web.Mvc;

namespace LightStreamWeb.Controllers
{
    [Authorize]
    public class AddCoApplicantApplicationController : BaseApplicationController
    {
        [Inject]
        public AddCoApplicantApplicationController(ICurrentUser user)
            : base(user)
        {
        }

        [RequireApplicationId]
        public ActionResult Start()
        {
            return View("~/Views/Apply/Shared/LoadExistingApp.cshtml", new LoadExistingAppModel()
                {
                    DataURL = Url.Action("LoadApp"),
                    NextPage = Url.Action("Index") 
                });
        }

        [RequireApplicationId]
        [InjectLoanApplicationDataSet]
        [InjectCustomerUserIdDataSet]
        public ActionResult LoadApp(LoanApplicationDataSet lads, CustomerUserIdDataSet customerData)
        {
            try
            {
                WebUser.AddCoApplicant = true;

                var model = new AddCoApplicantModel();
                model.Populate(lads, customerData, true);

                if (WebUser.IsAccountServices)
                {
                    model.UserCredentials = new FirstAgain.Domain.SharedTypes.LoanApplication.LoanApplicationPostData.CustomerUserCredentialsPostData()
                    {
                        UserId = customerData.UserId,
                        IsTemporary = false
                    };
                }
                else
                {
                    WebUser.SetFACT(lads.FirstAgainCodeTrackingId);
                }

                if (lads.HasFlag(FirstAgain.Domain.Lookups.FirstLook.FlagLookup.Flag.DeclineReferralEligible))
                {
                    model.FACTData.IsEligibleForDeclineReferral = true;
                }

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

        //
        // GET: /Apply/Joint/
        public ActionResult Index()
        {
            return View("~/Views/Apply/Joint/Index.cshtml", new ApplyPageModel(WebUser));
        }

        public override ActionResult SecurityInfo()
        {
            if (WebUser.IsAccountServices)
            {
                return View("~/Views/Apply/AccountServices/SecurityInfo.cshtml", new AccountServicesApplyPageModel(WebUser));
            }

            // a new view is needed for this "tab" - because of different copy
            return View("~/Views/Apply/Joint/SecurityInfo.cshtml", new ApplyPageModel(WebUser));
        }

        public override ActionResult PersonalInfo()
        {
            if (WebUser.IsAccountServices)
            {
                return View("~/Views/Apply/AccountServices/PersonalInfo.cshtml", new AccountServicesApplyPageModel(WebUser));
            }
            return View("~/Views/Apply/PersonalInfo.cshtml", new ApplyPageModel(WebUser));
        }

        //
        // GET: /Apply/Joint/Confirm
        public override ActionResult Confirm()
        {
            return View("~/Views/Apply/Confirm.cshtml", new ApplyPageModel(WebUser));
        }

        // 
        // GET: /Apply/Joint/ThankYou
        public ActionResult ThankYou()
        {
            ApplyPageModel model = new ApplyPageModel(WebUser);
            return base.ThankYou(model, "~/Views/Apply/ThankYou.cshtml");
        }

        //
        // POST: /Apply/Submit
        [HttpPost]
        public ActionResult Submit(AddCoApplicantModel model)
        {
            LightStreamWeb.Models.Apply.QueueApplicationPostModel.QueueApplicationPostResult result = null;

            try
            {
                result = QueueApplicationPostModel.SetDefaultsAndValidate(model);
                if (result.Success)
                {
                    QueueApplicationPostModel.SetMarketingData_AddCoApplicant(model, WebUser);
                    if (!WebUser.IsAccountServices)
                    {
                        result = QueueApplicationPostModel.ValidateSecurityInfo(model);
                    }
                    if (result.Success)
                    {
                        QueueApplicationPostModel.SubmitAddCoApplicantApp(model, WebUser);
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

            return base.Submit(model, result);
        }


        public override ActionResult LoanInfo()
        {
            return RedirectToAction("Index");
        }

        public override ActionResult BasicInfo()
        {
            return RedirectToAction("Index");
        }

        public override ActionResult ElectronicDisclosures()
        {
            return RedirectToAction("Confirm");
        }

        public override ActionResult ZipCode()
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
    }
}
