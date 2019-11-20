using FirstAgain.Common;
using FirstAgain.Domain.ServiceModel.Contracts.MessageContracts.CustomerOperations;
using LightStreamWeb.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.AccountServices
{
    public class AccountServicesPageModel : BaseLightstreamPageModel, IPageWithHeading
    {
        protected GetAccountInfoResponse _accountInfo = null;

        public AccountServicesPageModel(GetAccountInfoResponse accountInfo)
        {
            _accountInfo = accountInfo;
        }

        public AccountServicesPageModel()
        {
            Heading = "Account Services";
        }

        public AccountServicesPageModel(string bodyClass) : base()
        {
            this.BodyClass = bodyClass;
        }

        public string Heading { get; set; }

        public int ActiveApplicationId
        {
            get
            {
                if (!WebUser.ApplicationId.HasValue)
                {
                    WebUser.ApplicationId = _accountInfo.ApplicationsDates.MostRecentFundedOrViewableApplicationId;
                }

                return WebUser.ApplicationId.Value;
            }
        }

        public string GetApplicantNames()
        {
            Guard.AgainstNull<GetAccountInfoResponse>(_accountInfo, "_accountInfo");
            var contactInfo = _accountInfo.CustomerUserIdDataSet.ContactInfo;
            string result;
            //customer name
            if (contactInfo.CoApplicantInfo != null)
            {
                if (contactInfo.ApplicantInfo.LastName.ToLower().Equals(contactInfo.CoApplicantInfo.LastName.ToLower()))
                    result = string.Format("{0} and {1} {2}", contactInfo.ApplicantInfo.FirstName, contactInfo.CoApplicantInfo.FirstName, contactInfo.ApplicantInfo.LastName);
                else
                    result = string.Format("{0} {1} and {2} {3}", contactInfo.ApplicantInfo.FirstName, contactInfo.ApplicantInfo.LastName, contactInfo.CoApplicantInfo.FirstName, contactInfo.CoApplicantInfo.LastName);
            }
            else
            {
                result = string.Format("{0} {1}", contactInfo.ApplicantInfo.FirstName, contactInfo.ApplicantInfo.LastName);
            }
            return result;
        }

    }


}