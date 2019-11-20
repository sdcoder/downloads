using FirstAgain.Common;
using FirstAgain.Domain.Common;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using System;
using System.Linq;

namespace LightStreamWeb.Models.ApplicationStatus
{
    public class StatusCheckModel
    {
        public bool Success { get; set; } = false;
        public bool DoRedirect { get; set; } = false;
        public bool IsSignOut { get; set; } = false;
        public long NextStatusCheckSeconds { get; set; } = 15;

        public StatusCheckModel GetNextCheck(int? applicationId, ApplicationStatusTypeLookup.ApplicationStatusType currentStatus)
        {
            // bad data or session timed out, return error condition
            if (!applicationId.HasValue || currentStatus == ApplicationStatusTypeLookup.ApplicationStatusType.NotSelected)
            {
                return new StatusCheckModel()
                {
                    Success = false
                };
            }

            var newStatus = GetApplicationStatusFromBackend(applicationId.Value);

            // handle the edge cases of new exceptions or invalid loan agreements. Log out the user.
            if (currentStatus.IsActiveApplication())
            {
                var eal = GetEventAuditLogFromBackend(applicationId.Value);
                if (eal != null && eal.EventAuditLog != null)
                {
                    var invalidatedLoanAgreement = eal.EventAuditLog.OrderByDescending(o => o.EventAuditLogId).FirstOrDefault(e => e.EventType == EventTypeLookup.EventType.InvalidateLoanAgreement && e.ApplicationId == applicationId.Value);
                    var newExceptionRaised = eal.EventAuditLog.OrderByDescending(o => o.EventAuditLogId).FirstOrDefault(e => e.EventType == EventTypeLookup.EventType.NewException && e.ApplicationId == applicationId.Value);
                    var signedIn = eal.EventAuditLog.Where(e => e.EventType == EventTypeLookup.EventType.LoggedIn).OrderBy(e => e.CreatedDate).LastOrDefault();

                    if (signedIn != null)
                    {
                        if (invalidatedLoanAgreement != null && invalidatedLoanAgreement.CreatedDate > signedIn.CreatedDate)
                        {
                            return new StatusCheckModel() { Success = true, DoRedirect = true, IsSignOut = true };
                        }

                        if (newExceptionRaised != null && newExceptionRaised.CreatedDate > signedIn.CreatedDate)
                        {
                            return new StatusCheckModel() { Success = true, DoRedirect = true, IsSignOut = true };
                        }
                    }
                }
            }

            long nextCheckInSeconds = (long)((IsInBusinessHours()) ? TimeSpan.FromSeconds(5).TotalSeconds : TimeSpan.FromMinutes(2).TotalSeconds);
            return ReturnNextCheckBasedOnStatus(currentStatus, newStatus, nextCheckInSeconds);
        }

        private StatusCheckModel ReturnNextCheckBasedOnStatus(ApplicationStatusTypeLookup.ApplicationStatusType currentStatus, ApplicationStatusTypeLookup.ApplicationStatusType newStatus, long nextIntervalIfNoRedirect)
        {
            if (NewStatusShouldBeRedirectedTo(currentStatus, newStatus))
            {
                return new StatusCheckModel()
                {
                    Success = true,
                    DoRedirect = true,
                    NextStatusCheckSeconds = (long)TimeSpan.FromHours(1).TotalSeconds // effectively, don't check again
                };
            }

            // else - no redirect
            return new StatusCheckModel()
            {
                Success = true,
                DoRedirect = false,
                NextStatusCheckSeconds = nextIntervalIfNoRedirect
            };
        }

        private bool NewStatusShouldBeRedirectedTo(ApplicationStatusTypeLookup.ApplicationStatusType currentStatus, ApplicationStatusTypeLookup.ApplicationStatusType newStatus)
        {
            if (currentStatus == newStatus)
            {
                return false;
            }

            switch (newStatus)
            {
                case ApplicationStatusTypeLookup.ApplicationStatusType.Counter:
                case ApplicationStatusTypeLookup.ApplicationStatusType.CounterV:
                case ApplicationStatusTypeLookup.ApplicationStatusType.Approved:
                case ApplicationStatusTypeLookup.ApplicationStatusType.ApprovedNLTR:
                case ApplicationStatusTypeLookup.ApplicationStatusType.PendingQ:
                case ApplicationStatusTypeLookup.ApplicationStatusType.PendingV:
                case ApplicationStatusTypeLookup.ApplicationStatusType.PreFunding:
                case ApplicationStatusTypeLookup.ApplicationStatusType.PreFundingNLTR:
                    return true;
                default:
                    return false;

            }
        }

        #region protected methods to allow for unit testing
        virtual protected ApplicationStatusTypeLookup.ApplicationStatusType GetApplicationStatusFromBackend(int applicationId)
        {
            return DomainServiceLoanApplicationOperations.GetApplicationStatus(applicationId);
        }

        virtual protected bool IsInBusinessHours()
        {
            BusinessCalendarDataSet calendar = BusinessCalendar.BusinessCalendarDataSet;
            DateTime currentDateTime = DateTime.Now;

            var nextOperationsDate = calendar.GetNextOrThisOperationsBusinessDate(currentDateTime);
            if (!DateTime.Equals(nextOperationsDate.Date, currentDateTime.Date))
            {
                return false;
            }

            TimeOfDaySpan hours = BusinessConstants.Instance.BusinessHours.GetHours(currentDateTime.DayOfWeek);

            // Check if in biz hours
            if (hours == null || currentDateTime.TimeOfDay < hours.StartTime)
            {
                return false;
            }

            if (currentDateTime.TimeOfDay >= hours.EndTime)
            {
                nextOperationsDate = calendar.GetNextOperationsBusinessDate(currentDateTime);
                return false;
            }

            return true;
        }

        virtual protected EventAuditLogDataSet GetEventAuditLogFromBackend(int applicationId)
        {
            return DomainServiceLoanApplicationOperations.GetEventAuditLogByApplicationId(applicationId);
        }
        #endregion
    }
}