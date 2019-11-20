using FirstAgain.Correspondence.SharedTypes;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.ServiceModel.Contracts.MessageContracts.CustomerOperations;
using FirstAgain.LoanServicing.ServiceModel.Client;
using FirstAgain.LoanServicing.SharedTypes;
using LightStreamWeb.Models.AccountServices;
using LightStreamWeb.Tests.Mocks;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace LightStreamWeb.Tests
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class ReamortizationTests
    {
        private BusinessCalendarDataSet _businessCalendar = new BusinessCalendarDataSet();
        private AccountServicesDataSet _accountServicesData = new AccountServicesDataSet();
        private GetAccountInfoResponse _accountInfo = null;

        // Note: only works against QA
        [Test]
        [Ignore("not a unit test")]
        public void ReamortizationTest()
        {
            string errorMessage;
            var user = new MockUser();
            var model = PopulateAccountServicesModel("em20130515");
            var account = model.Applications.Single(a => a.ApplicationId == 99544096);
            Assert.IsNotNull(model);
            Assert.IsNotNull(account);

            // reamortization schedule should show up
            Assert.IsTrue(account.EligibleToViewAmortizationSchedule);

            // did we already schedule one?
            if (account.HasScheduledPayment)
            {
                AccountServiceModelData.CancelExtraPayment(_accountServicesData, _accountInfo.CustomerUserIdDataSet, user, account);
                Assert.IsFalse(account.HasScheduledPayment, "An extra payment was already scheduled. Attempted to cancel. Please try again.");
            }

            // submit a large extra payment
            account.ScheduledPaymentDate = DateTime.Now.AddDays(1);
            account.ScheduledPaymentAmount = account.LoanBalanceCurrent / 2;
            var result = account.PaymentByACH(_accountServicesData, _accountInfo.CustomerUserIdDataSet, user, _businessCalendar, out errorMessage);
            Assert.IsTrue(result);

            // re-populate
            model = PopulateAccountServicesModel("em20130515");
            account = model.Applications.Single(a => a.ApplicationId == 99544096);
            Assert.IsTrue(account.MayBeEligibleForReamortization);

            // cancel the extra payment
            AccountServiceModelData.CancelExtraPayment(_accountServicesData, _accountInfo.CustomerUserIdDataSet, user, account);
        }

        // NOTE: only works against QA data
        [Test]
        [Ignore("not a unit test")]
        public void IsEligibe_QATest()
        {
            var model = PopulateAccountServicesModel("QA-STAMP26");
            Assert.IsNotNull(model);
            var account = model.Applications.Single(x => x.ApplicationId == 63025782);
            Assert.IsNotNull(account);

            Assert.IsTrue(account.MayBeEligibleForReamortization);
            Assert.IsNotNull(account.ReamortizationDate);

            Assert.IsTrue(model.Applications.First().EligibleToViewAmortizationSchedule);
        }

        [Test]
        [Ignore("not a unit test")]
        public void UpdateMonthlyPaymentAmount_MultipleAccounts()
        {
            var model = PopulateAccountServicesModel("em20140827e");
            Assert.IsNotNull(model);

            var account = model.Applications.Single(a => a.ApplicationId == 76965779);
            string effectiveDate;
            account.NewMonthlyPaymentAmount = account.MinimumMonthlyPaymentAmount + 1.0m;
            var result = account.UpdateMonthlyPayment(_accountServicesData, _accountInfo.CustomerUserIdDataSet, _businessCalendar, new MockUser(), out effectiveDate);
            Assert.IsTrue(result);
        }


        [Test]
        [Ignore("not a unit test")]
        public void CancelPayment()
        {
            var model = PopulateAccountServicesModel("Auto1118201409479");
            Assert.IsNotNull(model);

            var account = model.Applications.Single(a => a.ApplicationId == 76965779);
            string effectiveDate;
            var result = account.UpdateMonthlyPayment(_accountServicesData, _accountInfo.CustomerUserIdDataSet, _businessCalendar, new MockUser(), out effectiveDate);
            Assert.IsTrue(result);
        }
        // helper methods
        /// <summary>
        /// also populates "session" level objects
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        private AccountServiceModelData PopulateAccountServicesModel(string userName)
        {
            _accountInfo = DomainServiceCustomerOperations.GetAccountInfoByCustomerUserId(userName);
            Assert.IsNotNull(_accountInfo);
            Assert.IsNotNull(_accountInfo.ApplicationsDates);
            Assert.IsNotNull(_accountInfo.CustomerUserIdDataSet);

            DocumentStoreDataSet dds = new DocumentStoreDataSet();
            Dictionary<string, string> reAttemptsTriggeredMessages = new Dictionary<string, string>();

            LoanServicingOperations.GetAccountServicingDataByUserId(userName, out _businessCalendar, out _accountServicesData, out dds);
            Assert.IsNotNull(_businessCalendar);
            Assert.IsNotNull(_accountServicesData);

            var model = new AccountServiceModelData(new MockUser());
            model.Populate(_accountInfo, _accountServicesData, _businessCalendar);

            return model;

        }
    }
}
