using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Common.Extensions;
using System;
using System.ComponentModel.DataAnnotations;
using FirstAgain.Domain.Common;
using System.Text;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Client;
using LightStreamWeb.Helpers;
using LightStreamWeb.App_State;
using FirstAgain.Common;
using System.ComponentModel;

namespace LightStreamWeb.Models.SignIn
{
    public class ConfirmEmailAddressModel : BaseLightstreamPageModel
    {
        public const string BIND_FIELDS = "ApplicantEmail,CoApplicantEmail";
        public ConfirmEmailAddressModel()
        {
            BodyClass = "sign-in";
        }

        [MaxLength(255)]
        [DataType(DataType.EmailAddress)]
        public string ApplicantEmail { get; set; }

        [DataType(DataType.EmailAddress)]
        [MaxLength(255)]
        public string CoApplicantEmail { get; set; }

        [ReadOnly(true)]
        public string ErrorMessage { get; set; }

        #region public properties that are set by the model, to be used by the view, but not modified
        public string ApplicantNamesText { get; private set; }
        public string Introduction { get; private  set; }

        public string OriginalApplicantEmail { get; private set; }
        public string OriginalCoApplicantEmail { get; private set; }

        public bool ApplicantEmailIsUnconfirmed { get; private set; }
        public bool CoApplicantEmailIsUnconfirmed { get; private set; }

        public string ApplicantName { get; private set; }
        public string CoApplicantName { get; private set; }
        #endregion

        public void Populate(AccountContactInfo accountContactInfo)
        {
            Guard.AgainstNull<AccountContactInfo>(accountContactInfo, "accountContactInfo");

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0} {1}", accountContactInfo.ApplicantInfo.FirstName, accountContactInfo.ApplicantInfo.LastName);
            if (accountContactInfo.CoApplicantInfo != null)
            {
                sb.AppendFormat(" and {0} {1}", accountContactInfo.CoApplicantInfo.FirstName, accountContactInfo.CoApplicantInfo.LastName);
            }

            ApplicantNamesText = sb.ToString();

            Introduction = "We believe the email address that we have on file for your account is invalid. To update your email address or to confirm that the email address below is valid, please enter your current valid email address below before proceeding. Thank you.";
            if (accountContactInfo.CoApplicantInfo != null)
            {
                if (accountContactInfo.ApplicantInfo.HomeEmailAddress.IsConfirmed != accountContactInfo.CoApplicantInfo.HomeEmailAddress.IsConfirmed)
                {
                    string name = (!accountContactInfo.ApplicantInfo.HomeEmailAddress.IsConfirmed) ? accountContactInfo.ApplicantInfo.HomeEmailAddress.EmailAddress : accountContactInfo.CoApplicantInfo.HomeEmailAddress.EmailAddress;
                    Introduction = string.Format("We believe the email address that we have on file for {0} is invalid. To update the email address or to confirm that the email address below is valid, please enter a current valid email address below before proceeding. Thank you.", name);
                }
                else
                {
                    Introduction = "We believe the email addresses that we have on file for your account are invalid. To update your email addresses or to confirm that the email addresses below are valid, please enter your current valid email addresses below before proceeding. Thank you."; 
                }
            }

            ApplicantName = accountContactInfo.ApplicantInfo.Name;
            OriginalApplicantEmail = accountContactInfo.ApplicantInfo.HomeEmailAddress.EmailAddress;
            ApplicantEmailIsUnconfirmed = !accountContactInfo.ApplicantInfo.HomeEmailAddress.IsConfirmed;

            if (accountContactInfo.CoApplicantInfo != null)
            {
                CoApplicantName = accountContactInfo.CoApplicantInfo.Name;
                OriginalCoApplicantEmail = accountContactInfo.CoApplicantInfo.HomeEmailAddress.EmailAddress;
                CoApplicantEmailIsUnconfirmed = !accountContactInfo.CoApplicantInfo.HomeEmailAddress.IsConfirmed;
            }
        }

        // for unit tests, most logic here
        public void GetConfirmationResult(AccountContactInfo accountContactInfo, out string note, out EventTypeLookup.EventType eventType)
        {
            bool isJoint = accountContactInfo.CoApplicantInfo != null && accountContactInfo.CoApplicantInfo.HomeEmailAddress != null;

            if (!ApplicantEmail.IsNullOrEmpty() && EmailAddressValidator.IsValidEmail(ApplicantEmail))
            {
                if (accountContactInfo.ApplicantInfo.HomeEmailAddress.EmailAddress.Equals(ApplicantEmail, StringComparison.OrdinalIgnoreCase))
                {
                    accountContactInfo.ApplicantEmailConfirmType = AccountContactInfo.EmailConfirmType.Confirmed;
                }
                else
                {
                    accountContactInfo.ApplicantEmailConfirmType = AccountContactInfo.EmailConfirmType.Updated;
                }
                accountContactInfo.ApplicantInfo.HomeEmailAddress.IsConfirmed = true;
                accountContactInfo.ApplicantInfo.HomeEmailAddress.EmailAddress = ApplicantEmail;
            }
            if (isJoint && !CoApplicantEmail.IsNullOrEmpty() && EmailAddressValidator.IsValidEmail(CoApplicantEmail))
            {
                if (accountContactInfo.CoApplicantInfo.HomeEmailAddress.EmailAddress.Equals(CoApplicantEmail, StringComparison.OrdinalIgnoreCase))
                {
                    accountContactInfo.CoApplicantEmailConfirmType = AccountContactInfo.EmailConfirmType.Confirmed;
                }
                else
                {
                    accountContactInfo.CoApplicantEmailConfirmType = AccountContactInfo.EmailConfirmType.Updated;
                }

                accountContactInfo.CoApplicantInfo.HomeEmailAddress.IsConfirmed = true;
                accountContactInfo.CoApplicantInfo.HomeEmailAddress.EmailAddress = CoApplicantEmail;
            }

            // create note text
            StringBuilder noteText = new StringBuilder();

            if (!isJoint)
            {
                if (accountContactInfo.ApplicantEmailConfirmType == AccountContactInfo.EmailConfirmType.Updated)
                {
                    noteText.Append("email address updated");
                }
                else if (accountContactInfo.ApplicantEmailConfirmType == AccountContactInfo.EmailConfirmType.Confirmed)
                {
                    noteText.Append("email address confirmed");
                }
            }
            else
            {
                if (accountContactInfo.ApplicantEmailConfirmType == AccountContactInfo.EmailConfirmType.Updated)
                {
                    noteText.Append("email address updated for applicant");
                }
                else if (accountContactInfo.ApplicantEmailConfirmType == AccountContactInfo.EmailConfirmType.Confirmed)
                {
                    noteText.Append("email address confirmed for applicant");
                }

                if (accountContactInfo.CoApplicantEmailConfirmType == AccountContactInfo.EmailConfirmType.Updated)
                {
                    if (noteText.Length > 1)
                    {
                        noteText.Append(", ");
                    }
                    noteText.Append("email address updated for co-applicant");
                }
                else if (accountContactInfo.CoApplicantEmailConfirmType == AccountContactInfo.EmailConfirmType.Confirmed)
                {
                    if (noteText.Length > 1)
                    {
                        noteText.Append(", ");
                    }
                    noteText.Append("email address confirmed for co-applicant");
                }

            }

            /////////////////////////////////////////////////
            //
            // Set appropriate event type, SR 2073, req. 1.1
            //
            eventType = EventTypeLookup.EventType.UpdateContactInfo;
            if (accountContactInfo.ApplicantEmailConfirmType == AccountContactInfo.EmailConfirmType.Confirmed
                && (accountContactInfo.CoApplicantEmailConfirmType.IsOneOf(AccountContactInfo.EmailConfirmType.None, AccountContactInfo.EmailConfirmType.Confirmed)))
            {
                eventType = EventTypeLookup.EventType.ConfirmedEmailAddress;
            }

            note = noteText.ToString();

        }

        // this method calls the back end. Not suitable for unit testing
        public bool Submit(CustomerUserIdDataSet customerData, ICurrentUser webUser)
        {
            Guard.AgainstNull<CustomerUserIdDataSet>(customerData, "customerDataSet");

            string note = string.Empty;
            EventTypeLookup.EventType eventType;
            var accountContactInfo = customerData.ContactInfo;

            Populate(accountContactInfo);
            if (Validate())
            {
                // run logic to get proper note and event type, and submit to back end
                GetConfirmationResult(accountContactInfo, out note, out eventType);
                DomainServiceCustomerOperations.UpdateContactInfo(accountContactInfo, EventAuditLogHelper.PopulateEventAuditLogDataSet(customerData, webUser, eventType, note));

                return true;
            }

            return false;
        }

        private bool Validate()
        {
            if (ApplicantEmailIsUnconfirmed)
            {
                if (ApplicantEmail.IsNullOrEmpty())
                {
                    ErrorMessage = "Applicant email address is required.";
                    return false;
                }
                if (!EmailAddressValidator.IsValidEmail(ApplicantEmail))
                {
                    ErrorMessage = "Please enter a valid applicant email address.";
                    return false;
                }
            }

            if (CoApplicantEmailIsUnconfirmed)
            {
                if (CoApplicantEmail.IsNullOrEmpty())
                {
                    ErrorMessage = "Co-applicant email address is required.";
                    return false;
                }
                if (!EmailAddressValidator.IsValidEmail(CoApplicantEmail))
                {
                    ErrorMessage = "Please enter a valid co-applicant email address.";
                    return false;
                }
            }

            return true;
        }
    }
}