using FirstAgain.Common.Logging;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.SharedTypes.Customer;
using LightStreamWeb.App_State;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace LightStreamWeb.Models.AccountServices
{
    public class AccountServicesContactUsModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Message { get; set; }

        [Required]
        public string Subject { get; set; }

        public int? ApplicationId { get; set; }
        public bool MessageSent { get; private set; }

        private CustomerUserIdDataSet _customerUserIdDataSet = null;
        private CustomerApplicationsDates _customerApplicationsDates = null;
        private ICurrentUser _webUser = null;
        public AccountServicesContactUsModel(CustomerUserIdDataSet c, CustomerApplicationsDates d, ICurrentUser user)
        {
            _customerUserIdDataSet = c;
            _customerApplicationsDates = d;
            _webUser = user;
        }

        protected class ReasonSelectListItem : SelectListItem
        {
            public DateTime Date { get; set; }
            public int SortOrder { get; set; }
            public int? ApplicationId { get; set; }
        }

        public IEnumerable<SelectListItem> ReasonOptions
        {
            get
            {
                List<ReasonSelectListItem> results = new List<ReasonSelectListItem>();
                results.Add(new ReasonSelectListItem() { Text = "General Question or Comment", Value = "", SortOrder = 3 });


                foreach (CustomerUserIdDataSet.ApplicationRow application in _customerUserIdDataSet.Application.Rows)
                {
                    CustomerUserIdDataSet.ApplicationDetailRow detail = _customerUserIdDataSet.ApplicationDetail.FindByApplicationId(application.ApplicationId);
                    ReasonSelectListItem item = new ReasonSelectListItem();

                    item.Value = application.ApplicationId.ToString();
                    if (application.IsFunded)
                    {
                        item.Text = string.Format("Account {0}", (detail.IsApplicationNickNameNull()) ? "#" + application.ApplicationId.ToString() : detail.ApplicationNickName);
                        item.Date = (DateTime)application.FundingDate;
                        item.SortOrder = 2;
                    }
                    else
                    {
                        item.Text = string.Format("Application {0}", (detail.IsApplicationNickNameNull()) ? "#" + application.ApplicationId.ToString() : detail.ApplicationNickName);
                        item.ApplicationId = application.ApplicationId;
                        item.Date = detail.SubmittedDate;
                        item.SortOrder = 1;
                    }
                    results.Add(item);
                }

                if (_webUser.ApplicationId.HasValue)
                {
                    return results.OrderByDescending(x => x.ApplicationId == _webUser.ApplicationId.Value).ThenBy(x => x.SortOrder).ThenByDescending(x => x.Date); 
                }
                return results.OrderBy(x => x.SortOrder).ThenByDescending(x => x.Date); 
            }
        }

        public IEnumerable<SelectListItem> NameOptions
        {
            get
            {
                List<string> ssns = new List<string>();
                List<string> nameAL = new List<string>();

                foreach (CustomerUserIdDataSet.ApplicationRow application in _customerUserIdDataSet.Application.Rows)
                {
                    CustomerUserIdDataSet.ApplicationDetailRow detail = _customerUserIdDataSet.ApplicationDetail.FindByApplicationId(application.ApplicationId);
                    if (_customerApplicationsDates.IsAppViewable(application.ApplicationId))
                    {
                        CustomerUserIdDataSet.ApplicantRow[] applicants = application.GetApplicantRows();
                        foreach (CustomerUserIdDataSet.ApplicantRow applicant in applicants)
                        {
                            string name = string.Format("{0} {1}", applicant.FirstName, applicant.LastName);
                            if ((!ssns.Contains(applicant.SocialSecurityNumber)) && !nameAL.Contains(name))
                            {
                                ssns.Add(applicant.SocialSecurityNumber);
                                nameAL.Add(name);
                            }
                        }
                        if (application.IsJoint && applicants.Length == 2)
                        {
                            string name = string.Format("{0} {1} and {2} {3}", applicants[0].FirstName, applicants[0].LastName, applicants[1].FirstName, applicants[1].LastName);

                            if (!nameAL.Contains(name))
                            {
                                nameAL.Add(name);
                            }
                        }
                    }
                }

                return nameAL
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct()
                    .Select(x => new SelectListItem()
                    {
                        Text = x,
                        Value = x,
                        Selected = x.Equals(EmailAddress, StringComparison.InvariantCultureIgnoreCase)
                    });
            }
        }

        
        public IEnumerable<SelectListItem> EmailAddressOptions
        {
            get
            {
                List<string> emailAL = new List<string>();
                foreach (CustomerUserIdDataSet.ApplicantEmailRow applicantEmail in _customerUserIdDataSet.ApplicantEmail)
                {
                    emailAL.Add(applicantEmail.EmailAddress);
                }

                foreach (CustomerUserIdDataSet.ApplicationRow application in _customerUserIdDataSet.Application)
                {
                    if (_customerApplicationsDates.IsAppViewable(application.ApplicationId))
                    {
                        emailAL.Add(application.PrimaryApplicant.EmailAddress.EmailAddress);
                        if (application.SecondaryApplicant != null)
                        {
                            emailAL.Add(application.SecondaryApplicant.EmailAddress.EmailAddress);
                        }
                    }
                }

                return emailAL
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct()
                    .Select(x => new SelectListItem() { 
                        Text = x, 
                        Value = x, 
                        Selected = x.Equals(EmailAddress, StringComparison.InvariantCultureIgnoreCase) 
                    });
            }
        }

        public AccountServicesContactUsModel()
        {

        }

        public int? UserId
        {
            get
            {
                if (_customerUserIdDataSet == null)
                {
                    return (int?)null;
                }

                return _customerUserIdDataSet.UserId;
            }
        }

        public void Send()
        {
            try
            {
                if (ApplicationId.HasValue)
                {
                    DomainServiceCustomerOperations.SendContactUsEmail(null, ApplicationId, CorrespondenceInboxLookup.CorrespondenceInbox.CustomerService, EmailAddress, Name, Subject, Message);
                }
                else
                {
                    DomainServiceCustomerOperations.SendContactUsEmail(UserId, null, CorrespondenceInboxLookup.CorrespondenceInbox.CustomerService, EmailAddress, Name, Subject, Message);
                }

                MessageSent = true;
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
            }
        }

    }
}