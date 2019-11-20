using NUnit.Framework;
using LightStreamWeb.Models.ApplicationStatus;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.TestingCommon.Builders;
using FirstAgain.Domain.Lookups.FirstLook;
using LightStreamWeb.Helpers;
using FirstAgain.Common.Caching;
using FirstAgain.Domain.ServiceModel.Contracts.MessageContracts.CustomerOperations;
using System.Diagnostics.CodeAnalysis;

namespace LightStreamWeb.UnitTests.Models.ApplicationStatus
{

    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class PendingPageModelTests
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


        [Test]
        public void TestDataSetMapping_PurposeOfLoan()
        {
            CustomerUserIdDataSet dsNewAutoPurchase = new CustomerUserIdDataSetBuilder().WithPurposeOfLoan(PurposeOfLoanLookup.PurposeOfLoan.NewAutoPurchase).Build();
            AssertDataMappingForPurposeOfLoan(dsNewAutoPurchase, PurposeOfLoanLookup.PurposeOfLoan.NewAutoPurchase);

            CustomerUserIdDataSet dsUsedAutoPurchase = new CustomerUserIdDataSetBuilder().WithPurposeOfLoan(PurposeOfLoanLookup.PurposeOfLoan.UsedAutoPurchase).Build();
            AssertDataMappingForPurposeOfLoan(dsUsedAutoPurchase, PurposeOfLoanLookup.PurposeOfLoan.UsedAutoPurchase);

        }

        private void AssertDataMappingForPurposeOfLoan(CustomerUserIdDataSet cuidds, PurposeOfLoanLookup.PurposeOfLoan purposeOfLoan)
        {
            var response = new GetAccountInfoResponse()
            {
                CustomerUserIdDataSet = cuidds
            };
            var dataSetModel = new PendingPageModel(cuidds, null);
            Assert.AreEqual(purposeOfLoan, dataSetModel.GetPurposeOfLoan());

            var sessionData = DataSetToSessionStateMapper.Map(cuidds.Application[0].ApplicationId, response);
            var sessionModel = new PendingPageModel(sessionData);
            Assert.AreEqual(purposeOfLoan, sessionModel.GetPurposeOfLoan());
        }
        private void AssertDataMappingForHasEnotices(CustomerUserIdDataSet cuidds, bool expected)
        {
            var response = new GetAccountInfoResponse()
            {
                CustomerUserIdDataSet = cuidds
            };
            var dataSetModel = new PendingPageModel(cuidds, null);
            Assert.AreEqual(expected, dataSetModel.HasEnotices());

            var sessionData = DataSetToSessionStateMapper.Map(cuidds.Application[0].ApplicationId, response);
            var sessionModel = new PendingPageModel(sessionData);
            Assert.AreEqual(expected, sessionModel.HasEnotices());
        }


        [Test]
        public void TestDataSetMapping_Is_Approved_NLTR_Q()
        {
            // yes, NLTR-Q
            LoanOfferDataSet loanOfferDataSet_nltrQ = new LoanOfferDataSetBuilder().WithLatestLoanTermsRequestStatus(LoanTermsRequestStatusLookup.LoanTermsRequestStatus.PendingQ);
            AssertDataMappingForIsNLTRQ(loanOfferDataSet_nltrQ, true);

            // no, not NLTR-Q
            LoanOfferDataSet loanOfferDataSet_not_nltrQ = new LoanOfferDataSetBuilder();
            AssertDataMappingForIsNLTRQ(loanOfferDataSet_not_nltrQ, false);
        }

        [Test]
        public void TestDataSetMapping_HasENotices()
        {
            // yes, NLTR-Q
            var cuidds = new CustomerUserIdDataSetBuilder().WithApplicationDocument(CorrespondenceCategoryLookup.CorrespondenceCategory.DeclineNotice).Build();
            AssertDataMappingForHasEnotices(cuidds, true);

            // no, not NLTR-Q
            var cuidds2 = new CustomerUserIdDataSetBuilder().WithApplicationDocument(CorrespondenceCategoryLookup.CorrespondenceCategory.DuplicateApp).Build();
            AssertDataMappingForHasEnotices(cuidds2, false);

        }

        private void AssertDataMappingForIsNLTRQ(LoanOfferDataSet loanOfferDataSet, bool expectedResult)
        {
            CustomerUserIdDataSet cuidds = new CustomerUserIdDataSetBuilder().Build();

            LoanOfferDataSet loanOfferDataSet_nltrQ = new LoanOfferDataSetBuilder().WithLatestLoanTermsRequestStatus(LoanTermsRequestStatusLookup.LoanTermsRequestStatus.PendingQ);
            var dataSetModel = new PendingPageModel(cuidds, loanOfferDataSet);
            Assert.AreEqual(expectedResult, dataSetModel.Is_Approved_NLTR_Q());

            var sessionData = DataSetToSessionStateMapper.Map(cuidds.Application[0].ApplicationId, cuidds, loanOfferDataSet);
            var sessionModel = new PendingPageModel(sessionData);
            Assert.AreEqual(expectedResult, sessionModel.Is_Approved_NLTR_Q());
        }

        [Test]
        public void TestDataSetMapping_AddCoApplicantIsEnabled()
        {
            LoanOfferDataSet lods = new LoanOfferDataSetBuilder().Build();

            // not add coapplicant, when no flag is set
            Assert_DataMapping_For_AddCoApplicantIsEnabled(
                new CustomerUserIdDataSetBuilder().Build(),
                false);

            // yes, add co applicant when flag is set
            Assert_DataMapping_For_AddCoApplicantIsEnabled(
                new CustomerUserIdDataSetBuilder().WithFlag(FlagLookup.Flag.AddCoApplicant, true).Build(),
                true);

            // no, when flag is set but off
            Assert_DataMapping_For_AddCoApplicantIsEnabled(
                new CustomerUserIdDataSetBuilder().WithFlag(FlagLookup.Flag.AddCoApplicant, false).Build(),
                false);

            // no, when flag is set and in AML review
            Assert_DataMapping_For_AddCoApplicantIsEnabled(
                new CustomerUserIdDataSetBuilder()
                .WithFlag(FlagLookup.Flag.AddCoApplicant, true)
                .WithFlag(FlagLookup.Flag.IsInAMLReview, true)
                .Build(),
                false);
        }

        private void Assert_DataMapping_For_AddCoApplicantIsEnabled(CustomerUserIdDataSet cuidds, bool expectedResult)
        {
            var response = new GetAccountInfoResponse()
            {
                CustomerUserIdDataSet = cuidds
            };
            LoanOfferDataSet lods = new LoanOfferDataSetBuilder().Build();

            var dataSetModel = new PendingPageModel(cuidds, lods);
            Assert.AreEqual(expectedResult, dataSetModel.AddCoApplicantIsEnabled());

            var sessionData = DataSetToSessionStateMapper.Map(cuidds.Application[0].ApplicationId, response);
            var sessionModel = new PendingPageModel(sessionData);
            Assert.AreEqual(expectedResult, sessionModel.AddCoApplicantIsEnabled());
        }

    }
}
