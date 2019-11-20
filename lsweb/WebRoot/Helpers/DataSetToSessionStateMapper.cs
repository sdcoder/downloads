using System;
using System.Linq;
using System.Text;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Contracts.MessageContracts.CustomerOperations;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using LightStreamWeb.App_State;

namespace LightStreamWeb.Helpers
{
    public static class DataSetToSessionStateMapper
    {
        public static CurrentApplicationData Map(int applicationId, GetAccountInfoResponse accountInfo, LoanOfferDataSet loanOfferDataSet)
        {
            CurrentApplicationData appData = Map(applicationId, accountInfo);
            return MapLoanOfferDataSet(loanOfferDataSet, appData);
        }

        public static ICurrentApplicationData Map(int applicationId, CustomerUserIdDataSet cuidds, LoanOfferDataSet loanOfferDataSet)
        {
            CurrentApplicationData appData = Map(applicationId, cuidds);

            return MapLoanOfferDataSet(loanOfferDataSet, appData);
        }

        private static CurrentApplicationData MapLoanOfferDataSet(LoanOfferDataSet loanOfferDataSet, CurrentApplicationData appData)
        {
            if (loanOfferDataSet != null && loanOfferDataSet.LatestApprovedLoanTerms != null)
            {
                appData.PaymentType = loanOfferDataSet.LatestApprovedLoanTerms.PaymentType;
            }

            if (loanOfferDataSet != null && loanOfferDataSet.LatestLoanTermsRequest != null)
            {
                appData.CreditTier = loanOfferDataSet.LatestLoanTermsRequest.CreditTier;
                appData.LatestLoanTermsRequestStatus = loanOfferDataSet.LatestLoanTermsRequest.Status;
            }

            return appData;
        }

        private static CurrentApplicationData Map(int applicationId, CustomerUserIdDataSet cuidds)
        {
            CurrentApplicationData appData = new CurrentApplicationData()
            {
                ApplicationId = applicationId
            };

            if (cuidds != null && cuidds.Application.FirstOrDefault(x => x.ApplicationId == applicationId) != null)
            {
                appData.HasEnotices = cuidds.DocumentStore.Any(a => a.ApplicationId == applicationId && a.IsViewable &&
                   (a.CorrespondenceCategory == CorrespondenceCategoryLookup.CorrespondenceCategory.DeclineNotice
                   || a.CorrespondenceCategory == CorrespondenceCategoryLookup.CorrespondenceCategory.CreditScoreDisclosureHtml));

                appData.HasDeclineNotice = cuidds.DocumentStore
                    .Any(a => a.ApplicationId == applicationId && a.IsViewable &&
                         a.CorrespondenceCategory == CorrespondenceCategoryLookup.CorrespondenceCategory.DeclineNotice);

                var applicationRow = cuidds.Application.FirstOrDefault(x => x.ApplicationId == applicationId);
                appData.ApplicationStatus = applicationRow.ApplicationStatusType;
                appData.ApplicationType = applicationRow.ApplicationType;

                // add co applicant is enabled if the flag is set, and it's not currently in AML review
                appData.AddCoApplicantIsEnabled = applicationRow.FlagIsSet(FlagLookup.Flag.AddCoApplicant) && !applicationRow.FlagIsSet(FlagLookup.Flag.IsInAMLReview);

                var appDetail = applicationRow.GetApplicationDetailRows()[0];
                appData.PaymentType = appDetail.PaymentType;
                appData.CreditTier = appDetail.CreditTier;
                appData.PurposeOfLoan = appDetail.PurposeOfLoan;
                appData.WithdrawDate = (appDetail.IsWithdrawDateNull()) ? (DateTime?)null : appDetail.WithdrawDate;

                // applicant names
                StringBuilder sb = new StringBuilder();
                var primary = applicationRow.Applicants.FirstOrDefault(a => a.ApplicantType == ApplicantTypeLookup.ApplicantType.Primary);
                sb.AppendFormat("{0} {1}", primary?.FirstName, primary?.LastName);
                if (applicationRow.IsJoint)
                {
                    var secondary = applicationRow.Applicants.FirstOrDefault(a => a.ApplicantType == ApplicantTypeLookup.ApplicantType.Secondary);
                    sb.AppendFormat(" and {0} {1}", secondary?.FirstName, secondary?.LastName);
                }

                appData.ApplicantNames = sb.ToString();

                // reg o application
                appData.IsRegOApplication = cuidds.ApplicationFlag.HasSunTrustOfficerFlagOn;

                // IsAutoApprovalEligible
                appData.IsAutoApprovalEligible = cuidds.ApplicationFlag.IsAutoApprovalEligible;

                // counter amount
                var counterRow = cuidds.LoanTermsRequest.GetCounterLoanTerms(applicationId);
                if (counterRow != null)
                {
                    appData.CounterAmount = counterRow.Amount;
                }
            }


            return appData;
        }

        public static CurrentApplicationData Map(int applicationId, GetAccountInfoResponse accountInfo)
        {
            CurrentApplicationData appData = Map(applicationId, accountInfo.CustomerUserIdDataSet);
            appData.ApplicationResultedFromAddCoApplicant = accountInfo.AddCoApplicantInfo != null && accountInfo.AddCoApplicantInfo.Any(a => a.ApplicationId == applicationId);

            return appData;
        }
    }
}