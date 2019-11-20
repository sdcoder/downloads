using Moq;
using FluentAssertions;
using NUnit.Framework;
using LightStreamWeb.Models.PublicSite;
using LightStreamWeb.UnitTests.Mocks;
using LightStreamWeb.App_State;
using LightStreamWeb.Models.Middleware;
using LightStreamWeb.Models.Components;
using FirstAgain.Domain.Lookups.FirstLook;
using System.Diagnostics.CodeAnalysis;

namespace LightStreamWeb.UnitTests.Models
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class HomePageModelTests
    {
        private LightStreamPageDefault Defaults;
        
        [OneTimeSetUp]
        public void Init()
        {
            Defaults = new LightStreamPageDefault
            {
                Banner = "http://url.com/image.png",
                BannerAlt = "header"
            };
        }

        [Test]
        [Ignore("HomePageModel is not testable yet")]
        public void Given_Nothing_When_Accessing_AdObject_Then_Is_Not_Null()
        {
            var user = new MockUser();
            var httpRequest = new Mock<ICurrentHttpRequest>();
            var contentManager = new ContentManager(httpRequest.Object, user);
            
            var homepage = new HomePageModel(contentManager, Defaults);
            homepage.FirstAdObject.Should().NotBeNull();
        }

        [Test]
        [Ignore("HomePageModel is not testable yet")]
        public void Given_Nothing_When_Accessing_Banner_Then_Is_Not_Null()
        {
            var user = new MockUser();
            var httpRequest = new Mock<ICurrentHttpRequest>();
            var contentManager = new ContentManager(httpRequest.Object, user);

            var homepage = new HomePageModel(contentManager, Defaults);
            homepage.Banner.Should().NotBeNull();
        }

        [Test]
        public void TestEnvironmentWidget()
        {
            var devModel = new EnvironmentModel(EnvironmentLookup.Environment.Development);
            Assert.IsFalse(devModel.IsProduction);
            var prodModel = new EnvironmentModel(EnvironmentLookup.Environment.Production);
            Assert.IsTrue(prodModel.IsProduction);
        }

    }
}
