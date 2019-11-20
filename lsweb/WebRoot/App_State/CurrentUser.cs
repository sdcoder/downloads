using FirstAgain.Common.Web;
using FirstAgain.Domain.ServiceModel.Contracts.MessageContracts.CustomerOperations;
using FirstAgain.Web.Cookie;
using LightStreamWeb.ServerState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.App_State
{
    public class CurrentUser : ICurrentUser
    {
        public void SignOut()
        {
            SessionUtility.CleanUpSession();
            CookieUtility.Delete("ApplicationId");
        }

        public void SetFACT(int? fact)
        {
            if (fact.HasValue)
            {
                CookieUtility.SetFACTandFAIR(fact.Value, null, string.Empty);
            }
        }

        public void SetMarketingReferrerInfoCookie(MarketingReferrerInfo mri)
        {
            CookieUtility.SetMarketingReferrerInfoCookie(mri);
        }

        public string FirstAgainIdReferral
        {
            get
            {
                return CookieUtility.FirstAgainIdReferral;
            }
        }

        public string UniqueCookie
        {
            get
            {
                return CookieUtility.UniqueCookie;
            }
        }

        // gets the cookie, but does not set it
        public string ReadUniqueCookie()
        {
            return CookieUtility.ReadUniqueCookie();
        }


        public int? TeammateReferralId
        {
            get
            {
                return CookieUtility.TeammateReferralCookieId;
            }
        }

        /// <summary>
        /// ApplicationId is stored in session, and in a cookie. Cookie is partly for backup, but mostly for tracking in the IIS logs
        /// </summary>
        public int? ApplicationId
        {
            get
            {
                int? appId = null;

                // always use ctx if available
                try
                {
                    if (HttpContext.Current.Request.Params["ctx"] != null)
                    {
                        appId = WebSecurityUtility.Descramble(HttpContext.Current.Request.Params["ctx"].ToString());
                        if (appId.HasValue)
                        {
                            return appId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    FirstAgain.Common.Logging.LightStreamLogger.WriteWarning(ex);
                }

                if (!appId.HasValue)
                {
                    appId = SessionUtility.ActiveApplicationId;
                }
                if (appId.GetValueOrDefault(0) == 0)
                {
                    var ctx = CookieUtility.GetCookieValue("ApplicationId");
                    if (!string.IsNullOrEmpty(ctx))
                    {
                        appId = WebSecurityUtility.Descramble(ctx);
                    }
                }
                return appId;
            }
            set
            {
                if (value == null)
                {
                    SessionUtility.ActiveApplicationId = null;
                    CookieUtility.Delete("ApplicationId");
                }
                else
                {
                    SessionUtility.ActiveApplicationId = value;
                    CookieUtility.SetCookie("ApplicationId", WebSecurityUtility.Scramble(value.Value));
                }
            }
        }

        public int? FirstAgainCodeTrackingId
        {
            get
            {
                return CookieUtility.FirstAgainCodeTrackingId;
            }
        }

        public int? CMSWebContentId
        {
            get
            {
                return SessionUtility.CMSWebContentId;
            }
            set
            {
                SessionUtility.CMSWebContentId = value;
            }
        }

        public int? CMSRevision 
        {
            get
            {
                return SessionUtility.CMSRevision;
            }
            set
            {
                SessionUtility.CMSRevision = value;
            }
        }

        private const string MOCK_IP_COOKIE_NAME = "MockIpAddress";
        public string IPAddress
        {
            get
            {
                if (HttpContext.Current.Request.Cookies[MOCK_IP_COOKIE_NAME] != null)
                {
                    string cookie = HttpContext.Current.Request.Cookies[MOCK_IP_COOKIE_NAME].Value;
                    if (System.Text.RegularExpressions.Regex.Match(cookie, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b").Success)
                    {
                        return cookie;
                    }
                }
                if (HttpContext.Current.Request.Headers["X-Forwarded-For"] != null)
                {
                    if (HttpContext.Current.Request.Headers["X-Forwarded-For"].Contains(","))
                    {
                        return HttpContext.Current.Request.Headers["X-Forwarded-For"].Split(',').First();
                    }
                    return HttpContext.Current.Request.Headers["X-Forwarded-For"];
                }
                return HttpContext.Current.Request.UserHostAddress;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool MockIPAddress(string ipAddress)
        {
            if (System.Text.RegularExpressions.Regex.Match(ipAddress, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b").Success)
            {
                CookieUtility.SetCookie(MOCK_IP_COOKIE_NAME, ipAddress.Trim());
                return true;
            }

            return false;
        }

        public string UserAgent
        {
            get
            {
                string userAgent = HttpContext.Current.Request.Headers["User-Agent"] ?? "";
                if (userAgent.Length > 255)
                {
                    userAgent = userAgent.Substring(0, 254);
                }
                return userAgent;  
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string AcceptLanguage
        {
            get
            {
                var acceptLangauge = HttpContext.Current.Request.Headers["Accept-Language"] ?? "";
                if (acceptLangauge.Length > 255)
                {
                    acceptLangauge = acceptLangauge.Substring(0, 254);
                }
                return acceptLangauge;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string SessionApplyCookie
        {
            get
            {
                return CookieUtility.SessionApplyCookie;
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        public bool IsAccountServices
        {
            get
            {
                return SessionUtility.IsAccountServices;
            }
            set
            {
                SessionUtility.IsAccountServices = value;
            }
        }

        public bool AddCoApplicant
        {
            get
            {
                return SessionUtility.AddCoApplicant;
            }
            set
            {
                SessionUtility.AddCoApplicant = value;
            }
        }

        public bool IsGenericPostingPartner
        {
            get
            {
                return SessionUtility.IsGenericPostingPartner;
            }
            set
            {
                SessionUtility.IsGenericPostingPartner = value;
            }
        }


        public string SearchTermsCookie
        {
            get
            {
                return CookieUtility.GetCookieValue("SearchTerms");
            }
            // set via javascript, nothing to do here. Setter only exists for unit testing
            set {} 
        }

        public string SearchEngineCookie
        {
            get
            {
                return CookieUtility.GetCookieValue("SearchEngine");
            }
            // set via javascript, nothing to do here. Setter only exists for unit testing
            set { }
        }


        public int? SplitTestCookie
        {
            get
            {
                return CookieUtility.SplitTestCookie;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int? WebRedirectId
        {
            get
            {
                return CookieUtility.WebRedirectId;
            }
            set
            {
                CookieUtility.WebRedirectId = value;
            }
        }

        public DateTime FactSetDate
        {
            get
            {
                return CookieUtility.FactSetDate;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public DateTime FirstVisitDate
        {
            get
            {
                return CookieUtility.FirstVisitDate;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public MarketingReferrerInfo MarketingReferrerInfo
        {
            get
            {
                return CookieUtility.MarketingReferrerInfo;
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        public string SubId
        {
            get
            {
                return CookieUtility.SubId;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string BRLId
        {
            get
            {
                return CookieUtility.BRLId;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string GSLID
        {
            get
            {
                return CookieUtility.GSLID; 
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string TntId
        {
            get
            {
                return CookieUtility.TntId;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string AID
        {
            get
            {
                return CookieUtility.AID;
            }
            set
            {
                CookieUtility.SetAIDCookie(value);
            }
        }

        public string EFID
        {
            get
            {
                return CookieUtility.EFID;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string TTID
        {
            get
            {
                return GetTuneValuesFromTuneCookie(CookieUtility.TUNE_TTID);
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string TAID
        {
            get
            {
                return GetTuneValuesFromTuneCookie(CookieUtility.TUNE_TAID);
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string TURL
        {
            get
            {
                return GetTuneValuesFromTuneCookie(CookieUtility.TUNE_TURL);
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string TuneAffiliateName
        {
            get
            {
                return GetTuneValuesFromTuneCookie(CookieUtility.TUNE_Affiliate_Name);
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        private static string GetTuneValuesFromTuneCookie(string name)
        {
            var tuneCookie = CookieUtility.GetCookieValue(CookieUtility.TUNE_COOKIE_NAME);

            if(tuneCookie == null)
            {
                return null;
            }

            dynamic items = Newtonsoft.Json.JsonConvert.DeserializeObject(tuneCookie);
            foreach (var item in items)
            {
                if (item.Name == name)
                {
                    return item.Value;
                }
            }
            return null;
        }

        public string TransUnionSessionId
        {
            get
            {
                return CookieUtility.GetCookieValue(CookieUtility.TRANS_UNION_SESSION_ID);
            }
            // set via javascript, nothing to do here. 
            set { }
        }


        public void ResetSignatures()
        {
            SessionUtility.ResetLoanAgreementSignature();
        }

        public byte[] PrimarySignatureImageBytes
        { 
            get 
            {
                return SessionUtility.PrimarySignatureImageBytes;
            }
            set 
            {
                SessionUtility.PrimarySignatureImageBytes = value;
            }
        }
        public byte[] SecondarySignatureImageBytes 
        {
            get
            {
                return SessionUtility.SecondarySignatureImageBytes;
            }
            set
            {
                SessionUtility.SecondarySignatureImageBytes = value;
            }
        }

        public string PrimarySignatureTimestamp
        {
            get
            {
                return SessionUtility.PrimarySignatureTstamp;
            }
            set
            {
                SessionUtility.PrimarySignatureTstamp = value;
            }
        }
        public string SecondarySignatureTimestamp
        {
            get
            {
                return SessionUtility.SecondarySignatureTstamp;
            }
            set
            {
                SessionUtility.SecondarySignatureTstamp = value;
            }
        }

        public bool PrimarySignatureOnFile
        {
            get
            {
                return SessionUtility.FoundApplicantSignatureOnFile;
            }
            set
            {
                SessionUtility.FoundApplicantSignatureOnFile = value;
            }
        }

        public bool SecondarySignatureOnFile
        {
            get
            {
                return SessionUtility.FoundCoApplicantSignatureOnFile;
            }
            set
            {
                SessionUtility.FoundCoApplicantSignatureOnFile = value;
            }
        }

        public bool FoundApplicantSignatureOnFile
        {
            get
            {
                return SessionUtility.FoundApplicantSignatureOnFile;
            }
            set
            {
                SessionUtility.FoundApplicantSignatureOnFile = value;
            }
        }
        public bool FoundCoApplicantSignatureOnFile
        {
            get
            {
                return SessionUtility.FoundCoApplicantSignatureOnFile;
            }
            set
            {
                SessionUtility.FoundCoApplicantSignatureOnFile = value;
            }
        }

        public bool FoundActiveLoanAgreementOnFile
        {
            get
            {
                return SessionUtility.FoundActiveLoanAgreementOnFile;
            }
            set
            {
                SessionUtility.FoundActiveLoanAgreementOnFile = value;
            }
        }

        public decimal? LoanAmount
        {
            get { return CookieUtility.OfferLoanAmount; }
            set { CookieUtility.OfferLoanAmount = value; }
        }
        public int? LoanTerm
        {
            get { return CookieUtility.OfferLoanTerm; }
            set { CookieUtility.OfferLoanTerm = value; }
        }

        public int? SelectedLoanTerm
        {
            get { return CookieUtility.OfferLoanTerm; }
            set { CookieUtility.OfferLoanTerm = value; }
        }

    }
}