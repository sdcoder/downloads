using LightStreamWeb.Models.Rates;
using LightStreamWeb.App_State;
using LightStreamWeb.Controllers;
using LightStreamWeb.Models.PublicSite;
using System.Web;
using System.IO;
using System.Linq;
using Moq;
using Ninject;
using LightStreamWeb.Tests.Mocks;
using LightStreamWeb.App_Start;
using System.Web.Mvc;
using NUnit.Framework;
using FirstAgain.Domain.Lookups.FirstLook;
using RateCalculatorController = LightStreamWeb.Controllers.Services.RateCalculatorController;
using LightStreamWeb.Models.Middleware;
using System.Diagnostics.CodeAnalysis;
using LightStreamWeb.Shared.Rates;

namespace LightStreamWeb.Tests
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class RateModelTests
    {
        [OneTimeSetUp]
        public static void NinjectSetup()
        {
            NinjectWebCommon.Start();
            NinjectWebCommon.Bootstrapper.Kernel.Rebind<ICurrentHttpRequest>().To<MockHttpRequest>();
            NinjectWebCommon.Bootstrapper.Kernel.Rebind<ICurrentUser>().To<MockUser>();
        }

        [Test]
        [Ignore("not a unit test")]
        public void ValidateFACTDiscount()
        {
            var kernel = new StandardKernel();
            kernel.Bind<ICurrentHttpRequest>().To<MockHttpRequest>();

            var nativeUser = new Mock<ICurrentUser>();
            nativeUser.SetupGet(x => x.FirstAgainCodeTrackingId).Returns(1);

            var settings = new Mock<IAppSettings>();

            var poolCorpUser = new Mock<ICurrentUser>();
            poolCorpUser.SetupGet(x => x.FirstAgainCodeTrackingId).Returns(15012);

            var sb = new System.Text.StringBuilder();
            TextWriter w = new StringWriter(sb);
            var context = new HttpContext(new HttpRequest("", "http://www.example.com", ""), new HttpResponse(w));
            HttpContext.Current = context;
            var nativeResult = new PublicSiteController(nativeUser.Object, settings.Object).LandingPage("home-improvement") as ViewResult;
            var poolCorpResult = new PublicSiteController(poolCorpUser.Object, settings.Object).LandingPage("home-improvement") as ViewResult;

            var nativeModel = nativeResult.Model as LandingPageModel;
            var poolCorpModel = poolCorpResult.Model as LandingPageModel;

            Assert.IsTrue(nativeModel.DisplayRate.Rate > poolCorpModel.DisplayRate.Rate);
        }

        //validate that “Not Selected” shows up in the list of loan purposes
        [Test]
        public void ValidateNotSelectedVisibleInLoanPurposes()
        {
            var model = new RateTableModel(null);
            model.Populate();

            Assert.IsTrue(model.LoanPurposes.Any(l => l.Value == "NotSelected" && l.Caption == "Please select a loan purpose"));
        }

        [Test]
        public void ValidateMinMaxLoanRatesPopulated()
        {
            var model = new RateTableModel(null);
            model.Populate();

            Assert.True(model.MinLoanAmountHint != 0 && model.MaxLoanAmountHint != 0);
        }

    }
}