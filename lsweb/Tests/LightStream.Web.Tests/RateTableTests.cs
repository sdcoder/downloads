using System.Linq;
using LightStreamWeb.Models.Rates;
using FirstAgain.Domain.Lookups.FirstLook;
using LightStreamWeb.Tests.Mocks;
using LightStreamWeb.App_State;
using LightStreamWeb.App_Start;
using NUnit.Framework;
using FirstAgain.Domain.SharedTypes.ContentManagement;
using System.Diagnostics.CodeAnalysis;
using FirstAgain.Domain.TestingCommon.Builders;
using FirstAgain.Domain.SharedTypes.InterestRate;
using LightStreamWeb.Shared.Rates;

namespace LightStreamWeb.Tests
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class RateTableTests
    {
        [OneTimeSetUp]
        public static void NinjectSetup()
        {
            NinjectWebCommon.Start();
            NinjectWebCommon.Bootstrapper.Kernel.Rebind<ICurrentHttpRequest>().To<MockHttpRequest>();
            NinjectWebCommon.Bootstrapper.Kernel.Rebind<ICurrentUser>().To<MockUser>();
            NinjectWebCommon.Bootstrapper.Kernel.Rebind<ICMSRatesPage>().To<MockRatesPage>();
        }

        [Test]
        public void ValidateTimeshareDisclosure_Terms()
        {
            const string expectedDisclosureStatement = @"Rates for each loan amount and term combination are shown below as a range because rates vary based on your credit profile. The lowest rates are for <a href=""/partial/cms/excellent-credit"" data-popup=""true"">excellent credit<a>.";
            var model = new RateTableModel(new MockRatesPage()) {PurposeOfLoan = PurposeOfLoanLookup.PurposeOfLoan.TimeSharePurchase};

            Assert.AreEqual(expectedDisclosureStatement, model.Disclosure);

            Assert.IsNotNull(model.LoanTermDisclosures);
            Assert.AreEqual(model.LoanTermDisclosures.Count, 4);
            Assert.AreEqual(model.LoanTermDisclosures[0], "Fixed rate, simple interest installment loans, no fees or prepayment penalties.");
            Assert.AreEqual(model.LoanTermDisclosures[1], "Loan proceeds may <u>not</u> be used to refinance any existing loan with LightStream.");
            Assert.AreEqual(model.LoanTermDisclosures[2], "Florida loans subject to <a href=\"/Apply/FloridaDocumentaryStampTaxInfo.aspx\" data-width=\"550\" data-height=\"185\" data-popup=\"true\">Documentary Stamp Tax.</a> The tax amount is not included in the quoted APR.");
            Assert.AreEqual(model.LoanTermDisclosures[3], "Rates quoted with AutoPay option. Invoicing option is 0.50 points higher.");
        }

        [Test]
        public void ValidateUnSecuredAuto_Disclosure_Terms()
        {
            const string expectedDisclosureStatement = @"Rates for each loan amount and term combination are shown below as a range because rates vary based on your credit profile. The lowest rates are for LightStream’s unsecured loan product and require <a href=""/partial/cms/excellent-credit"" data-popup=""true"">excellent credit</a>.  If your application does not qualify, you may be eligible for our secured auto loan.";
            var model = new RateTableModel(new MockRatesPage()) { PurposeOfLoan = PurposeOfLoanLookup.PurposeOfLoan.NewAutoPurchase};

            Assert.AreEqual(expectedDisclosureStatement, model.Disclosure);

            Assert.IsNotNull(model.LoanTermDisclosures);
            Assert.AreEqual(model.LoanTermDisclosures.Count, 4);
            Assert.AreEqual(model.LoanTermDisclosures[0], "Fixed rate, simple interest installment loans, no fees or prepayment penalties.");
            Assert.AreEqual(model.LoanTermDisclosures[1], "Loan proceeds may <u>not</u> be used to refinance any existing loan with LightStream.");
            Assert.AreEqual(model.LoanTermDisclosures[2], "Florida loans subject to <a href=\"/Apply/FloridaDocumentaryStampTaxInfo.aspx\" data-width=\"550\" data-height=\"185\" data-popup=\"true\">Documentary Stamp Tax.</a> The tax amount is not included in the quoted APR.");
            Assert.AreEqual(model.LoanTermDisclosures[3], "Rates quoted with AutoPay option. Invoicing option is 0.50 points higher.");
        }

        [Test]
        public void MaxRateInGrid_MatchesMaxRateOnPage()
        {
            var loanPurpose = PurposeOfLoanLookup.PurposeOfLoan.NewAutoPurchase;
            var table = new RateTableModel(new MockRatesPage());
            table.PurposeOfLoan = loanPurpose;
            table.Populate();

            var x = table.RateTable.SelectMany(row => row.Rates);
            var minRateInGrid = table.RateTable.SelectMany(row => row.Rates).Min(r => r.Min);
            var maxRateInGrid = table.RateTable.SelectMany(row => row.Rates).Max(r => r.Max);

            var builder = new InterestRatesBuilder();
            InterestRateParams p = new InterestRateParamBuilder()
                                        .WithAmount(1001m)
                                        .WithTerm(36)
                                        .WithApplicantState(StateLookup.State.Alabama)
                                        .WithPurpose(PurposeOfLoanLookup.PurposeOfLoan.AutoRefinancingSecured);


            var displayRate = new DisplayRateModel(builder.Build()).GetForPurposeOfLoan(table.PurposeOfLoan);

            Assert.AreEqual(minRateInGrid, displayRate.MinRate);
            Assert.AreEqual(maxRateInGrid, displayRate.MaxRate);
        }

        [Test]
        public void RateInGrid_MatchesCalculation()
        {
            var loanPurpose = PurposeOfLoanLookup.PurposeOfLoan.NewAutoPurchase;
            var table = new RateTableModel(new MockRatesPage());
            table.PurposeOfLoan = loanPurpose;

            table.LoanTermMonths = 24;
            table.LoanAmount = 30000;
            table.Populate();

            var x = table.RateTable.SelectMany(row => row.Rates);
            var amountRow = table.RateTable.First(row => row.LoanAmount.Min <= table.LoanAmount && row.LoanAmount.Max >= table.LoanAmount);
            var rateCell = amountRow.Rates.First();
            Assert.AreEqual(rateCell.Min, table.Rate.Min);
            Assert.AreEqual(rateCell.Max, table.Rate.Max);
        }

        [Test]
        public void RateInGrid_MatchesCalculation_HighLoanAmount()
        {
            var loanPurpose = PurposeOfLoanLookup.PurposeOfLoan.NewAutoPurchase;
            var table = new RateTableModel(new MockRatesPage());

            table.PurposeOfLoan = loanPurpose;
            table.LoanTermMonths = 24;
            table.LoanAmount = 60000;
            table.Populate();

            // this is what is displayed on the calculator
            var rateDisplayedInCalculator = table.Rate;
            
            // this is displayed in the table
            var amountRowFromRateGrid = table.RateTable.First(row => row.LoanAmount.Min <= table.LoanAmount && row.LoanAmount.Max >= table.LoanAmount);
            var cellFromRateGrid = amountRowFromRateGrid.Rates.First();

            Assert.AreEqual(cellFromRateGrid.Min, rateDisplayedInCalculator.Min);
            Assert.AreEqual(cellFromRateGrid.Max, rateDisplayedInCalculator.Max);
        }

        // at the time of writing, we do not offer rates for under $10,000, over 72 months.
        // thus, the max allowed term should change when chaning the loan amount, for this loan purpose
        // this test will fail if we start to offer that product
        [Test]
        public void MaxTerm_Changes()
        {
            var loanPurpose = PurposeOfLoanLookup.PurposeOfLoan.NewAutoPurchase;
            var table = new RateTableModel(new MockRatesPage());
            table.PurposeOfLoan = loanPurpose;

            table.LoanTermMonths = 74;
            table.LoanAmount = 7500;
            table.Populate();

            Assert.AreEqual(table.MaxTerm, 72);

            table.LoanAmount = 50000;
            table.Populate();
            Assert.AreEqual(table.MaxTerm, 84);
        }

        [Test]
        public void PWM_Rates()
        {
            var loanPurpose = PurposeOfLoanLookup.PurposeOfLoan.NotSelected;
            var table = new RateTableModel(new MockRatesPage());
            table.Discount = FlagLookup.Flag.SuntrustPrivateWealth;
            table.PurposeOfLoan = loanPurpose;

            table.LoanTermMonths = 74;
            table.LoanAmount = 7500;
            table.Populate();

            Assert.AreEqual(250000, table.MaxLoanAmount);
        }
    }
}
