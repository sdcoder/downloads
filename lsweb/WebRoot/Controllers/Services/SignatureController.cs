using FirstAgain.Common.Logging;
using LightStreamWeb.ServerState;
using LightStreamWeb.App_State;
using LightStreamWeb.Controllers.Shared;
using LightStreamWeb.Models.ApplicationStatus;
using Ninject;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FirstAgain.Common.Drawing;
using LightStreamWeb.Filters;

namespace LightStreamWeb.Controllers
{
    [MaintenanceModeCheck]
    public class SignatureController : BaseController
    {
        [Inject]
        public SignatureController(ICurrentUser user)
            : base(user)
        {
        }

        //
        // GET: /Signature/Generate
        public ActionResult Generate(string name)
        {
            return new FileContentResult(SignatureUtility.GenerateSignatureFromFont(name), "image/jpeg");
        }

        //
        // GET: /Signature/Display
        public ActionResult Display(bool? isCoApplicant)
        {
            if (!isCoApplicant.HasValue)
            {
                return new HttpNotFoundResult();
            }

            byte[] signatureImageBytes = (isCoApplicant.Value) ? SessionUtility.SecondarySignatureImageBytes : SessionUtility.PrimarySignatureImageBytes;
            if (signatureImageBytes == null || signatureImageBytes.Length == 0)
            {
                return new HttpNotFoundResult();
            }
            return new FileContentResult(signatureImageBytes, "image/jpeg");
        }

        // POST: /Signature/PersistPartial
        public ActionResult PersistPartial(int? applicationId)
        {
            try
            {
                if (!applicationId.HasValue)
                {
                    LightStreamLogger.WriteWarning("Application ID not submitted to PersistPartial");
                    applicationId = SessionUtility.ActiveApplicationId.Value;
                }
                if (applicationId.HasValue)
                {
                    var loanOfferDataSet = SessionUtility.GetLoanOfferDataSet();

                    var model = new LoanAgreementModel(SessionUtility.CustomerUserIdDataSet, applicationId.Value, WebUser, loanOfferDataSet.LatestApprovedLoanTerms);
                    model.PersistLoanAgreement();
                }

                return new EmptyResult();
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
                throw ex;
            }
        }

        // POST: /Signature/Submit
        [HttpPost]
        public ActionResult Submit(bool? usingScriptFont, string signatureJSON, bool isCoApplicant, string applicantFullName)
        {
            try
            {
                var existingSignature = isCoApplicant ? SessionUtility.SecondarySignatureImageBytes : SessionUtility.PrimarySignatureImageBytes;
                var existingTimestamp = isCoApplicant ? SessionUtility.SecondarySignatureTstamp : SessionUtility.PrimarySignatureTstamp;

                byte[] bytes;
                var ts = SignatureUtility.GenerateTimeStamp();

                // if they signed, this will be a huge JSON blob. Convert it to an image
                if (!usingScriptFont.GetValueOrDefault() && !string.IsNullOrEmpty(signatureJSON))
                {
                    var bitmap = SignatureUtility.SigJsonToImage(signatureJSON);

                    if (!ImageChecker.IsValidSignature(bitmap))
                    {
                        return new JsonResult
                        {
                            Data = new
                            {
                                Success = false,
                            }
                        };
                    }

                    bytes = SignatureUtility.BmpToBytes(bitmap);
                }
                else if (usingScriptFont.GetValueOrDefault())
                {
                    bytes = SignatureUtility.GenerateSignatureFromFont(applicantFullName);
                }
                else
                {
                    // signature not found?
                    return new JsonResult
                    {
                        Data = new
                        {
                            Success = false
                        }
                    };
                }

                if (existingSignature == null || !existingSignature.SequenceEqual(bytes) || existingTimestamp == null)
                {
                    if (isCoApplicant)
                    {
                        SessionUtility.SecondarySignatureImageBytes = bytes;
                        SessionUtility.SecondarySignatureTstamp = ts;
                    }
                    else
                    {
                        SessionUtility.PrimarySignatureImageBytes = bytes;
                        SessionUtility.PrimarySignatureTstamp = ts;
                    }
                }


                // and the signature text - must set after timestamp has been set above
                return new JsonResult
                {
                    Data = new
                    {
                        Success = true,
                        TimeStamp = SessionUtility.SetSignatureText(isCoApplicant, applicantFullName),
                        ImageURL = SignatureUtility.GetImageUrl(isCoApplicant)
                    }
                };
            }
            catch(Exception ex)
            {
                LightStreamLogger.WriteError(ex);
                throw ex;
            }

        }
    }
}
