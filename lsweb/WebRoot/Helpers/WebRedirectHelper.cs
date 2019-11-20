using FirstAgain.Common.Extensions;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.SharedTypes.ContentManagement;
using FirstAgain.Web.Cookie;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Routing;

namespace LightStreamWeb.Helpers
{
    public class WebRedirectHelper : /* This is a temporary inheritance to pull in CheckForImpactRadius.  TODO: Remove */ MarketingRedirectHelper
    {

        #region Constants
        
        private const string IS_REDIRECT_PARAMETER = "isredirect";

        #endregion


        #region Inner Types

        // For Unit Testing
        public interface IDateTimeProvider
        {
            DateTime UtcNow { get; }
        }

        // We can't unit test this.
        [ExcludeFromCodeCoverage]
        public class SystemDateTimeProvider : IDateTimeProvider
        {
            public DateTime UtcNow { get { return DateTime.UtcNow; } }
        }

        #endregion


        #region Variables

        private readonly ICookieUtility _cookieUtility;
        private readonly IDateTimeProvider _dateTimeProvider;

        #endregion


        #region Constructors

        public WebRedirectHelper()
            : this(new SystemDateTimeProvider(), new CookieUtilityWrapper()) { }

        public WebRedirectHelper(IDateTimeProvider dateTimeProvider, ICookieUtility cookieUtility)
            : base(cookieUtility) // TODO: Remove when we no longer need CheckForImpactRadius.
        {
            _cookieUtility = cookieUtility;
            _dateTimeProvider = dateTimeProvider;
        }

        #endregion


        #region Methods

        public WebRedirectGroup GetWebRedirectGroup(Uri requestUri, int? factId, IEnumerable<WebRedirectGroup> webRedirectGroups)
        {
            if (requestUri == null)
                throw new ArgumentNullException(nameof(requestUri));

            if (webRedirectGroups == null)
                throw new ArgumentNullException(nameof(webRedirectGroups));

            var homePageUri = new Uri("~/", UriKind.RelativeOrAbsolute);

            // Get the web redirect object which represents the redirect.
            if (requestUri.EqualsPath(homePageUri) && factId.HasValue && factId.Value > 0)
                return GetWebRedirectGroup(factId.Value, webRedirectGroups);
            else
                return GetWebRedirectGroup(requestUri, webRedirectGroups);
        }

        public WebRedirectGroup GetWebRedirectGroup(int factId, IEnumerable<WebRedirectGroup> webRedirectGroups)
        {
            return (from g in webRedirectGroups
                    where g.FirstAgainCodeTrackingIdRange.Contains(factId)
                    select g).FirstOrDefault();
        }

        private WebRedirectGroup GetWebRedirectGroup(Uri requestUri, IEnumerable<WebRedirectGroup> webRedirectGroups)
        {
            return (from g in webRedirectGroups
                    where g.GetVanityUri().EqualsPath(requestUri)
                    select g).FirstOrDefault();
        }

        public string GetWebRedirectUrl(WebRedirectGroup webRedirectGroup, MarketingRedirectParameters marketingRedirectParameters)
        {
            int factId = -1;
            if (webRedirectGroup == null ||
                marketingRedirectParameters == null ||
                !int.TryParse(marketingRedirectParameters.fact, out factId))
                return null;

            // If the redirect hasn't started yet, has expired, or is disabled, just redirect to the home page.
            if (!webRedirectGroup.IsEnabled || !webRedirectGroup.ValidDateRange.Contains(_dateTimeProvider.UtcNow.Date))
                return "~/";

            Dictionary<string, object> queryParameters = new Dictionary<string, object>(marketingRedirectParameters.OtherValues,StringComparer.CurrentCultureIgnoreCase);
            
            var propertyList = from p in typeof(MarketingRedirectParameters).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty)
                               where p.Name != "OtherValues"
                               select p;

            // Add all properties on the marketing redirect parameters if they have a value.
            foreach (var property in propertyList)
            {
                var value = property.GetValue(marketingRedirectParameters);
                if (value != null && !queryParameters.ContainsKey(property.Name))
                    queryParameters.Add(property.Name, value);
            }

            queryParameters.Add(IS_REDIRECT_PARAMETER, true);

            // Append the fact id to the URL so that it can be tracked.
            return webRedirectGroup.GetTargetUri().AddQueryParameters(queryParameters).ToString();
        }

        public void SetCookies(WebRedirectGroup webRedirectGroup, MarketingRedirectParameters marketingRedirectParameters)
        {
            if (webRedirectGroup == null)
                throw new ArgumentNullException(nameof(webRedirectGroup));

            if (marketingRedirectParameters == null)
                throw new ArgumentNullException(nameof(marketingRedirectParameters));

            // Intentionally throw if fail to parse fact id.
            int factId = int.Parse(marketingRedirectParameters.fact);

            string supplementalData = null;
            if (!string.IsNullOrWhiteSpace(marketingRedirectParameters.fair))
                supplementalData = marketingRedirectParameters.fair;

            // If we have made it this far, factId has a value.
            _cookieUtility.ResetExistingFactCookie(factId);

            // Check for new marketing referrer info
            // CheckForImpactRadius is being pulled from MarketingRedirectHelper to avoid duplication here.
            // Eventually these two classes should be merged, but it would increase the risk too much at this point.
            MarketingReferrerInfo mktingRefInfo = CheckForImpactRadius(factId, marketingRedirectParameters.ClickId, marketingRedirectParameters.irmp, marketingRedirectParameters.irpid);

            if (mktingRefInfo != null)
            {
                supplementalData = mktingRefInfo.GetDataValue(MarketingDataEntityLookup.MarketingDataEntity.IRMPID.ToString());
                _cookieUtility.SetMarketingReferrerInfoCookie(mktingRefInfo);
            }

            if (!string.IsNullOrWhiteSpace(marketingRedirectParameters.subId))
                _cookieUtility.SetSubIdCookie(marketingRedirectParameters.subId);

            if (!string.IsNullOrWhiteSpace(marketingRedirectParameters.AID))
                _cookieUtility.SetAidCookie(marketingRedirectParameters.AID);

            if (!string.IsNullOrWhiteSpace(marketingRedirectParameters.BRLId))
                _cookieUtility.SetBRLIdCookie(marketingRedirectParameters.BRLId);

            if (!string.IsNullOrWhiteSpace(marketingRedirectParameters.GSLID))
                _cookieUtility.SetGCLIDCookie(marketingRedirectParameters.GSLID);

            if (!string.IsNullOrWhiteSpace(marketingRedirectParameters.ef_id))
                _cookieUtility.SetEfidCookie(marketingRedirectParameters.ef_id);

            // TODO: Modifiy the cookie utility to accept a group id.
            _cookieUtility.SetFactAndFair(factId, webRedirectGroup.WebRedirectGroupId, supplementalData);
        }        
        #endregion
    }
}