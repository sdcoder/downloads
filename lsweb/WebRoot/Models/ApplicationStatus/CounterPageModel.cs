using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using LightStreamWeb.App_State;
using LightStreamWeb.Helpers;
using LightStreamWeb.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LightStreamWeb.Models.ApplicationStatus
{
    public class CounterPageModel : ApplicationStatusPageModel
    {
        protected LoanApplicationDataSet _loanApplicationDataSet;
        #region constructors
        public CounterPageModel(CustomerUserIdDataSet cuidds, LoanOfferDataSet loanOfferDataSet, LoanApplicationDataSet lads)
            : base(cuidds, loanOfferDataSet)
        {
            _loanApplicationDataSet = lads;
            Populate();
        }

        #endregion

        public List<string> Statements { get; protected set; }

        public override PurposeOfLoanLookup.PurposeOfLoan GetPurposeOfLoan()
        {
            return Counter.Offered.PurposeOfLoan;
        }

        public bool IsEligibleForDeclineReferralOptIn
        {
            get
            {
                return DomainServiceLoanApplicationOperations.IsEligibleForDeclineReferralOptIn(ApplicationId);
            }
        }

        public static ApplicationStatusTypeLookup.ApplicationStatusType AcceptCounter(ICurrentUser user, CustomerUserIdDataSet customerData)
        {
            var applicationRow = customerData.Application.FirstOrDefault(a => a.ApplicationId == user.ApplicationId);
            var wads = WebActivityDataSetHelper.Populate(user, applicationRow.ApplicationStatusType);
            try
            {
                DomainServiceLoanApplicationOperations.AcceptCounterOfferWithWebActivity(applicationRow.ApplicationId, wads);
                return ApplicationStatusTypeLookup.ApplicationStatusType.Approved;
            }
            catch(Exception)
            {
                var status = DomainServiceLoanApplicationOperations.GetApplicationStatus(applicationRow.ApplicationId);
                switch (status)
                {
                    case ApplicationStatusTypeLookup.ApplicationStatusType.Approved:
                    case ApplicationStatusTypeLookup.ApplicationStatusType.Declined:
                        return status;
                    default:
                        throw;
                }
            }
        }

        public static ApplicationStatusTypeLookup.ApplicationStatusType RejectCounter(ICurrentUser user, CustomerUserIdDataSet customerData)
        {
            var applicationRow = customerData.Application.FirstOrDefault(a => a.ApplicationId == user.ApplicationId);
            var wads = WebActivityDataSetHelper.Populate(user, applicationRow.ApplicationStatusType);
            try
            {
                DomainServiceLoanApplicationOperations.DeclineCounterOfferWithWebActivity(applicationRow.ApplicationId, wads);
                return ApplicationStatusTypeLookup.ApplicationStatusType.Declined;
            }
            catch (Exception)
            {
                var status = DomainServiceLoanApplicationOperations.GetApplicationStatus(applicationRow.ApplicationId);
                switch (status)
                {
                    case ApplicationStatusTypeLookup.ApplicationStatusType.Approved:
                    case ApplicationStatusTypeLookup.ApplicationStatusType.Declined:
                        return status;
                    default:
                        throw;
                }
            }
        }

        protected virtual void Populate()
        {
            RateModalDisplay = RateModalDisplayType.ApplicationRates;
            Statements = new List<string>();

            // populate table
            Counter = CounterOfferTermsModel.Populate(_customerUserIdDataSet, ApplicationId);

            if (AddCoApplicantIsEnabled())
            {
                Statements.Add("To submit a new joint loan application, click on the Submit Joint Application button below. Please keep in mind your individual loan application will be cancelled when you submit a joint application. ");
            }
            else
            {
                Statements.Add("Thank you for your recent loan application. " +
                               "While we are unable to approve your " + 
                               ((GetPurposeOfLoan().IsSecured()) ? GetPurposeOfLoanText() : "loan application ") + 
                               " for the terms requested, we are able to make you a counter offer, shown below, that we hope meets your needs.");
            }
        }

        protected bool IsNLTR()
        {
            if (Application.ApplicationStatusType == ApplicationStatusTypeLookup.ApplicationStatusType.Approved || Application.ApplicationStatusType == ApplicationStatusTypeLookup.ApplicationStatusType.ApprovedNLTR)
            {
                if (_loanOfferDataSet != null)
                {
                    var latestLtr = _loanOfferDataSet.LatestLoanTermsRequest;
                    if (latestLtr.Status == LoanTermsRequestStatusLookup.LoanTermsRequestStatus.Counter)
                    {
                        return true;
                    }

                    throw new ArgumentException("Unexpected status " + latestLtr.Status.ToString());
                }
            }


            return false;
        }

        public CounterOfferTermsModel Counter { get; set; }

    }
}