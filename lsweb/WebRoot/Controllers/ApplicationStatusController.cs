using FirstAgain.Common.Logging;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Contracts.MessageContracts.CustomerOperations;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using FirstAgain.Web.UI;
using LightStreamWeb.App_State;
using LightStreamWeb.Controllers.Shared;
using LightStreamWeb.Filters;
using LightStreamWeb.Models.ApplicationStatus;
using LightStreamWeb.Models.Apply;
using LightStreamWeb.ServerState;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LightStreamWeb.Controllers
{
    [Authorize]
    [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
    public class ApplicationStatusController : BaseApplicationStatusController
    {
        [Inject]
        public ApplicationStatusController(ICurrentUser user)
            : base(user)
        {
        }

        //
        // GET: /AppStatus/
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet]
        public ActionResult Index(CustomerUserIdDataSet customerData)
        {
            if (customerData == null)
            {
                return new EmptyResult();
            }
            return new RedirectResult(StatusRedirect.GetRedirectBasedOnStatus(customerData, WebUser.ApplicationId.GetValueOrDefault()));
        }

        //
        // GET: /AppStatus/Withdrawn
        [HttpGet]
        [RequireApplicationId]
        [InjectCurrentApplicationData(ApplicationStatusTypeLookup.ApplicationStatusType.Withdrawn)]
        public ActionResult Withdrawn(ICurrentApplicationData applicationData)
        {
            return View("Withdrawn", new WithdrawnPageModel(applicationData));
        }

        //
        // POST: /AppStatus/StatusCheck
        [RequireApplicationId]
        [HttpPost]
        [InjectCurrentApplicationData]
        public ActionResult StatusCheck(ApplicationStatusTypeLookup.ApplicationStatusType? currentStatus, ICurrentApplicationData applicationData)
        {
            if (currentStatus == null)
            {
                currentStatus = applicationData.ApplicationStatus;
            }

            return new JsonResult()
            {
                Data = new StatusCheckModel().GetNextCheck(
                    WebUser.ApplicationId, 
                    currentStatus.GetValueOrDefault(ApplicationStatusTypeLookup.ApplicationStatusType.NotSelected)
                )
            };
        }

        //
        // GET: /AppStatus/LogoutStatusChange
        [HttpGet]
        [InjectCurrentApplicationData]
        public ActionResult LogoutStatusChange(ICurrentApplicationData applicationData)
        {
            TempData["Alert"] = Resources.FAMessages.UnexpectedStatusError;
            return new RedirectResult("/customer-sign-in");
        }

        //
        // GET: /AppStatus/Pending
        [HttpGet]
        [InjectCurrentApplicationData]
        public ActionResult Pending(ICurrentApplicationData applicationData)
        {
            return View( "Pending", new PendingPageModel(applicationData));
        }


        //
        // GET: /AppStatus/Decline
        [HttpGet]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet]
        [RestrictToStatus(FirstAgain.Domain.Lookups.FirstLook.ApplicationStatusTypeLookup.ApplicationStatusType.Declined)]
        public ActionResult Decline(CustomerUserIdDataSet customerData)
        {
            return View(new DeclinePageModel(customerData));
        }

        //
        // GET: /AppStatus/Terminated
        [HttpGet]
        [RequireApplicationId]
        [InjectCurrentApplicationData(ApplicationStatusTypeLookup.ApplicationStatusType.Terminated)]
        [RestrictToStatus(ApplicationStatusTypeLookup.ApplicationStatusType.Terminated)]
        public ActionResult Terminated(ICurrentApplicationData applicationData)
        {
            return View("Decline", new DeclinePageModel(applicationData));
        }

        // 
        // GET: /AppStatus/DeclineReferralOptIn
        [HttpPost]
        [RequireApplicationId]
        public ActionResult DeclineReferralOptIn()
        {
            DeclinePageModel.DeclineReferralOptIn(WebUser.ApplicationId.Value);
            return new JsonResult() { };
        }

        //
        // GET: /AppStatus/Counter
        [HttpGet]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet]
        [InjectLoanOfferDataSet]
        [RestrictToStatus(FirstAgain.Domain.Lookups.FirstLook.ApplicationStatusTypeLookup.ApplicationStatusType.Counter)]
        public ActionResult Counter(CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet)
        {
            return View(new CounterPageModel(customerData, loanOfferDataSet, null));
        }

        //
        // GET: /AppStatus/AcceptCounter
        [HttpGet]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet]
        public ActionResult AcceptCounter(CustomerUserIdDataSet customerData)
        {
            CounterPageModel.AcceptCounter(WebUser, customerData);

            return new RedirectResult("Refresh");
        }

        //
        // GET: /AppStatus/RejectCounter
        [HttpGet]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet]
        [InjectLoanOfferDataSet]
        public ActionResult RejectCounter(CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet)
        {
            var resultingStatus = CounterPageModel.RejectCounter(WebUser, customerData);
            if(resultingStatus == ApplicationStatusTypeLookup.ApplicationStatusType.Declined)
            {
                return View(new CounterPageModel(customerData, loanOfferDataSet, null));
            }
            else // Counter was already accepted.
            {
                return new RedirectResult("Refresh");
            }
        }

        //
        // GET: /AppStatus/CancelNLTR
        [HttpGet]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet]
        [InjectLoanOfferDataSet]
        public ActionResult CancelNLTR(CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet)
        {
            ApplicationStatusPageModel.CancelNLTR(WebUser, customerData, loanOfferDataSet);
            if (ApplicationStatusPageModel.HasExceptionAfterDecisionBeenTriggered(customerData, WebUser.ApplicationId.Value))
            {
                return new RedirectResult("~/appstatus/inreview");
            }
            return RedirectToAction("Refresh");
        }

        
        //
        // GET: /AppStatus/PendingV
        [HttpGet]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet]
        [InjectLoanOfferDataSet]
        [InjectLoanApplicationDataSet]
        [RestrictToStatus(ApplicationStatusTypeLookup.ApplicationStatusType.PendingV, 
                          LoanTermsRequestStatusLookup.LoanTermsRequestStatus.PendingV)]
        public ActionResult PendingV(CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet, LoanApplicationDataSet lads)
        {
            var pageModel = new PendingVPageModel(customerData, loanOfferDataSet, lads);
            if (pageModel.HasNothingToShow)
            {
                // return Pending page if PendingV application is no longer requesting any verification documents
                return View("Pending", new PendingPageModel(customerData, loanOfferDataSet));
            }
            else
            {
                return View(pageModel);
            }
        }

        //
        // GET: /AppStatus/CounterV
        [HttpGet]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet]
        [InjectLoanOfferDataSet]
        [InjectLoanApplicationDataSet]
        [RestrictToStatus(FirstAgain.Domain.Lookups.FirstLook.ApplicationStatusTypeLookup.ApplicationStatusType.CounterV,
                          FirstAgain.Domain.Lookups.FirstLook.LoanTermsRequestStatusLookup.LoanTermsRequestStatus.CounterV)]
        public ActionResult CounterV(CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet, LoanApplicationDataSet lads)
        {
            return View(new CounterVPageModel(customerData, loanOfferDataSet, lads));
        }

        //
        // GET: /AppStatus/Cancelled
        [HttpGet]
        [RequireApplicationId]
        [InjectAccountInfo]
        [RestrictToStatus(ApplicationStatusTypeLookup.ApplicationStatusType.Cancelled)]
        public ActionResult Cancelled(GetAccountInfoResponse accountInfo)
        {
            return View(new CancelledPageModel(accountInfo));
        }

        //
        // GET: /AppStatus/Cancelled/ThankYou
        [HttpGet]
        [RequireApplicationId]
        [InjectAccountInfo]
        [RestrictToStatus(ApplicationStatusTypeLookup.ApplicationStatusType.Cancelled)]
        public ActionResult CancelledThankYou(GetAccountInfoResponse accountInfo)
        {
            return View(new CancelledPageModel(accountInfo));
        }

        //
        // GET: /AppStatus/Expired
        [HttpGet]
        [InjectCurrentApplicationData(ApplicationStatusTypeLookup.ApplicationStatusType.Expired)]
        public ActionResult Expired(ICurrentApplicationData applicationData)
        {
            return View("Expired", new ExpiredPageModel(applicationData));
        }

        //
        // GET: /AppStatus/ApprovedExpired
        [HttpGet]
        [InjectCurrentApplicationData(false, new ApplicationStatusTypeLookup.ApplicationStatusType[] { ApplicationStatusTypeLookup.ApplicationStatusType.Approved, ApplicationStatusTypeLookup.ApplicationStatusType.ApprovedNLTR })]
        public ActionResult ApprovedExpired(ICurrentApplicationData applicationData)
        {
            return View("Expired", new ExpiredPageModel(applicationData));
        }

        //
        // GET: /AppStatus/InReview
        [HttpGet]
        [InjectCurrentApplicationData]
        public ActionResult InReview(ICurrentApplicationData applicationData)
        {
            return View("InReview", new InReviewPageModel(applicationData));
        }

        //
        // POST: /AppStatus/InReviewStatusCheck
        [HttpPost]
        [InjectCurrentApplicationData]
        public ActionResult InReviewStatusCheck(ICurrentApplicationData applicationData)
        {

            return new JsonNetResult()
            {
                Data = new {
                        DoRedirect = new InReviewPageModel(applicationData).HasBeenCleared()
                    }
            };
        }

        //
        // POST: /AppStatus/Upload
        [HttpPost]
        [RequireApplicationId(RequireApplicationIdAttribute.ResultType.JSON)]
        [InjectCustomerUserIdDataSet]
        public ActionResult Upload(IEnumerable<HttpPostedFileBase> file,
                                   int applicationId,
                                   int verificationRequestId,
                                   CustomerUserIdDataSet customerData)
        {
            var model = new DocumentUploadModel();

            model.DoUpload(file, WebUser, customerData, applicationId, verificationRequestId);

            model.RecordDocumentUploads(customerData, applicationId, verificationRequestId);
            SessionUtility.AccountInfo.CustomerUserIdDataSet = customerData;

            return new JsonNetResult()
            {
                Data = model
            };
        }

        //
        // GET: /AppStatus/CustomerIdentification
        [HttpGet]
        public ActionResult CustomerIdentification(string name, ApplicantTypeLookup.ApplicantType applicantType)
        {
            var model = new CustomerIdentificationPageModel() 
            { 
                ApplicantName = name,
                ApplicantType = applicantType,
                InformationRequestUrl = "/appstatus/refresh"
            };
            return PartialView("~/Views/Components/_CustomerIdentification.cshtml", model);
        }

        //
        // GET: /AppStatus/Approved
        [HttpGet]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet]
        [InjectLoanOfferDataSet]
        public ActionResult Approved(CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet)
        {
            try
            {
                WebUser.ResetSignatures();
                return View(new ApprovedPageModel(customerData, loanOfferDataSet));
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
                if (ex.StackTrace != null)
                {
                    LightStreamLogger.WriteError(ex.StackTrace);
                }
                throw ex;
            }
        }

        //
        // POST: /AppStatus/LoadDeclineNotice
        [HttpPost]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet(Refresh = true)]
        public ActionResult LoadDeclineNotice(string postData, CustomerUserIdDataSet customerData)
        {
            return new JsonNetResult()
            {
                Data = ApprovedPageModel.HasDeclineNotice(customerData)
            };
        }

        //
        // GET: /AppStatus/FaxCover
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet]
        [InjectLoanOfferDataSet]
        public ActionResult FaxCover(CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet)
        {
            return View(new FaxCoverPageModel(customerData, loanOfferDataSet));
        }

        // GET: /AppStatus/SwitchToAutoPay
        [HttpGet]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet(Refresh=true)]
        [InjectLoanOfferDataSet(Refresh = true)]
        public ActionResult SwitchToAutoPay(CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet)
        {
            var model = new ApprovedPageModel(customerData, loanOfferDataSet);
            bool isAutoApproved;
            model.SwitchToAutoPay(out isAutoApproved);
            if (isAutoApproved)
            {
                TempData["Alert"] = "Your account has been switched to AutoPay";
            }

            return RedirectToAction("Refresh");    
        }

        // GET: /AppStatus/CancelLoan
        [HttpGet]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet]
        public ActionResult CancelLoan(CustomerUserIdDataSet customerData)
        {
            return View("~/Views/ApplicationStatus/Approved/Cancel.cshtml", new CancelLoanPageModel(customerData));
        }

        // POST: /AppStatus/CancelLoan
        [HttpPost]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet]
        public ActionResult CancelLoan(CustomerUserIdDataSet customerData, WithdrawReasonTypeLookup.WithdrawReasonType WithdrawReason, string WithdrawReasonDescription)
        {
            var model = new CancelLoanPageModel(customerData);

            try
            {
                model.CancelApprovedLoan(WebUser.ApplicationId.Value, WithdrawReason, WithdrawReasonDescription);
                base.SignOut();
                return View("~/Views/ApplicationStatus/Approved/Cancelled.cshtml", new LightStreamWeb.Models.SignIn.SignInModel());
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
            }
            
            return View("~/Views/ApplicationStatus/Approved/Cancel.cshtml", model);
        }

        // GET: /AppStatus/NLTRApproved
        [RequireApplicationId]
        public ActionResult NLTRApproved()
        {
            Refresh();
            SessionUtility.ResetLoanAgreementSignature();
            return RedirectToAction("Approved");
        }

        #region Loan Acceptance
        // POST: /AppStatus/PersistLoanAgreement
        [HttpPost]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet]
        [InjectLoanOfferDataSet(Refresh =true)]
        public ActionResult PersistLoanAgreement(CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet, int loanTermsRequestId)
        {
            try
            {
                var model = new ApprovedPageModel(customerData, loanOfferDataSet);

                if (model.LoanTerms.LoanTermsRequestId != loanTermsRequestId)
                {
                    return new JsonResult { Data = new { Success = false } };
                }

                model.PersistLoanAgreement();
                return new EmptyResult();
            }
            catch
            {
                return new JsonResult { Data = new { Success = false } };
            }
        }

        // POST: /AppStatus/LoadLoanAcceptanceData
        [HttpPost]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet]
        [InjectLoanOfferDataSet]
        [InjectLoanApplicationDataSet]
        public ActionResult LoadLoanAcceptanceData(LoanApplicationDataSet lads, CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet)
        {
            try
            {
                var model = new LoanAcceptanceModel();
                model.Populate(WebUser, lads, customerData, loanOfferDataSet);
                return new JsonNetResult()
                {
                    Data = model
                };
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
                throw ex;
            }
        }

        // POST: /AppStatus/SubmitLoanContract
        [HttpPost]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet(Refresh:true, ExpectedStatus: new ApplicationStatusTypeLookup.ApplicationStatusType[] { ApplicationStatusTypeLookup.ApplicationStatusType.Approved, ApplicationStatusTypeLookup.ApplicationStatusType.PreFundingNLTR })]
        [InjectLoanOfferDataSet(Refresh=true)]
        [InjectLoanApplicationDataSet]
        public ActionResult SubmitLoanContract(LoanApplicationDataSet lads, CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet, LoanAcceptanceModel model)
        {
            try
            {
                if (model.ApplicationId != WebUser.ApplicationId)
                {
                    LightStreamLogger.WriteError(string.Format("SubmitLoanContract: Submitted application id of {0} does not match active application id of {1}. ", model.ApplicationId, WebUser.ApplicationId));
                    TempData["Alert"] = "We are sorry but an error has occurred and we are unable to process your request. Please try again.";
                    return new JsonNetResult()
                    {
                        Data = new LightStreamWeb.Models.ApplicationStatus.LoanAcceptanceModel.SubmitLoanContractResult()
                        {
                             ErrorMessage = "We are sorry but an error has occurred and we are unable to process your request. Please try again.",
                             SignOut = true,
                             Success = false
                        }
                    };
                }
                // client side properties will be pre-populated, but we load data sets from session
                model.Populate(WebUser, lads, customerData, loanOfferDataSet);

                // validate 
                var result = model.SubmitLoanContract();
                return new JsonNetResult()
                {
                    Data = result
                };
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
                return new JsonNetResult()
                {
                    Data = new
                    {
                        Success = false,
                        ErrorMessage = "We're sorry, but an unexpected error has occurred"
                    }
                };
            }

        }
        
        // POST: /AppStatus/ValidateBankingInfo
        [HttpPost]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet]
        [InjectLoanOfferDataSet(Refresh = true)]
        [InjectLoanApplicationDataSet]
        public ActionResult ValidateBankingInfo(LoanApplicationDataSet lads, CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet, LoanAcceptanceModel model)
        {
            try
            {
                var loanTermsRequestId = model.LoanTerms.LoanTermsRequestId;

                // client side properties will be pre-populated, but we load data sets from session
                model.Populate(WebUser, lads, customerData, loanOfferDataSet);

                if (model.LoanTerms.LoanTermsRequestId != loanTermsRequestId 
                    || loanOfferDataSet.Application.Single().ApplicationStatusType != ApplicationStatusTypeLookup.ApplicationStatusType.Approved)
                {
                    LightStreamLogger.WriteWarning("Unable to validate banking info for {ApplicationId}. ({ModelLoanTermsLoanTermsRequestId} != {LoanTermsRequestId} || {ApplicationStatusType} != {ApplicationStatusTypeApproved}) == true.",
                                                    loanOfferDataSet.ApplicationId,
                                                    model.LoanTerms.LoanTermsRequestId,
                                                    loanTermsRequestId,
                                                    loanOfferDataSet.Application.Single().ApplicationStatusType,
                                                    ApplicationStatusTypeLookup.ApplicationStatusType.Approved);

                    return new JsonNetResult()
                    {
                        Data = new
                        {
                            Success = false
                        }
                    };
                }

                // validate 
                var result = model.ValidateBankingInfo();

                if (!result.Success)
                    LightStreamLogger.WriteWarning("Unable to validate banking info for {ApplicationId}. {ErrorMessage}", loanOfferDataSet.ApplicationId, result.ErrorMessage);

                return new JsonNetResult()
                {
                    Data = result
                };
            }
            catch (Exception ex)
            {
                if (ex is LightStreamWeb.Models.ApplicationStatus.LoanAcceptanceModel.ApprovedApplicationNotFoundException)
                {
                    LightStreamLogger.WriteWarning(ex);
                }
                else
                {
                    LightStreamLogger.WriteError(ex);
                }
            }

            return new JsonNetResult()
            {
                Data = new
                {
                    Success = false,
                    ErrorMessage = "We're sorry, but an unexpected error has occurred"
                }
            };
        }

        // POST: /AppStatus/SubmitChangeLoanTermsRequest
        [HttpPost]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet(true, new ApplicationStatusTypeLookup.ApplicationStatusType[] { ApplicationStatusTypeLookup.ApplicationStatusType.Approved, ApplicationStatusTypeLookup.ApplicationStatusType.PreFunding, ApplicationStatusTypeLookup.ApplicationStatusType.PreFundingNLTR })]
        [InjectLoanOfferDataSet(Refresh = true)]
        public ActionResult SubmitChangeLoanTermsRequest(CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet, LoanAcceptanceModel model)
        {
            try
            {
                SessionUtility.AcceptedPreviousTerms = false;

                if (model.LoanTerms.LoanTermsRequestId != loanOfferDataSet.LatestAliveLoanTerms.LoanTermsRequestId)
                {
                    var errorResult = LoanAcceptanceModel.SubmitChangeLoanTermsRequestResult.Failure("");
                    errorResult.IsStale = true;

                    return new JsonNetResult { Data = errorResult };
                }

                model.Populate(WebUser, null, customerData, loanOfferDataSet);

                var result = model.SubmitChangeLoanTermsRequest();
                return new JsonNetResult()
                {
                    Data = result
                };
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
                return new JsonNetResult()
                {
                    Data = LoanAcceptanceModel.SubmitChangeLoanTermsRequestResult.Failure(ex.Message)
                };
            }
        }

        // POST: /AppStatus/ConfirmChangeLoanTermsRequest
        [HttpPost]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet(true, new ApplicationStatusTypeLookup.ApplicationStatusType[] { ApplicationStatusTypeLookup.ApplicationStatusType.Approved, ApplicationStatusTypeLookup.ApplicationStatusType.PreFunding, ApplicationStatusTypeLookup.ApplicationStatusType.PreFundingNLTR })]
        [InjectLoanOfferDataSet]
        public ActionResult ConfirmChangeLoanTermsRequest(CustomerUserIdDataSet customerData, LoanOfferDataSet loanOfferDataSet, LoanAcceptanceModel model)
        {
            try
            {
                model.Populate(WebUser, null, customerData, loanOfferDataSet);
                var result = model.ConfirmChangeLoanTermsRequest();
                return new JsonNetResult()
                {
                    Data = result
                };
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
                return new JsonNetResult()
                {
                    Data = LoanAcceptanceModel.SubmitChangeLoanTermsRequestResult.Failure(ex.Message)
                };
            }
        }
        #endregion

        [HttpGet]
        public ActionResult Barcode(string number)
        {
            using (System.Drawing.Bitmap bm = BarCoderUtility.GetBarcodeBitmap(number, 20))
            {
                //Render the barcode bitmap
                //Get the image encoding info
                System.Drawing.Imaging.ImageCodecInfo jpgCodec = BarCoderUtility.GetImageCodecInfo();
                // Create quality parameter
                System.Drawing.Imaging.EncoderParameters encoderParams = BarCoderUtility.GetEncoderParams();

                var ms = new System.IO.MemoryStream();
                bm.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                return File(ms.ToArray(), "image/jpeg");
            }

        }
    }
}
