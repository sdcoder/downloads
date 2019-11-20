using FirstAgain.Common.Xml;
using FirstAgain.Common.Extensions;
using FirstAgain.Domain.Common;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using FirstAgain.Domain.SharedTypes.LoanApplication.LoanApplicationPostData;
using LightStreamWeb.App_State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using FirstAgain.Domain.Lookups.FirstLook;

namespace LightStreamWeb.Models.Apply
{
    public class QueueSuntrustApplicationPostModel : QueueApplicationPostModel
    {
        public static QueueApplicationPostResult SetDefaultsAndValidateSuntrustApp(SuntrustLoanApplicationPostData postData, ICurrentUser webUser, int factOverride)
        {
            QueueApplicationPostResult result = SetDefaultsAndValidate(postData);
            if (result.Success)
            {
                // override the FACT and populate all marketing data
                SetMarketingData_Native(postData, webUser, factOverride);
        
                // Suntrust specific.....
                if ((int)postData.EmployeeVerificationResult == 0 && !string.IsNullOrEmpty(postData.RelationshipManagerId))
                {
                    postData.EmployeeVerificationResult = EmployeeVerificationResultLookup.EmployeeVerificationResult.NA;
                    postData.TeammateEmployeeIdType = ApplicationReferralTypeLookup.ApplicationReferralType.OriginatingOfficer;
                }
                else
                {
                    postData.TeammateEmployeeIdType = ApplicationReferralTypeLookup.ApplicationReferralType.Employee;
                }
                if (postData.BranchEmployeePhoneNumber.IsNotNull())
                {
                    postData.BranchEmployeePhoneNumber.Type = TelecommTypeLookup.TelecommType.NotSelected;
                }
                if (postData.RelationshipManagerPhoneNumber.IsNotNull())
                {
                    postData.RelationshipManagerPhoneNumber.Type = TelecommTypeLookup.TelecommType.NotSelected;
                }
                if (string.IsNullOrEmpty(postData.BranchCostCenter))
                {
                    return QueueApplicationPostResult.ReturnError("BranchCostCenter is required", redirect: QueueApplicationPostResult.RedirectTo.BasicInfo);
                }
                if (string.IsNullOrEmpty(postData.TeammateEmailAddress) && string.IsNullOrEmpty(postData.RelationshipManagerEmailAddress))
                {
                    return QueueApplicationPostResult.ReturnError("TeammateEmailAddress is required", redirect: QueueApplicationPostResult.RedirectTo.BasicInfo);
                }
                if (string.IsNullOrEmpty(postData.TeammateEmployeeId))
                {
                    return QueueApplicationPostResult.ReturnError("TeammateEmployeeId is required", redirect: QueueApplicationPostResult.RedirectTo.BasicInfo);
                }

                if (!new Regex("^[0-9]{5,20}$").IsMatch(postData.TeammateEmployeeId))
                {
                    return QueueApplicationPostResult.ReturnError("Teammate Id must consist of at least 5 numeric characters", redirect: QueueApplicationPostResult.RedirectTo.BasicInfo);
                }

                if (!new Regex("^[0-9]{7,20}$").IsMatch(postData.BranchCostCenter))
                {
                    return QueueApplicationPostResult.ReturnError("Cost Center must consist of at least 7 numeric characters", redirect: QueueApplicationPostResult.RedirectTo.BasicInfo);
                }

                if (!EmailAddressValidator.IsValidEmail(postData.TeammateEmailAddress))
                {
                    return QueueApplicationPostResult.ReturnError(string.Format("Teammate Email Address {0} is not valid.", postData.TeammateEmailAddress), redirect: QueueApplicationPostResult.RedirectTo.BasicInfo);
                }

                if (postData.BranchEmployeePhoneNumber.IsNull() && postData.RelationshipManagerPhoneNumber.IsNull())
                {
                    return QueueApplicationPostResult.ReturnError("Teammate Phone Number is required.", redirect: QueueApplicationPostResult.RedirectTo.BasicInfo);
                }
            }

            return result;
        }

        public static void SubmitSuntrustAppApp(SuntrustLoanApplicationModel postData, ICurrentUser user)
        {
            ApplicationPostPublicSite appPost = new ApplicationPostPublicSite();
            appPost.PostingPartner = FirstAgain.Domain.Lookups.Logging.PostingPartnerLookup.PostingPartner.Suntrust;
            appPost.PostedApplicationData = XmlUtility.SerializeToPlainString((SuntrustLoanApplicationPostData)postData);
            appPost.UserAgent = user.UserAgent;
            appPost.Cookie = user.UniqueCookie;
            appPost.IpAddress = user.IPAddress;
            appPost.AcceptLanguage = user.AcceptLanguage;

            DomainServiceLoanApplicationOperations.SubmitQueuedApplicationPost(appPost);
        }


        internal static QueueApplicationPostResult SubmitSuntrustReferral(SuntrustTeammateReferralModel model, ICurrentUser currentUser, int FACTOverride)
        {
            var appPost = new ApplicationPostTeammateReferral();

            if (model.ApplicantEmailAddress.IsNullOrEmpty() || !EmailAddressValidator.IsValidEmail(model.ApplicantEmailAddress))
            {
                return QueueApplicationPostResult.ReturnError("Please enter a valid email address", QueueApplicationPostResult.RedirectTo.BasicInfo);
            }
            if (model.PurposeOfLoan == null || model.PurposeOfLoan.Type == PurposeOfLoanLookup.PurposeOfLoan.NotSelected)
            {
                return QueueApplicationPostResult.ReturnError("Purpose of loan is required", QueueApplicationPostResult.RedirectTo.BasicInfo);
            }

            appPost.SocialSecurityNumber = model.SocialSecurityNumber;
            appPost.EmailAddress = model.ApplicantEmailAddress;
            appPost.PurposeOfLoan = model.PurposeOfLoan.Type;
            if (model.PurposeOfLoan.Type.IsHomeImprovement())
            {
                if (model.HmdaComplianceProperty == null || model.HmdaComplianceProperty.Address == null || model.HmdaComplianceProperty.Address.AddressLine.IsNullOrEmpty())
                {
                    return QueueApplicationPostResult.ReturnError("HMDA address is required", QueueApplicationPostResult.RedirectTo.BasicInfo);
                }
                appPost.SubjectProperty = model.HmdaComplianceProperty.Address;
            }
            if (model.BranchCostCenter.IsNullOrEmpty())
            {
                return QueueApplicationPostResult.ReturnError("Branch Cost Center is required", QueueApplicationPostResult.RedirectTo.BasicInfo);
            }
            appPost.BranchCostCenter = model.BranchCostCenter;
            if (model.TeammateEmployeeId.IsNullOrEmpty())
            {
                return QueueApplicationPostResult.ReturnError("Employee Id is required", QueueApplicationPostResult.RedirectTo.BasicInfo);
            }
            appPost.TeammateEmployeeId = model.TeammateEmployeeId;
            appPost.EmployeeVerificationResult = model.EmployeeVerificationResult;
            if (model.TeammateEmailAddress.IsNullOrEmpty() && model.RelationshipManagerEmailAddress.IsNullOrEmpty())
            {
                return QueueApplicationPostResult.ReturnError("Teammate email address is required", QueueApplicationPostResult.RedirectTo.BasicInfo);
            }
            appPost.TeammateEmailAddress = model.TeammateEmailAddress;
            appPost.TeammateIsContractorOrTemporary = model.TeammateIsContractorOrTemporary;
            if ((int)model.EmployeeVerificationResult == 0 && !string.IsNullOrEmpty(model.RelationshipManagerId))
            {
                appPost.EmployeeVerificationResult = EmployeeVerificationResultLookup.EmployeeVerificationResult.NA;
            }

            appPost.PostingPartner = FirstAgain.Domain.Lookups.Logging.PostingPartnerLookup.PostingPartner.TeammateReferral;
            appPost.UserAgent = currentUser.UserAgent;
            appPost.Cookie = currentUser.SessionApplyCookie;
            appPost.IpAddress = currentUser.IPAddress;
            appPost.AcceptLanguage = currentUser.AcceptLanguage;
            appPost.LoanAmount = model.LoanAmount;
            appPost.ApplicationType = ((int)model.ApplicationType == 0) ? ApplicationTypeLookup.ApplicationType.NotSelected : model.ApplicationType;
            appPost.FirstAgainCodeTrackingId = FACTOverride;
            appPost.RelationshipManagerId = model.RelationshipManagerId;
            appPost.RelationshipManagerEmailAddress = model.RelationshipManagerEmailAddress;
            if (model.BranchEmployeePhoneNumber.IsNotNull())
            {
                model.BranchEmployeePhoneNumber.Type = TelecommTypeLookup.TelecommType.NotSelected;
                appPost.BranchEmployeePhoneNumber = model.BranchEmployeePhoneNumber;
            }
            if (model.RelationshipManagerPhoneNumber.IsNotNull())
            {
                model.RelationshipManagerPhoneNumber.Type = TelecommTypeLookup.TelecommType.NotSelected;
                appPost.RelationshipManagerPhoneNumber = model.RelationshipManagerPhoneNumber;
            }
            
            appPost.ApplicantCIN = model.ApplicantCIN;
            
            DomainServiceLoanApplicationOperations.SubmitQueuedApplicationPost(appPost);

            return new QueueApplicationPostResult()
            {
                Success = true
            };
        }

        public static string GetExistingReferralInfo(string primarySSN, string secondarySSN, string primaryEmailAddress, string secondaryEmailAddress, int teammateReferralId)
        {
            // If SSN is provided, override the email to empty string as we do not look for it. 
            if( !string.IsNullOrEmpty(primarySSN) || !string.IsNullOrEmpty(secondarySSN))
            {
                var response = DomainServiceLoanApplicationOperations.GetTeammateReferral(primarySSN, secondarySSN, "", "", 0);
                if (response != null && response.TeammateReferralId != 0)
                {
                    return response.TeammateEmailAddress;
                }
            }
            else
            {
               var response = DomainServiceLoanApplicationOperations.GetTeammateReferral(primarySSN, secondarySSN, primaryEmailAddress, secondaryEmailAddress, teammateReferralId);
                if (response != null && response.TeammateReferralId != 0)
                {
                    return response.TeammateEmailAddress;
                }
            }
            return string.Empty;
        }
    }
}