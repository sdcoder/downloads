using FirstAgain.Common.Xml;
using FirstAgain.Common.Extensions;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using FirstAgain.Domain.SharedTypes.LoanApplication.LoanApplicationPostData;
using LightStreamWeb.App_State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirstAgain.Domain.Common;
using FirstAgain.Domain.Lookups.FirstLook;
using LightStreamWeb.Helpers;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Common.Logging;
using Newtonsoft.Json;
using FirstAgain.Common.Caching;
using System.Configuration;
using System.Text.RegularExpressions;

namespace LightStreamWeb.Models.Apply
{
    public class QueueApplicationPostModel
    {
        public static void SubmitAccountServicesApp(AccountServicesAppModel postData, ICurrentUser user, CustomerUserIdDataSet customerData)
        {
            ApplicationPost appPost;
            if (user.AddCoApplicant)
            {
                appPost = new ApplicationPostAddCoApplicant(true)
                {
                    CancelApplicationId = user.ApplicationId.GetValueOrDefault()
                };
            }
            else
            {
                appPost = new ApplicationPostAccountServices();
            }

            appPost.PostingPartner = FirstAgain.Domain.Lookups.Logging.PostingPartnerLookup.PostingPartner.Native;
            appPost.PostedApplicationData = XmlUtility.SerializeToPlainString(postData);
            appPost.UserAgent = user.UserAgent;
            appPost.Cookie = user.UniqueCookie;
            appPost.IpAddress = user.IPAddress;
            appPost.AcceptLanguage = user.AcceptLanguage;
            appPost.TransUnionSessionId = user.TransUnionSessionId;

            DomainServiceLoanApplicationOperations.SubmitQueuedApplicationPost(appPost);
        }

        public static void SubmitAddCoApplicantApp(AddCoApplicantModel postData, ICurrentUser user)
        {
            ApplicationPostAddCoApplicant appPost = new ApplicationPostAddCoApplicant(user.IsAccountServices);

            var lads = DomainServiceLoanApplicationOperations.GetLoanApplicationByApplicationId(user.ApplicationId.Value);
            postData.Applicants.First().SocialSecurityNumber = lads.PrimaryApplicant.SocialSecurityNumber;

            appPost.PostingPartner = FirstAgain.Domain.Lookups.Logging.PostingPartnerLookup.PostingPartner.Native;
            appPost.PostedApplicationData = XmlUtility.SerializeToPlainString(postData);
            appPost.UserAgent = user.UserAgent;
            appPost.Cookie = user.UniqueCookie;
            appPost.IpAddress = user.IPAddress;
            appPost.CancelApplicationId = user.ApplicationId.Value;
            appPost.AcceptLanguage = user.AcceptLanguage;
            appPost.TransUnionSessionId = user.TransUnionSessionId;

            DomainServiceLoanApplicationOperations.SubmitQueuedApplicationPost(appPost);
        }

        public static bool CanSumbitToSmokeTest(ApplicantPostData applicantPostData)
        {
            if (applicantPostData.LastName.Equals("Consumer", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            string SmoketestSSNs = ConfigurationManager.AppSettings["SmoketestSSNs"];
            if(string.IsNullOrEmpty(SmoketestSSNs) == false)
            {
                return SmoketestSSNs.Split(',').Contains(applicantPostData.SocialSecurityNumber);
            }
            return false;
        }

        public static void SubmitNativeApp(NativeLoanApplicationPostData postData, ICurrentUser user)
        {
            ApplicationPostPublicSite appPost = new ApplicationPostPublicSite();
            appPost.PostingPartner = FirstAgain.Domain.Lookups.Logging.PostingPartnerLookup.PostingPartner.Native;
            appPost.PostedApplicationData = XmlUtility.SerializeToPlainString(postData);
            appPost.UserAgent = user.UserAgent;
            appPost.Cookie = user.UniqueCookie;
            appPost.IpAddress = user.IPAddress;
            appPost.AcceptLanguage = user.AcceptLanguage;
            appPost.TransUnionSessionId = user.TransUnionSessionId;

            // smoke test?
            if (postData.Applicants != null && CanSumbitToSmokeTest(postData.Applicants.First()))
            {
                appPost.PostingPartner = FirstAgain.Domain.Lookups.Logging.PostingPartnerLookup.PostingPartner.SmokeTest;
            }

            // drop in queue
            DomainServiceLoanApplicationOperations.SubmitQueuedApplicationPost(appPost);
        }

        public static QueueApplicationPostResult ValidateAccountServicesSecurityInfo(AccountServicesAppModel postData)
        {
            if (postData.UserCredentials == null)
            {
                return QueueApplicationPostResult.ReturnError("Customer user id and password are required", QueueApplicationPostResult.RedirectTo.SecurityInfo);
            }

            if (!postData.UserCredentials.UserId.HasValue)
            {
                return QueueApplicationPostResult.ReturnError("User id is required", QueueApplicationPostResult.RedirectTo.SecurityInfo);
            }

            return new QueueApplicationPostResult() { Success = true };
        }

        public static QueueApplicationPostResult ValidateSecurityInfo(NativeLoanApplicationPostData postData)
        {
            if (postData.UserCredentials == null)
            {
                return QueueApplicationPostResult.ReturnError("Customer user id and password are required", QueueApplicationPostResult.RedirectTo.SecurityInfo);
            }

            if (string.IsNullOrEmpty(postData.UserCredentials.UserName))
            {
                return QueueApplicationPostResult.ReturnError("User name is required", QueueApplicationPostResult.RedirectTo.SecurityInfo);
            }

            if (postData.UserCredentials.UserName.Length < 6)
            {
                return QueueApplicationPostResult.ReturnError("User name is required and must be at least 6 characters", QueueApplicationPostResult.RedirectTo.SecurityInfo);
            }


            if (postData.UserCredentials.UserName.Length > 20)
            {
                return QueueApplicationPostResult.ReturnError("User name is required and must be less than 20 characters", QueueApplicationPostResult.RedirectTo.SecurityInfo);
            }

            if ((int)postData.UserCredentials.SecurityQuestionType == 0 || postData.UserCredentials.SecurityQuestionType == SecurityQuestionLookup.SecurityQuestion.NotSelected)
            {
                return QueueApplicationPostResult.ReturnError("Security question is required", QueueApplicationPostResult.RedirectTo.SecurityInfo);
            }

            if (postData.UserCredentials.SecurityQuestionAnswer.IsNullOrEmpty())
            {
                return QueueApplicationPostResult.ReturnError("Security question is required", QueueApplicationPostResult.RedirectTo.SecurityInfo);
            }

            if (postData.UserCredentials.SecurityQuestionAnswer.Length > 50)
            {
                return QueueApplicationPostResult.ReturnError("Security question is required and must be less than 50 characters", QueueApplicationPostResult.RedirectTo.SecurityInfo);
            }

            return new QueueApplicationPostResult() { Success = true };
        }
        public static QueueApplicationPostResult ValidateUserIdReservation(NativeLoanApplicationPostData postData)
        {
            if (!MaintenanceConfiguration.IsInMaintenanceMode)
            {
                if (!postData.UserCredentials.IsTemporary && postData.UserCredentials.UserId.IsNull())
                {
                    return QueueApplicationPostResult.ReturnError("Please supply a username and password", redirect: QueueApplicationPostResult.RedirectTo.SecurityInfo);
                }
            }
            return new QueueApplicationPostResult() { Success = true };
        }

        public static QueueApplicationPostResult SetDefaultsAndValidate(NativeLoanApplicationPostData postData)
        {
            if (postData.Applicants == null || postData.Applicants.Count < 1)
            {
                return QueueApplicationPostResult.ReturnError("No primary applicant information found", QueueApplicationPostResult.RedirectTo.ApplicantInfo);
            }

            if (postData.PurposeOfLoan == null || postData.PurposeOfLoan.Type == PurposeOfLoanLookup.PurposeOfLoan.NotSelected || (int)postData.PurposeOfLoan.Type == 0)
            {
                return QueueApplicationPostResult.ReturnError("Loan purpose is required", QueueApplicationPostResult.RedirectTo.LoanInfo);
            }
            if (postData.InterestRate == 0)
            {
                return QueueApplicationPostResult.ReturnError("Interest rate not found", QueueApplicationPostResult.RedirectTo.LoanInfo);
            }
            if (postData.LoanAmount == 0)
            {
                return QueueApplicationPostResult.ReturnError("Loan amount is required", QueueApplicationPostResult.RedirectTo.LoanInfo);
            }
            if (postData.LoanTermMonths == 0)
            {
                return QueueApplicationPostResult.ReturnError("Loan term is required", QueueApplicationPostResult.RedirectTo.LoanInfo);
            }

            // if not a joint app, toss all the defaulted co-applicant info
            if (postData.ApplicationType == ApplicationTypeLookup.ApplicationType.Individual)
            {
                if (postData.Applicants.Count > 1)
                {
                    postData.Applicants.RemoveAt(1);
                }
            }

            foreach (var app in postData.Applicants)
            {
                if (app.Occupation.Employer != null)
                {
                    if (string.IsNullOrWhiteSpace(app.Occupation.Employer.Title) &&
                        string.IsNullOrWhiteSpace(app.Occupation.Employer.EmployerName))
                    {
                        app.Occupation.Employer = null;
                    }
                }

                if (app.Occupation.IsNotNull())
                {
                    if (app.Occupation.Type.IsNotNull() && app.Occupation.Employer.IsNotNull())
                    {
                        if (app.Occupation.Type.IsOneOf(OccupationTypeLookup.OccupationType.Retired, OccupationTypeLookup.OccupationType.Student, OccupationTypeLookup.OccupationType.NotEmployed) && app.Occupation.Employer.GrossAnnualIncome > 0)
                        {
                            LightStreamLogger.WriteWarning("Occupation type marked as " + app.Occupation.Type.ToString() + " but occupation annual income was not reset to 0. Has now been reset.");
                            app.Occupation.Employer.GrossAnnualIncome = 0;
                        }
                    }
                }

                if (app.Residence?.Address == null)
                {
                    return QueueApplicationPostResult.ReturnError("Primary residence is required", QueueApplicationPostResult.RedirectTo.ApplicantInfo);
                }

                app.Residence.Address.Type = PostalAddressTypeLookup.PostalAddressType.PrimaryResidence;
                if (app.Occupation?.Employer?.Address != null)
                {
                    app.Occupation.Employer.Address.Type = PostalAddressTypeLookup.PostalAddressType.PrimaryEmployer;
                }
                if (app.Occupation?.Employer?.PhoneNumber != null && (int)app.Occupation.Employer.PhoneNumber.Type == 0)
                {
                    app.Occupation.Employer.PhoneNumber.Type = TelecommTypeLookup.TelecommType.WorkPhone;
                }
                if (app.DriversLicenseLastFourDigits != null && app.DriversLicenseLastFourDigits.Length > 4)
                {
                    app.DriversLicenseLastFourDigits = app.DriversLicenseLastFourDigits.Substring(app.DriversLicenseLastFourDigits.Length - 4);
                }
                if (app.Residence?.Address?.SecondaryUnit != null && app.Residence.Address.SecondaryUnit.Type == PostalAddressSecondaryUnitTypeLookup.PostalAddressSecondaryUnitType.NotSelected)
                {
                    app.Residence.Address.SecondaryUnit.Value = null;
                }
                if (app.Residence.Citizenships == null || app.Residence.Citizenships.Count == 0)
                {
                    LightStreamLogger.WriteWarning("Citizenship was missing from application post. User redirected to correct info." + JsonConvert.SerializeObject(app.Residence));

                    return QueueApplicationPostResult.ReturnError("Citizenship is required", (app.Type == ApplicantTypeLookup.ApplicantType.Primary) ? QueueApplicationPostResult.RedirectTo.ApplicantInfo : QueueApplicationPostResult.RedirectTo.CoApplicantInfo);
                }
            }
            postData.Applicants.First().Type = ApplicantTypeLookup.ApplicantType.Primary;

            if (postData.ApplicationType == ApplicationTypeLookup.ApplicationType.Joint)
            {
                if (postData.Applicants == null || postData.Applicants.Count < 2)
                {
                    return QueueApplicationPostResult.ReturnError("Joint application submitted, but no Co-Applicant information found", QueueApplicationPostResult.RedirectTo.CoApplicantInfo);
                }
                postData.Applicants.Skip(1).Single().Type = ApplicantTypeLookup.ApplicantType.Secondary;

               
                var primarySsn = GetPrimarySsn(postData);

                if (primarySsn == postData.Applicants[1].SocialSecurityNumber)
                {
                    return QueueApplicationPostResult.ReturnError("Applicant and Co-Applicant SSNs must not match", QueueApplicationPostResult.RedirectTo.CoApplicantInfo);
                }
            }

            if (postData.Applicants.Any(a => a.Residence?.Address == null))
            {
                return QueueApplicationPostResult.ReturnError("Residence information not found for one or more applicants", QueueApplicationPostResult.RedirectTo.ApplicantInfo);
            }


            var invalidZipEmployer = postData.Applicants.FirstOrDefault(a => a.Occupation?.Employer?.Address != null && 
                                                                             a.Occupation.Employer.Address.ZipCode.ToDigits().IsNullOrEmpty());
            if (invalidZipEmployer != null)
            {
                return QueueApplicationPostResult.ReturnError("Invalid Employer Zip Code", invalidZipEmployer);
            }

            var invalidZipApplicant = postData.Applicants.FirstOrDefault(a => string.IsNullOrEmpty(a.Residence.Address.ZipCode) || a.Residence.Address.ZipCode.Length != 5 || a.Residence.Address.ZipCode.ToDigits().IsNullOrEmpty());
            if (invalidZipApplicant != null)
            {
                if (invalidZipApplicant.Type == ApplicantTypeLookup.ApplicantType.Primary)
                {
                    return QueueApplicationPostResult.ReturnError("Please enter a zip code", redirect: QueueApplicationPostResult.RedirectTo.BasicInfo);
                }
                return QueueApplicationPostResult.ReturnError("Invalid Zip Code", invalidZipApplicant);
            }

            foreach (var applicant in postData.Applicants)
            {
                // Validate applicant's name 
                if (!ValidateApplicantName(applicant.FirstName) ||
                    !ValidateApplicantName(applicant.LastName))
                {
                    string applicantType = applicant.Type == ApplicantTypeLookup.ApplicantType.Primary ? "applicant" : "co-applicant";
                    return QueueApplicationPostResult.ReturnError($"Invalid {applicantType}'s name", QueueApplicationPostResult.RedirectTo.ApplicantInfo);
                }

                if (applicant.EmailAddress == null || !FirstAgain.Domain.Common.EmailAddressValidator.IsValidEmail(applicant.EmailAddress))
                {
                    return QueueApplicationPostResult.ReturnError("Invalid email address",
                        (applicant.Type == ApplicantTypeLookup.ApplicantType.Primary) ? QueueApplicationPostResult.RedirectTo.ApplicantInfo : QueueApplicationPostResult.RedirectTo.CoApplicantInfo);
                }

                if (applicant.Residence.Address.SecondaryUnit?.Value != null && applicant.Residence.Address.SecondaryUnit.Type == PostalAddressSecondaryUnitTypeLookup.PostalAddressSecondaryUnitType.NotSelected)
                {
                    return QueueApplicationPostResult.ReturnError("When a SecondaryUnit value is specified, the type (Apt, Suite, etc) must also be specified.", QueueApplicationPostResult.RedirectTo.ApplicantInfo);
                }

                if (applicant.DateOfBirth == DateTime.MinValue)
                {
                    return QueueApplicationPostResult.ReturnError("Date of birth is required", QueueApplicationPostResult.RedirectTo.ApplicantInfo);
                }
            }
            // scrub HMDA data if not home improvement. The UI may populate it if they switch back and forth
            if (!postData.PurposeOfLoan.Type.IsHomeImprovement())
            {
                postData.HmdaComplianceProperty = null;
            }
            else
            {
                if (postData.HmdaComplianceProperty == null)
                {
                    return QueueApplicationPostResult.ReturnError("Home Improvement application submitted, but no subject property information found", QueueApplicationPostResult.RedirectTo.SubjectProperty);
                }
                if (postData.HmdaComplianceProperty.Address == null ||
                    postData.HmdaComplianceProperty.Address.State == StateLookup.State.NotSelected ||
                    (int)postData.HmdaComplianceProperty.Address.State == 0)
                {
                    return QueueApplicationPostResult.ReturnError("Information for Property being Improved is required", QueueApplicationPostResult.RedirectTo.SubjectProperty);
                }
            }

            // If we have a temporary user id, then set the password hash, since the raw password
            // should not be- and is not- stored in the message on the loan app queue
            if (postData.UserCredentials.IsTemporary == true)
            {
                postData.UserCredentials.PasswordHash = FirstAgain.Domain.SharedTypes.Security.PasswordHash.GetPasswordHash(postData.UserCredentials.Password);
            }
            if ((int)postData.UserCredentials.SecurityQuestionType == 0)
            {
                postData.UserCredentials.SecurityQuestionType = SecurityQuestionLookup.SecurityQuestion.NotSelected;
            }

            // field length checks
            if (!postData.MaskedSSN)
            {
                if (postData.Applicants.Any(a => a.SocialSecurityNumber == null || a.SocialSecurityNumber.Length != 9))
                {
                    return QueueApplicationPostResult.ReturnError("Social security number is invalid.",
                                (postData.Applicants.First(a => a.SocialSecurityNumber == null || a.SocialSecurityNumber.Length != 9).Type == ApplicantTypeLookup.ApplicantType.Primary)
                                ? QueueApplicationPostResult.RedirectTo.ApplicantInfo : QueueApplicationPostResult.RedirectTo.CoApplicantInfo
                        );
                }
            }



            // defaults, business constants
            postData.SubmittedDate = DateTime.Now;
            if (postData.RateLockDate == DateTime.MinValue)
            {
                postData.RateLockDate = DateTime.Now;
            }

            // State specific checks
            if (!postData.Applicants.Any(a => a.Residence?.Address != null && a.Residence.Address.State == StateLookup.State.Wisconsin))
            {
                if (postData.ApplicationOtherIncome != null && postData.ApplicationOtherIncome.Any(a => a.IncomeType == OtherIncomeTypeLookup.OtherIncomeType.WisconsinSpouseIncome))
                {
                    return QueueApplicationPostResult.ReturnError("Non-Applicant spouse income is only valid if one or more applicants reside in the state of Wisconsin", redirect: QueueApplicationPostResult.RedirectTo.ErrorMessage);
                }
            }
            
            // tax exempt stuff for application other income
            PopulateTaxExemptData(postData);

            // Replace all smart quotes with regular quotes.
            MapSmartQuotesToRegularQuotesInLoanApplicationPostData(postData);

            return new QueueApplicationPostResult() { Success = true };
        }

        private static bool ValidateApplicantName(string name)
        {
            string nameRegex = "^([A-Za-z]+[\']?[\\.]?[\\-]?[ ]?)*$";
            return new Regex(nameRegex).IsMatch(name);
        }

        private static void MapSmartQuotesToRegularQuotesInAddressPostData(AddressPostData postData)
        {
            if (postData.IsNull())
                return;

            postData.AddressLine = SmartQuotesToRegularQuotes.ToRegularQuotes(postData.AddressLine);
            postData.City = SmartQuotesToRegularQuotes.ToRegularQuotes(postData.City);
            if (postData.SecondaryUnit.IsNotNull())
                postData.SecondaryUnit.Value = SmartQuotesToRegularQuotes.ToRegularQuotes(postData.SecondaryUnit.Value);
        }

        private static void MapSmartQuotesToRegularQuotesInEmployerPostData(EmployerPostData postData)
        {
            if (postData.IsNull())
                return;

            postData.EmployerName = SmartQuotesToRegularQuotes.ToRegularQuotes(postData.EmployerName);
            postData.Title = SmartQuotesToRegularQuotes.ToRegularQuotes(postData.Title);
            MapSmartQuotesToRegularQuotesInAddressPostData(postData.Address);
        }

        private static void MapSmartQuotesToRegularQuotesInOccupationPostData(OccupationPostData postData)
        {
            if (postData.IsNull())
                return;

            postData.OccupationDescription = SmartQuotesToRegularQuotes.ToRegularQuotes(postData.OccupationDescription);
            MapSmartQuotesToRegularQuotesInEmployerPostData(postData.Employer);
        }

        // Bug #90063 - Map smart quotes to regular quotes.
        private static void MapSmartQuotesToRegularQuotesInLoanApplicationPostData(NativeLoanApplicationPostData postData)
        {
            if (postData.IsNull() || postData.Applicants.IsNull())
                return;

            foreach (var applicant in postData.Applicants)
            {
                applicant.FirstName = SmartQuotesToRegularQuotes.ToRegularQuotes(applicant.FirstName);
                applicant.LastName = SmartQuotesToRegularQuotes.ToRegularQuotes(applicant.LastName);
                applicant.MiddleInitial = SmartQuotesToRegularQuotes.ToRegularQuotes(applicant.MiddleInitial);

                MapSmartQuotesToRegularQuotesInOccupationPostData(applicant.Occupation);

                if (applicant.Residence.IsNotNull())
                    MapSmartQuotesToRegularQuotesInAddressPostData(applicant.Residence.Address);
            }
        }

        /// <summary>
        /// Get the Primary Applicant's SSN even if it's masked
        /// </summary>
        /// <param name="postData"></param>
        /// <returns>SSN or Empty string</returns>
        private static string GetPrimarySsn(NativeLoanApplicationPostData postData)
        {
            var primarySsn = string.Empty;

            if (postData.MaskedSSN)
            {
                if (!postData.ApplicationId.HasValue) return primarySsn;

                var lads = DomainServiceLoanApplicationOperations.GetLoanApplicationByApplicationId(postData.ApplicationId.Value);
                primarySsn = lads.PrimaryApplicant.SocialSecurityNumber;
            }
            else
            {
                primarySsn = postData.Applicants[0].SocialSecurityNumber;
            }

            return primarySsn;
        }


        private static void PopulateTaxExemptData(NativeLoanApplicationPostData postData)
        {
            postData.TaxExemptGrossUpFactor = BusinessConstants.Instance.TaxExemptGrossUpFactor;

            if (postData.ApplicationOtherIncome != null)
            {
                for (int idx = postData.ApplicationOtherIncome.Count - 1; idx >= 0; idx--)
                {
                    if (postData.ApplicationOtherIncome[idx].Amount == 0M)
                        postData.ApplicationOtherIncome.RemoveAt(idx);
                }

                postData.ApplicationOtherIncome.ForEach(aoi =>
                {
                    if (aoi.IncomeType.IsTaxExempt())
                    {
                        aoi.GrossedUpAmount = aoi.Amount * postData.TaxExemptGrossUpFactor;
                    }
                    else
                        aoi.GrossedUpAmount = aoi.Amount;
                });
            }
        }

        public static void SetMarketingData_AccountServices(NativeLoanApplicationPostData postData, ICurrentUser webUser)
        {
            SetMarketingData_Native(postData, webUser, LoanApplicationDataSet.KnownFACTs.LIGHTSTREAM_NATIVETRAFFIC_ACCOUNTSERVICES); // hard code this since there is no cookie to derive this from
        }

        public static void SetMarketingData_GenericPartner(NativeLoanApplicationPostData postData, ICurrentUser webUser)
        {
            SetMarketingData_SearchTerms(postData, webUser);

            postData.MarketingEntityData.SetIfNotNullOrEmpty(MarketingDataEntityLookup.MarketingDataEntity.SessionApplyCookie, webUser.SessionApplyCookie);

            SetMarketingData_SplitTest(postData, webUser);
        }

        public static void SetMarketingData_AddCoApplicant(NativeLoanApplicationModel postData, ICurrentUser webUser)
        {
            // decline referral
            if (MarketingDataSetHelper.IsEligibleForDeclineReferral(webUser.FirstAgainCodeTrackingId))
            {
                postData.SetFlag(FlagLookup.Flag.DeclineReferralEligible, true);
            }

            if (postData.ApplicationFlags != null && postData.ApplicationFlags.Any(f => f.FlagType == FlagLookup.Flag.DeclineReferralEligible && f.FlagIsOn))
            {
                if (postData.DeclineReferralOptIn)
                {
                    postData.SetFlag(FlagLookup.Flag.DeclineReferralOptIn, true);
                }
                else
                {
                    postData.SetFlag(FlagLookup.Flag.DeclineReferralOptIn, true);
                }
            }
            postData.MarketingEntityData.SetIfNotNullOrEmpty(MarketingDataEntityLookup.MarketingDataEntity.SessionApplyCookie, webUser.SessionApplyCookie);
        }

        public static void SetMarketingData_Native(NativeLoanApplicationPostData postData, ICurrentUser webUser, int? FACTOverride)
        {
            if (postData.FACTData == null)
            {
                postData.FACTData = new MarketingFACTDataPostData();
            }

            // default FACT if it wasn't set via the drop-down
            if (postData.FACTData.FACT == 0)
            {
                postData.FACTData.FACT = webUser.FirstAgainCodeTrackingId.GetValueOrDefault(1);
            }

            // override the FACT, such as when this is a post from Account services, or a suntrust branch / channel app
            if (FACTOverride != null)
            {
                postData.FACTData.FACT = FACTOverride.Value;
            }

            // decline referral
            if (MarketingDataSetHelper.IsEligibleForDeclineReferral(webUser.FirstAgainCodeTrackingId))
            {
                if (postData.ApplicationFlags == null)
                {
                    postData.ApplicationFlags = new List<ApplicationFlagPostData>();
                }
                postData.ApplicationFlags.Add(new ApplicationFlagPostData() { FlagType = FlagLookup.Flag.DeclineReferralEligible, FlagIsOn = true });
            }

            // SupplementalData
            if (!string.IsNullOrEmpty(webUser.FirstAgainIdReferral))
            {
                postData.FACTData.SupplementalData = webUser.FirstAgainIdReferral;
            }

            // if we have cookie data, save it
            if (webUser.FirstAgainCodeTrackingId != null)
            {
                //WebRedirectId 
                if (webUser.WebRedirectId != null)
                {
                    postData.MarketingEntityData.Set(MarketingDataEntityLookup.MarketingDataEntity.WebRedirectId, webUser.WebRedirectId.ToString());
                }

                //FirstVisitDate
                if (webUser.FirstVisitDate != DateTime.MinValue)
                {
                    postData.MarketingEntityData.Set(MarketingDataEntityLookup.MarketingDataEntity.FirstVisitDate, webUser.FirstVisitDate.ToString());
                }

                //FactSetDate
                if (webUser.FactSetDate != DateTime.MinValue)
                {
                    postData.MarketingEntityData.Set(MarketingDataEntityLookup.MarketingDataEntity.FACTSetDate, webUser.FirstVisitDate.ToString());
                }

                // Impact Radius
                var mktri = webUser.MarketingReferrerInfo;
                if (mktri != null && mktri.ReferrerName == "ImpactRadius")
                {

                    postData.MarketingEntityData.Set(MarketingDataEntityLookup.MarketingDataEntity.IRMPID,
                                                        mktri.GetDataValue(MarketingDataEntityLookup.MarketingDataEntity.IRMPID.ToString()));
                    postData.MarketingEntityData.Set(MarketingDataEntityLookup.MarketingDataEntity.IRPID,
                                                        mktri.GetDataValue(MarketingDataEntityLookup.MarketingDataEntity.IRPID.ToString()));
                    postData.MarketingEntityData.Set(MarketingDataEntityLookup.MarketingDataEntity.IRCampaignId,
                                                        mktri.GetDataValue(MarketingDataEntityLookup.MarketingDataEntity.IRCampaignId.ToString()));
                    postData.MarketingEntityData.Set(MarketingDataEntityLookup.MarketingDataEntity.IRActionTrackerId,
                                                        mktri.GetDataValue(MarketingDataEntityLookup.MarketingDataEntity.IRActionTrackerId.ToString()));
                    postData.MarketingEntityData.Set(MarketingDataEntityLookup.MarketingDataEntity.IRClickId,
                                                        mktri.GetDataValue(MarketingDataEntityLookup.MarketingDataEntity.IRClickId.ToString()));
                }

                // SUBID
                postData.MarketingEntityData.SetIfNotNullOrEmpty(MarketingDataEntityLookup.MarketingDataEntity.SubId, webUser.SubId);
                // AID
                postData.MarketingEntityData.SetIfNotNullOrEmpty(MarketingDataEntityLookup.MarketingDataEntity.AID, webUser.AID);
                // Bankrate Link Id
                postData.MarketingEntityData.SetIfNotNullOrEmpty(MarketingDataEntityLookup.MarketingDataEntity.BRLID, webUser.BRLId);
                // Google DS id 
                postData.MarketingEntityData.SetIfNotNullOrEmpty(MarketingDataEntityLookup.MarketingDataEntity.GSLID, webUser.GSLID);
                // EFID
                postData.MarketingEntityData.SetIfNotNullOrEmpty(MarketingDataEntityLookup.MarketingDataEntity.EFID, webUser.EFID);
                // TUNE Transaction ID
                postData.MarketingEntityData.SetIfNotNullOrEmpty(MarketingDataEntityLookup.MarketingDataEntity.TTID, webUser.TTID);
                // TUNE Affiliate ID
                postData.MarketingEntityData.SetIfNotNullOrEmpty(MarketingDataEntityLookup.MarketingDataEntity.TAID, webUser.TAID);
                // TUNE Affiliate Name
                postData.MarketingEntityData.SetIfNotNullOrEmpty(MarketingDataEntityLookup.MarketingDataEntity.TuneAffiliateName, webUser.TuneAffiliateName);
                // TUNE Referring URL
                postData.MarketingEntityData.SetIfNotNullOrEmpty(MarketingDataEntityLookup.MarketingDataEntity.TURL, webUser.TURL);
            }

            // TNTID
            postData.MarketingEntityData.SetIfNotNullOrEmpty(MarketingDataEntityLookup.MarketingDataEntity.TntId, webUser.TntId);

            SetMarketingData_SearchTerms(postData, webUser);

            postData.MarketingEntityData.SetIfNotNullOrEmpty(MarketingDataEntityLookup.MarketingDataEntity.SessionApplyCookie, webUser.SessionApplyCookie);

            SetMarketingData_SplitTest(postData, webUser);

            if (postData.TeammateReferralId == null) // set the teammate referral ID from the 30-day cookie if we didn't get it in the URL param.
            {
                postData.TeammateReferralId = webUser.TeammateReferralId;
            }
        }

        private static void SetMarketingData_SearchTerms(NativeLoanApplicationPostData postData, ICurrentUser webUser)
        {
            postData.MarketingEntityData.SetIfNotNullOrEmpty(MarketingDataEntityLookup.MarketingDataEntity.SearchTerms, webUser.SearchTermsCookie);
            postData.MarketingEntityData.SetIfNotNullOrEmpty(MarketingDataEntityLookup.MarketingDataEntity.SearchEngine, webUser.SearchEngineCookie);
        }

        private static void SetMarketingData_SplitTest(NativeLoanApplicationPostData postData, ICurrentUser webUser)
        {
            if (webUser.SplitTestCookie.HasValue)
            {
                postData.MarketingEntityData.Set(MarketingDataEntityLookup.MarketingDataEntity.SplitTestCookie, webUser.SplitTestCookie.ToString());
            }
        }

        #region Inner Class(es)

        public class QueueApplicationPostResult
        {
            public QueueApplicationPostResult()
            {
            }

            public static QueueApplicationPostResult ReturnError(string errorMessage, RedirectTo redirect)
            {
                try
                {
                    var browser = new CurrentUser().UserAgent;
                    LightStreamLogger.WriteWarning("ErrorMessage message presented to user: {ErrorMessage}, {Browser}", errorMessage, browser);
                }
                catch (Exception ex)
                {
                    LightStreamLogger.WriteError(ex);
                }
                
                return new QueueApplicationPostResult()
                {
                    Success = false,
                    ErrorMessage = errorMessage,
                    RedirectResult = redirect
                };
            }

            public static QueueApplicationPostResult ReturnError(string errorMessage, ApplicantPostData applicant)
            {
                RedirectTo redirect = (applicant.Type == ApplicantTypeLookup.ApplicantType.Primary) ? QueueApplicationPostResult.RedirectTo.ApplicantInfo : QueueApplicationPostResult.RedirectTo.CoApplicantInfo;
                return QueueApplicationPostResult.ReturnError(errorMessage, redirect);
            }

            public bool Success { get; set; }
            public string ErrorMessage { get; set; }
            private RedirectTo _redirect;
            public string Redirect
            {
                get
                {
                    return _redirect.ToString();
                }
            }
            private RedirectTo RedirectResult
            {
                set
                {
                    _redirect = value;
                }
            }

            public enum RedirectTo
            {
                None,
                ApplicantInfo,
                CoApplicantInfo,
                SecurityInfo,
                HMDA,
                SubjectProperty,
                LoanInfo,
                BasicInfo,
                ElectronicDisclosures,
                ThankYou,
                ErrorMessage
            }
        }
        #endregion
    }

}