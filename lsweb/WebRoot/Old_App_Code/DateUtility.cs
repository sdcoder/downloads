using System;
using FirstAgain.Common;
using FirstAgain.Domain.Common;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Client;

/// <summary>
/// Summary description for DateUtility
/// </summary>
public static class DateUtility
{
    public static string CustomerResponseTimeFrameText
    {
        get
        {
            DateTime nextBizDate;
            BusinessCalendarDataSet calendar = BusinessCalendar.BusinessCalendarDataSet;

            if (calendar.IsCurrentDateTimeInBusinessHours(out nextBizDate, DateTime.Now))
            {
                return MaintenanceConfiguration.IsInMaintenanceMode ? "soon" : "shortly";
            }

            return String.Format("during our normal business hours, no later than {0:dddd} morning, {0:MMMM d, yyyy} Pacific time", nextBizDate); //Format: Monday, April 1, 2006
        }
    }

    public static DateTime? GetVerificationDocDeadlineDate(int applicationId, ApplicationStatusTypeLookup.ApplicationStatusType appStatus, CustomerUserIdDataSet cuids)
    {
        DateTime deadlineDate = DateTime.Now.AddDays(7);
        
        switch (appStatus)
        {
            case ApplicationStatusTypeLookup.ApplicationStatusType.PendingV:
            case ApplicationStatusTypeLookup.ApplicationStatusType.CounterV:
                HstLoanApplicationDataSet appHistData = DomainServiceLoanApplicationOperations.GetLoanApplicationHistory(applicationId,
                                                    HstLoanApplicationDataSet.ApplicationDataTableFlags.Application | HstLoanApplicationDataSet.ApplicationDataTableFlags.ApplicationFlag,
                                                    HstLoanApplicationDataSet.ApplicantDataTableFlags.None,
                                                    HstLoanApplicationDataSet.LoanDataTableFlags.None,
                                                    HstLoanApplicationDataSet.OtherDataTableFlags.None);
                DateTime? appStatusDate = appHistData.GetApplicationStatusDate(appStatus);
                DateTime? scheduledAutoDeclineDate = appHistData.AutoDeclineDate;

                if (appStatusDate != null && scheduledAutoDeclineDate != null)
                {
                    DateTime pendingVorCounterVDate = Convert.ToDateTime(appStatusDate.Value);
                    deadlineDate = pendingVorCounterVDate.AddDays(10); //PendingV application business requirements.doc - Sec 2.4.12.1
                    deadlineDate = deadlineDate >= scheduledAutoDeclineDate ? ((DateTime)scheduledAutoDeclineDate).AddDays(-1) : deadlineDate;
                }
                break;
            case ApplicationStatusTypeLookup.ApplicationStatusType.ApprovedNLTR:
            case ApplicationStatusTypeLookup.ApplicationStatusType.PreFundingNLTR:
                CustomerUserIdDataSet.ApplicationRow app = cuids.Application.FindByApplicationId(applicationId);
                CustomerUserIdDataSet.ApplicationDetailRow[] appDetails = app.GetApplicationDetailRows();
                DateTime lastDateToScheduleFunding = appDetails[0].LastDateToScheduleFunding;

                DateTime nltrCreateDate = cuids.LoanTermsRequest.GetLatestNLTRLoanTermsRequest(applicationId).CreatedDate;
                DateTime nltrCreateDatePlusSeven = BusinessCalendar.MoveBusinessDays(nltrCreateDate, 7);
                if (nltrCreateDatePlusSeven >= lastDateToScheduleFunding)
                {
                    deadlineDate = BusinessCalendar.GetClosestBankingDay(lastDateToScheduleFunding.AddDays(-1));
                }
                else
                {
                    deadlineDate = BusinessCalendar.GetClosestBankingDay(nltrCreateDatePlusSeven);
                }
                break;
        }

        return deadlineDate;
    }
}
