using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;

namespace LightStreamWeb.Helpers.Tests
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class TrustedSiteHelperTests
    {
        [Test]
        [TestCase("https://test.lightstream.com/Test/IFrame", "https://www.lightstream.com/kbb-auto", true)]
        [TestCase("https://test2.lightstream.com/Test/IFrame", "https://www.lightstream.com/kbb-auto", true)]
        [TestCase("https://www.kbb.com", "https://www.lightstream.com/kbb-auto", true)]
        [TestCase("https://test.kbb.com", "https://www.lightstream.com/kbb-auto", true)]
        [TestCase("https://demo2.kbb.com", "https://www.lightstream.com/kbb-auto", true)]
        [TestCase("http://www.kbb.com", "https://www.lightstream.com/kbb-auto", true)]
        [TestCase("http://demo2.kbb.com", "https://www.lightstream.com/kbb-auto", true)]
        [TestCase("https://www.kbb.com", "https://www.lightstream.com/kbb-other-page", false)]
        [TestCase("https://test.kbb.com", "https://www.lightstream.com/kbb-other-page", false)]
        [TestCase("https://demo2.kbb.com", "https://www.lightstream.com/kbb-other-page", false)]
        [TestCase("http://10.7.218.8/Static/IpLightstream.html", "https://test.lightstream.com/rates/widget?purposeOfLoan=HomeImprovement", true)]
        [TestCase("https://10.7.218.8/Static/IpLightstream.html", "https://test.lightstream.com/rates/widget?purposeOfLoan=HomeImprovement", true)]
        [TestCase("http://10.7.215.8/lightstream.html", "https://test.lightstream.com/rates/widget?purposeOfLoan=HomeImprovement", true)]
        [TestCase("https://test-ice.suntrust.com/iframe", "https://test.lightstream.com/rates/widget?purposeOfLoan=HomeImprovement", true)]
        public void It_should_allow_to_iFrame_domain(string urlReferrer, string urlRequest, bool shouldTestPass)
        {
            // Setup
            var expected = shouldTestPass;

            // Trigger
            var actual = TrustedSiteHelper.CanIframe(urlReferrer, urlRequest);

            // Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

    }
}