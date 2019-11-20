using System;
using System.Collections.Generic;
using System.Linq;
using FirstAgain.Common;
using FirstAgain.Common.Logging;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.SharedTypes.Customer;
using LightStreamWeb.ServerState;
using LightStreamWeb.App_State;
using LightStreamWeb.Helpers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace LightStreamWeb.Models.AccountServices
{
    public class SecurityInformationPageModel
    {
        public string OldPassword { get; set; }
        public string Password { get; set; }
        public string PasswordConfirm { get; set; }
        public string UserName { get; set; }
        public string SecurityQuestionType { get; set; }
        public string SecurityQuestionAnswer { get; set; }
        public string SecurityQuestionAnswerConfirm { get; set; }
        public IEnumerable<string> SelectedApplications { get; set; }

        public SecurityInformationPageModel()
        {
            SelectedApplications = new List<string>();
        }

        public bool ChangeUserId(CustomerUserIdDataSet customerDataSet, string newCustomerUserId, IList<int> applicationIds, ICurrentUser webUser)
        {
            Guard.AgainstNull(customerDataSet, "customerDataSet");
            customerDataSet.AccountInfo.CustomerUserId = newCustomerUserId;
            var result = DomainServiceCustomerOperations.UpdateCustomerUserId(customerDataSet.CustomerUserId);

            if (result == ChangeUserIdResultEnum.Success)
            {
                EventAuditLogHelper.PopulateEventAuditLogDataSet(customerDataSet, webUser, EventTypeLookup.EventType.ChangedUserId, null);
                return true;
            }
            return false;
        }

        public bool LinkAccountsToNewLogin(CustomerUserIdDataSet customerDataSet, string newCustomerUserId, IList<int> applicationIds, ICurrentUser webUser)
        {
            Guard.AgainstNull(customerDataSet, "customerDataSet");
            var result = DomainServiceCustomerOperations.LinkAccountsToNewLogin(customerDataSet.AccountInfo.UserId, newCustomerUserId, applicationIds.ToList());

            if (result.Result == LinkAccountToNewResultEnum.Success)
            {
                EventAuditLogHelper.PopulateEventAuditLogDataSet(customerDataSet, webUser, EventTypeLookup.EventType.ChangedUserId, null);
                return true;
            }
            return false;
        }

        public bool UpdatePassword(CustomerUserIdDataSet customerDataSet, ICurrentUser webUser)
        {
            Guard.AgainstNull(customerDataSet, "customerDataSet");
            var result = DomainServiceCustomerOperations.ChangePassword(customerDataSet.AccountInfo.UserId, Password);
            SessionUtility.RefreshAccountInfo();

            if (result != ChangePasswordResultEnum.Success)
            {
                return false;
            }
            EventAuditLogHelper.PopulateEventAuditLogDataSet(customerDataSet, webUser, EventTypeLookup.EventType.ChangedPassword, null);
            return true;
        }

        public bool UpdateSecurityQuestionAndAnswser(CustomerUserIdDataSet customerDataSet, ICurrentUser webUser)
        {
            Guard.AgainstNull(customerDataSet, "customerDataSet");
            try
            {
                customerDataSet.AccountInfo.SecurityQuestion = SecurityQuestionLookup.GetEnumeration(SecurityQuestionType);
                customerDataSet.AccountInfo.SecurityAnswer = SecurityQuestionAnswer;
                DomainServiceCustomerOperations.UpdateSecurityQuestion(customerDataSet.CustomerUserIdSecurityQuestion);
                SessionUtility.RefreshAccountInfo();
                EventAuditLogHelper.PopulateEventAuditLogDataSet(customerDataSet, webUser, EventTypeLookup.EventType.ChangedSecurityInfo, null);
                return true;
            }
            catch (Exception e1)
            {
                customerDataSet.AccountInfo.RejectChanges();
                LightStreamLogger.WriteError(e1);
                return false;
            }
        }

        public LinkAccountResultEnum MoveAccount(CustomerUserIdDataSet customerUserIdDataSet, int targetUserId, List<int> applicationIds, ICurrentUser webUser)
        {
            // Check the 2-ssn rule.
            return !DomainServiceCustomerOperations.Check2SSNRule(customerUserIdDataSet.AccountInfo.UserId, targetUserId, applicationIds)
                        ? LinkAccountResultEnum.FailsSSNRule
                        : DomainServiceCustomerOperations.LinkAccounts(customerUserIdDataSet.AccountInfo.UserId, targetUserId, applicationIds);
        }

        public List<AccountContactInfo> GetAccountSyncData(int sourceUserId, int targetUserId)
        {
            var result = DomainServiceCustomerOperations.GetAccountSyncDifferences(sourceUserId, targetUserId);
            return result.AccountDetailsList;
        }

        public LinkAccountResultEnum SyncAndMoveAccount(string accountSettings)
        {
            var set = JObject.Parse(accountSettings).SelectToken("Account").ToString();
            var accountSettingsList = JsonConvert.DeserializeObject<List<object>>(set);


            var primarySettings = JsonConvert.DeserializeObject<Dictionary<string, object>>(accountSettingsList.First().ToString());
            var secondarySettings = new Dictionary<string, object>();

            if (accountSettingsList.Count > 1)
            {
                secondarySettings = JsonConvert.DeserializeObject<Dictionary<string, object>>(accountSettingsList.Last().ToString());
            }

            var result = DomainServiceCustomerOperations.SyncAndMoveAccount(primarySettings, secondarySettings);


            return result.Success;
        }
    }
}