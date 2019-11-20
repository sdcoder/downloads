using LightStreamWeb.App_Start;
using LightStreamWeb.App_State;
using LightStreamWeb.Tests.Mocks;
using LightStreamWeb.Models.PublicSite;
using FirstAgain.Common.Extensions;
using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;

namespace LightStreamWeb.Tests
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class PublicSiteModelTests
    {
        [OneTimeSetUp]
        public static void NinjectSetup(TestContext testContext)
        {
            NinjectWebCommon.Start();
            NinjectWebCommon.Bootstrapper.Kernel.Rebind<ICurrentHttpRequest>().To<MockHttpRequest>();
            NinjectWebCommon.Bootstrapper.Kernel.Rebind<ICurrentUser>().To<MockUser>();
        }

        [Test]
        [Ignore("not a unit test")]
        public void SocialMediaIconsAreNotNull()
        {
            var model = new BasePublicSiteModel();
            Assert.IsNotNull(model, "BasePublicSiteModel is NULL");
            Assert.IsFalse(model.FacebookURL.IsNull(), "FacebookURL is NULL or empty");
            Assert.IsFalse(model.TwitterURL.IsNull(), "TwitterURL is NULL or empty");
        }

    }
}