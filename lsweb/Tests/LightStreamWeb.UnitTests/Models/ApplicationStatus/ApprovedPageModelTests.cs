using System.Diagnostics.CodeAnalysis;
using FirstAgain.Domain.Lookups.FirstLook;
using LightStreamWeb.Models.Components;
using NUnit.Framework;

namespace LightStreamWeb.UnitTests.Models.ApplicationStatus
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class ApprovedPageModelTests
    {
        [Test]
        public void TestEnvironmentWidget()
        {
            var devModel = new EnvironmentModel(EnvironmentLookup.Environment.Development);
            Assert.IsFalse(devModel.IsProduction);
            var prodModel = new EnvironmentModel(EnvironmentLookup.Environment.Production);
            Assert.IsTrue(prodModel.IsProduction);
        }

        // ApprovedPageModel current calls backend - not unit testable
        //[Test]
        //public void Test_Mapping_Counter_Amount()
        //{
        //    //Arrange
        //    decimal expectedCounter = 99;
        //    //Act
        //    var cuidds = new CustomerUserIdDataSetBuilder().WithWithLoanTermsRequest(expectedCounter, LoanTermsRequestTypeLookup.LoanTermsRequestType.ApplicationCounter).Build();
        //    var lods = new LoanOfferDataSetBuilder().Build();
        //    var model = new ApprovedPageModel(cuidds, lods);

        //    Assert.IsNotNull(model.ChangeLoanTermsMessage);
        //}


        // ApprovedPageModel current calls backend - not unit testable
        //[Test]
        //[Ignore("")]
        //public void TestWisconsinNonApplicantSpouseLoanAmountLimit()
        //{
        //    var customerUserIdData = new CustomerUserIdDataSet();
        //    var loanOfferData = new LoanOfferDataSet();

        //    var applicationRow = customerUserIdData.Application.NewApplicationRow();
        //    applicationRow.ApplicationId = 1;
        //    applicationRow.ApplicationStatusTypeId = (short)ApplicationStatusTypeLookup.ApplicationStatusType.Approved;
        //    customerUserIdData.Application.Rows.Add(applicationRow);

        //    var loanOfferRow = loanOfferData.LoanOffer.NewLoanOfferRow();
        //    loanOfferRow.ApprovalDate = new DateTime(2015, 5, 28);
        //    loanOfferRow.IsActive = true;

        //    var model = new ApprovedPageModel(customerUserIdData, loanOfferData);
        //    var temp = true;
        //    Assert.IsTrue(temp);
        //}

    }
}