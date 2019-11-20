using System;
using System.Web;
using System.Web.SessionState;
using FirstAgain.Common.Web;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Web.Cookie;
using LightStreamWeb.ServerState;

namespace FirstAgain.Web.UI
{
    /// <summary>
    /// Summary description for StatusRedirect
    /// </summary>
    public static class StatusRedirect
    {
        public static string GetRedirectBasedOnStatus(CustomerUserIdDataSet cuid, int applicationId)
        {
            string URL = string.Empty;

            CustomerUserIdDataSet.ApplicationRow appRow = cuid.Application.FindByApplicationId(applicationId);
            string ctx = WebSecurityUtility.Scramble(applicationId);
            SessionUtility.ActiveApplicationId = applicationId;

            switch (appRow.ApplicationStatusType)
            {
                    //InProcess and clean Pending states are treated the same as 
                    //far as the messaging received by the customer.
                case ApplicationStatusTypeLookup.ApplicationStatusType.InProcess:
                case ApplicationStatusTypeLookup.ApplicationStatusType.Pending:
                case ApplicationStatusTypeLookup.ApplicationStatusType.PendingQ:
                    return string.Format("~/appstatus/pending/?ctx={0}", ctx);

                case ApplicationStatusTypeLookup.ApplicationStatusType.PendingV:
                    return string.Format("~/appstatus/pendingv/?ctx={0}", ctx);

                case ApplicationStatusTypeLookup.ApplicationStatusType.Declined:
                    return string.Format("~/appstatus/decline/?ctx={0}", ctx);

                case ApplicationStatusTypeLookup.ApplicationStatusType.Approved:
                case ApplicationStatusTypeLookup.ApplicationStatusType.ApprovedNLTR:
                    SetLoanOfferData(applicationId);
                    if (LastDateToScheduleFundingPastDue(cuid, applicationId))
                    {
                        return string.Format("~/appstatus/approvedexpired/?ctx={0}", ctx);
                    }

                    LoanOfferDataSet loanOfferDataSet = SessionUtility.GetLoanOfferDataSet(HttpContext.Current.Session);
                    if (loanOfferDataSet.LatestLoanTermsRequest != null)
                    {
                        switch (loanOfferDataSet.LatestLoanTermsRequest.Status)
                        {
                            case LoanTermsRequestStatusLookup.LoanTermsRequestStatus.Cancelled:
                                return string.Format("~/appstatus/approved/?ctx={0}&CancelledNLTR=true", ctx);
                            case LoanTermsRequestStatusLookup.LoanTermsRequestStatus.PendingV:
                                return string.Format("~/appstatus/pendingv/?ctx={0}&CancelledNLTR=true", ctx);
                            case LoanTermsRequestStatusLookup.LoanTermsRequestStatus.PendingQ:
                            case LoanTermsRequestStatusLookup.LoanTermsRequestStatus.Pending:
                                return string.Format("~/appstatus/pending/?ctx={0}&IsNLTR=true", ctx);
                        }
                    }

                    return string.Format("~/appstatus/approved/?ctx={0}", ctx);

                case ApplicationStatusTypeLookup.ApplicationStatusType.Counter:
                    SetLoanOfferData(applicationId);
                    return string.Format("~/appstatus/counter/?ctx={0}", ctx);

                case ApplicationStatusTypeLookup.ApplicationStatusType.PreFunding:
                    SetLoanOfferData(applicationId);
                    return string.Format("~/appstatus/prefunding?ctx={0}", ctx);

                case ApplicationStatusTypeLookup.ApplicationStatusType.PreFundingNLTR:
                    SetLoanOfferData(applicationId);
                    if (LastDateToScheduleFundingPastDue(cuid, applicationId))
                    {
                        return string.Format("~/appstatus/expired/?ctx={0}", ctx);
                    }
                    return string.Format("~/appstatus/prefunding/nltr?ctx={0}", ctx);

                case ApplicationStatusTypeLookup.ApplicationStatusType.CounterV:
                    SetLoanOfferData(applicationId);
                    return string.Format("~/appstatus/counterv/?ctx={0}", ctx);

                case ApplicationStatusTypeLookup.ApplicationStatusType.Cancelled:
                    return string.Format("~/appstatus/cancelled/?ctx={0}", ctx);

                case ApplicationStatusTypeLookup.ApplicationStatusType.Withdrawn:
                    return string.Format("~/appstatus/withdrawn/?ctx={0}", ctx);

                case ApplicationStatusTypeLookup.ApplicationStatusType.Expired:
                    return string.Format("~/appstatus/expired/?ctx={0}", ctx);

                case ApplicationStatusTypeLookup.ApplicationStatusType.Incomplete:
                    return string.Format("/apply/incomplete?ctx={0}", ctx);

                case ApplicationStatusTypeLookup.ApplicationStatusType.Inquiry:
                    LoanApplicationDataSet lads = SessionUtility.GetApplicationData(applicationId);

                    URL = string.Format("/Apply/Partner?ctx={0}", ctx);

                    // SR 2116 - Append URL with Partner FACT code at Registration Time
                    int factId = (lads.MarketingSource.Rows.Count > 0) ? lads.MarketingSource[0].FirstAgainCodeTrackingId : -1;
                    if (factId != -1)
                    {
                        CookieUtility.SetFACTandFAIR(factId, null, null);
                        URL += "&fact=" + factId;
                    }
                    return URL;

                case ApplicationStatusTypeLookup.ApplicationStatusType.FundingFailed:
                    SetLoanOfferData(applicationId);
                    return string.Format("~/appstatus/prefunding/fundingfailed?ctx=", ctx);

                case ApplicationStatusTypeLookup.ApplicationStatusType.Funded:
                    return string.Format("~/account?ctx={0}", ctx);

                case ApplicationStatusTypeLookup.ApplicationStatusType.Terminated:
                    return string.Format("~/appstatus/terminated/?ctx={0}", ctx);

                default:
                    return string.Format("~/preferences/accountpreferences.aspx?ctx={0}", ctx);
            }
        }

        private static bool LastDateToScheduleFundingPastDue(CustomerUserIdDataSet cuid, int applicationId)
        {
            CustomerUserIdDataSet.ApplicationRow app = cuid.Application.FindByApplicationId(applicationId);
            CustomerUserIdDataSet.ApplicationDetailRow[] appDetails = app.GetApplicationDetailRows();
            if (DateTime.Now >= appDetails[0].LastDateToScheduleFunding)
            {
                return true;
            }
            return false;
        }

        private static void SetLoanOfferData(int applicationId)
        {
            LoanOfferDataSet loanOfferDataSet = DomainServiceLoanApplicationOperations.GetLoanOffer(applicationId);

            // Set the application interest rate, i.e., the "minimum rate".
            decimal interestRate = DomainServiceInterestRateOperations.GetApplicationInterestRate(applicationId);
            decimal newRate = interestRate * 100M;
            if ((loanOfferDataSet.LoanOffer.Count > 0) && (newRate < loanOfferDataSet.LoanOffer[0].LoanTermsRequestRow.InterestRate))
            {
                loanOfferDataSet.LoanOffer[0].LoanTermsRequestRow.InterestRate = newRate;
                loanOfferDataSet.LoanTermsRequest.AcceptChanges(); // <-- Else we inadvertently update the database!
            }

            SessionUtility.SetLoanOfferDataSet(loanOfferDataSet);
        }
    }
}