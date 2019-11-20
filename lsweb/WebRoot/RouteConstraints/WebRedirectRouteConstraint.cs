using FirstAgain.Common.Extensions;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.SharedTypes.ContentManagement;
using LightStreamWeb.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Routing;

namespace LightStreamWeb.RouteConstraints
{
    public class WebRedirectRouteConstraint : IRouteConstraint
    {

        #region Constants

        private const string FACT_QUERY_PARAMETER = "fact";
        private const string IS_REDIRECT_PARAMETER = "isredirect";

        #endregion


        #region Inner Types

        // For Unit testing.
        public interface IVirtualPathUtilityProvider
        {
            string ToAppRelative(string virtualPath);
        }

        // We can't unit test this.
        [ExcludeFromCodeCoverage]
        public class AspNetVirtualPathUtilityProvider : IVirtualPathUtilityProvider
        {
            public string ToAppRelative(string virtualPath)
            {
                return VirtualPathUtility.ToAppRelative(virtualPath);
            }
        }

        #endregion


        #region Variables

        private IVirtualPathUtilityProvider _virtualPathUtilityProvider;

        #endregion


        #region Constructors

        public WebRedirectRouteConstraint()
            : this(new AspNetVirtualPathUtilityProvider())
        {
            // Intentionally empty.
        }

        public WebRedirectRouteConstraint(IVirtualPathUtilityProvider virtualPathProvider)
        {
            _virtualPathUtilityProvider = virtualPathProvider;
        }

        #endregion


        #region Methods

        // We can't unit test this.
        [ExcludeFromCodeCoverage]
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            IEnumerable<WebRedirectGroup> webRedirectGroups = DomainServiceContentManagementOperations.GetCachedWebRedirectGroups();

            return Match(httpContext, route, parameterName, values, routeDirection, webRedirectGroups);
        }

        // This overload is used solely so that we can provide a web redirect group list for unit testing.
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection, IEnumerable<WebRedirectGroup> webRedirectGroups)
        {
            //
            // WARNING: We want to do the minimal amount of work here.  This will execute EVERY hit to the site.
            //

            if (httpContext.Request.QueryString.AllKeys.Count()  > 0 &&
                httpContext.Request.QueryString.AllKeys[0] == null)
                return false;

            var requestUri = (new Uri(_virtualPathUtilityProvider.ToAppRelative(httpContext.Request.Url.AbsolutePath), UriKind.RelativeOrAbsolute))
                                .AddQueryParameters(httpContext.Request.QueryString);
            WebRedirectHelper helper = new WebRedirectHelper();

            int? factId = null;
            int factIdOut = -1;
            if (int.TryParse(httpContext.Request.QueryString[FACT_QUERY_PARAMETER], out factIdOut))
                factId = factIdOut;
            WebRedirectGroup webRedirectGroup = helper.GetWebRedirectGroup(requestUri, factId, webRedirectGroups);

            // Assuming we found a valid redirect, let's move on.
            if (webRedirectGroup != null)
            {
                factId = !factId.HasValue ? webRedirectGroup.FirstAgainCodeTrackingIdRange.Start : factId;

                values.Add(nameof(WebRedirectGroup), webRedirectGroup);
                values.Add(nameof(MarketingRedirectParameters), DeserializeMarketingRedirectParameters(httpContext.Request.QueryString, factId.GetValueOrDefault()));

                bool isRedirectOut = false;
                if (bool.TryParse(httpContext.Request.QueryString[IS_REDIRECT_PARAMETER], out isRedirectOut) &&
                    isRedirectOut)
                    return false;

                return true;
            }

            // We didn't match this route.
            return false;
        }

        private MarketingRedirectParameters DeserializeMarketingRedirectParameters(NameValueCollection queryString, int? factId)
        {
            var marketingRedirectParameters = new MarketingRedirectParameters();

            var propertyList = typeof(MarketingRedirectParameters).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
            var filteredPropertyList = (from p in propertyList
                                        where p.Name != "OtherValues"
                                        select p).ToArray();

            // Loop over all of the properties and add their values to the marketingRedirectParameters object.
            foreach (var prop in filteredPropertyList)
            {
                prop.SetValue(marketingRedirectParameters, queryString[prop.Name]);
            }

            if (factId.HasValue && factId.Value > 0)
                marketingRedirectParameters.fact = factId.Value.ToString();

            foreach (var key in queryString.AllKeys.Except(filteredPropertyList.Select(p => p.Name).ToArray()))
            {
                marketingRedirectParameters.OtherValues.Add(key ?? string.Empty, queryString[key]);
            }

            return marketingRedirectParameters;
        }

        #endregion

    }
}