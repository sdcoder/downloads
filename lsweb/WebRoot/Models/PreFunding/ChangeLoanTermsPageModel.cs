using FirstAgain.Domain.Common;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using LightStreamWeb.Models.ApplicationStatus;
using LightStreamWeb.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.PreFunding
{
    public class ChangeLoanTermsPageModel : PreFundingPageModel, IChangeLoanTermsModel
    {
        public LoanTermsModel LoanTerms { get; private set; }
        public string ChangeLoanTermsMessage { get; private set; }

        public ChangeLoanTermsPageModel(CustomerUserIdDataSet cuidds, LoanOfferDataSet loanOfferDataSet)
            : base(cuidds, loanOfferDataSet)
        {
        }

        public override void Populate()
        {
            base.Populate();

            LoanTerms = new LoanTermsModel();
            var approvedTerms = _loanOfferDataSet.LatestApprovedLoanTerms;

            LoanTerms.Populate(approvedTerms);

            ChangeLoanTermsMessage = Resources.FAMessages.LoanTerms_ChangeTerms_Prefunding;

            // secured auto copy
            if (PurposeOfLoanIsSecured())
            {
                ChangeLoanTermsMessage =  Resources.FAMessages.LoanTerms_Approved_SecuredAuto;
            }

            // check for a counter offer to limit the amount if the NLTR
            CustomerUserIdDataSet.LoanTermsRequestRow counterData =  _customerUserIdDataSet.LoanTermsRequest.GetCounterLoanTerms(ApplicationId);

            if (counterData != null)
            {
                //if current amount is equal to the counter amount
                if (_loanOfferDataSet.LatestApprovedLoanTerms.Amount == counterData.Amount)
                {
                    ChangeLoanTermsMessage = Resources.FAMessages.LoanTerms_Approved_ForMaxAmount;
                }
                else
                {
                    //current amount must be less than the counter amount
                    ChangeLoanTermsMessage = string.Format(Resources.FAMessages.LoanTerms_Current_Below_Counter, DataConversions.FormatAndGetMoney(counterData.Amount));
                }
            }
        }
    }
}