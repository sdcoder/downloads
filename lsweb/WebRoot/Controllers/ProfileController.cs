using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Security;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.ServiceModel.Contracts.MessageContracts.CustomerOperations;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.LoanServicing.SharedTypes;
using LightStreamWeb.ServerState;
using LightStreamWeb.App_State;
using LightStreamWeb.Controllers.Shared;
using LightStreamWeb.Filters;
using LightStreamWeb.Models.AccountServices;
using LightStreamWeb.Models.Shared;
using LightStreamWeb.Models.SignIn;
using Ninject;
using System.Web.Mvc;
using FirstAgain.Domain.Lookups.FirstLook;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LightStreamWeb.Controllers
{
    [Authorize]
    [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
    public class ProfileController : BaseController
    {
        private const string _errorMessage = "The established User ID and Password entered is either invalid or is not eligible to move.  Please re-enter the information.  Thank you.";
        private const string _conflictErrorMessage = "The contact information associated with the user ID you have selected differs from the information on file for your current user ID.";

        [Inject]
        public ProfileController(ICurrentUser user) : base(user) { }

        //
        // GET: /Profile/
        [InjectAccountInfo]
        public ActionResult Index(GetAccountInfoResponse accountInfo)
        {
            return View(new ProfilePageModel(accountInfo));
        }

        [InjectAccountInfo]
        [InjectAccountServicesDataSet]
        public ActionResult Load(GetAccountInfoResponse accountInfo, AccountServicesDataSet accountServicesData)
        {
            var model = new AccountPreferenceModelData();
            model.Populate(accountInfo, accountServicesData);
            return new JsonNetResult
            {
                Data = model
            };
        }

        [HttpPost]
        [InjectAccountInfo]
        [InjectAccountServicesDataSet]
        public ActionResult UpdateContactInformation(GetAccountInfoResponse accountInfo, AccountServicesDataSet accountServicesData, int userId, int applicantId, ContactInformationPageModel applicantModel)
        {
            var accountContactInfo = accountInfo.CustomerUserIdDataSet.ContactInfo;

            if (accountContactInfo.UserId != userId)
            {
                return new HttpNotFoundResult();
            }

            if (!accountContactInfo.CanUpdate)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            applicantModel.UpdateAccountInfo(applicantId, accountInfo.CustomerUserIdDataSet, WebUser);

            return new JSONSuccessResult();
        }

        [HttpPost]
        [InjectAccountInfo]
        [InjectAccountServicesDataSet]
        public ActionResult UpdateAccountLock(GetAccountInfoResponse accountInfo, AccountServicesDataSet accountServicesData, int userId, bool enableAccountLock)
        {
            if (accountInfo.CustomerUserIdDataSet.ContactInfo.UserId != userId)
            {
                return new HttpNotFoundResult();
            }

            DomainServiceCustomerOperations.EnableAccountLock(accountInfo.CustomerUserIdDataSet.ContactInfo.UserId, enableAccountLock);
            SessionUtility.RefreshAccountInfo();

            return new JSONSuccessResult();
        }

        [HttpPost]
        [InjectAccountInfo]
        public ActionResult UpdatePrivacyPreferences(GetAccountInfoResponse accountInfo, int userId, int applicantId, ApplicantPreferencesPageModel applicantPreferences)
        {
            if (accountInfo.CustomerUserIdDataSet.ContactInfo.UserId != userId)
            {
                return new HttpNotFoundResult();
            }

            applicantPreferences.UpdatePreferences(applicantId, accountInfo.CustomerUserIdDataSet, WebUser);

            return new JSONSuccessResult();
        }

        [HttpPost]
        [InjectAccountInfo]
        public ActionResult ChangeUserId(GetAccountInfoResponse accountInfo, int userId, SecurityInformationPageModel securityInformation)
        {
            var accountContactInfo = accountInfo.CustomerUserIdDataSet.ContactInfo;
            if (accountInfo.CustomerUserIdDataSet.ContactInfo.UserId != userId)
            {
                return new HttpNotFoundResult();
            }

            var existingCustomerUserId = accountInfo.CustomerUserIdDataSet.CustomerUserId[0].CustomerUserId;
            if (!accountContactInfo.CanUpdate || !Membership.ValidateUser(existingCustomerUserId, securityInformation.Password))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            var newCustomerUserId = securityInformation.UserName;
            var existingUserId = DomainServiceCustomerOperations.GetUserIdFromCustomerUserId(newCustomerUserId);

            if (existingUserId != null)
            {
                return new JSONSuccessResult(false, "The user ID you have requested is unavailable, please enter another user ID.");
            }

            var applicationIds = securityInformation.SelectedApplications.Select(a => Convert.ToInt32(a)).ToList();
            if (securityInformation.SelectedApplications.Count() == accountInfo.CustomerUserIdDataSet.Applications.Length)
            {
                var result = securityInformation.ChangeUserId(accountInfo.CustomerUserIdDataSet, newCustomerUserId, applicationIds, WebUser);
                if (!result)
                {
                    return new JSONSuccessResult(false, "The information you have entered is invalid, please re-enter your user ID.");
                }

                FormsAuthentication.SetAuthCookie(accountInfo.CustomerUserIdDataSet.CustomerUserId[0].CustomerUserId, false);
                SessionUtility.SetUpAccountServicesData(accountInfo.CustomerUserIdDataSet.CustomerUserId[0].CustomerUserId);
                SessionUtility.RefreshAccountInfo();

                return new JSONSuccessResult();
            }
            else
            {
                var result = securityInformation.LinkAccountsToNewLogin(accountInfo.CustomerUserIdDataSet, newCustomerUserId, applicationIds, WebUser);
                if (!result)
                {
                    return new JSONSuccessResult(false, "The established user ID and Password entered is either invalid or is not eligible to move.  Please re-enter the information.  Thank you.");
                }

                SessionUtility.SetUpAccountServicesData(accountInfo.CustomerUserIdDataSet.CustomerUserId[0].CustomerUserId);
                SessionUtility.RefreshAccountInfo();

                return new JSONSuccessResult();
            }
        }

        [HttpPost]
        [InjectAccountInfo]
        public ActionResult MoveAccount(GetAccountInfoResponse accountInfo, int userId, SecurityInformationPageModel securityInformation)
        {
            var accountContactInfo = accountInfo.CustomerUserIdDataSet.ContactInfo;
            if (accountInfo.CustomerUserIdDataSet.ContactInfo.UserId != userId)
            {
                return new HttpNotFoundResult();
            }

            if (!accountContactInfo.CanUpdate)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            var targetCustomerUserId = securityInformation.UserName;
            var targetUserId = DomainServiceCustomerOperations.GetUserIdFromCustomerUserId(targetCustomerUserId);

            if (targetUserId == null || !Membership.ValidateUser(securityInformation.UserName, securityInformation.Password))
            {
                return new JSONSuccessResult(false, _errorMessage);
            }

            var haveAllApplicationsMoved = securityInformation.SelectedApplications.Count() == accountInfo.CustomerUserIdDataSet.Applications.Length;
            var applicationIds = securityInformation.SelectedApplications.Select(a => Convert.ToInt32(a)).ToList();
            var result = securityInformation.MoveAccount(accountInfo.CustomerUserIdDataSet, targetUserId.Value, applicationIds, WebUser);

            if (result != LinkAccountResultEnum.Success)
            {
                if (result == LinkAccountResultEnum.ContactInformationConflict)
                {
                    return new JSONSuccessResult(false, _conflictErrorMessage, true);
                }

                return new JSONSuccessResult(false, _errorMessage);
            }

            var currentCustomerUserId = accountInfo.CustomerUserIdDataSet.CustomerUserId[0].CustomerUserId;

            if (haveAllApplicationsMoved)
            {
                currentCustomerUserId = targetCustomerUserId;
                FormsAuthentication.SetAuthCookie(currentCustomerUserId, false);
                // TODO: (Kevin P. 11/25/2014) - Need to cleanup this duplicated code taken from SignController's InitializeSessionObjects method
                var accountData = new SignInModel().GetAccountStatus(currentCustomerUserId);
                if (accountData != null && accountData.CustomerData != null)
                {
                    SessionUtility.AccountInfo = accountData.AccountInfo;
                }

                if (accountData != null && accountData.ApplicationId > 0)
                {
                    WebUser.ApplicationId = accountData.ApplicationId;
                }
            }

            SessionUtility.SetUpAccountServicesData(currentCustomerUserId);
            SessionUtility.RefreshAccountInfo();

            return new JSONSuccessResult();
        }


        [HttpPost]
        [InjectAccountInfo]
        public ActionResult GetAccountSync(GetAccountInfoResponse accountInfo, string newUserName, SecurityInformationPageModel securityInformation)
        {
            var targetUserId = DomainServiceCustomerOperations.GetUserIdFromCustomerUserId(newUserName);

            if (!targetUserId.HasValue) return new JSONSuccessResult(false);

            var accountData =
                securityInformation.GetAccountSyncData(accountInfo.CustomerUserIdDataSet.UserId, targetUserId.Value);

            var resultdata = new List<object>();


            var source = accountData.FirstOrDefault();
            var dest = accountData.LastOrDefault();
            var swapContactInfo = source.ApplicantInfo.SSN != dest.ApplicantInfo.SSN;

            //source Applicant => dest coaaplicant
            var applicant1 = dest.ApplicantInfo;
            var applicant2 = dest.CoApplicantInfo;


            if (swapContactInfo)
            {
                applicant1 = dest.CoApplicantInfo;
                applicant2 = dest.ApplicantInfo;
            }

            if (applicant1 != null)
            {
                resultdata.Add(CreateResultItem(source.ApplicantInfo, applicant1, source.UserId, true));
            }

            if (source.CoApplicantInfo != null && applicant2 != null)
            {
                resultdata.Add(CreateResultItem(source.CoApplicantInfo, applicant2, source.UserId, true));
            }

            if (applicant1 != null)
            {
                resultdata.Add(CreateResultItem(applicant1, source.ApplicantInfo, dest.UserId, false));
            }

            if (applicant2 != null)
            {
                resultdata.Add(CreateResultItem(applicant2, source.CoApplicantInfo, dest.UserId, false));
            }

            return new JsonResult
            {
                Data = resultdata
            };

        }

        [HttpPost]
        [InjectAccountInfo]
        public ActionResult SyncAndMoveAccount(GetAccountInfoResponse accountInfo, int userId, string accountSettings, SecurityInformationPageModel securityInformation)
        {
            var result = securityInformation.SyncAndMoveAccount(accountSettings);

            if (result != LinkAccountResultEnum.Success)
            {
                if (result == LinkAccountResultEnum.ContactInformationConflict)
                {
                    return new JSONSuccessResult(false, _conflictErrorMessage, true);
                }

                return new JSONSuccessResult(false, _errorMessage);
            }

            return new JSONSuccessResult(true);
        }

        private static object CreateResultItem(AccountContactInfo.AccountInfo accountInfo1, AccountContactInfo.AccountInfo accountInfo2, int userId, bool isSource)
        {

            var resultItem = new
            {
                IsSource = isSource,
                UserId = userId,
                Name = accountInfo1.Name,
                Email = accountInfo1.HomeEmailAddress.EmailAddress,
                AddressLine1 = accountInfo1.HomeAddress.Address,
                SecondaryUnitType = accountInfo1.HomeAddress.UnitType.ToString(),
                SecondaryUnit = accountInfo1.HomeAddress.UnitValue,
                City = accountInfo1.HomeAddress.City,
                State = accountInfo1.HomeAddress.State.ToString(),
                ZipCode = accountInfo1.HomeAddress.ZipCode,
                Phone = accountInfo1.HomePhone.PhoneNumberFull,
                WorkPhone = accountInfo1.WorkPhone?.PhoneNumberFull,
                Differences = new
                {
                    Email = !accountInfo1.HomeEmailAddress.Equals(accountInfo2?.HomeEmailAddress),
                    Address = !accountInfo1.HomeAddress.Equals(accountInfo2?.HomeAddress),
                    Phone = !accountInfo1.HomePhone.Equals(accountInfo2?.HomePhone),
                    WorkPhone = !accountInfo1?.WorkPhone?.Equals(accountInfo2?.WorkPhone)
                }
            };

            return resultItem;
        }

        [HttpPost]
        [InjectAccountInfo]
        public ActionResult UpdatePassword(GetAccountInfoResponse accountInfo, int userId, SecurityInformationPageModel securityInformation)
        {
            if (accountInfo.CustomerUserIdDataSet.ContactInfo.UserId != userId)
            {
                return new HttpNotFoundResult();
            }

            if (!Membership.ValidateUser(accountInfo.CustomerUserIdDataSet.CustomerUserId[0].CustomerUserId, securityInformation.OldPassword))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            var result = securityInformation.UpdatePassword(accountInfo.CustomerUserIdDataSet, WebUser);

            return new JSONSuccessResult(result);
        }

        [HttpPost]
        [InjectAccountInfo]
        public ActionResult UpdateSecurityQuestionAndAnswer(GetAccountInfoResponse accountInfo, int userId, SecurityInformationPageModel securityInformation)
        {
            if (accountInfo.CustomerUserIdDataSet.ContactInfo.UserId != userId)
            {
                return new HttpNotFoundResult();
            }

            if (!Membership.ValidateUser(accountInfo.CustomerUserIdDataSet.CustomerUserId[0].CustomerUserId, securityInformation.Password))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            var result = securityInformation.UpdateSecurityQuestionAndAnswser(accountInfo.CustomerUserIdDataSet, WebUser);

            return new JSONSuccessResult(result);
        }
    }
}