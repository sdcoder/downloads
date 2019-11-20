using System;
using System.Linq;
using NUnit.Framework;
using LightStreamWeb.Models.ApplicationStatus;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.TestingCommon.Builders;
using FirstAgain.Domain.Lookups.FirstLook;
using LightStreamWeb.Helpers;
using FirstAgain.Common.Caching;
using System.Diagnostics.CodeAnalysis;

namespace LightStreamWeb.UnitTests.Models.ApplicationStatus
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class ApplicationStatusPageModelTests
    {

        #region test fixture set-up
        [OneTimeSetUp]
        public void InitFeature()
        {
            MachineCache.Entries.Clear();
            MachineCache.AddCacheEntry(new MachineCacheEntry("BusinessConstants", new BusinessConstantsBuilder().Build()));
            MachineCache.AddCacheEntry(new MachineCacheEntry("InterestRateTable", new InterestRatesBuilder(leanAndMean: true).Build()));
        }
        #endregion

        #region withdraw date

        [Test]
        public void Test_DataSetMapping_WithdrawDate()
        {
            var cuidds = new CustomerUserIdDataSetBuilder().Build();
            DateTime expectedDate = DateTime.Now.AddDays(7);
            cuidds.ApplicationDetail.First().WithdrawDate = expectedDate;

            AssertDataSetMapping_WithdrawDate(cuidds, expectedDate);

        }
        private void AssertDataSetMapping_WithdrawDate(CustomerUserIdDataSet cuidds, DateTime? expectedDate)
        {
            var currentApplicationData = DataSetToSessionStateMapper.Map(cuidds.Application[0].ApplicationId, cuidds, null);
            Assert.AreEqual(expectedDate, currentApplicationData.WithdrawDate);
        }

        #endregion

        #region counter
        [Test]
        public void Test_Mapping_When_No_Counter()
        {
            //Arrange
            //Act
            var cuidds = new CustomerUserIdDataSetBuilder().Build();
            //Assert
            AssertDataSetMapping_CounterAmount(cuidds, null);
        }

        [Test]
        public void Test_Mapping_When_Counter_Exists()
        {
            //Arrange
            decimal expectedCounter = 99;
            //Act
            var cuidds = new CustomerUserIdDataSetBuilder().WithWithLoanTermsRequest(expectedCounter, LoanTermsRequestTypeLookup.LoanTermsRequestType.ApplicationCounter).Build();

            //Assert
            AssertDataSetMapping_CounterAmount(cuidds, expectedCounter);
        }

        private void AssertDataSetMapping_CounterAmount(CustomerUserIdDataSet cuidds, decimal? expectedAmount)
        {
            var currentApplicationData = DataSetToSessionStateMapper.Map(cuidds.Application[0].ApplicationId, cuidds, null);
            Assert.AreEqual(expectedAmount, currentApplicationData.CounterAmount);
        }

        #endregion

        #region IsAutoApprovalEligible

        /// <summary>
        /// totally irrelevant flag here
        /// </summary>
        [Test]
        public void Test_Mapping_When_Not_IsAutoApprovalEligible()
        {
            //Arrange
            //Act
            var cuidds = new CustomerUserIdDataSetBuilder().WithFlag(FlagLookup.Flag.AddCoApplicant, true).Build();
            //Assert
            TestDataSetMapping_IsAutoApprovalEligible(cuidds, false);
        }

        [TestCase(FlagLookup.Flag.MLAutoApprovalEligible)]
        [TestCase(FlagLookup.Flag.AutoApprovalEligible)]
        public void Test_Mapping_When_IsAutoApprovalEligible(FlagLookup.Flag flag)
        {
            //Arrange
            //Act
            var cuidds = new CustomerUserIdDataSetBuilder().WithFlag(flag, true).Build();
            //Assert
            TestDataSetMapping_IsAutoApprovalEligible(cuidds, true);
        }

        private void TestDataSetMapping_IsAutoApprovalEligible(CustomerUserIdDataSet cuidds, bool expectedResult)
        {
            //var DSModel = new ApplicationStatusPageModel(cuidds);
            var currentApplicationData = DataSetToSessionStateMapper.Map(cuidds.Application[0].ApplicationId, cuidds, null);
            Assert.AreEqual(expectedResult, currentApplicationData.IsAutoApprovalEligible);
        }


        #endregion


        public void TestDataSetMapping_HasENotices(CustomerUserIdDataSet cuidds, bool expectedResult)
        {
            var DSModel = new ApplicationStatusPageModel(cuidds);

            var sessionData = DataSetToSessionStateMapper.Map(cuidds.Application[0].ApplicationId, cuidds, null);
            var sessionModel = new ApplicationStatusPageModel(sessionData);
            Assert.AreEqual(expectedResult, sessionModel.HasEnotices());
        }





        [Test]
        public void Test_HasEnotices()
        {
            var cuidds = new CustomerUserIdDataSetBuilder().WithApplicationDocument(CorrespondenceCategoryLookup.CorrespondenceCategory.AccountLockPasscode).Build();
            TestDataSetMapping_HasENotices(cuidds, false);

            var sessionData = DataSetToSessionStateMapper.Map(cuidds.Application[0].ApplicationId, cuidds, null);
            Assert.IsFalse(new ApplicationStatusPageModel(sessionData).HasEnotices());
            var modelWin = new ApplicationStatusPageModel(new CustomerUserIdDataSetBuilder().WithApplicationDocument(CorrespondenceCategoryLookup.CorrespondenceCategory.CreditScoreDisclosureHtml).Build());
            Assert.IsTrue(modelWin.HasEnotices());


        }

        [Test]
        public void TestDataSetMapping_EmailPreferences()
        {
            AssertDataMapping_EmailPreferences(
                new CustomerUserIdDataSetBuilder().WithPaymentType(PaymentTypeLookup.PaymentType.AutoPay).Build(),
                null,
                SolicitationPreferenceLookup.FilterType.Autopay);

            AssertDataMapping_EmailPreferences(
                new CustomerUserIdDataSetBuilder().WithPaymentType(PaymentTypeLookup.PaymentType.Invoice).Build(),
                null,
                SolicitationPreferenceLookup.FilterType.Invoice);

            AssertDataMapping_EmailPreferences(
                new CustomerUserIdDataSetBuilder().WithPaymentType(PaymentTypeLookup.PaymentType.Invoice).Build(),
                new LoanOfferDataSetBuilder().WithPaymentType(PaymentTypeLookup.PaymentType.AutoPay).Build(),
                SolicitationPreferenceLookup.FilterType.Autopay);

            AssertDataMapping_EmailPreferences(
                new CustomerUserIdDataSetBuilder().WithPaymentType(PaymentTypeLookup.PaymentType.Invoice).Build(),
                new LoanOfferDataSetBuilder().WithPaymentType(PaymentTypeLookup.PaymentType.Invoice).Build(),
                SolicitationPreferenceLookup.FilterType.Invoice);
        }

        private void AssertDataMapping_EmailPreferences(
                    CustomerUserIdDataSet cuidds,
                    LoanOfferDataSet lods,
                    SolicitationPreferenceLookup.FilterType expectedResult)
        {
            var dataSetModel = new ApplicationStatusPageModel(cuidds, lods);
            Assert.AreEqual(expectedResult, dataSetModel.GetSolicitationPreferenceType());

            var sessionData = DataSetToSessionStateMapper.Map(cuidds.Application[0].ApplicationId, cuidds, lods);
            var sessionDataModel = new ApplicationStatusPageModel(sessionData);
            Assert.AreEqual(expectedResult, sessionDataModel.GetSolicitationPreferenceType());
        }

        [Test]
        public void TestDataSetMapping_CreditTier()
        {
            CustomerUserIdDataSet cuidds = new CustomerUserIdDataSetBuilder().Build();

            AssertDataMapping_IsPrime5(
                new LoanOfferDataSetBuilder().WithCreditTier(CreditTierLookup.CreditTier.Prime5),
                true);

            AssertDataMapping_IsPrime5(
                new LoanOfferDataSetBuilder().WithCreditTier(CreditTierLookup.CreditTier.SuperPrime),
                false);
        }

        private void AssertDataMapping_IsPrime5(
                    LoanOfferDataSet lods,
                    bool expectedResult)
        {
            CustomerUserIdDataSet cuidds = new CustomerUserIdDataSetBuilder().Build();

            var dataSetModel = new ApplicationStatusPageModel(cuidds, lods);
            Assert.AreEqual(expectedResult, dataSetModel.IsPrime5);

            var sessionData = DataSetToSessionStateMapper.Map(cuidds.Application[0].ApplicationId, cuidds, lods);
            var sessionDataModel = new ApplicationStatusPageModel(sessionData);
            Assert.AreEqual(expectedResult, sessionDataModel.IsPrime5);
        }

    }
}
