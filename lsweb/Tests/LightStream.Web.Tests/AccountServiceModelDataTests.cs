using FirstAgain.Correspondence.SharedTypes;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.LoanServicing.ServiceModel.Client;
using FirstAgain.LoanServicing.SharedTypes;
using FirstAgain.LoanServicing.SharedTypes.Entities.AmortizationSchedule;
using LightStreamWeb.Models.AccountServices;
using LightStreamWeb.Tests.Mocks;
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace LightStreamWeb.Tests
{
    [ExcludeFromCodeCoverage]
    public class AccountServiceModelDataTests: AccountServiceModelData
    {
        public AccountServiceModelDataTests()
            : base(new MockUser())
        {
        }

        [OneTimeSetUp]
        public void Setup()
        {
            AutoMapper.Mapper.Initialize(cfg => { cfg.CreateMap<PredictReamortizationAvailabilityResponse, FundedAccountModel>(); });
        }

        [Test]
        public void PinningTest_Positive()
        {
            string userName = "AstridCRAat15test";
            int applicationId = 35425524;

            var account = SetupModel(userName, applicationId);

            Assert.IsTrue(account.MayBeEligibleForReamortization);
            Assert.AreEqual(108.88, account.ProjectedMinimumMonthlyPaymentAmount);
            Assert.AreEqual(DateTime.Parse("December 18, 2015"), account.ReamortizationDate);
        }
        [Test]
        public void PinningTest_Negative()
        {
            string userName = "prime5test09";
            int applicationId = 35486295;

            var account = SetupModel(userName, applicationId);

            Assert.IsFalse(account.MayBeEligibleForReamortization);
            //Assert.AreEqual(108.88, account.ProjectedMinimumMonthlyPaymentAmount);
            //Assert.AreEqual(DateTime.Parse("December 18, 2015"), account.ReamortizationDate);
        }

        [Test]
        public void PinningTest_MinimalSetup_Positive()
        {
            int applicationId = 35425524;
            var request = new FundedAccountModel()
            {
                ApplicationId = applicationId,
                MinimumMonthlyPaymentAmount = 894.95m,
                MonthlyPayment = 894.95m,
                ScheduledPaymentAmount = 500,
                ScheduledPaymentDate = DateTime.Parse("12/3/2015 12:00:00 AM")
            };
            var account = MinimalSetup(applicationId, request);
            Assert.IsTrue(account.MayBeEligibleForReamortization);

            // mock business calendar results in different data than our real business calendar, so the expected values needed to be adjusted
            Assert.AreEqual(108.88, account.ProjectedMinimumMonthlyPaymentAmount);
            Assert.AreEqual(DateTime.Parse("12/18/2015"), account.ReamortizationDate);
        }

        [Test]
        public void PinningTest_MinimalSetup_Negative()
        {
            int applicationId = 35486295;
            var request = new FundedAccountModel()
            {
                ApplicationId = applicationId,
                MinimumMonthlyPaymentAmount = 894.95m,
                MonthlyPayment = 894.95m,
                ScheduledPaymentAmount = null,
                ScheduledPaymentDate = null
            }; 
            var account = MinimalSetup(applicationId, request);
            Assert.IsFalse(account.MayBeEligibleForReamortization);
        }

        private PredictReamortizationAvailabilityResponse MinimalSetup(int applicationId, FundedAccountModel request)
        {
            return CheckReamortizationAvailability(request);
        }

        private FundedAccountModel SetupModel(string userName, int applicationId)
        {
            var response = DomainServiceCustomerOperations.GetAccountInfoByCustomerUserId(userName);
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.ApplicationsDates);
            Assert.IsNotNull(response.CustomerUserIdDataSet);

            BusinessCalendarDataSet businessCalendar = new BusinessCalendarDataSet();
            AccountServicesDataSet accountServicesData = new AccountServicesDataSet();
            DocumentStoreDataSet dds = new DocumentStoreDataSet();

            LoanServicingOperations.GetAccountServicingDataByUserId(userName, out businessCalendar, out accountServicesData, out dds);
            Assert.IsNotNull(businessCalendar);
            Assert.IsNotNull(accountServicesData);

            Populate(response, accountServicesData, businessCalendar);

            var account = Applications.Single(a => a.ApplicationId == applicationId);

            CheckReamortizationAvailability(account);
            return account;
        }

        #region mocks
        public class MockBusinessCalendarService : IBusinessCalendarService
        {
            public DateTime GetBankingDateNBankingDaysFromDate(DateTime fromDate, int incrementBankingDays)
            {
                return fromDate.AddDays(incrementBankingDays);
            }

            public DateTime GetLargePaymentClearingDate(DateTime fromDate)
            {
                return GetBankingDateNBankingDaysFromDate(fromDate, 11);
            }
        }
        #endregion
    }
}
