using FirstAgain.Domain.SharedTypes.ContentManagement;
using LightStreamWeb.Helpers;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace LightStreamWeb.UnitTests.Helpers
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    public class WebRedirectHelperTests
    {
        WebRedirectGroup[] _webRedirectGroups = new WebRedirectGroup[]
        {
            new WebRedirectGroup(31, 11, null, 4, "Test 2", 8, "Test Mkt Plmnt Cat", 16, "Test Mkt Plmnt", new DateTime(2016, 1, 2), null, "~/targeturl", "~/vanityurl", false),
            new WebRedirectGroup(25, 5, 10, 4, "Test 2", 8, "Test Mkt Plmnt Cat", 16, "Test Mkt Plmnt", new DateTime(2016, 1, 2), null, "~/targeturl", null, false)
        };

        [TestCase]
        public void GetWebRedirectGroup_FactId_Found_Test()
        {
            var expected = _webRedirectGroups[1];

            var webRedirectHelper = new WebRedirectHelper();
            var getWebRedirectGroupMethodInfo = typeof(WebRedirectHelper)
                .GetMethod(
                    "GetWebRedirectGroup",
                    BindingFlags.Instance | BindingFlags.Public,
                    Type.DefaultBinder,
                    new Type[] { typeof(int), typeof(IEnumerable<WebRedirectGroup>) },
                    null);

            var result = (WebRedirectGroup)getWebRedirectGroupMethodInfo.Invoke(webRedirectHelper, new object[] { 6, _webRedirectGroups });

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);
        }

        [TestCase]
        public void GetWebRedirectGroup_FactId_NotFound_Test()
        {
            var webRedirectHelper = new WebRedirectHelper();
            var getWebRedirectGroupMethodInfo = typeof(WebRedirectHelper)
                .GetMethod(
                    "GetWebRedirectGroup",
                    BindingFlags.Instance | BindingFlags.Public,
                    Type.DefaultBinder,
                    new Type[] { typeof(int), typeof(IEnumerable<WebRedirectGroup>) },
                    null);

            var result = (WebRedirectGroup)getWebRedirectGroupMethodInfo.Invoke(webRedirectHelper, new object[] { 365, _webRedirectGroups });

            Assert.IsNull(result);
        }

        [TestCase]
        public void GetWebRedirectGroup_VanityUrl_Found_Test()
        {
            var expected = _webRedirectGroups[0];

            var webRedirectHelper = new WebRedirectHelper();
            var getWebRedirectGroupMethodInfo = typeof(WebRedirectHelper)
                .GetMethod(
                    "GetWebRedirectGroup",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    Type.DefaultBinder,
                    new Type[] { typeof(Uri), typeof(IEnumerable<WebRedirectGroup>) },
                    null);

            var result = (WebRedirectGroup)getWebRedirectGroupMethodInfo.Invoke(webRedirectHelper, new object[] { new Uri("~/vanityurl", UriKind.RelativeOrAbsolute), _webRedirectGroups });

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);
        }

        [TestCase]
        public void GetWebRedirectGroup_VanityUrl_NotFound_Test()
        {
            var webRedirectHelper = new WebRedirectHelper();
            var getWebRedirectGroupMethodInfo = typeof(WebRedirectHelper)
                .GetMethod(
                    "GetWebRedirectGroup",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    Type.DefaultBinder,
                    new Type[] { typeof(Uri), typeof(IEnumerable<WebRedirectGroup>) },
                    null);

            var result = (WebRedirectGroup)getWebRedirectGroupMethodInfo.Invoke(webRedirectHelper, new object[] { new Uri("~/vanityurl2", UriKind.RelativeOrAbsolute), _webRedirectGroups });

            Assert.IsNull(result);
        }

        [TestCase]
        public void GetWebRedirectGroup_HomeFactId_Test()
        {
            var webRedirectHelper = new WebRedirectHelper();

            var result = webRedirectHelper.GetWebRedirectGroup(new Uri("~/", UriKind.RelativeOrAbsolute), 6, _webRedirectGroups);

            Assert.IsNotNull(result);
            Assert.AreEqual(expected: _webRedirectGroups[1], actual: result);
        }

        [TestCase]
        public void GetWebRedirectGroup_HomeFactId_OtherParameter_Test()
        {
            var webRedirectHelper = new WebRedirectHelper();

            var result = webRedirectHelper.GetWebRedirectGroup(new Uri("~/?test=42", UriKind.RelativeOrAbsolute), 6, _webRedirectGroups);

            Assert.IsNotNull(result);
            Assert.AreEqual(expected: _webRedirectGroups[1], actual: result);
        }

        [TestCase]
        public void GetWebRedirectGroup_NotHomeFactId_Test()
        {
            var webRedirectHelper = new WebRedirectHelper();

            var result = webRedirectHelper.GetWebRedirectGroup(new Uri("~/nothome", UriKind.RelativeOrAbsolute), 6, _webRedirectGroups);

            Assert.IsNull(result);
        }

        [TestCase(6)]
        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(null)]
        public void GetWebRedirectGroup_VanityUrl_Test(int? factId)
        {
            var webRedirectHelper = new WebRedirectHelper();

            var result = webRedirectHelper.GetWebRedirectGroup(new Uri("~/vanityurl", UriKind.RelativeOrAbsolute), factId, _webRedirectGroups);

            Assert.IsNotNull(result);
            Assert.AreEqual(expected: _webRedirectGroups[0], actual: result);
        }

        [TestCase]
        public void GetWebRedirectGroup_NullUri_Test()
        {
            var webRedirectHelper = new WebRedirectHelper();

            try
            {
                webRedirectHelper.GetWebRedirectGroup(null, 6, _webRedirectGroups);

                Assert.Fail();
            }
            catch (ArgumentNullException)
            {
                // Pass
            }
        }

        [TestCase]
        public void GetWebRedirectGroup_NullWebRedirectGroups_Test()
        {
            var webRedirectHelper = new WebRedirectHelper();

            try
            {
                webRedirectHelper.GetWebRedirectGroup(new Uri("~/vanityurl", UriKind.RelativeOrAbsolute), 6, null);

                Assert.Fail();
            }
            catch (ArgumentNullException)
            {
                // Pass
            }
        }

        [TestCase]
        public void GetWebRedirectUrl_Disabled_Test()
        {
            var dateTimeProviderMock = new Mock<WebRedirectHelper.IDateTimeProvider>();
            dateTimeProviderMock.Setup(dtp => dtp.UtcNow).Returns(new DateTime(2016, 5, 1));

            var helper = new WebRedirectHelper(dateTimeProviderMock.Object, null);

            var factId = 27;
            var webRedirectGroup = new WebRedirectGroup(
                109,
                3, 30,
                5, "Test Marketing Organization",
                8, "Test Mkt Plmnt Cat",
                16, "Test Mkt Plmnt",
                new DateTime(2016, 4, 5), null,
                "~/targetUrl",
                null,
                false
            );
            var marketingRedirectParameters = new MarketingRedirectParameters() { fact = factId.ToString() };

            var result = helper.GetWebRedirectUrl(webRedirectGroup, marketingRedirectParameters);

            Assert.IsNotNull(result);
            Assert.AreEqual("~/", result);
        }

        [TestCase]
        public void GetWebRedirectUrl_NotStarted_Test()
        {
            var dateTimeProviderMock = new Mock<WebRedirectHelper.IDateTimeProvider>();
            dateTimeProviderMock.Setup(dtp => dtp.UtcNow).Returns(new DateTime(2016, 3, 1));

            var helper = new WebRedirectHelper(dateTimeProviderMock.Object, null);

            var factId = 27;
            var webRedirectGroup = new WebRedirectGroup(
                109,
                3, 30,
                5, "Test Marketing Organization",
                8, "Test Mkt Plmnt Cat", 
                16, "Test Mkt Plmnt",
                new DateTime(2016, 4, 5), new DateTime(2016, 4, 6),
                "~/targetUrl",
                null,
                true
            );
            var marketingRedirectParameters = new MarketingRedirectParameters() { fact = factId.ToString() };

            var result = helper.GetWebRedirectUrl(webRedirectGroup, marketingRedirectParameters);

            Assert.IsNotNull(result);
            Assert.AreEqual("~/", result);
        }

        [TestCase]
        public void GetWebRedirectUrl_Expired_Test()
        {
            var dateTimeProviderMock = new Mock<WebRedirectHelper.IDateTimeProvider>();
            dateTimeProviderMock.Setup(dtp => dtp.UtcNow).Returns(new DateTime(2016, 5, 1));

            var helper = new WebRedirectHelper(dateTimeProviderMock.Object, null);

            var factId = 27;
            var webRedirectGroup = new WebRedirectGroup(
                109,
                3, 30,
                5, "Test Marketing Organization",
                8, "Test Mkt Plmnt Cat", 
                16, "Test Mkt Plmnt",
                new DateTime(2016, 4, 5), new DateTime(2016, 4, 6),
                "~/targetUrl",
                null,
                true
            );
            var marketingRedirectParameters = new MarketingRedirectParameters() { fact = factId.ToString() };

            var result = helper.GetWebRedirectUrl(webRedirectGroup, marketingRedirectParameters);

            Assert.IsNotNull(result);
            Assert.AreEqual("~/", result);
        }

        [TestCase]
        public void GetWebRedirectUrl_Enabled_Test()
        {
            var dateTimeProviderMock = new Mock<WebRedirectHelper.IDateTimeProvider>();
            dateTimeProviderMock.Setup(dtp => dtp.UtcNow).Returns(new DateTime(2016, 5, 1));

            var helper = new WebRedirectHelper(dateTimeProviderMock.Object, null);

            var factId = 27;
            var webRedirectGroup = new WebRedirectGroup(
                109,
                3, 30,
                5, "Test Marketing Organization",
                8, "Test Mkt Plmnt Cat", 
                16, "Test Mkt Plmnt",
                new DateTime(2016, 4, 5), null,
                "~/targetUrl",
                null,
                true
            );

            var marketingRedirectParameters = new MarketingRedirectParameters() { fact = factId.ToString() };

            var result = helper.GetWebRedirectUrl(webRedirectGroup, marketingRedirectParameters);

            Assert.IsNotNull(result);
            Assert.AreEqual("~/targetUrl?fact=27&isredirect=True", result);
        }

        [TestCase]
        public void GetWebRedirectUrl_NullMarketingRedirectParameters_Test()
        {
            var helper = new WebRedirectHelper();

            var webRedirectGroup = new WebRedirectGroup(
                109,
                3, 30,
                5, "Test Marketing Organization",
                8, "Test Mkt Plmnt Cat",
                16, "Test Mkt Plmnt",
                new DateTime(2016, 4, 5), null,
                "~/targetUrl",
                null,
                true
            );

            var result = helper.GetWebRedirectUrl(webRedirectGroup, null);

            Assert.IsNull(result);
        }

        [TestCase]
        public void GetWebRedirectUrl_NullWebRedirectGroup_Test()
        {
            var helper = new WebRedirectHelper();

            var factId = 27;

            var marketingRedirectParameters = new MarketingRedirectParameters() { fact = factId.ToString() };

            var result = helper.GetWebRedirectUrl(null, marketingRedirectParameters);

            Assert.IsNull(result);
        }

        [TestCase]
        public void GetWebRedirectUrl_BothNull_Test()
        {
            var helper = new WebRedirectHelper();

            var result = helper.GetWebRedirectUrl(null, null);

            Assert.IsNull(result);
        }
    }
}
