using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Web;
using FirstAgain.Common.Logging;
using FirstAgain.Common;
using FirstAgain.Common.Web;
using Newtonsoft.Json;
using FirstAgain.Common.Extensions;
using System.Net.Http;
using System.Net;
using System.Linq;

namespace FirstAgain.Web.Cookie
{
    public interface ICookieUtility
    {
        int? SplitTestCookie { get; }
        int? FirstAgainCodeTrackingId { get; }
        void ResetExistingFactCookie(int firstAgainCodeTrackingId);
        void SetMarketingReferrerInfoCookie(MarketingReferrerInfo mri);
        void SetSubIdCookie(string subid);
        void SetAidCookie(string aId);
        void SetBRLIdCookie(string id);
        void SetGCLIDCookie(string id);
        void SetEfidCookie(string efid);
        void SetFactAndFair(int fact, int? webRedirectId, string firstAgainIdReferrer);
    }

    public class CookieUtilityWrapper : ICookieUtility
    {
        public int? SplitTestCookie
        {
            get { return CookieUtility.SplitTestCookie; }
        }

        public int? FirstAgainCodeTrackingId
        {
            get { return CookieUtility.FirstAgainCodeTrackingId; }
        }
        
        public void ResetExistingFactCookie(int firstAgainCodeTrackingId)
        {
            CookieUtility.ResetExistingFactCookie(firstAgainCodeTrackingId);
        }

        public void SetMarketingReferrerInfoCookie(MarketingReferrerInfo mri)
        {
            CookieUtility.SetMarketingReferrerInfoCookie(mri);
        }

        public void SetSubIdCookie(string subid)
        {
            CookieUtility.SetSubIdCookie(subid);
        }

        public void SetAidCookie(string aId)
        {
            CookieUtility.SetAIDCookie(aId);
        }

        public void SetBRLIdCookie(string id)
        {
            CookieUtility.SetBRLIdCookie(id);
        }

        public void SetGCLIDCookie(string id)
        {
            CookieUtility.SetGCLIDCookie(id); 
        }

        public void SetEfidCookie(string efid)
        {
            CookieUtility.SetEFIDCookie(efid);
        }

        public void SetFactAndFair(int fact, int? webRedirectId, string firstAgainIdReferrer)
        {
            CookieUtility.SetFACTandFAIR(fact, webRedirectId, firstAgainIdReferrer);
        }
    }

    /// <summary>
    /// Summary description for CookieUtility
    /// </summary>
    public static class CookieUtility
    {
        #region constants

        const string UNIQUE_COOKIE_NAME = "UniqueCookie";

        //FirstAgainCodeTracking - Tells us where the user came from (google, yahoo, partner site, etc.)
        const string FACT_COOKIE_NAME = "FACT";

        //FirstAgainIdReferral - This is extra data sent to us by the partner site. 
        //For example, it could be the partner's unique ID for the customer.
        const string FAIR_COOKIE_NAME = "FAIR";

        //Used for determining if a user has cookies turned on
        const string TEST_SESSION_COOKIE_NAME = "SessionTest";

        //Random value from 1-100 used for marketing tests such as multiple landing pages for one fact code.
        const string SPLIT_TEST_COOKIE_NAME = "SplitTest";

        //This is the id of the page that displayed as a result of a fact code coming in on the URL.
        const string WEB_REDIRECT_ID_COOKIE_NAME = "WebRedirectId";

        //This is used by the home page type landing pages in order to keep it as the home page for the session.
        const string HOME_PAGE_COOKIE_NAME = "HomePage";

        //This is used to make sure our customer survey requests pop-up only once.
        const string SURVEY_COOKIE_NAME = "Survey";

        //date the current fact was set
        const string FACT_SET_DATE_COOKIE_NAME = "FACTSetDate";

        //fact history
        const string FACT_HISTORY_COOKIE_NAME = "FACTHistory";

        //first visit date
        const string FIRST_VISIT_DATE_COOKIE_NAME = "FirstVisitDate";

        public const int NEVER_EXPIRE_DAYS = 18000;

        // session apply cookie -- needed to tie back to applications when creating reports
        public const string SESSION_APPLY_COOKIE_NAME = "SessionApplyCookie";

        // tune cookie -- needed to tie back to applications when creating reports
        public const string TUNE_COOKIE_NAME = "Tune";
        public const string TUNE_TTID = "TTId";
        public const string TUNE_TAID = "TAId";
        public const string TUNE_TURL = "TUrl";
        public const string TUNE_Affiliate_Name = "AffiliateName";

        public const string TUNE_API_KEY_NAME = "Tune:ApiKey";
        public const string TUNE_API_URL = "Tune:ApiUrl";

        // teammate referral cookie to tie applications to teammate referral id
        public const string TEAMMATE_REFERRAL_COOKIE_NAME = "TeammateReferralToken";
        public const string TEAMMATE_EMAIL_ADDRESS= "TeammateEmailAddress";

        // cookies for MBOX application tracking
        public const string SESSION_APPLY_LOAN_AMOUNT = "SessionApplyLoanAmount";
        public const string SESSION_APPLY_PURPOSE_OF_LOAN = "SessionApplyPurposeOfLoan";
        public const string APPLCIATION_ALREADY_SUBMITTED = "SessionApplyReachedThankYouPage";

        // MarketingReferrerInfo cookie
        const string MARKETING_REFERRER_INFO_COOKIE_NAME = "MarketingReferrerInfo";

        // SubId cookie
        const string SUBID_COOKIE_NAME = "SubID";

        // AID cookie
        const string AID_COOKIE_NAME = "AID";

        // Bankrate Link Id cookie
        const string BRLId_COOKIE_NAME = "BRLId";

        // Google DS Id cookie 
        const string GCLID_COOKIE_NAME = "GSLID";

        // EFID cookie
        const string EFID_COOKIE_NAME = "EFID";

        // TntId cookie
        const string TNTID_COOKIE_NAME = "TntId";

        // Offer cookie
        const string OFFER_LOAN_AMOUNT = "OfferLoanAmount";
        const string OFFER_LOAN_TERM = "OfferLoanTerm";

        public const string TRANS_UNION_SESSION_ID = "TransUnionSessionId";

    

        #endregion

        public static string SetUniqueCookie(bool forceNew)
        {
            if (forceNew || HttpContext.Current.Request.Cookies[UNIQUE_COOKIE_NAME] == null)
            {
                TimeSpan ts = new TimeSpan();
                ts = TimeSpan.FromDays(NEVER_EXPIRE_DAYS);
                string guid = Guid.NewGuid().ToString();
                SetCookie(UNIQUE_COOKIE_NAME, guid, ts);
                FirstVisitDate = DateTime.Now;
                return guid;
            }
            else
            {
                return (string)HttpContext.Current.Request.Cookies[UNIQUE_COOKIE_NAME].Value;
            }
        }

        public static string ReadUniqueCookie()
        {
            return GetCookieValue(UNIQUE_COOKIE_NAME);
        }
        public static string UniqueCookie
        {
            get
            {
                return SetUniqueCookie(false);
            }
        }

        public static string SessionApplyCookie
        {
            get
            {
                return (string)HttpContext.Current.Request.Cookies[SESSION_APPLY_COOKIE_NAME].Value;
            }
        }

        public static string TuneCookie
        {
            get
            {
                return (string)HttpContext.Current.Request.Cookies[TUNE_COOKIE_NAME].Value;
            }
        }

        public static string TeammateReferralCookie
        {
            get
            {
                return (string)HttpContext.Current.Request.Cookies[SESSION_APPLY_COOKIE_NAME].Value;
            }
        }

        public static void SetTeammateReferralCookie(int referralId)
        {
            if (TeammateReferralCookieId == null)
            {
                SetCookie(TEAMMATE_REFERRAL_COOKIE_NAME, WebSecurityUtility.Scramble(referralId), TimeSpan.FromDays(30), false);
            }
        }

        public static string TeammateEmailAddress
        {
            get
            {
                var cookie = GetCookieValue(TEAMMATE_EMAIL_ADDRESS);
                return cookie == null? null: WebSecurityUtility.DecryptString(cookie);
            }
            set
            {
                if (value == null)
                {
                    ExpireCookie(TEAMMATE_EMAIL_ADDRESS);
                }
                else
                {
                    SetCookie(TEAMMATE_EMAIL_ADDRESS, WebSecurityUtility.EncryptString(value));
                }
            }
        }

        public static int? TeammateReferralCookieId
        {
            get
            {
                var scrambledReferralId = GetCookieValue(TEAMMATE_REFERRAL_COOKIE_NAME);

                if (string.IsNullOrEmpty(scrambledReferralId))
                {
                    return null;
                }

                return WebSecurityUtility.Descramble(scrambledReferralId);
            }
        }

        public static string SessionApplyLoanAmount
        {
            get
            {
                return GetCookieValue(SESSION_APPLY_LOAN_AMOUNT);
            }
            set
            {
                SetCookie(SESSION_APPLY_LOAN_AMOUNT, value);
            }
        }

        public static string SessionApplyPurposeOfLoan
        {
            get
            {
                return GetCookieValue(SESSION_APPLY_PURPOSE_OF_LOAN);
            }
            set
            {
                SetCookie(SESSION_APPLY_PURPOSE_OF_LOAN, value);
            }
        }

        public static void ExpireApplicationCookies()
        {
            ExpireCookie(SESSION_APPLY_COOKIE_NAME);
            ExpireCookie(FACT_COOKIE_NAME);
            ExpireCookie(FAIR_COOKIE_NAME);
            ExpireCookie(SESSION_APPLY_LOAN_AMOUNT);
            ExpireCookie(SESSION_APPLY_PURPOSE_OF_LOAN);
            ExpireCookie(TUNE_COOKIE_NAME);
        }

        public static void SetApplicationSubmittedCookie()
        {
            SetCookie(APPLCIATION_ALREADY_SUBMITTED, "true", TimeSpan.FromSeconds(5));
        }

        public static bool IsApplicationSubmittedCookieSet()
        {
            return GetCookieValue(APPLCIATION_ALREADY_SUBMITTED) == "true";
        }

        public static void ClearApplicationSubmittedCookie()
        {
            ExpireCookie(APPLCIATION_ALREADY_SUBMITTED);
        }

        public static void SetFACTandFAIR(int fact, int? webRedirectId, string firstAgainIdReferrer)
        {
            DateTime now = DateTime.Now;
            TimeSpan ts = new TimeSpan();
            ts = TimeSpan.FromDays(30);
            SetCookie(FACT_COOKIE_NAME, fact.ToString(), ts, false);
            SetCookie(FACT_SET_DATE_COOKIE_NAME, now.ToString(), ts);

            SetFactHistory(fact, firstAgainIdReferrer, now);

            if (webRedirectId != null)
            {
                SetCookie(WEB_REDIRECT_ID_COOKIE_NAME, webRedirectId.ToString(), ts);
            }
            else
            {
                // if it is null then we want to delete anything that might already be there
                if (HttpContext.Current.Request.Cookies[WEB_REDIRECT_ID_COOKIE_NAME] != null)
                {
                    ExpireCookie(WEB_REDIRECT_ID_COOKIE_NAME);
                }
            }

            if (!String.IsNullOrEmpty(firstAgainIdReferrer))
            {
                if (firstAgainIdReferrer.Length > 1000)
                {
                    firstAgainIdReferrer = firstAgainIdReferrer.Substring(0, 1000);
                }

                SetCookie(FAIR_COOKIE_NAME, firstAgainIdReferrer, ts);
            }
            else
            {
                // if it is null then we want to delete anything that might already be there
                if (HttpContext.Current.Request.Cookies[FAIR_COOKIE_NAME] != null)
                {
                    ExpireCookie(FAIR_COOKIE_NAME);
                }
            }
        }

        private static void SetFactHistory(int fact, string firstAgainIdReferrer, DateTime now)
        {
            try
            {
                FactHistory factHistory = new FactHistory(FactHistoryCookie);
                factHistory.Add(new FactHistoryItem(now, fact, firstAgainIdReferrer));
                FactHistoryCookie = factHistory.ToString();
            }
            catch (Exception e)
            {
                Dictionary<string, string> info = new Dictionary<string, string>();
                info.Add("Support Note:", "Non-fatal error trying to retrieve/set FACT History in CookieUtility.SetFACTandFAIR. Processing continued and user did not see error, but the the cause of this error must be resolved.");
                info.Add("FactHistoryCookie", FactHistoryCookie);
                info.Add("firstAgainIdReferrer", firstAgainIdReferrer);
                LightStreamLogger.WriteError(e.ToString(), info);
            }
        }

        public static void ResetExistingFactCookie(int firstAgainCodeTrackingId)
        {
            // If have a different fact code than one already stored in the cookie, then
            // expire other cookies...
            int? existingFACT = FirstAgainCodeTrackingId;
            if (existingFACT.HasValue && existingFACT.Value != firstAgainCodeTrackingId)
            {
                // Set cookies to null to expire them
                SetMarketingReferrerInfoCookie(null);
                SetSubIdCookie(null);
            }
        }

        /// <summary>
        /// Set the MarketingReferrerInfo cookie. Pass null to expire the cookie immediately
        /// </summary>
        public static void SetMarketingReferrerInfoCookie(MarketingReferrerInfo mri)
        {
            if (mri != null)
            {
                TimeSpan cookieExpiration = TimeSpan.FromDays(30);
                SetCookie(MARKETING_REFERRER_INFO_COOKIE_NAME, mri.SerializeToString(), cookieExpiration);
            }
            else
            {
                ExpireCookie(MARKETING_REFERRER_INFO_COOKIE_NAME);
            }
        }

        public static MarketingReferrerInfo MarketingReferrerInfo
        {
            get
            {
                string s = GetCookieValue(MARKETING_REFERRER_INFO_COOKIE_NAME);
                return MarketingReferrerInfo.FromSerializedString(s);
            }
        }

        public static void SetSubIdCookie(string subid)
        {
            if (!String.IsNullOrEmpty(subid))
            {
                TimeSpan cookieExpiration = TimeSpan.FromDays(30);
                SetCookie(SUBID_COOKIE_NAME, subid, cookieExpiration);
            }
            else
            {
                ExpireCookie(SUBID_COOKIE_NAME);
            }
        }

        public static string SubId
        {
            get
            {
                return GetCookieValue(SUBID_COOKIE_NAME);
            }
        }

        public static void SetTntIdCookie(string tntId)
        {
            if (!String.IsNullOrEmpty(tntId))
            {
                SetCookie(TNTID_COOKIE_NAME, tntId, TimeSpan.FromDays(30));
            }
            else
            {
                ExpireCookie(TNTID_COOKIE_NAME);
            }
        }

        public static string TntId
        {
            get
            {
                return GetCookieValue(TNTID_COOKIE_NAME);
            }
        }

        public static void SetAIDCookie(string aId)
        {
            if (!String.IsNullOrEmpty(aId))
            {
                TimeSpan cookieExpiration = TimeSpan.FromDays(30);
                SetCookie(AID_COOKIE_NAME, aId, cookieExpiration);
            }
            else
            {
                ExpireCookie(AID_COOKIE_NAME);
            }
        }

        public static string AID
        {
            get
            {
                return GetCookieValue(AID_COOKIE_NAME);
            }
        }

        public static void SetBRLIdCookie(string BRLId)
        {
            if (!String.IsNullOrEmpty(BRLId))
            {
                TimeSpan cookieExpiration = TimeSpan.FromDays(30);
                SetCookie(BRLId_COOKIE_NAME, BRLId, cookieExpiration);
            }
            else
            {
                ExpireCookie(BRLId_COOKIE_NAME);
            }
        }

        public static string BRLId
        {
            get
            {
                return GetCookieValue(BRLId_COOKIE_NAME);
            }
        }

        public static void SetGCLIDCookie(string dsclickid)
        {
            if (!String.IsNullOrEmpty(dsclickid))
            {
                TimeSpan cookieExpiration = TimeSpan.FromDays(30);
                SetCookie(GCLID_COOKIE_NAME, dsclickid, cookieExpiration);
            }
            else
            {
                ExpireCookie(GCLID_COOKIE_NAME);
            }
        }

        public static string GSLID
        {
            get
            {
                return GetCookieValue(GCLID_COOKIE_NAME);
            }
        }

        public static void SetEFIDCookie(string efid)
        {
            if (!String.IsNullOrEmpty(efid))
            {
                TimeSpan cookieExpiration = TimeSpan.FromDays(30);
                SetCookie(EFID_COOKIE_NAME, efid, cookieExpiration);
            }
            else
            {
                ExpireCookie(EFID_COOKIE_NAME);
            }
        }

        public static string EFID
        {
            get
            {
                return GetCookieValue(EFID_COOKIE_NAME);
            }
        }        

        public static int? FirstAgainCodeTrackingId
        {
            get
            {
                int id;
                return int.TryParse(GetCookieValue(FACT_COOKIE_NAME), out id) ? (int?)id : null;
            }
        }

        public static string FirstAgainIdReferral
        {
            get
            {
                return GetCookieValue(FAIR_COOKIE_NAME);
            }
        }

        public static void SetTestSessionCookie()
        {
            SetCookie(TEST_SESSION_COOKIE_NAME, "test", null);
        }

        public static string TestSessionCookie
        {
            get
            {
                HttpCookie cookie = HttpContext.Current.Request.Cookies[TEST_SESSION_COOKIE_NAME];
                return cookie == null ? null : cookie.Value;
            }
        }

        public static int? SetSplitTestCookie(bool forceNew)
        {
            if (forceNew || SplitTestCookie == null)
            {
                TimeSpan ts = new TimeSpan();
                ts = TimeSpan.FromDays(NEVER_EXPIRE_DAYS);

                int splitTestValue = new CryptoRandom().Next(1, 101);

                SetCookie(SPLIT_TEST_COOKIE_NAME, splitTestValue.ToString(), ts);

                return splitTestValue;
            }
            else
            {
                return SplitTestCookie;
            }
        }

        public static int? SplitTestCookie
        {
            get
            {
                int splitValue;
                HttpCookie cookie = HttpContext.Current.Request.Cookies[SPLIT_TEST_COOKIE_NAME];
                return cookie != null && int.TryParse(cookie.Value, out splitValue) ? (int?)splitValue : null;
            }
        }

        public static int? WebRedirectId
        {
            get
            {
                int webRedirectId;
                HttpCookie cookie = HttpContext.Current.Request.Cookies[WEB_REDIRECT_ID_COOKIE_NAME];
                return cookie != null && int.TryParse(cookie.Value, out webRedirectId) ? (int?)webRedirectId : null;
            }
            set
            {
                CookieUtility.SetCookie(WEB_REDIRECT_ID_COOKIE_NAME, value.ToString());
            }
        }

        /// <summary>
        /// NOTE: This is temporary code that will be refactored when the full Landing Pages
        /// project goes in. The purpose is to change the home page for a session.
        /// </summary>
        public static string HomePageCookie
        {
            get
            {
                return GetCookieValue(HOME_PAGE_COOKIE_NAME);
            }

            set
            {
                SetCookie(HOME_PAGE_COOKIE_NAME, value, null);
            }
        }

        public static string SurveyCookie
        {
            get
            {
                return GetCookieValue(SURVEY_COOKIE_NAME);
            }

            set
            {
                TimeSpan ts = new TimeSpan();
                ts = TimeSpan.FromDays(30);
                SetCookie(SURVEY_COOKIE_NAME, value, ts);
            }
        }

        public static string FactHistoryCookie
        {
            get
            {
                string cookieValue = GetCookieValue(FACT_HISTORY_COOKIE_NAME);

                if (cookieValue == null)
                {
                    cookieValue = string.Empty;
                }

                return cookieValue;
            }

            set
            {
                TimeSpan ts = new TimeSpan();
                ts = TimeSpan.FromDays(NEVER_EXPIRE_DAYS);
                SetCookie(FACT_HISTORY_COOKIE_NAME, value, ts);
            }
        }

        public static DateTime FirstVisitDate
        {
            get
            {
                string firstDateString = GetCookieValue(FIRST_VISIT_DATE_COOKIE_NAME);
                DateTime firstDate;

                if (DateTime.TryParse(firstDateString, out firstDate))
                {
                    return (firstDate);
                }
                else
                {
                    return DateTime.MinValue;
                }
            }

            set
            {
                TimeSpan ts = new TimeSpan();
                ts = TimeSpan.FromDays(NEVER_EXPIRE_DAYS);
                SetCookie(FIRST_VISIT_DATE_COOKIE_NAME, value.ToString(), ts);
            }
        }

        public static DateTime FactSetDate
        {
            get
            {
                string factSetDateString = GetCookieValue(FACT_SET_DATE_COOKIE_NAME);
                DateTime factSetDate;

                if (DateTime.TryParse(factSetDateString, out factSetDate))
                {
                    return (factSetDate);
                }
                else
                {
                    return DateTime.MinValue;
                }
            }
        }

        public static decimal? OfferLoanAmount
        {
            get
            {
                string sLoanAmount = GetCookieValue(OFFER_LOAN_AMOUNT);
                decimal loanAmount;
                if (Decimal.TryParse(sLoanAmount, out loanAmount))
                {
                    return loanAmount;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value == null)
                {
                    ExpireCookie(OFFER_LOAN_AMOUNT);
                }
                else
                {
                    var ts = new TimeSpan();
                    ts = TimeSpan.FromDays(30);
                    SetCookie(OFFER_LOAN_AMOUNT, value.ToString(), ts);
                }
            }
        }
        public static int? OfferLoanTerm
        {
            get
            {
                string sLoanTerm = GetCookieValue(OFFER_LOAN_TERM);
                int loanTerm;
                if (Int32.TryParse(sLoanTerm, out loanTerm))
                {
                    return loanTerm;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value == null)
                {
                    ExpireCookie(OFFER_LOAN_TERM);
                }
                else
                {
                    var ts = new TimeSpan();
                    ts = TimeSpan.FromDays(30);
                    SetCookie(OFFER_LOAN_TERM, value.ToString(), ts);
                }
            }
        }

        #region helper methods

        /// <summary>
        /// Sets a cookie. If lifetime is null, the cookie will be a session (browser) cookie
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="lifeTime"></param>
        public static void SetCookie(string name, string value, TimeSpan? lifeTime = null, bool httpOnly = true)
        {
            HttpCookie cookie = new HttpCookie(name);
            cookie.Value = HttpUtility.UrlEncode(value);
            cookie.HttpOnly = httpOnly;

            if (lifeTime != null)
            {
                cookie.Expires = DateTime.Now.Add((TimeSpan)lifeTime);
            }

            string domain = GetDomain();

            //use a domain cookie for the root domain (lightstream.com or www.lightstream.com)
            if (domain != null)
            {
                cookie.Domain = domain;
            }

            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        /// <summary>
        /// Gets a cookies value
        /// </summary>
        /// <param name="cookieName"></param>
        /// <returns></returns>
        public static string GetCookieValue(string cookieName)
        {
            HttpCookie cookie = null;
            //if it was just created then it will exists in the response
            //check to make sure it doesn't exist in the response as an expire
            if (Array.IndexOf(HttpContext.Current.Response.Cookies.AllKeys, cookieName) >= 0)
            {
                cookie = HttpContext.Current.Response.Cookies[cookieName];
            }
            else
            {
                cookie = HttpContext.Current.Request.Cookies[cookieName];
            }
            return cookie == null ? null : HttpUtility.UrlDecode(cookie.Value);
        }

        public static T GetCookieAs<T>(string cookieName)
        {
            try
            {
                var value = GetCookieValue(cookieName);
                return JsonConvert.DeserializeObject<T>(value);
            }
            catch
            {
                return default(T);
            }
        }

        public static void SetCookie(string cookieName, object value, TimeSpan? lifeTime = null)
        {
            var cookie = JsonConvert.SerializeObject(value);
            SetCookie(cookieName, cookie, lifeTime);
        }

        /// <summary>
        /// Expires Cookie
        /// </summary>
        /// <param name="cookieName"></param>
        private static void ExpireCookie(string cookieName)
        {
            HttpCookie cookie = HttpContext.Current.Response.Cookies[cookieName];

            if (cookie == null)
            {
                return;
            }

            string domain = GetDomain();

            //use a domain cookie for the root domain (lightstream.com or www.lightstream.com)
            if (domain != null)
            {
                cookie.Domain = domain;
            }

            cookie.Expires = DateTime.Now.AddDays(-30);
        }

        private static string GetDomain()
        {
            string rootDomain = ConfigurationManager.AppSettings["RootDomain"];

            //use a domain cookie for the root domain (lightstream.com or www.lightstream.com)
            if ((HttpContext.Current.Request.Url.Host.Equals(rootDomain, StringComparison.OrdinalIgnoreCase))
                 || (HttpContext.Current.Request.Url.Host.Equals("www." + rootDomain, StringComparison.OrdinalIgnoreCase))
               )
            {
                return rootDomain;
            }
            else
            {
                return null;
            }
        }

        private static dynamic HttpQuerySringParams
        {
            get
            {
                return HttpContext.Current.Request?.QueryString?.AllKeys?.Select(i => new
                {
                    Param = i,
                    Value = HttpContext.Current.Request.QueryString[i]
                });
            }
        }

        #endregion

        internal static void Delete(string p)
        {
            HttpContext.Current.Response.Cookies.Add(new HttpCookie(p) { Expires = DateTime.Now.AddDays(-30) });
        }
    }

    public class FactHistory
    {
        #region Constants

        const int MAXITEMS = 10;
        const char DELIM1 = '|';
        const char DELIM2 = ',';

        #endregion

        #region Private Members

        private char[] _delim1 = { DELIM1 };
        private char[] _delim2 = { DELIM2 };

        private List<FactHistoryItem> _factHistory;

        #endregion

        #region Public Properties
        /// <summary>
        /// return read-only since the proper way is to go through this.Add
        /// </summary>
        public IList<FactHistoryItem> Items
        {
            get
            {
                return _factHistory.AsReadOnly();
            }
        }

        #endregion

        #region Constructors

        public FactHistory()
        {
            Init();
        }

        public FactHistory(string factHistoryString)
        {
            Init();
            Populate(factHistoryString);
        }

        #endregion

        #region Public Methods

        public void Add(FactHistoryItem item)
        {

            bool isDuplicate = false;
            if ((_factHistory.Count > 0) && (item.Fact == _factHistory[_factHistory.Count - 1].Fact))
            {
                isDuplicate = true;
            }

            if (!isDuplicate) //Don't add duplicates of previous fact
            {
                _factHistory.Add(item);
            }

            //restrict to a max of MAXITEMS
            if (_factHistory.Count == MAXITEMS + 1)
            {
                _factHistory.RemoveAt(0);
            }
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < _factHistory.Count; i++)
            {
                if (sb.Length > 0)
                {
                    sb.Append(_delim1[0]);
                }
                //using compressed date format to save cookie space.
                sb.AppendFormat("{0}{1}{2}{3}{4}", _factHistory[i].DateSet.ToString("MMddyyyyHHmmss"), _delim2[0], _factHistory[i].Fact, _delim2[0], _factHistory[i].Fair);
            }

            return sb.ToString();
        }

        #endregion

        #region Private Methods

        private void Init()
        {
            _factHistory = new List<FactHistoryItem>();
        }

        private void Populate(string factHistoryString)
        {
            string[] factHistoryItems = factHistoryString.Split(_delim1, StringSplitOptions.RemoveEmptyEntries);

            foreach (string item in factHistoryItems)
            {
                string fair = string.Empty;

                string[] split = item.Split(_delim2);

                string dateString = string.Format("{0}/{1}/{2} {3}:{4}:{5}",
                                        split[0].Substring(0, 2),
                                        split[0].Substring(2, 2),
                                        split[0].Substring(4, 4),
                                        split[0].Substring(8, 2),
                                        split[0].Substring(10, 2),
                                        split[0].Substring(12, 2));

                DateTime dateSet = DateTime.Parse(dateString);

                int fact = int.Parse(split[1]);

                if (split.Length > 2)
                {
                    fair = split[2];
                }

                this.Add(new FactHistoryItem(dateSet, fact, fair));
            }
        }

        #endregion
    }

    public class FactHistoryItem
    {
        #region Private Members

        private DateTime _dateSet;
        private int _fact;
        private string _fair;

        #endregion

        #region Public Properties

        public DateTime DateSet
        {
            get
            {
                return _dateSet;
            }
            set
            {
                _dateSet = value;
            }
        }

        public int Fact
        {
            get
            {
                return _fact;
            }
            set
            {
                _fact = value;
            }
        }

        public string Fair
        {
            get
            {
                return _fair;
            }
            set
            {
                _fair = value;
            }
        }

        #endregion

        #region Constructors

        public FactHistoryItem(DateTime dateSet, int fact, string fair)
        {
            _dateSet = dateSet;
            _fact = fact;

            if (fair == null)
            {
                fair = string.Empty;
            }
            fair = fair.Replace(',', '^');
            fair = fair.Replace('|', '\\');
            _fair = fair;
        }

        #endregion
    }
}
