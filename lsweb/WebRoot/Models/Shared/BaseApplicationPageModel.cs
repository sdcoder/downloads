using FirstAgain.Common.Web;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace LightStreamWeb.Models.Shared
{
    public class BaseApplicationPageModel : BaseLightstreamPageModel
    {
        protected LoanApplicationDataSet LADS { get; private set; }

        public BaseApplicationPageModel(LoanApplicationDataSet lads)
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

        public virtual PurposeOfLoanLookup.PurposeOfLoan GetPurposeOfLoan()
        {
            if (LADS == null || LADS.ApplicationDetail == null || LADS.ApplicationDetail.Count == 0)
            {
                return PurposeOfLoanLookup.PurposeOfLoan.NotSelected;
            }
            return LADS.ApplicationDetail[0].PurposeOfLoan;
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

    }
}