using System.Linq;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using FirstAgain.Domain.Lookups.FirstLook;
using System.Collections.Generic;
using LightStreamWeb.Models.Shared;

namespace LightStreamWeb.Models.ApplicationStatus
{
    public class CounterVPageModel : CounterPageModel
    {
        #region constructors
        public CounterVPageModel(CustomerUserIdDataSet cuidds, LoanOfferDataSet loanOfferDataSet, LoanApplicationDataSet lads)
            : base(cuidds, loanOfferDataSet, lads)
        {
            EnableFileUpload = true;
        }
        #endregion

        public VerificationRequestsModel VerificationRequests { get; protected set; }

        public bool CanRejectOffer { get; protected set; }

        override protected void Populate()
        {
            RateModalDisplay = RateModalDisplayType.ApplicationRates;

            Statements = new List<string>();
            VerificationRequests = new VerificationRequestsModel();

            // populate table
            Counter = CounterOfferTermsModel.Populate(_customerUserIdDataSet, ApplicationId);

            // and verification requests
            VerificationRequests.Populate(_customerUserIdDataSet, ApplicationId, Counter.Offered.PurposeOfLoan, _loanApplicationDataSet);

            if (AddCoApplicantIsEnabled())
            {
                Statements.Add("To submit a new joint loan application, click on the Submit Joint Application button below. Please keep in mind your individual loan application will be cancelled when you submit a joint application. ");
            }
            else
            {
                if (!VerificationRequests.Any())
                {
                    Statements.Add(VerificationRequests.GetAllItemsCompletedMessage() + ". ");
                    Statements.Add("While we are unable to approve your " +
                                   ((GetPurposeOfLoan().IsSecured()) ? GetPurposeOfLoanText() : "loan application") +
                                   " for the terms requested, we are able to make you a counter offer, shown below");
                    Statements.Add("You will receive an email notification once we have completed our review of your information.") ;
                }
                else
                {
                    Statements.Add("Thank you for your recent loan application. " +
                                   "While we are unable to approve your " +
                                   ((GetPurposeOfLoan().IsSecured()) ? GetPurposeOfLoanText() : "loan application") +
                                   " for the terms requested, we are able to make you a counter offer, shown below, subject to satisfactory verification of the " + VerificationRequests.Description + " listed below.");
                }
            }

            CanRejectOffer = !_customerUserIdDataSet.ApplicationFlag.Any(f => f.ApplicationId == ApplicationId && f.Flag == FlagLookup.Flag.IsInAMLReview && f.FlagIsOn);
        }

    }
}