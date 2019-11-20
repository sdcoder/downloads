using FirstAgain.Correspondence.ServiceModel.Client;
using FirstAgain.Correspondence.SharedTypes;
using FirstAgain.Domain.Common;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using FirstAgain.Domain.SharedTypes.Security;
using LightStreamWeb.App_State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.ApplicationStatus
{
    /// <summary>
    /// PendingPageModel is used for any status where the user is presented with a generic "pending" message, and little ir no user input is required.
    /// 
    /// The only action that is supported is "AddCoApplicant"
    /// </summary>
    public class PendingPageModel : ApplicationStatusPageModel
    {

        #region Constants

        private static readonly string[] FraudAnalystRoles =
        {
            "Fraud Analyst 1",
            "Fraud Analyst 2",
            "Fraud Analyst 3"
        };

        #endregion

        #region Constructors

        public PendingPageModel(FirstAgain.Domain.SharedTypes.Customer.CustomerUserIdDataSet cuidds, LoanOfferDataSet loanOfferDataSet)
            : base(cuidds, loanOfferDataSet)
        {
            this._currentApplicationData = Helpers.DataSetToSessionStateMapper.Map(ApplicationId, cuidds, loanOfferDataSet);
            Populate();
        }

        public PendingPageModel(ICurrentApplicationData appData)
            : base(appData)
        {
            Populate();
        }

        #endregion

        public List<string> Statements { get; private set; }

        public string Heading { get; set; }

        public override PurposeOfLoanLookup.PurposeOfLoan GetPurposeOfLoan()
        {
            if (CurrentStatus == ApplicationStatusTypeLookup.ApplicationStatusType.ApprovedNLTR && _loanOfferDataSet != null)
            {
                return _loanOfferDataSet.LatestApprovedLoanTerms.PurposeOfLoan;
            }
            var purposeOfLoan = base.GetPurposeOfLoan();
            if (purposeOfLoan.IsSecured())
            {
                return purposeOfLoan.GetUnsecuredPurpose();
            }

            return purposeOfLoan;
        }

        private void Populate()
        {
            RateModalDisplay = RateModalDisplayType.ReadOnlyPurposeOfLoan;

            Statements = new List<string>();

            switch (CurrentStatus)
            {
                case ApplicationStatusTypeLookup.ApplicationStatusType.InProcess:
                case ApplicationStatusTypeLookup.ApplicationStatusType.Pending:
                    if (!_currentApplicationData.IsRegOApplication)
                    {
                        Statements.Add("Thank you for your recently submitted loan application. Your application is currently being processed.");
                        Heading = "Loan Application in Process";
                    }
                    else
                    {
                        Statements.Add("Thank you for your recently submitted loan application.");
                        Statements.Add("Your application is under review for Regulation O compliance with the SunTrust Credit Risk department.  " +
                                        "Once we have received the result of the Regulation O compliance review, LightStream will email you regarding our credit decision or any questions we may have.  " +
                                        "If you would like to inquire the status of the compliance review, please contact your Client Advisor(s) for an update.");

                        Heading = "Loan Application in Process";
                    }

                    break;
                case ApplicationStatusTypeLookup.ApplicationStatusType.PendingV:// => This is to handle false PendingV status accounts - See PendingV Action for details
                    Statements.Add("Thank you for your recently submitted loan application. Your application is currently being processed.");
                    Heading = "Loan Application in Process";
                    break;
                case ApplicationStatusTypeLookup.ApplicationStatusType.PendingQ:
                    Statements.Add(string.Format("Thank you for your recently submitted loan application (Application Reference #{0}).  After reviewing your information, we have a few questions.", ApplicationId));
                    Statements.Add(string.Format("Please contact us at your earliest convenience at <a href=\"tel:{0}\">{0}</a>.", GetContactPhoneNumber()));
                    Heading = "Loan Application Pending";
                    break;
                case ApplicationStatusTypeLookup.ApplicationStatusType.ApprovedNLTR:
                    RateModalDisplay = RateModalDisplayType.ApplicationRates;
                    if (Is_Approved_NLTR_Q())
                    {
                        Statements.Add(string.Format("Thank you for your recently submitted new loan terms request (Reference #{0}).  After reviewing your information, we have a few questions.", ApplicationId));
                        Statements.Add(string.Format("Please contact us at your earliest convenience at <a href=\"tel:{0}\">{0}</a>.", GetUnderwritingPhoneNumber()));
                        Heading = "New Loan Terms Request Pending";
                    }
                    else
                    {
                        Statements.Add(string.Format("The new loan terms request you recently submitted is currently in process. We will send you an email {0} regarding our credit decision or questions that we may have.",
                            DateUtility.CustomerResponseTimeFrameText));
                        Statements.Add("Thank You.");
                        Heading = "New Loan Terms Request in Process";
                    }
                    break;
            }

            if (AddCoApplicantIsEnabled())
            {
                Statements.Add("To note, we have now enabled the option for you to add a co-applicant to your application. At this time you may click the Submit Joint Application button below to submit a joint application.");
            }
        }

        private string GetContactPhoneNumber()
        {
            if (_currentApplicationData != null && _currentApplicationData.IsAutoApprovalEligible)
            {
                return BusinessConstants.Instance.PhoneNumberVerificationTeam;
            }
            else if (Application != null && (Application.FlagIsSet(FlagLookup.Flag.MLAutoApprovalEligible) || Application.FlagIsSet(FlagLookup.Flag.AutoApprovalEligible)))
            {
                return BusinessConstants.Instance.PhoneNumberVerificationTeam;
            }

            if (!string.IsNullOrEmpty(BusinessConstants.Instance.PhoneNumberVerificationTeam) && _currentApplicationData.ApplicationStatus == ApplicationStatusTypeLookup.ApplicationStatusType.PendingQ)
            {
                HstLoanApplicationDataSet hstLoanApplicationDataSet = DomainServiceLoanApplicationOperations.GetLoanApplicationHistory(ApplicationId, HstLoanApplicationDataSet.ApplicationDataTableFlags.Application, HstLoanApplicationDataSet.ApplicantDataTableFlags.None, HstLoanApplicationDataSet.LoanDataTableFlags.None, HstLoanApplicationDataSet.OtherDataTableFlags.None);

                string[] roles = DomainServiceSecurityOperations.GetUserRoles(hstLoanApplicationDataSet?.HstApplication.Last(a => a.ApplicationStatusType == ApplicationStatusTypeLookup.ApplicationStatusType.PendingQ).LastModifiedBy);

                if (Array.Exists(roles, r => FraudAnalystRoles.Contains(r)))
                {
                    return BusinessConstants.Instance.PhoneNumberVerificationTeam;
                }
            }

            return GetUnderwritingPhoneNumber();
        }

        private string GetUnderwritingPhoneNumber()
        {
            UnderwriterPhoneLogic uphone = CorrespondenceServiceCorrespondenceOperations.GetUnderwriterPhoneLogic(ApplicationId);
            // #9689 - Don't show fraud # on website
            if (uphone.HasAlert) return BusinessConstants.Instance.PhoneNumberMain;

            if (uphone.IsSenior)
            {
                if (String.IsNullOrEmpty(uphone.NLTRPendingQPhoneNumber) || uphone.NLTRPendingQPhoneNumber.Trim().Equals(BusinessConstants.Instance.PhoneNumberSeniorUnderwriter))
                    return BusinessConstants.Instance.PhoneNumberSeniorUnderwriter;

                else return uphone.NLTRPendingQPhoneNumber;
            }

            if (uphone.NLTRPendingQPhoneNumber != null) return uphone.NLTRPendingQPhoneNumber;

            return BusinessConstants.Instance.PhoneNumberUnderwriting;
        }

        public bool Is_Approved_NLTR_Q()
        {
            if (_currentApplicationData != null)
            {
                return _currentApplicationData.LatestLoanTermsRequestStatus == LoanTermsRequestStatusLookup.LoanTermsRequestStatus.PendingQ;
            }

            if (_loanOfferDataSet != null)
            {
                var latestLtr = _loanOfferDataSet.LatestLoanTermsRequest;

                return latestLtr.Status == LoanTermsRequestStatusLookup.LoanTermsRequestStatus.PendingQ;
            }

            return false;
        }

        private void GetAddCoApplicantStatements()
        {
            Statements.Add("To submit a joint application, please click on Submit Joint Application below.");
            Statements.Add(string.Format("If you have any questions, you may contact us at <a href=\"tel:{0}\">{0}</a>. Our business hours are {1}.",
                BusinessConstants.Instance.PhoneNumberMain,
                BusinessConstants.Instance.BusinessHours.ToString(FirstAgain.Common.TimeZoneUS.EasternStandardTime)
                ));
        }

    }
}