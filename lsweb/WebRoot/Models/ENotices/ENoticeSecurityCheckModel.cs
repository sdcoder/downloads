using FirstAgain.Domain.SharedTypes.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.ENotices
{
    public class ENoticeSecurityCheckModel
    {
        public DateTime DateOfBirth { get; set; }
        public string SSN { get; set; }
        public int ApplicantId { get; set; }

        public string ErrorMessage { get; private set; }
        public bool IsValid { get; private set; }

        public void Validate(CustomerUserIdDataSet customerData)
        {
            if (DateOfBirth == DateTime.MinValue)
            {
                ErrorMessage = "Date of Birth is required";
                if (string.IsNullOrEmpty(SSN))
                {
                    ErrorMessage = "SSN and Date of Birth are required";
                }

                IsValid = false;
                return;
            }
            if (string.IsNullOrEmpty(SSN))
            {
                ErrorMessage = "SSN is required";
                IsValid = false;
                return;
            }
            if (customerData == null)
            {
                ErrorMessage = "Customer data not found - your session may have expired";
                IsValid = false;
                return;
            }

            var applicant = customerData.Applicant.FirstOrDefault(x => x.ApplicantId == ApplicantId);
            if (applicant == null)
            {
                ErrorMessage = "Applicant not found - your session may have expired";
                IsValid = false;
                return;
            }

            IsValid = applicant.DateOfBirth == DateOfBirth && applicant.SocialSecurityNumber.EndsWith(SSN) && SSN.Length == 4;
            if (!IsValid)
            {
                ErrorMessage = "The security information you provided does not match our records.";
            }
        }
    }
}