using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using FirstAgain.Domain.TestingCommon.Builders;
using LightStreamWeb.Models.ApplicationStatus;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightStreamWeb.UnitTests.Models.ApplicationStatus
{
    public class StatusCheckModelTests
    {
        [Test]
        public void NullParametersShouldReturnFalse()
        {
            var model = new StatusCheckModel();
            var result = model.GetNextCheck(null, ApplicationStatusTypeLookup.ApplicationStatusType.NotSelected);
            Assert.AreEqual(false, result.Success);
        }

        [Test]
        public void PollMoreFrequentlyDuringBusinessHours()
        {
            var anyApplicationId = 1;
            var anyApplicationStatus = ApplicationStatusTypeLookup.ApplicationStatusType.Pending;
            var resultDuringBusinessHours = new StatusCheckModelShunt(true, anyApplicationStatus).GetNextCheck(anyApplicationId, anyApplicationStatus);
            var resultNotDuringBusinessHours = new StatusCheckModelShunt(false, anyApplicationStatus).GetNextCheck(anyApplicationId, anyApplicationStatus);

            Assert.Greater(resultNotDuringBusinessHours.NextStatusCheckSeconds, resultDuringBusinessHours.NextStatusCheckSeconds);
        }

        [Test]
        public void PendingAppShouldRedirectWhenApproved()
        {
            var anyApplicationId = 1;
            var anyBusinessHours = false;
            var pendingStatus = ApplicationStatusTypeLookup.ApplicationStatusType.Pending;
            var approvedStatus = ApplicationStatusTypeLookup.ApplicationStatusType.Approved;

            // page thinks it's pending, backend says it's approved
            var result = new StatusCheckModelShunt(anyBusinessHours, approvedStatus).GetNextCheck(anyApplicationId, pendingStatus);

            Assert.AreEqual(true, result.DoRedirect);
            Assert.AreEqual(true, result.Success);
            Assert.AreEqual(false, result.IsSignOut);
        }

        [Test]
        public void MatchingStatusShouldNotRedirect()
        {
            var anyApplicationId = 1;
            var anyBusinessHours = false;
            var anyStatus = ApplicationStatusTypeLookup.ApplicationStatusType.Pending;

            // page thinks it's one status, backend says it's the same status
            var result = new StatusCheckModelShunt(anyBusinessHours, anyStatus).GetNextCheck(anyApplicationId, anyStatus);

            Assert.AreEqual(false, result.DoRedirect);
            Assert.AreEqual(true, result.Success);
            Assert.AreEqual(false, result.IsSignOut);
        }

        [Test]
        public void LoanAgreementUpdatedRecentlyShouldLogout()
        {
            var anyApplicationId = 1;
            var duringBusinessHours = true;
            var approvedStatus = ApplicationStatusTypeLookup.ApplicationStatusType.Approved;

            EventAuditLogDataSet dataSet = new EventAuditLogDataSetBuilder()
                .WithEvent(EventTypeLookup.EventType.LoggedIn, anyApplicationId)
                .WithNote("testing")
                .WithEvent(EventTypeLookup.EventType.InvalidateLoanAgreement, anyApplicationId).Build();

            // they signed in before the loan agreement was invalidated
            var loginEvent = dataSet.EventAuditLog.Single(e => e.EventType == EventTypeLookup.EventType.LoggedIn);
            loginEvent.CreatedDate = loginEvent.CreatedDate.Subtract(TimeSpan.FromMinutes(5));

            var result = new StatusCheckModelShunt(duringBusinessHours, approvedStatus, dataSet).GetNextCheck(anyApplicationId, approvedStatus);

            Assert.AreEqual(true, result.DoRedirect);
            Assert.AreEqual(true, result.Success);
            Assert.AreEqual(true, result.IsSignOut);

        }

        [Test]
        public void NewExceptionShouldLogout()
        {
            var anyApplicationId = 1;
            var duringBusinessHours = true;
            var anyStatus = ApplicationStatusTypeLookup.ApplicationStatusType.InProcess;

            EventAuditLogDataSet dataSet = new EventAuditLogDataSetBuilder()
                .WithEvent(EventTypeLookup.EventType.LoggedIn, anyApplicationId)
                .WithNote("testing")
                .WithEvent(EventTypeLookup.EventType.NewException, anyApplicationId).Build();

            // they signed in before the loan agreement was invalidated
            var loginEvent = dataSet.EventAuditLog.Single(e => e.EventType == EventTypeLookup.EventType.LoggedIn);
            loginEvent.CreatedDate = loginEvent.CreatedDate.Subtract(TimeSpan.FromMinutes(5));

            var result = new StatusCheckModelShunt(duringBusinessHours, anyStatus, dataSet).GetNextCheck(anyApplicationId, anyStatus);

            Assert.AreEqual(true, result.DoRedirect);
            Assert.AreEqual(true, result.Success);
            Assert.AreEqual(true, result.IsSignOut);

        }

        private class StatusCheckModelShunt : StatusCheckModel
        {
            private bool _isInBusinessHours;
            private ApplicationStatusTypeLookup.ApplicationStatusType _applicationStatus;
            private EventAuditLogDataSet _eventAuditLog;

            public StatusCheckModelShunt(bool isInBusinessHours, ApplicationStatusTypeLookup.ApplicationStatusType backendStatus) : base()
            {
                _isInBusinessHours = isInBusinessHours;
                _applicationStatus = backendStatus;
            }

            public StatusCheckModelShunt(
                bool isInBusinessHours, 
                ApplicationStatusTypeLookup.ApplicationStatusType backendStatus,
                EventAuditLogDataSet eventAuditLog) : base()
            {
                _isInBusinessHours = isInBusinessHours;
                _applicationStatus = backendStatus;
                _eventAuditLog = eventAuditLog;
            }

            protected override EventAuditLogDataSet GetEventAuditLogFromBackend(int applicationId)
            {
                return _eventAuditLog;
            }

            override protected bool IsInBusinessHours()
            {
                return _isInBusinessHours;
            }

            override protected ApplicationStatusTypeLookup.ApplicationStatusType  GetApplicationStatusFromBackend(int applicationId)
            {
                return _applicationStatus;
            }
        }
    }
}
