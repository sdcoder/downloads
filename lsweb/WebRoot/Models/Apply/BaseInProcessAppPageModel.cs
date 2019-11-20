using FirstAgain.Common.Web;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.SharedTypes.ContentManagement;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using FirstAgain.Domain.SharedTypes.LoanApplication.LoanApplicationPostData;
using LightStreamWeb.App_State;
using LightStreamWeb.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace LightStreamWeb.Models.Apply
{
    public class BaseInProcessAppPageModel : BaseLightstreamPageModel
    {
        protected LoanApplicationDataSet LADS { get; private set; }

        public BaseInProcessAppPageModel(LoanApplicationDataSet lads)
        {
            BodyClass = "pre-funding";
            LADS = lads;
        }

        public LoanApplicationDataSet.ApplicationRow Application
        {
            get
            {
                return LADS.Application[0];
            }
        }

        public int ApplicationId
        {
            get
            {
                return Application.ApplicationId;
            }
        }

        public string Ctx
        {
            get
            {
                return WebSecurityUtility.Scramble(Application.ApplicationId);
            }
        }

        public PurposeOfLoanLookup.PurposeOfLoan PurposeOfLoan
        {
            get
            {
                if (LADS == null || LADS.ApplicationDetail == null || LADS.ApplicationDetail.Count == 0)
                {
                    return PurposeOfLoanLookup.PurposeOfLoan.NotSelected;
                }
                return LADS.ApplicationDetail[0].PurposeOfLoan;
            }
        }

        /// <summary>
        /// Returns the applicant names text that can be displayed directly on a web page,
        /// e.g. "Joe Schmo and Jane Schmo"
        /// </summary>
        public string ApplicantNamesText
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("{0} {1}", Application.GetPrimaryApplicant().FirstName, Application.GetPrimaryApplicant().LastName);
                if (Application.IsJoint)
                {
                    sb.AppendFormat(" and {0} {1}", Application.GetSecondaryApplicant().FirstName, Application.GetSecondaryApplicant().LastName);
                }

                return sb.ToString();
            }
        }


        public ApplyPage ApplyPage
        {
            get
            {
                return new ContentManager().Get<ApplyPage>();
            }
        }

        public decimal Amount
        {
            get
            {
                return LADS.ApplicationDetail[0].Amount;
            }
        }

        public string PurposeOfLoanCaption
        {
            get
            {
                return LADS.ApplicationDetail[0].PurposeOfLoan.ToString();
            }
        }

        internal CompleteLoanApplicationResultEnum CompleteLoanApplication(CustomerUserCredentialsPostData userCredentials, ICurrentUser WebUser)
        {
            // check for changes required if changed from home improvement to other
            if (!LADS.ApplicationDetail[0].PurposeOfLoan.IsHomeImprovement() && LADS.ApplicationPostalAddress.Any(a => a.ApplicationPostalAddressType == ApplicationPostalAddressTypeLookup.ApplicationPostalAddressType.ImprovedProperty))
            {
                LADS.ApplicationPostalAddress.Single(a => a.ApplicationPostalAddressType == ApplicationPostalAddressTypeLookup.ApplicationPostalAddressType.ImprovedProperty).Delete();
            }
            LADS.CustomerUserId[0].PasswordHash = FirstAgain.Domain.SharedTypes.Security.PasswordHash.GetPasswordHash(userCredentials.Password);
            LADS.CustomerUserId[0].IsTemporary = false;
            LADS.CustomerUserId[0].CustomerUserId = userCredentials.UserName;

            if (LADS.CustomerUserIdSecurityQuestion.Count == 0)
            {
                LoanApplicationDataSet.CustomerUserIdSecurityQuestionRow customerUserIdSecurityQuestionRow = LADS.CustomerUserId[0].NewRelatedCustomerUserIdSecurityQuestionRow(userCredentials.SecurityQuestionType, userCredentials.SecurityQuestionAnswer);
                LADS.CustomerUserIdSecurityQuestion.AddCustomerUserIdSecurityQuestionRow(customerUserIdSecurityQuestionRow);
            }
            else
            {
                LADS.CustomerUserIdSecurityQuestion[0].SecurityQuestion = userCredentials.SecurityQuestionType;
                LADS.CustomerUserIdSecurityQuestion[0].SecurityAnswer = userCredentials.SecurityQuestionAnswer;
            }

            return DomainServiceLoanApplicationOperations.CompleteLoanApplication(LADS, WebUser.UniqueCookie, WebUser.IPAddress);
        }
    }
}