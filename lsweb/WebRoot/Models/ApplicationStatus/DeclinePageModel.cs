using FirstAgain.Correspondence.ServiceModel.Client;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.ServiceModel.Contracts.MessageContracts.CustomerOperations;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using LightStreamWeb.App_State;
using LightStreamWeb.Helpers;
using Ninject;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.ApplicationStatus
{
    public class DeclinePageModel : ApplicationStatusPageModel
    {
        public List<string> Statements { get; private set; }

        public List<DeclinePageNotice> Notices { get; protected set; }
        public string Heading { get; protected set; }
        public bool IsEligibleForDeclineReferralOptIn { get; protected set; }

        private bool _isAvailabilityNotice;
        public bool IsAvailabilityNotice 
        {
            get
            {
                return _isAvailabilityNotice;
            }
        }

        public override bool HasEnotices()
        {
            // eNotices are not displayed via the top navigation, but are displayed on the page
            return false;
        }

        #region constructors
        [Inject]
        public DeclinePageModel(ICurrentUser webUser, CustomerUserIdDataSet cuidds)
            : base(webUser, cuidds)
        {
            Populate();
        }

        [Inject]
        public DeclinePageModel(ICurrentApplicationData applicationData)
            : base(applicationData)
        {
            Populate();
        }


        public DeclinePageModel(CustomerUserIdDataSet cuidds)
            : base(cuidds)
        {
            Populate();
        }

        #endregion

        public static void DeclineReferralOptIn(int applicationId)
        {
            DomainServiceLoanApplicationOperations.OptInAndSubmitForDeclineReferral(applicationId);
        }

        private void Populate()
        {
            RateModalDisplay = RateModalDisplayType.NotSelected;

            Notices = new List<DeclinePageNotice>();

            // If here, then application is decline, however if the decline resulted from rejected counter offer,
            // then need to pass the id of the original declined ltr to get the correct decline notice.
            int? loanTermsRequestForDeclineReasons = null;
            LoanTermsRequestDataSet ltrds = DomainServiceLoanApplicationOperations.GetLoanTermsRequestHistory(ApplicationId);
            // Latest approved ltr should be the counter offer (Application Counter or NLTR Counter)
            LoanTermsRequestDataSet.LoanTermsRequestRow latestApprovedLtr = ltrds.LatestApprovedOrCounteredLoanTermsRequest;
            if (latestApprovedLtr != null &&
                (latestApprovedLtr.LoanTermsRequestType == LoanTermsRequestTypeLookup.LoanTermsRequestType.ApplicationCounter
                || latestApprovedLtr.LoanTermsRequestType == LoanTermsRequestTypeLookup.LoanTermsRequestType.NLTRCounter))
            {
                // Now find the ltr for the original declined terms
                LoanTermsRequestDataSet.LoanTermsRequestRow originalDeclinedLtr = ltrds.LoanTermsRequestStatus.Join(
                                                                                         ltrds.LoanTermsRequest,
                                                                                         ltrs => ltrs.LoanTermsRequestId,
                                                                                         ltr => ltr.LoanTermsRequestId,
                                                                                         (ltrs, ltr) => new { ltrs, ltr })
                                                                                 .Where(n => n.ltrs.LoanTermsRequestStatus == LoanTermsRequestStatusLookup.LoanTermsRequestStatus.Declined
                                                                                             && n.ltr.LoanTermsRequestId == latestApprovedLtr.ParentRequestId)
                                                                                 .Select(n => n.ltr)
                                                                                 .FirstOrDefault();
                if (originalDeclinedLtr != null)
                    loanTermsRequestForDeclineReasons = originalDeclinedLtr.LoanTermsRequestId;
            }

            string declineNotice = CorrespondenceServiceCorrespondenceOperations.RetrieveDeclineNotice(ApplicationId, loanTermsRequestForDeclineReasons, false, out _isAvailabilityNotice);

            // there used to be some mis-formed decline notices. So we tried to fix them. 
            // Problem is... what if it's just a teeny tiny bit broken? 
            // We might as well *try* to display, it, right?
            try
            {
                declineNotice = ENoticeHtmlHelper.FixUpDeclineNoticeHtml(declineNotice);
            }
            catch (Exception ex)
            {
                FirstAgain.Common.Logging.LightStreamLogger.WriteWarning(ex);
            }

            if (IsAvailabilityNotice)
            {
                Heading = "Loan Application Decision";
                Statements = new List<string> { declineNotice };
                IsEligibleForDeclineReferralOptIn = false;
            }
            else
            {
                Heading = "Loan Application Decline Notice";
                Statements = new List<string>();

                if (base.ApplicationType == ApplicationTypeLookup.ApplicationType.Individual)
                {
                    Statements.Add($"{ApplicantNamesText}, your decline notice is provided via the link below.");
                }
                else
                {
                    Statements.Add($"{ApplicantNamesText}, your respective decline notice information is provided via the link below.");
                }

                if (ApplicationResultedFromAddCoApplicant())
                {
                    Statements.Add(FAMessages.AddCoApplicantNewUserIdReminder);
                }

                Statements.Add("Thank you.");
            }

            IsEligibleForDeclineReferralOptIn = DomainServiceLoanApplicationOperations.IsEligibleForDeclineReferralOptIn(ApplicationId);
        }

        public class DeclinePageNotice
        {
            public string Title { get; set; }
            public int EDocId { get; set; }
        }


    }
}