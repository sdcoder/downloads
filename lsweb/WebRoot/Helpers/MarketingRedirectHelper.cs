using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Common.Logging;
using FirstAgain.Web.Cookie;
using System;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace LightStreamWeb.Helpers
{
    [Obsolete("Use WebRedirectHelper for all new redirect work.")]
    public class MarketingRedirectHelper
    {
        private readonly ICookieUtility _cookieUtility;
        public MarketingRedirectHelper()
            : this(new CookieUtilityWrapper()) { }

        public MarketingRedirectHelper(ICookieUtility cookieUtility)
        {
            _cookieUtility = cookieUtility;
        }

        public bool CheckForMarketingRedirect(MarketingRedirectParameters parameters, string urlQuery, out string redirectUrl)
        {
            // Start addition to disable this code path.
            string enableOldWebRedirectsString = WebConfigurationManager.AppSettings["EnableOldWebRedirects"];
            bool enableOldWebRedirects = false;

            if (Boolean.TryParse(enableOldWebRedirectsString, out enableOldWebRedirects) && !enableOldWebRedirects)
            {
                redirectUrl = null;
                return false;
            }
            // End addition to disable this code path.

            int firstAgainCodeTrackingId = -1;
            string supplementalData = null;
            int splitValue = _cookieUtility.SplitTestCookie ?? 1;

            bool isRedirectOut = false;

            if (parameters != null &&
                (!bool.TryParse(parameters.isredirect, out isRedirectOut) || !isRedirectOut) && // when isredirect=true is not part of the query string.
                int.TryParse(parameters.fact, out firstAgainCodeTrackingId))
            {
                int? webRedirectId;
                bool validFact = DomainServiceLoanApplicationOperations.GetCachedMarketingData().GetWebRedirectUrl(firstAgainCodeTrackingId, splitValue, out redirectUrl, out webRedirectId);

                if (validFact)
                {
                    if (!string.IsNullOrEmpty(parameters.fair))
                    {
                        supplementalData = parameters.fair;
                    }

                    _cookieUtility.ResetExistingFactCookie(firstAgainCodeTrackingId);

                    // Check for new marketing referrer info
                    MarketingReferrerInfo mktingRefInfo = CheckForImpactRadius(firstAgainCodeTrackingId, parameters.ClickId, parameters.irmp, parameters.irpid);

                    if (mktingRefInfo != null)
                    {
                        supplementalData = mktingRefInfo.GetDataValue(MarketingDataEntityLookup.MarketingDataEntity.IRMPID.ToString());
                        _cookieUtility.SetMarketingReferrerInfoCookie(mktingRefInfo);
                    }

                    if (!string.IsNullOrWhiteSpace(parameters.subId))
                    {
                        _cookieUtility.SetSubIdCookie(parameters.subId);
                    }

                    if (!string.IsNullOrWhiteSpace(parameters.AID))
                    {
                        _cookieUtility.SetAidCookie(parameters.AID);
                    }

                    if (!string.IsNullOrWhiteSpace(parameters.BRLId))
                    {
                        _cookieUtility.SetBRLIdCookie(parameters.BRLId);
                    }

                    if (!string.IsNullOrWhiteSpace(parameters.GSLID))
                    {
                        _cookieUtility.SetGCLIDCookie(parameters.GSLID); 
                    }

                    if (!string.IsNullOrWhiteSpace(parameters.ef_id))
                    {
                        _cookieUtility.SetEfidCookie(parameters.ef_id);
                    }

                    if (webRedirectId == null)
                    {
                        webRedirectId = 1;
                        LightStreamLogger.WriteError(String.Format("Could not get WebRedirect for FACT: \"{0}\", with split value: \"{1}\". Setting webRedirectId to 1 and showing home page instead. Check the FirstLook.MarketingWebRedirectTable to make sure the proper values are there.", firstAgainCodeTrackingId.ToString(), splitValue.ToString()));
                    }

                    _cookieUtility.SetFactAndFair(firstAgainCodeTrackingId, webRedirectId, supplementalData);

                    if ((redirectUrl != null) && (redirectUrl != "/Default.aspx"))
                    {
                        // Redirect to the new page, but replicate the fact code in the query string so the campaign id will get logged properly.
                        string newURL = VirtualPathUtility.ToAbsolute("~" + redirectUrl);
                        string tmpQuery = urlQuery;
                        if (newURL.Contains("?"))
                        {
                            if (tmpQuery.StartsWith("?"))
                            {
                                tmpQuery = "&" + tmpQuery.Substring(1);
                            }
                        }
                        newURL += tmpQuery;
                        redirectUrl = newURL;
                        return true;
                    }
                }
            }
            else
            {
                var fact = -1;
                if (int.TryParse(parameters?.fact, out fact))
                    _cookieUtility.SetFactAndFair(fact, null, null);
            }
            redirectUrl = string.Empty;
            return false;
        }

        protected MarketingReferrerInfo CheckForImpactRadius(int firstAgainCodeTrackingId, string clickId, string irMarketingPartnerId, string irPublisherId)
        {
            MarketingReferrerInfo mktRefInfo = null;
            FirstAgain.Domain.SharedTypes.LoanApplication.MarketingDataSet mktds = DomainServiceLoanApplicationOperations.GetCachedMarketingData();
            bool isImpactRadius = mktds.FirstAgainCodeTrackDetail.Any(r => r.FirstAgainCodeTrackingId == firstAgainCodeTrackingId && r.MarketingOrganizationId == 126 /*Impact Radius*/);

            if (isImpactRadius)
            {
                mktRefInfo = new MarketingReferrerInfo() { ReferrerName = "ImpactRadius", FACT = firstAgainCodeTrackingId };

                // Is the Impact Radius link an indirect or direct link?            
                mktRefInfo.SetDataNameValue(MarketingDataEntityLookup.MarketingDataEntity.IRClickId.ToString(), String.IsNullOrEmpty(clickId) ? "" : clickId);
                if (String.IsNullOrEmpty(clickId) == false)
                {
                    // Indirect link data
                    mktRefInfo.SetDataNameValue("LinkType", "IndirectLink");
                    mktRefInfo.SetDataNameValue(MarketingDataEntityLookup.MarketingDataEntity.IRCampaignId.ToString(), "1463");
                    mktRefInfo.SetDataNameValue(MarketingDataEntityLookup.MarketingDataEntity.IRActionTrackerId.ToString(), "3573");
                }
                else
                {
                    // Direct link data
                    mktRefInfo.SetDataNameValue("LinkType", "DirectLink");
                    mktRefInfo.SetDataNameValue(MarketingDataEntityLookup.MarketingDataEntity.IRCampaignId.ToString(), "1695");
                    mktRefInfo.SetDataNameValue(MarketingDataEntityLookup.MarketingDataEntity.IRActionTrackerId.ToString(), "3981");
                }

                // IRMPID
                irMarketingPartnerId = GetValidatedId(firstAgainCodeTrackingId, irMarketingPartnerId, "marketing partner id");
                mktRefInfo.SetDataNameValue(MarketingDataEntityLookup.MarketingDataEntity.IRMPID.ToString(), String.IsNullOrEmpty(irMarketingPartnerId) ? "" : irMarketingPartnerId);

                // IRPID
                irPublisherId = GetValidatedId(firstAgainCodeTrackingId, irPublisherId, "publisher id");
                mktRefInfo.SetDataNameValue(MarketingDataEntityLookup.MarketingDataEntity.IRPID.ToString(), String.IsNullOrEmpty(irPublisherId) ? "" : irPublisherId);
            }

            return mktRefInfo;
        }

        private string GetValidatedId(int firstAgainCodeTrackingId, string id, string idName)
        {
            int parsedId;
            if (!string.IsNullOrEmpty(id) && !int.TryParse(id, out parsedId))
            {
                string original = id;
                id = new string(id.TakeWhile(char.IsDigit).ToArray());
                LightStreamLogger.WriteWarning(
                    string.Format(
                        "Improperly formatted impact radius {0} found: {0} = '{1}', FACT = {2}. Stripping invalid characters to result in: '{3}'",
                        idName, original, firstAgainCodeTrackingId, id));
            }

            return id;
        }
    }
}