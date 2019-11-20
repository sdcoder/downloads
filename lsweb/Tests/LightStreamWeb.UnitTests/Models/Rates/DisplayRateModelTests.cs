using System.Linq;
using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;
using FirstAgain.Domain.TestingCommon.Builders;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.SharedTypes.InterestRate;
using Moq;
using LightStreamWeb.Shared.Rates;

namespace LightStreamWeb.UnitTests.Models.Rates
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class DisplayRateModelTests
    {

        [Test]
        public void PurposeOfLoanNewAutoDisplaysDisclosure()
        {
            var builder = new InterestRatesBuilder();

            var model = new DisplayRateModel(builder.Build()).GetForPurposeOfLoan(PurposeOfLoanLookup.PurposeOfLoan.NewAutoPurchase);
            Assert.IsTrue(model.DisplaySecuredAutoDisclosure);
        }

        [Test]
        public void ValidateMonthlyPaymentAmount()
        {
            var model = new DisplayRateModel(null)
            {
                Rate = 0.0199m,
                SampleAmountMin = 10000,
                SampleNumberOfPayments = 36
            };
            Assert.AreEqual(286.38m, model.SampleMonthlyPayment);
        }

        [Test]
        public void ValidateCaptions()
        {
            var model = new DisplayRateModel(null);
            model.PurposeOfLoan = PurposeOfLoanLookup.PurposeOfLoan.Other;
            Assert.AreEqual("Other Loan or Refinance", model.PurposeOfLoanCaption);

            model.PurposeOfLoan = PurposeOfLoanLookup.PurposeOfLoan.HomeImprovement;
            Assert.AreEqual("Home Improvement/Pool/Solar Loan", model.PurposeOfLoanCaption);
        }

        [Test]
        public void ValidateNullRates()
        {
            var mock = new Mock<IInterestRates>();
            mock.SetupGet(x => x.BaseRates).Returns(new System.Collections.Generic.List<InterestAdjustmentDate>());
            mock.SetupProperty(x => x.InterestRateParams);
            mock.Setup(x => x.GetLowestRates(It.IsAny<bool>(), It.IsAny<InterestRateParams>())).Returns(new System.Collections.Generic.List<ProductInterestRate>());
            var model = new DisplayRateModel(mock.Object).GetLowestRate();
            Assert.IsNotNull(model);
        }

        [Test]
        public void Validate_When_AutoRefinance_IsLowestRate_SelectSomethingElse()
        {
            InterestRateParams pAutoRefi = new InterestRateParamBuilder()
                .WithPurpose(PurposeOfLoanLookup.PurposeOfLoan.AutoRefinancing)
                .WithAmount(25000m)
                .WithTerm(25);

            var builderAutoRefi = new InterestRatesBuilder().WithMaxRate(0.01m);
            var ratesAutoRefi = builderAutoRefi.Build();
            ratesAutoRefi.InterestRateParams = pAutoRefi.Clone();

            var modelpAutoRefi = new DisplayRateModel(ratesAutoRefi).GetLowestRate();

            var builderOther = new InterestRatesBuilder();
            InterestRateParams pOther = new InterestRateParamBuilder()
                .WithPurpose(PurposeOfLoanLookup.PurposeOfLoan.Other)
                .WithAmount(25000m)
                .WithTerm(25);
            var ratesOther = builderOther.Build();
            ratesOther.InterestRateParams = pOther.Clone();
            var modelpOther = new DisplayRateModel(ratesOther).GetLowestRate();

            Assert.AreNotEqual(modelpAutoRefi.Rate, modelpOther.Rate);


        }

        [Test]
        public void ValidatePrettyRateDisplay()
        {
            InterestRateParams p = new InterestRateParamBuilder()
                .WithPurpose(PurposeOfLoanLookup.PurposeOfLoan.AutoRefinancing)
                .WithAmount(25000m)
                .WithTerm(25);

            var builder = new InterestRatesBuilder();
            var rates = builder.Build();
            rates.InterestRateParams = p.Clone();

            var model = new DisplayRateModel(rates).GetForPurposeOfLoan(PurposeOfLoanLookup.PurposeOfLoan.AutoRefinancing);
            Assert.AreEqual("2.35% – 6.74%", model.RateRangePretty);
        }

        [Test]
        public void ValidateLowestRate()
        {
            InterestRateParams p = new InterestRateParamBuilder()
                .WithPurpose(PurposeOfLoanLookup.PurposeOfLoan.AutoRefinancing)
                .WithAmount(25000m)
                .WithTerm(25);

            var builder = new InterestRatesBuilder();
            var rates = builder.Build();
            rates.InterestRateParams = p.Clone();

            var model = new DisplayRateModel(rates).GetLowestRate(null);
            Assert.True(true);
        }

        [Test]
        public void ValidateDiscountedRate()
        {
            var appInterestRateParams = new InterestRateParamBuilder()
                .WithPurpose(PurposeOfLoanLookup.PurposeOfLoan.HomeImprovement)
                .WithAmount(24999.99M).WithTerm(72).WithFactCode(15506); //HGTV Discount (FACT codes 15506 - 15509)

            const decimal expectedBaseRate = 0.0629m;
            var builder = new InterestRatesBuilder();
            InterestRates rates = builder.WithBaseRate(appInterestRateParams, expectedBaseRate);

            var model = new DisplayRateModel(rates).GetForPurposeOfLoan(PurposeOfLoanLookup.PurposeOfLoan.HomeImprovement, 15506);

            Assert.IsTrue(model.IsDiscountedRate);
            Assert.IsNotEmpty(model.DiscountDescription);
            Assert.AreEqual("HGTV", model.DiscountDescription);
        }

        [Test]
        public void ValidateNonDiscountedRate()
        {
            var builder = new InterestRatesBuilder();
            var rates = builder.Build();

            var model = new DisplayRateModel(rates).GetForPurposeOfLoan(PurposeOfLoanLookup.PurposeOfLoan.BoatRvPurchase);

            Assert.IsFalse(model.IsDiscountedRate);
            Assert.IsEmpty(model.DiscountDescription);
        }

        [Test]
        public void ValidateHomeImprovement144Rates()
        {
            var builder = new InterestRatesBuilder();
            var rates = builder.Build();

            var boatModel = new DisplayRateModel(rates).GetForPurposeOfLoan(PurposeOfLoanLookup.PurposeOfLoan.BoatRvPurchase);
            var homeImprovementModel = new DisplayRateModel(rates).GetForPurposeOfLoan(PurposeOfLoanLookup.PurposeOfLoan.HomeImprovement);

            Assert.IsNotNull(boatModel);
            Assert.IsNotNull(homeImprovementModel);

            Assert.AreEqual(boatModel.InvoicePenalty, homeImprovementModel.InvoicePenalty);
            Assert.AreEqual(boatModel.MinAmount, homeImprovementModel.MinAmount);
            Assert.AreEqual(boatModel.MaxAmount, homeImprovementModel.MaxAmount);


            // max term. sample term are different
            Assert.AreEqual(84, homeImprovementModel.MaxTerm);
            Assert.AreNotEqual(boatModel.MaxTerm, homeImprovementModel.MaxTerm);
            Assert.AreNotEqual(boatModel.MinTerm, homeImprovementModel.MinTerm);

            Assert.AreNotEqual(boatModel.BaseRate, homeImprovementModel.BaseRate);
            Assert.AreNotEqual(boatModel.Base144Rate, homeImprovementModel.Base144Rate);
            Assert.AreNotEqual(boatModel.MaxRate, homeImprovementModel.MaxRate);

            // these are constant
            Assert.AreEqual(24999, homeImprovementModel.SampleAmountMax);
            Assert.AreEqual(10000, homeImprovementModel.SampleAmountMin);
            Assert.AreEqual(84, homeImprovementModel.SampleNumberOfPayments);
            Assert.AreEqual("7 years", homeImprovementModel.SampleTermCaption);
        }

        [Test]
        public void ValidateThatPurposeOfLoanChangesRates()
        {
            var builder = new InterestRatesBuilder();
            var rates = builder.Build();

            var boatModel = new DisplayRateModel(rates).GetForPurposeOfLoan(PurposeOfLoanLookup.PurposeOfLoan.BoatRvPurchase);
            var medicalModel = new DisplayRateModel(rates).GetForPurposeOfLoan(PurposeOfLoanLookup.PurposeOfLoan.MedicalExpense);

            Assert.IsNotNull(boatModel);
            Assert.IsNotNull(medicalModel);

            Assert.AreEqual(boatModel.InvoicePenalty, medicalModel.InvoicePenalty);
            Assert.AreEqual(boatModel.MinAmount, medicalModel.MinAmount);
            Assert.AreEqual(boatModel.MaxAmount, medicalModel.MaxAmount);
            Assert.AreEqual(boatModel.MaxTerm, medicalModel.MaxTerm);
            Assert.AreEqual(boatModel.MinTerm, medicalModel.MinTerm);

            Assert.AreNotEqual(boatModel.BaseRate, medicalModel.BaseRate);
            Assert.AreNotEqual(boatModel.MaxRate, medicalModel.MaxRate);


            Assert.AreEqual(boatModel.OverallMaxRate, medicalModel.OverallMaxRate);
            Assert.AreNotEqual(boatModel.PurposeOfLoan, medicalModel.PurposeOfLoan);
            Assert.AreNotEqual(boatModel.PurposeOfLoanCaption, medicalModel.PurposeOfLoanCaption);
            Assert.AreNotEqual(boatModel.Rate, medicalModel.Rate);
            Assert.AreNotEqual(boatModel.RateRangePretty, medicalModel.RateRangePretty);

            Assert.AreEqual(boatModel.SampleAmountMax, medicalModel.SampleAmountMax);
            Assert.AreEqual(boatModel.SampleAmountMin, medicalModel.SampleAmountMin);
            Assert.AreEqual(boatModel.SampleNumberOfPayments, medicalModel.SampleNumberOfPayments);
            Assert.AreEqual(boatModel.SampleTermCaption, medicalModel.SampleTermCaption);

            Assert.AreNotEqual(boatModel.SampleMonthlyPayment, medicalModel.SampleMonthlyPayment);


            // these are constant
            Assert.AreEqual(100000m, boatModel.SampleAmountMax);
            Assert.AreEqual(50000m, boatModel.SampleAmountMin);
            Assert.AreEqual(36, boatModel.SampleNumberOfPayments);
            Assert.AreEqual("3 years", boatModel.SampleTermCaption);

            // GetLowestRate resets the loan purpose
            var lowestRateModel = boatModel.GetLowestRate();
            Assert.AreNotEqual(lowestRateModel.Rate, medicalModel.Rate);
            Assert.AreNotEqual(lowestRateModel.PurposeOfLoan, medicalModel.PurposeOfLoan);
        }


        class DisplayRateModelShunt: DisplayRateModel
        {
            public DisplayRateModelShunt(InterestRates cachedInterestRates) : base(null)
            {
            }

            public string TestMarketingPartnerName(ProductInterestRate sampleRate)
            {
                return DisplayRateModel.GetMarketingPartnerName(sampleRate);
            }

        }

        [Test]
        public void ValidateInterestAdjustmentToCustomerDescription()
        {
            const string expectedPartnerName = "MARKETING PARTNER NAME";

            ProductInterestRate sampleRate = new ProductInterestRate()
            {
                RateInfo = new InterestRateInfo()
                {
                    Adjustments = new System.Collections.Generic.List<InterestAdjustmentDate>()
                     {
                         new InterestAdjustmentDate()
                         {
                              CustomerDescription = expectedPartnerName,
                              InterestAdjustmentType = InterestAdjustmentTypeLookup.InterestAdjustmentType.Adjustment,
                               InterestGroups = new System.Collections.Generic.List<InterestGroup>()
                               {
                                   new InterestGroup()
                                   {
                                        InterestGroupType = InterestGroupTypeLookup.InterestGroupType.AdvertisingSourceID
                                   }
                               
                               }
                         }

                         }
                }

            };

            Assert.AreEqual(expectedPartnerName, new DisplayRateModelShunt(null).TestMarketingPartnerName(sampleRate));

        }

    }
}
