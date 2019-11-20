using System;
using System.Web;
using FirstAgain.Common;
using FirstAgain.Common.Logging;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.SharedTypes.Customer;
using LightStreamWeb.ServerState;
using LightStreamWeb.App_State;
using FirstAgain.Common.Extensions;

namespace LightStreamWeb.Models.AccountServices
{
    public class ContactInformationPageModel
    {
        public string FirstName { get; set; }
        public string MiddleInitial { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public string Address { get; set; }
        public string UnitType { get; set; }
        public string UnitValue { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public ApplicantPhonePageModel HomePhone { get; set; }
        public ApplicantPhonePageModel WorkPhone { get; set; }

        public void UpdateAccountInfo(int applicantId, CustomerUserIdDataSet customerDataSet, ICurrentUser webUser)
        {
            Guard.AgainstNull(customerDataSet, "customerDataSet");
            if (applicantId == 0)
            {
                LightStreamLogger.WriteWarning("ApplicantId is zero in ContactInformationPageModel.UpdateAccountInfo");
                throw new HttpException(500, Resources.FAMessages.GenericError);
            }

            var accountContactInfo = customerDataSet.ContactInfo;
            if (accountContactInfo.ApplicantInfo.ApplicantId == applicantId)
            {
                UpdateDataSet(accountContactInfo.ApplicantInfo);
            }
            else if (accountContactInfo.CoApplicantInfo != null && accountContactInfo.CoApplicantInfo.ApplicantId == applicantId)
            {
                UpdateDataSet(accountContactInfo.CoApplicantInfo);
            }

            DomainServiceCustomerOperations.UpdateContactInfo(accountContactInfo, Helpers.EventAuditLogHelper.PopulateEventAuditLogDataSet(customerDataSet, webUser, EventTypeLookup.EventType.UpdateContactInfo, null));
            SessionUtility.RefreshAccountInfo();
        }

        private void UpdateDataSet(AccountContactInfo.AccountInfo accountInfo)
        {
            accountInfo.HomeEmailAddress.EmailAddress = EmailAddress;

            //HomeAddress
            accountInfo.HomeAddress.Address = Address;
            accountInfo.HomeAddress.City = City;
            accountInfo.HomeAddress.State = (StateLookup.State)Enum.Parse(typeof(StateLookup.State), State);
            accountInfo.HomeAddress.ZipCode = ZipCode;
            if (UnitType.IsNotNull() && !string.IsNullOrEmpty(UnitValue))
            {
                accountInfo.HomeAddress.UnitValue = UnitValue;
                accountInfo.HomeAddress.UnitType =
                    (PostalAddressSecondaryUnitTypeLookup.PostalAddressSecondaryUnitType)
                        Enum.Parse(typeof(PostalAddressSecondaryUnitTypeLookup.PostalAddressSecondaryUnitType), UnitType);
            }
            else
            {
                accountInfo.HomeAddress.UnitType = PostalAddressSecondaryUnitTypeLookup.PostalAddressSecondaryUnitType.NotSelected;
                accountInfo.HomeAddress.UnitValue = null;
            }

            //HomePhone
            accountInfo.HomePhone.AreaCode = HomePhone.AreaCode;
            accountInfo.HomePhone.Exchange = HomePhone.CentralOfficeCode;
            accountInfo.HomePhone.LocalNumber = HomePhone.LineNumber;

            if (WorkPhone != null)
            {
                if (accountInfo.WorkPhone == null)
                {
                    accountInfo.WorkPhone = new AccountContactInfo.PhoneNumber();
                }
                //WorkPhone
                accountInfo.WorkPhone.AreaCode = WorkPhone.AreaCode;
                accountInfo.WorkPhone.Exchange = WorkPhone.CentralOfficeCode;
                accountInfo.WorkPhone.LocalNumber = WorkPhone.LineNumber;
                accountInfo.WorkPhone.Extension = WorkPhone.Extension;
            }
        }

        public static ContactInformationPageModel Populate(AccountContactInfo.AccountInfo applicantInfo)
        {
            Guard.AgainstNull(applicantInfo, "applicantInfo");
            return new ContactInformationPageModel
            {
                FirstName = applicantInfo.FirstName,
                MiddleInitial = applicantInfo.MiddleInitial,
                LastName = applicantInfo.LastName,
                FullName = applicantInfo.Name,
                EmailAddress = applicantInfo.HomeEmailAddress.EmailAddress,
                Address = applicantInfo.HomeAddress.Address,
                UnitType = applicantInfo.HomeAddress.UnitType.ToString(),
                UnitValue = applicantInfo.HomeAddress.UnitValue,
                City = applicantInfo.HomeAddress.City,
                State = applicantInfo.HomeAddress.State.ToString(),
                ZipCode = applicantInfo.HomeAddress.ZipCode,
                HomePhone = ApplicantPhonePageModel.Populate(applicantInfo.HomePhone),
                WorkPhone = ApplicantPhonePageModel.Populate(applicantInfo.WorkPhone)
            };
        }
    }
}