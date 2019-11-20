using FirstAgain.Common.Logging;
using FirstAgain.Common.Extensions;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using LightStreamWeb.App_State;
using LightStreamWeb.Controllers.Shared;
using LightStreamWeb.Filters;
using LightStreamWeb.Models.ApplicationStatus;
using LightStreamWeb.Models.PreFunding;
using LightStreamWeb.Models.SignIn;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Threading;
using LightStreamWeb.Models.Shared;
using LightStreamWeb.ServerState;

namespace LightStreamWeb.Controllers
{
    [Authorize]
    public class PreFundingController : BaseApplicationStatusController
    {
        [Inject]
        public PreFundingController(ICurrentUser user)
            : base(user)
        {
        }

        //
        // GET: /AppStatus/PreFunding/
        [HttpGet]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet]
        [InjectLoanOfferDataSet]
        public ActionResult Index(CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet)
        {
            return View("~/Views/ApplicationStatus/PreFunding/Index.cshtml", new PreFundingPageModel(customerData, loanOfferDataSet));
        }

        //
        // GET: /AppStatus/PreFunding/Cancel
        [HttpGet]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet]
        public ActionResult Cancel(CustomerUserIdDataSet customerData)
        {
            var model = new PreFundingPageModel(customerData);
            if (model.IsPastLockOut)
            {
                return Refresh();
            }
            return View("~/Views/ApplicationStatus/PreFunding/Cancel.cshtml", model);
        }

        //
        // GET: /AppStatus/PreFunding/LoanCancelled
        [HttpGet]
        [AllowAnonymous]
        public ActionResult LoanCancelled()
        {
            base.SignOut();
            return View("~/Views/ApplicationStatus/PreFunding/Cancelled.cshtml", new SignInModel());
        }

        // POST: /AppStatus/PreFunding/Cancel
        [HttpPost]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet]
        public ActionResult Cancel(CustomerUserIdDataSet customerData, WithdrawReasonTypeLookup.WithdrawReasonType WithdrawReason, string WithdrawReasonDescription)
        {
            var model = new PreFundingPageModel(customerData);

            try
            {
                model.CancelPreFundingLoan(WebUser.ApplicationId.Value, WithdrawReason, WithdrawReasonDescription);
                Refresh();
                return RedirectToAction("LoanCancelled");
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
                return new RedirectResult("/error/general");
            }
        }

        // GET: /AppStatus/PreFunding/ChangePaymentDate
        [HttpGet]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet(Refresh: true, ExpectedStatus: ApplicationStatusTypeLookup.ApplicationStatusType.PreFunding)]
        [InjectLoanOfferDataSet]
        public ActionResult ChangePaymentDate(CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet)
        {
            return View("~/Views/ApplicationStatus/PreFunding/ChangePaymentDate.cshtml", new PreFundingPageModel(customerData, loanOfferDataSet));
        }

        // POST: /AppStatus/PreFunding/ChangePaymentDate
        [HttpPost]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet(Refresh: true, ExpectedStatus: ApplicationStatusTypeLookup.ApplicationStatusType.PreFunding)]
        [InjectLoanOfferDataSet]
        public ActionResult ChangePaymentDate(CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet, int? NewPaymentDayOfMonth)
        {
            if (NewPaymentDayOfMonth.HasValue)
            {
                var model = new PreFundingPageModel(customerData, loanOfferDataSet);

                try
                {
                    model.ChangePaymentDate(NewPaymentDayOfMonth.Value);
                    Refresh();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    LightStreamLogger.WriteError(ex);
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Payment day of month is required";
            }

            return RedirectToAction("ChangePaymentDate");
        }

        // GET: /AppStatus/PreFunding/RescheduleFundingDate
        [HttpGet]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet(Refresh: true, ExpectedStatus: ApplicationStatusTypeLookup.ApplicationStatusType.PreFunding)]
        [InjectLoanOfferDataSet]
        public ActionResult RescheduleFundingDate(CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet)
        {
            return View("~/Views/ApplicationStatus/PreFunding/RescheduleFundingDate.cshtml", new RescheduleFundingDateModel(customerData, loanOfferDataSet));
        }

        // POST: /AppStatus/PreFunding/RescheduleFundingDate
        [HttpPost]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet(Refresh: true, ExpectedStatus: ApplicationStatusTypeLookup.ApplicationStatusType.PreFunding)]
        [InjectLoanOfferDataSet]
        public ActionResult RescheduleFundingDate(CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet, DateTime? FundingDate)
        {
            if (!FundingDate.HasValue)
            {
                TempData["ErrorMessage"] = "Funding Date is required";
                return RedirectToAction("RescheduleFundingDate");
            }
            var model = new RescheduleFundingDateModel(customerData, loanOfferDataSet);
            string errorMessage;
            if (model.RescheduleFundingDate(FundingDate.Value, out errorMessage))
            {
                Refresh();
                TempData["Alert"] = "Your funding date has been updated.";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = errorMessage;
            return View("~/Views/ApplicationStatus/PreFunding/RescheduleFundingDate.cshtml", model);
        }

        // GET: /AppStatus/PreFunding/ChangeLoanTerms
        [HttpGet]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet(true, new[] { ApplicationStatusTypeLookup.ApplicationStatusType.PreFunding, ApplicationStatusTypeLookup.ApplicationStatusType.PreFundingNLTR })]
        [InjectLoanOfferDataSet]
        public ActionResult ChangeLoanTerms(CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet)
        {
            return View("~/Views/ApplicationStatus/PreFunding/ChangeLoanTerms.cshtml", new ChangeLoanTermsPageModel(customerData, loanOfferDataSet));
        }

        // GET: /AppStatus/PreFunding/AccountInformationChange
        [HttpGet]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet]
        [InjectLoanOfferDataSet]
        public ActionResult AccountInformationChange(CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet)
        {
            return View("~/Views/ApplicationStatus/PreFunding/AccountInformationChange.cshtml", new AccountInformationChangeModel(customerData, loanOfferDataSet));
        }

        // POST: /AppStatus/PreFunding/AccountInformationChange
        [HttpPost]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet(Refresh: true, ExpectedStatus: ApplicationStatusTypeLookup.ApplicationStatusType.PreFunding)]
        [InjectLoanOfferDataSet]
        public ActionResult AccountInformationChange(CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet, AccountInformationChangeModel.AccountUpdateData data)
        {
            var model = new AccountInformationChangeModel(customerData, loanOfferDataSet);
            
            if(!model.ValidateSecurityAnswer(data.SecurityQuestionAnswer))
                return new JsonResult() { Data = new { Success = false, ErrorMessage = @"̿ ̿’ ̿’\̵͇̿̿\з=(•̪●)=ε/̵͇̿̿/’̿”̿ ̿" } };


            string errorMessage;
            bool result = model.Update(data, out errorMessage);

            return new JsonResult()
            {
                Data = new
                {
                    Success = result,
                    ErrorMessage = errorMessage
                }
            };

        }

        // POST: /AppStatus/PreFunding/ValidateSecurityAnswer
        [HttpPost]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet]
        [InjectLoanOfferDataSet]
        public ActionResult ValidateSecurityAnswer(CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet, string SecurityAnswer)
        {
            var model = new AccountInformationChangeModel(customerData, loanOfferDataSet);
            return new JsonResult()
            {
                Data = new
                {
                    Success = model.ValidateSecurityAnswer(SecurityAnswer)
                }

            };
        }

        // GET: /AppStatus/PreFunding/NLTRThankYou
        [HttpGet]
        [InjectCustomerUserIdDataSet(Refresh = true)]
        public ActionResult NLTRThankYou(CustomerUserIdDataSet customerData)
        {
            var model = new ApplicationStatusPageModel(customerData);
            model.BodyClass = "pre-funding counter";
            return View("~/Views/ApplicationStatus/PreFunding/NLTRThankYou.cshtml", model);
        }

        // GET: /AppStatus/NLTRApproved
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet(Refresh = true)]
        [InjectLoanOfferDataSet(Refresh = true)]
        public ActionResult NLTRApproved(CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet)
        {
            SessionUtility.ResetLoanAgreementSignature();
            return Refresh();
        }

        // GET: /AppStatus/NLTRDeclined
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet(Refresh = true)]
        [InjectLoanOfferDataSet(Refresh = true)]
        public ActionResult NLTRDeclined(CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet)
        {
            return RedirectToAction("NLTR");
        }

        [HttpPost]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet]
        [InjectLoanOfferDataSet]
        public ActionResult AcceptPreviousTerms(CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet)
        {
            var model = new PreFundingNLTRPageModel(customerData, loanOfferDataSet);
            model.AcceptPreviousTerms();
            SessionUtility.AcceptedPreviousTerms = true;

            if (ApplicationStatusPageModel.HasExceptionAfterDecisionBeenTriggered(customerData, WebUser.ApplicationId.Value))
            {
                return new RedirectResult("~/appstatus/inreview");
            }
            return Refresh();
        }

        [HttpPost]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet(Refresh = true)]
        [InjectLoanOfferDataSet]
        public ActionResult CancelNLTROrLoan(string action, CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet)
        {
            if (string.IsNullOrEmpty(action))
            {
                TempData["Alert"] = "Please select a cancel action";
                return RedirectToAction("NLTR");
            }

            if (action.Equals("Loan", StringComparison.InvariantCultureIgnoreCase))
            {
                return RedirectToAction("Cancel");

            }

            // else, NLTR

            // check for double-cancel, multiple tab, or any other concurrency issue
            if (!customerData.Application.Any(a => a.ApplicationId == WebUser.ApplicationId)
                || !customerData.Application.First(a => a.ApplicationId == WebUser.ApplicationId).ApplicationStatusType.IsOneOf(ApplicationStatusTypeLookup.ApplicationStatusType.PreFundingNLTR, ApplicationStatusTypeLookup.ApplicationStatusType.ApprovedNLTR)
                )
            {
                TempData["Alert"] = "Your application was not found, or not in the expected status.";
                return RedirectToAction("Refresh");
            }

            var model = new PreFundingNLTRPageModel(customerData, loanOfferDataSet);
            model.CancelNLTR();
            return Refresh();
        }

        // GET: /AppStatus/PreFunding/NLTR
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet]
        [InjectLoanOfferDataSet]
        public ActionResult NLTR(CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet)
        {
            var model = new PreFundingNLTRPageModel(customerData, loanOfferDataSet);

            switch (model.GetNLTRStatus())
            {
                case PreFundingNLTRPageModel.PrefundingNLTRStatus.Approved:
                    return View("~/Views/ApplicationStatus/PreFunding/NLTRApproved.cshtml", model);
                case PreFundingNLTRPageModel.PrefundingNLTRStatus.ApprovedAfterDropDeadDateTime:
                    return RedirectToAction("NLTRRescheduleFundingDate");
                case PreFundingNLTRPageModel.PrefundingNLTRStatus.SwitchedFromInvoiceToAutoPayPostSign:
                    return RedirectToAction("GetNLTRCheckingAccountInfo");
                case PreFundingNLTRPageModel.PrefundingNLTRStatus.CounterV:
                    return new RedirectResult("/AppStatus/CounterV");
                case PreFundingNLTRPageModel.PrefundingNLTRStatus.Counter:
                    return new RedirectResult("/AppStatus/Counter");
                case PreFundingNLTRPageModel.PrefundingNLTRStatus.PendingV:
                    return new RedirectResult("/AppStatus/PendingV");
                case PreFundingNLTRPageModel.PrefundingNLTRStatus.Cancelled:
                    return View("~/Views/ApplicationStatus/PreFunding/NLTRDeclinedCancelled.cshtml", model);
                case PreFundingNLTRPageModel.PrefundingNLTRStatus.Declined:

                    if (model.WaitForLatestDeclineNotice(customerData))
                    {
                        var timeLimit = DateTime.Now.AddSeconds(20);
                        CustomerUserIdDataSet refreshedData;
                        do
                        {
                            Thread.Sleep(2000);
                            refreshedData = DomainServiceCustomerOperations.GetAccountInfoByApplicationId(customerData.Application.First().ApplicationId).CustomerUserIdDataSet;
                            if (!model.WaitForLatestDeclineNotice(refreshedData))
                            {
                                return RedirectToAction("NLTRDeclined");  // Redirect to self to properly reset session
                            }

                        } while (DateTime.Now < timeLimit);
                    }

                    return View("~/Views/ApplicationStatus/PreFunding/NLTRDeclinedCancelled.cshtml", model);

                default: // Pending, PendingQ, Etc...
                    return View("~/Views/ApplicationStatus/PreFunding/NLTR.cshtml", model);
            }
        }

        // GET: /AppStatus/PreFunding/SignLoanAgreement
        [HttpGet]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet(Refresh = true)]
        [InjectLoanOfferDataSet(Refresh = true)]
        [RestrictToStatus(ApplicationStatusTypeLookup.ApplicationStatusType.PreFundingNLTR)]
        public ActionResult SignLoanAgreement(CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet)
        {
            var model = new SignLoanAgreementPageModel(customerData, loanOfferDataSet, WebUser.ApplicationId);
            if (model.HasSignedLoanAgreement())
            {
                var nextStep = model.GetNextStep();
                if (nextStep == SignLoanAgreementPageModel.PersistLoanAgreementNextStep.IsComplete)
                {
                    if (ApplicationStatusPageModel.HasExceptionAfterDecisionBeenTriggered(customerData, WebUser.ApplicationId.Value))
                    {
                        return new RedirectResult("~/appstatus/inreview");
                    }

                    return Refresh();
                }
                return new RedirectResult(nextStep.ToString());
            }

            return View("~/Views/ApplicationStatus/PreFunding/SignLoanAgreement.cshtml", model);
        }

        [HttpGet]
        public ActionResult InReview()
        {
            return new RedirectResult("~/appstatus/inreview");
        }

        // POST: /AppStatus/PreFunding/PersistLoanAgreement
        [HttpPost]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet]
        [InjectLoanOfferDataSet(Refresh = true)]
        [RestrictToStatus(ApplicationStatusTypeLookup.ApplicationStatusType.PreFundingNLTR)]
        public ActionResult PersistLoanAgreement(CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet, LoanTermsRequestModel m)
        {
            var model = new SignLoanAgreementPageModel(customerData, loanOfferDataSet, m.ApplicationId);
            if(model.LoanTermsRequestId != m.LoanTermsRequestId)
            {
                return new JsonResult()
                {
                    Data = new { Success = false }
                };
            }
            var result = model.PersistLoanAgreement();

            if (result == SignLoanAgreementPageModel.PersistLoanAgreementNextStep.IsComplete)
            {
                if (ApplicationStatusPageModel.HasExceptionAfterDecisionBeenTriggered(customerData, WebUser.ApplicationId.Value))
                {
                    return new JsonResult()
                    {
                        Data = new
                        {
                            Success = true,
                            IsComplete = false,
                            NextStep = "/inreview"
                        }
                    };
                }

            }

            return new JsonResult()
            {
                Data = new
                {
                    Success = true,
                    IsComplete = result == SignLoanAgreementPageModel.PersistLoanAgreementNextStep.IsComplete,
                    NextStep = result.ToString()
                }
            };
        }

        // GET: /AppStatus/PreFunding/NLTRRescheduleFundingDate
        [HttpGet]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet]
        [InjectLoanOfferDataSet]
        public ActionResult NLTRRescheduleFundingDate(CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet)
        {
            return View("~/Views/ApplicationStatus/PreFunding/NLTRRescheduleFundingDate.cshtml", new RescheduleFundingDateModel(customerData, loanOfferDataSet));
        }

        // POST: /AppStatus/PreFunding/NLTRRescheduleFundingDate
        [HttpPost]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet(Refresh = true)]
        [InjectLoanOfferDataSet(Refresh = true)]
        public ActionResult NLTRRescheduleFundingDate(CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet, DateTime? FundingDate)
        {
            if (!FundingDate.HasValue)
            {
                TempData["ErrorMessage"] = "Funding Date is required";
                return RedirectToAction("RescheduleFundingDate");
            }
            var model = new RescheduleFundingDateModel(customerData, loanOfferDataSet);
            string errorMessage;
            if (model.RescheduleFundingDate(FundingDate.Value, out errorMessage, isNLTR: true))
            {
                if (ApplicationStatusPageModel.HasExceptionAfterDecisionBeenTriggered(customerData, WebUser.ApplicationId.Value))
                {
                    return new RedirectResult("~/appstatus/inreview");
                }
                TempData["Alert"] = "Your funding date has been updated.";
                return RedirectToAction("Refresh");
            }

            return View("~/Views/ApplicationStatus/PreFunding/NLTRRescheduleFundingDate.cshtml", model);
        }

        // GET: /AppStatus/PreFunding/GetNLTRCheckingAccountInfo
        [HttpGet]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet(Refresh = true)]
        [InjectLoanOfferDataSet(Refresh = true)]
        public ActionResult GetNLTRCheckingAccountInfo(CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet)
        {
            return View("~/Views/ApplicationStatus/PreFunding/GetNLTRCheckingAccountInfo.cshtml", new PreFundingNLTRPageModel(customerData, loanOfferDataSet));
        }

        // POST: /AppStatus/PreFunding/GetNLTRCheckingAccountInfo
        [HttpPost]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet]
        [InjectLoanOfferDataSet]
        public ActionResult GetNLTRCheckingAccountInfo(GetNLTRCheckingAccountModel data, CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet)
        {
            var model = new PreFundingNLTRPageModel(customerData, loanOfferDataSet);
            model.UpdateNLTRCheckingAccountInfo(data);
            Refresh();
            return new JsonResult()
            {
                Data = new
                {
                    Success = true
                }
            };
        }

        // GET: /AppStatus/PreFunding/SelectEmailPreferences
        [HttpGet]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet]
        [InjectLoanOfferDataSet]
        public ActionResult SelectEmailPreferences(CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet)
        {
            return View("~/Views/ApplicationStatus/PreFunding/SelectEmailPreferences.cshtml", new PreFundingNLTRPageModel(customerData, loanOfferDataSet));
        }

        // POST: /AppStatus/PreFunding/UpdateEmailPreferences
        [HttpPost]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet]
        [InjectLoanOfferDataSet]
        public ActionResult UpdateEmailPreferences(CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet, List<SolicitationPreferenceLookup.SolicitationPreference> EmailPreferences)
        {
            var model = new PreFundingNLTRPageModel(customerData, loanOfferDataSet);
            model.UpdateEmailPreferences(EmailPreferences);
            Refresh();
            return new JsonResult()
            {
                Data = new
                {
                    Success = true
                }
            };
        }

        // GET: /AppStatus/PreFunding/FundingFailed
        [HttpGet]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet]
        [InjectLoanOfferDataSet]
        public ActionResult FundingFailed(CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet)
        {
            var model = new FundingFailedPageModel(customerData, loanOfferDataSet);
            if (model.CalendarFundingDates.Count == 0)
            {
                return RedirectToAction("Expired", "ApplicationStatus");
            }
            return View("~/Views/ApplicationStatus/PreFunding/FundingFailed.cshtml", model);
        }

        // POST: /AppStatus/PreFunding/FundingFailed
        [HttpPost]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet]
        [InjectLoanOfferDataSet]
        public ActionResult FundingFailed(CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet, FundingFailedPageModel.FundingFailedData data)
        {
            var model = new FundingFailedPageModel(customerData, loanOfferDataSet);
            string errorMessage;
            if (model.Submit(data, out errorMessage))
            {
                Refresh();
                return new JsonResult()
                {
                    Data = new
                    {
                        Success = true
                    }
                };
            }

            return new JsonResult()
            {
                Data = new
                {
                    Success = false,
                    ErrorMessage = errorMessage
                }
            };

        }

        // GET: /AppStatus/PreFunding/GetPdfLoanAgreement
        [HttpGet]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet(Refresh =true)]
        public ActionResult GetPdfLoanAgreement(CustomerUserIdDataSet customerData)
        {
            var docStoreRow = customerData.DocumentStore.Where(d => d.CorrespondenceCategory == CorrespondenceCategoryLookup.CorrespondenceCategory.LoanAgreementPdf).OrderByDescending(c => c.CreatedDate).FirstOrDefault();
            if (docStoreRow != null)
            {
                string fileName;
                byte[] doc = new Models.ENotices.ENoticeDisplayModel().GetEnoticPdf(customerData, docStoreRow.EdocId, out fileName);

                Response.AddHeader("Content-Disposition", string.Format("attachment; filename=\"{0}.pdf\"", fileName));
                return new FileContentResult(doc, "application/pdf");

            }

            return new HttpNotFoundResult();
        }

    }
}
