using FirstAgain.Domain.SharedTypes.ContentManagement;
using FluentAssertions;
using LightStreamWeb.Helpers;
using LightStreamWeb.RouteConstraints;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Web;
using System.Web.Routing;

namespace LightStreamWeb.UnitTests.RouteConstraints
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    public class WebRedirectRouteConstraintTests
    {
        WebRedirectGroup[] _webRedirectGroups = new WebRedirectGroup[]
        {
            new WebRedirectGroup(31, 11, null, 4, "Test 2",  8, "Test Mkt Plmnt Cat", 16, "Test Mkt Plmnt",new DateTime(2016, 1, 2), null, "~/targeturl", "~/vanityurl", false),
            new WebRedirectGroup(25, 5, 10, 4, "Test 2",  8, "Test Mkt Plmnt Cat", 16, "Test Mkt Plmnt",new DateTime(2016, 1, 2), null, "~/targeturl", null, false)
        };

        [TestCase]
        public void Match_FactId_Found_Test()
        {
            var expected = _webRedirectGroups[1];

            var virtualPathUtilityMock = new Mock<WebRedirectRouteConstraint.IVirtualPathUtilityProvider>();
            virtualPathUtilityMock.Setup(vp => vp.ToAppRelative("/")).Returns("~/");

            var parameterCollection = new NameValueCollection();
            parameterCollection.Add("fact", "6");

            var httpRequestMock = GetHttpRequestMock("http://localhost/");
            httpRequestMock.Setup(r => r.QueryString).Returns(parameterCollection);

            var httpContextMock = GetHttpContextMock(httpRequestMock.Object);

            var routeValueDictionary = new RouteValueDictionary();

            var routeConstraint = new WebRedirectRouteConstraint(virtualPathUtilityMock.Object);
            var result = routeConstraint.Match(httpContextMock.Object, null, null, routeValueDictionary, RouteDirection.IncomingRequest, _webRedirectGroups);

            Assert.IsTrue(result);
            Assert.AreEqual(2, routeValueDictionary.Count);
            Assert.AreEqual(expected, routeValueDictionary[nameof(WebRedirectGroup)]);
            Assert.AreEqual(6, int.Parse(((MarketingRedirectParameters)routeValueDictionary[nameof(MarketingRedirectParameters)]).fact));
        }

        [TestCase]
        public void Match_FactId_NotFound_Test()
        {
            var virtualPathUtilityMock = new Mock<WebRedirectRouteConstraint.IVirtualPathUtilityProvider>();
            virtualPathUtilityMock.Setup(vp => vp.ToAppRelative("/")).Returns("~/");

            var parameterCollection = new NameValueCollection();
            parameterCollection.Add("fact", "352");

            var httpRequestMock = GetHttpRequestMock("http://localhost/");
            httpRequestMock.Setup(r => r.QueryString).Returns(parameterCollection);

            var httpContextMock = GetHttpContextMock(httpRequestMock.Object);

            var routeValueDictionary = new RouteValueDictionary();

            var routeConstraint = new WebRedirectRouteConstraint(virtualPathUtilityMock.Object);
            var result = routeConstraint.Match(httpContextMock.Object, null, null, routeValueDictionary, RouteDirection.IncomingRequest, _webRedirectGroups);

            Assert.IsFalse(result);
            Assert.AreEqual(0, routeValueDictionary.Count);
            Assert.IsNull(routeValueDictionary[nameof(WebRedirectGroup)]);
            Assert.IsNull(routeValueDictionary[nameof(MarketingRedirectParameters)]);
        }

        [TestCase]
        public void Match_VanityUrl_Found_Test()
        {
            var expected = _webRedirectGroups[0];

            var virtualPathUtilityMock = new Mock<WebRedirectRouteConstraint.IVirtualPathUtilityProvider>();
            virtualPathUtilityMock.Setup(vp => vp.ToAppRelative("/vanityurl")).Returns("~/vanityurl");

            var routeValueDictionary = new RouteValueDictionary();

            var httpContext = GetHttpContext("http://localhost/vanityurl");

            var routeConstraint = new WebRedirectRouteConstraint(virtualPathUtilityMock.Object);
            var result = routeConstraint.Match(httpContext, null, null, routeValueDictionary, RouteDirection.IncomingRequest, _webRedirectGroups);

            Assert.IsTrue(result);
            Assert.AreEqual(2, routeValueDictionary.Count);
            Assert.AreEqual(expected, routeValueDictionary[nameof(WebRedirectGroup)]);
            Assert.AreEqual(11, int.Parse(((MarketingRedirectParameters)routeValueDictionary[nameof(MarketingRedirectParameters)]).fact));
        }

        [TestCase]
        public void Match_VanityUrl_NotFound_Test()
        {
            var virtualPathUtilityMock = new Mock<WebRedirectRouteConstraint.IVirtualPathUtilityProvider>();
            virtualPathUtilityMock.Setup(vp => vp.ToAppRelative("/vanityurl2")).Returns("~/vanityurl2");

            var routeValueDictionary = new RouteValueDictionary();

            var httpContext = GetHttpContext("http://localhost/vanityurl2");

            var routeConstraint = new WebRedirectRouteConstraint(virtualPathUtilityMock.Object);
            var result = routeConstraint.Match(httpContext, null, null, routeValueDictionary, RouteDirection.IncomingRequest, _webRedirectGroups);

            Assert.IsFalse(result);
            Assert.AreEqual(0, routeValueDictionary.Count);
            Assert.IsNull(routeValueDictionary[nameof(WebRedirectGroup)]);
            Assert.IsNull(routeValueDictionary[nameof(MarketingRedirectParameters)]);
        }

        [TestCase]
        public void DeserializeMarketingRedirectParameters_AllValues_Test()
        {
            var expectedParameters = new MarketingRedirectParameters();
            expectedParameters.AID = "abc123";
            expectedParameters.ClickId = "click456";
            expectedParameters.ef_id = "hijk";
            expectedParameters.fact = "4636";
            expectedParameters.fair = "852";
            expectedParameters.irmp = "qwerty";
            expectedParameters.irpid = "yuiop";
            expectedParameters.subId = "789";

            var queryString = new NameValueCollection();
            queryString["AID"] = expectedParameters.AID;
            queryString["ClickId"] = expectedParameters.ClickId;
            queryString["ef_id"] = expectedParameters.ef_id;
            queryString["fact"] = expectedParameters.fact;
            queryString["fair"] = expectedParameters.fair;
            queryString["irmp"] = expectedParameters.irmp;
            queryString["irpid"] = expectedParameters.irpid;
            queryString["subId"] = expectedParameters.subId;

            var deserializeMarketingRedirectParametersMethod = typeof(WebRedirectRouteConstraint).GetMethod("DeserializeMarketingRedirectParameters", BindingFlags.NonPublic | BindingFlags.Instance);

            var constraint = new WebRedirectRouteConstraint();

            var result = (MarketingRedirectParameters)deserializeMarketingRedirectParametersMethod.Invoke(constraint, new object[] { queryString, 4636 });

            result.Should().BeEquivalentTo(expectedParameters);
        }

        [TestCase]
        public void DeserializeMarketingRedirectParameters_SomeNull_Test()
        {
            var expectedParameters = new MarketingRedirectParameters();
            expectedParameters.AID = "abc123";
            expectedParameters.ClickId = null;
            expectedParameters.ef_id = "hijk";
            expectedParameters.fact = "4636";
            expectedParameters.fair = null;
            expectedParameters.irmp = "qwerty";
            expectedParameters.irpid = "yuiop";
            expectedParameters.subId = null;

            var queryString = new NameValueCollection();
            queryString["AID"] = expectedParameters.AID;
            queryString["ClickId"] = expectedParameters.ClickId;
            queryString["ef_id"] = expectedParameters.ef_id;
            queryString["fact"] = expectedParameters.fact;
            queryString["fair"] = expectedParameters.fair;
            queryString["irmp"] = expectedParameters.irmp;
            queryString["irpid"] = expectedParameters.irpid;
            queryString["subId"] = expectedParameters.subId;

            var deserializeMarketingRedirectParametersMethod = typeof(WebRedirectRouteConstraint).GetMethod("DeserializeMarketingRedirectParameters", BindingFlags.NonPublic | BindingFlags.Instance);

            var constraint = new WebRedirectRouteConstraint();

            var result = (MarketingRedirectParameters)deserializeMarketingRedirectParametersMethod.Invoke(constraint, new object[] { queryString, 4636 });

            result.Should().BeEquivalentTo(expectedParameters);
        }

        [TestCase]
        public void DeserializeMarketingRedirectParameters_NullFact_Test()
        {
            var expectedParameters = new MarketingRedirectParameters();
            expectedParameters.AID = "abc123";
            expectedParameters.ClickId = "click456";
            expectedParameters.ef_id = "hijk";
            expectedParameters.fact = "4636";
            expectedParameters.fair = "852";
            expectedParameters.irmp = "qwerty";
            expectedParameters.irpid = "yuiop";
            expectedParameters.subId = "789";

            var queryString = new NameValueCollection();
            queryString["AID"] = expectedParameters.AID;
            queryString["ClickId"] = expectedParameters.ClickId;
            queryString["ef_id"] = expectedParameters.ef_id;
            queryString["fact"] = null;
            queryString["fair"] = expectedParameters.fair;
            queryString["irmp"] = expectedParameters.irmp;
            queryString["irpid"] = expectedParameters.irpid;
            queryString["subId"] = expectedParameters.subId;

            var deserializeMarketingRedirectParametersMethod = typeof(WebRedirectRouteConstraint).GetMethod("DeserializeMarketingRedirectParameters", BindingFlags.NonPublic | BindingFlags.Instance);

            var constraint = new WebRedirectRouteConstraint();

            var result = (MarketingRedirectParameters)deserializeMarketingRedirectParametersMethod.Invoke(constraint, new object[] { queryString, 4636 });

            result.Should().BeEquivalentTo(expectedParameters);
        }

        [TestCase]
        public void DeserializeMarketingRedirectParameters_ZeroFactId_Test()
        {
            var expectedParameters = new MarketingRedirectParameters();
            expectedParameters.AID = "abc123";
            expectedParameters.ClickId = "click456";
            expectedParameters.ef_id = "hijk";
            expectedParameters.fact = "4636";
            expectedParameters.fair = "852";
            expectedParameters.irmp = "qwerty";
            expectedParameters.irpid = "yuiop";
            expectedParameters.subId = "789";

            var queryString = new NameValueCollection();
            queryString["AID"] = expectedParameters.AID;
            queryString["ClickId"] = expectedParameters.ClickId;
            queryString["ef_id"] = expectedParameters.ef_id;
            queryString["fact"] = expectedParameters.fact;
            queryString["fair"] = expectedParameters.fair;
            queryString["irmp"] = expectedParameters.irmp;
            queryString["irpid"] = expectedParameters.irpid;
            queryString["subId"] = expectedParameters.subId;

            var deserializeMarketingRedirectParametersMethod = typeof(WebRedirectRouteConstraint).GetMethod("DeserializeMarketingRedirectParameters", BindingFlags.NonPublic | BindingFlags.Instance);

            var constraint = new WebRedirectRouteConstraint();

            var result = (MarketingRedirectParameters)deserializeMarketingRedirectParametersMethod.Invoke(constraint, new object[] { queryString, 0 });

            result.Should().BeEquivalentTo(expectedParameters);
        }

        [TestCase]
        public void DeserializeMarketingRedirectParameters_NegativeOneFactId_Test()
        {
            var expectedParameters = new MarketingRedirectParameters();
            expectedParameters.AID = "abc123";
            expectedParameters.ClickId = "click456";
            expectedParameters.ef_id = "hijk";
            expectedParameters.fact = "4636";
            expectedParameters.fair = "852";
            expectedParameters.irmp = "qwerty";
            expectedParameters.irpid = "yuiop";
            expectedParameters.subId = "789";

            var queryString = new NameValueCollection();
            queryString["AID"] = expectedParameters.AID;
            queryString["ClickId"] = expectedParameters.ClickId;
            queryString["ef_id"] = expectedParameters.ef_id;
            queryString["fact"] = expectedParameters.fact;
            queryString["fair"] = expectedParameters.fair;
            queryString["irmp"] = expectedParameters.irmp;
            queryString["irpid"] = expectedParameters.irpid;
            queryString["subId"] = expectedParameters.subId;

            var deserializeMarketingRedirectParametersMethod = typeof(WebRedirectRouteConstraint).GetMethod("DeserializeMarketingRedirectParameters", BindingFlags.NonPublic | BindingFlags.Instance);

            var constraint = new WebRedirectRouteConstraint();

            var result = (MarketingRedirectParameters)deserializeMarketingRedirectParametersMethod.Invoke(constraint, new object[] { queryString, -1 });

            result.Should().BeEquivalentTo(expectedParameters);
        }

        [TestCase]
        public void DeserializeMarketingRedirectParameters_NullFactId_Test()
        {
            var expectedParameters = new MarketingRedirectParameters();
            expectedParameters.AID = "abc123";
            expectedParameters.ClickId = "click456";
            expectedParameters.ef_id = "hijk";
            expectedParameters.fact = "4636";
            expectedParameters.fair = "852";
            expectedParameters.irmp = "qwerty";
            expectedParameters.irpid = "yuiop";
            expectedParameters.subId = "789";

            var queryString = new NameValueCollection();
            queryString["AID"] = expectedParameters.AID;
            queryString["ClickId"] = expectedParameters.ClickId;
            queryString["ef_id"] = expectedParameters.ef_id;
            queryString["fact"] = expectedParameters.fact;
            queryString["fair"] = expectedParameters.fair;
            queryString["irmp"] = expectedParameters.irmp;
            queryString["irpid"] = expectedParameters.irpid;
            queryString["subId"] = expectedParameters.subId;

            var deserializeMarketingRedirectParametersMethod = typeof(WebRedirectRouteConstraint).GetMethod("DeserializeMarketingRedirectParameters", BindingFlags.NonPublic | BindingFlags.Instance);

            var constraint = new WebRedirectRouteConstraint();

            var result = (MarketingRedirectParameters)deserializeMarketingRedirectParametersMethod.Invoke(constraint, new object[] { queryString, null });

            result.Should().BeEquivalentTo(expectedParameters);
        }

        private Mock<HttpRequestBase> GetHttpRequestMock(string url)
        {
            var parameters = new NameValueCollection();

            var mock = new Mock<HttpRequestBase>();
            mock.Setup(r => r.Url).Returns(new Uri(url));
            mock.Setup(r => r.QueryString).Returns(parameters);

            return mock;
        }

        private HttpRequestBase GetHttpRequest(string url)
        {
            return GetHttpRequestMock(url).Object;
        }

        private Mock<HttpContextBase> GetHttpContextMock(HttpRequestBase request)
        {
            var mock = new Mock<HttpContextBase>();
            mock.Setup(h => h.Request).Returns(request);

            return mock;
        }

        private HttpContextBase GetHttpContext(string url)
        {
            return GetHttpContextMock(GetHttpRequest(url)).Object;
        }
    }
}
