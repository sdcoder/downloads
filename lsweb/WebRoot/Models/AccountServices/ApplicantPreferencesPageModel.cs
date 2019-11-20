using System;
using System.Collections.Generic;
using System.Web;
using FirstAgain.Common;
using FirstAgain.Common.Logging;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.SharedTypes.Customer;
using LightStreamWeb.ServerState;
using LightStreamWeb.App_State;
using LightStreamWeb.Helpers;
using Resources;

namespace LightStreamWeb.Models.AccountServices
{
    public class ApplicantPreferencesPageModel
    {
        public IList<PreferencePageModel> PrivacyPreferences { get; set; }
        public IList<PreferencePageModel> EmailPreferences { get; set; }

        public bool UpdatePreferences(int applicantId, CustomerUserIdDataSet customerDataSet, ICurrentUser webUser)
        {
            Guard.AgainstNull(customerDataSet, "customerDataSet");
            if (applicantId == 0)
            {
                LightStreamLogger.WriteWarning("ApplicantId is zero in ApplicantPreferencesPageModel.UpdatePreferences");
                throw new HttpException(500, FAMessages.GenericError);
            }

            var accountContactInfo = customerDataSet.ContactInfo;
            if (accountContactInfo.ApplicantInfo.ApplicantId == applicantId)
            {
                UpdateSolicitationPreferences(customerDataSet, accountContactInfo.ApplicantInfo.SSN);
            }
            else if (accountContactInfo.CoApplicantInfo != null && accountContactInfo.CoApplicantInfo.ApplicantId == applicantId)
            {
                UpdateSolicitationPreferences(customerDataSet, accountContactInfo.CoApplicantInfo.SSN);
            }
            else
            {
                LightStreamLogger.WriteWarning("Applicant not found in ApplicantPreferencesPageModel.UpdatePreferences");
                throw new HttpException(500, FAMessages.GenericError);
            }

            try
            {
                DomainServiceCustomerOperations.UpdateCustomerUserIdData(-1, customerDataSet, EventAuditLogHelper.PopulateEventAuditLogDataSet(customerDataSet, webUser, EventTypeLookup.EventType.UpdateEmailPreferences, null));
                SessionUtility.RefreshAccountInfo();
                return true;
            }
            catch (Exception e1)
            {
                customerDataSet.CustomerSolicitationPreference.RejectChanges();
                LightStreamLogger.WriteError(e1);
                return false;
            }
        }

        private void UpdateSolicitationPreferences(CustomerUserIdDataSet customerDataSet, string socialSecurityNumber)
        {
            foreach (var preferencePageModel in PrivacyPreferences)
            {
                customerDataSet.SetCustomerPrivacyPreference(socialSecurityNumber, SolicitationPreferenceLookup.GetEnumeration(preferencePageModel.Key), preferencePageModel.IsSelected);
            }

            foreach (var preferencePageModel in EmailPreferences)
            {
                customerDataSet.SetCustomerEmailPreference(socialSecurityNumber, SolicitationPreferenceLookup.GetEnumeration(preferencePageModel.Key), preferencePageModel.IsSelected);
            }
        }
    }
}