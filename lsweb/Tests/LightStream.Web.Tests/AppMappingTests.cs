using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.SharedTypes.LoanApplication.LoanApplicationPostData;
using LightStreamWeb.Models.Apply;
using NUnit.Framework;

namespace LightStreamWeb.Tests
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class AppMappingTests
    {
        [Test]
        [Ignore("not a unit test")]
        public void IndividualLADSToNativeLoanAppModel()
        {
            // this app ID is in QA
            var applicationId = 47281069;

            var lads = DomainServiceLoanApplicationOperations.GetLoanApplicationByApplicationId(applicationId);
            Assert.IsNotNull(lads);

            var model = new NativeLoanApplicationModel();

            model.PopulateApplicationFromLADS(lads);
            model.PopulateApplicantsFromLADS(lads);
            model.PopulateOtherIncomeFromLADS(lads);
            model.PopulateCombinedFinancialsFromLADS(lads);
            Assert.AreEqual(lads.ApplicationDetail[0].AmountMinusFees, model.LoanAmount);

            Assert.IsNotNull(model.ApplicationOtherIncome);
            Assert.IsNotNull(model.PurposeOfLoan);
            Assert.AreEqual(lads.ApplicationDetail[0].PurposeOfLoan, model.PurposeOfLoan.Type);
            Assert.AreEqual(lads.ApplicationOtherIncome[0].OtherIncomeAmount, model.ApplicationOtherIncome.First().Amount);


            Assert.IsNotNull(model.Applicants.First().Residence);
            Assert.IsNotNull(model.Applicants.First().SocialSecurityNumber);
            Assert.IsNotNull(model.Applicants.First().DateOfBirth);
            Assert.IsNotNull(model.Applicants.First().Residence.Address);
            Assert.IsNotNull(model.Applicants.First().Residence.Address.AddressLine);

            Assert.IsNotNull(model.ApplicationOtherIncome);
            Assert.AreEqual(lads.ApplicationOtherIncome.Rows.Count, model.ApplicationOtherIncome.Count);
        }
    }
}
