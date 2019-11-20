using System;
using System.Web;
using System.Web.SessionState;
using FirstAgain.Domain.ServiceModel.Contracts.MessageContracts.CustomerOperations;
using ServicingClient = FirstAgain.LoanServicing.ServiceModel.Client;
using ServicingTypes = FirstAgain.LoanServicing.SharedTypes;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.LoanServicing.SharedTypes;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Correspondence.SharedTypes;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using FirstAgain.Common.Data;
using FirstAgain.Domain.Common;
using LightStreamWeb.App_State;
using LightStreamWeb.Models.Apply;
using Soss.Client;
using FirstAgain.Common.Logging;
using FirstAgain.Common;

namespace LightStreamWeb.ServerState
{  
    /// <summary>
    /// Utility for caching account information in the HttpSessionState.
    /// </summary>
    public static class SessionUtility
    {
        private const string ACCOUNT_INFO_SESSION_KEY = "AccountInfo";
        private const string ACCOUNT_USER_ID = "AccountUserId";
        private const string ADD_CO_APPLICANT = "AddCoApplicant";
        private const string APPLICATION_SESSION_KEY = "ApplicationInfo";
        private const string ACCEPTED_PREVIOUS_TERMS = "AcceptedPreviousTerms";
        private const string CUSTOMER_IDENTIFICATION_APPLICANT = "CustomerIdentificationApplicant";
        
        //LoanOffer    
        private const string LOAN_OFFER_KEY = "LoanOfferKey";
        private const string LOAN_AGREEMENT_KEY = "LoanAgreementKey";
        private const string SIGANTURE_TEXT_KEY = "SignatureTextKey";
        private const string PRIMARY_SIGNATURE_TSTAMP_KEY = "PrimarySignatureTstampKey";
        private const string SECONDARY_SIGNATURE_TSTAMP_KEY = "SecondarySignatureTstampKey";
        private const string PRIMARY_SIGNATURE_IMAGE_BYTES_KEY = "PrimarySignatureImageBytesKey";
        private const string SECONDARY_SIGNATURE_IMAGE_BYTES_KEY = "SecondarySignatureImageBytesKey";
        private const string FOUND_ACTIVE_LOAN_AGREEMENT_ON_FILE_KEY = "FoundActiveLoanAgreementOnFileKey";
        public const string FOUND_PARTIALLY_SIGNED_LOAN_AGREEMENT_ON_FILE_KEY = "FoundPartiallySignedLoanAgreementOnFileKey";
        private const string FOUND_APPLICANT_SIGNATURE_ON_FILE_KEY = "FoundApplicantSignatureOnFileKey";
        private const string FOUND_COAPPLICANT_SIGNATURE_ON_FILE_KEY = "FoundCoApplicantSignatureOnFileKey";

        //AccountServices
        private const string ACCOUNT_SERVICES_DATA_KEY = "AccountServicesDataKey";
        private const string BUSINESS_CALENDAR_KEY = "BusinessCalendarKey";
        private const string DOCUMENT_STORE_DATA_KEY = "DocumentStoreKey";
        private const string IS_ACCOUNT_SERVICES_KEY = "IsAccountServicesKey";
        private const string ACTIVE_APPLICATION_ID = "ActiveApplicationIdKey";

        // GenericPostingPartner
        private const string IS_GENERIC_POSTING_PARTNER_KEY = "IsGenericPostingPartner";

        // CMS
        private const string CMS_WEB_CONTENT_ID = "CMSWebContentId";
        private const string CMS_REVISION = "CMSRevision";
        
        public static string GetSignatureText(bool isCoApplicant)
        {
            var session = HttpContext.Current.Session;
            if (session == null)
                throw new ArgumentNullException("session");

            if (session[SIGANTURE_TEXT_KEY + isCoApplicant.ToString()] == null)
                return string.Empty;
            else
                return (string) session[SIGANTURE_TEXT_KEY + isCoApplicant.ToString()];			
        }

        public static string SetSignatureText(bool isCoApplicant, string name)
        {
            string timestamp = isCoApplicant ? SessionUtility.SecondarySignatureTstamp : SessionUtility.PrimarySignatureTstamp;

            string text = string.Format("{0} submitted above signature/mark on {1}", name, timestamp);

            HttpContext.Current.Session[SIGANTURE_TEXT_KEY + isCoApplicant.ToString()] = text;

            return text;
        }
               
        public static string PrimarySignatureTstamp
        {
            get { return Get<string>(PRIMARY_SIGNATURE_TSTAMP_KEY, string.Empty); }
            set { Set(PRIMARY_SIGNATURE_TSTAMP_KEY, value); }
        }

        public static string SecondarySignatureTstamp
        {
            get { return Get<string>(SECONDARY_SIGNATURE_TSTAMP_KEY, string.Empty); }
            set { Set(SECONDARY_SIGNATURE_TSTAMP_KEY, value); }
        }
        
        public static byte[] PrimarySignatureImageBytes
        {
            get { return Get<byte[]>(PRIMARY_SIGNATURE_IMAGE_BYTES_KEY, null); }
            set { Set(PRIMARY_SIGNATURE_IMAGE_BYTES_KEY, value); }           
        }

        public static byte[] SecondarySignatureImageBytes
        {
            get { return Get<byte[]>(SECONDARY_SIGNATURE_IMAGE_BYTES_KEY, null); }
            set { Set(SECONDARY_SIGNATURE_IMAGE_BYTES_KEY, value); }           
        }

        /// <summary>
        /// This method was created to address the issue of having a signed loan
        /// agreement in the Session after making change to one's loan terms.  Once
        /// a customer changes their loan terms, this method should be called to
        /// clear the old signature from the Session.  THEY HAVE TO ACKNOWLEDGE
        /// THE NEW TERMS BY RE-SIGNING THE LOAN AGREEMENT.  This also addresses
        /// bug #409 in Team Plain.
        /// </summary>
        public static void ResetLoanAgreementSignature()
        {
            HttpContext.Current.Session[LOAN_AGREEMENT_KEY] = null;
            //SessionUtility.PrimarySignatureTstamp = null;
            HttpContext.Current.Session[PRIMARY_SIGNATURE_TSTAMP_KEY] = null;
            SessionUtility.SecondarySignatureTstamp = null;
            SessionUtility.PrimarySignatureImageBytes = null;
            SessionUtility.SecondarySignatureImageBytes = null;
            //SessionUtility.IsLoanAgreementPersisted = false;

            //SessionUtility.FoundActiveLoanAgreementOnFile = false;
            HttpContext.Current.Session[FOUND_ACTIVE_LOAN_AGREEMENT_ON_FILE_KEY] = null;
            //SessionUtility.FoundPartiallySignedLoanAgreementOnFile = false;
            HttpContext.Current.Session[FOUND_PARTIALLY_SIGNED_LOAN_AGREEMENT_ON_FILE_KEY] = null;
            //SessionUtility.FoundApplicantSignatureOnFile = false;
            HttpContext.Current.Session[FOUND_APPLICANT_SIGNATURE_ON_FILE_KEY] = null;
            //SessionUtility.FoundCoApplicantSignatureOnFile = false;
            HttpContext.Current.Session[FOUND_COAPPLICANT_SIGNATURE_ON_FILE_KEY] = null;
            //SessionUtility.LoanAgreementHtmlEdocId = -1;
        }

        public static CustomerUserIdDataSet CustomerUserIdDataSet
        {
            get
            {
                var info = AccountInfo;
                return info == null ? null : info.CustomerUserIdDataSet;
            }
        }

        public static CurrentApplicationData GetCurrentApplicationData(int applicationId)
        {
            return Get<CurrentApplicationData>("AppData_" + applicationId, null);
        }
        public static void SetCurrentApplicationData(int applicationId, ICurrentApplicationData appData)
        {
            Set("AppData_" + applicationId, appData);
        }

        public static GetAccountInfoResponse AccountInfo
        {
            get { return Get<GetAccountInfoResponse>(ACCOUNT_INFO_SESSION_KEY, null); }
            set
            {
                if (value != null && value.CustomerUserIdDataSet != null)
                {
                    DataSetUtility.ConfigureDataSetForHighEfficiencySerialization(value.CustomerUserIdDataSet);
                }
                Set(ACCOUNT_INFO_SESSION_KEY, value);
            }
        }

        public static int? AccountUserId
        {
            get { return Get<int?>(ACCOUNT_USER_ID, null); }
            set{ Set(ACCOUNT_USER_ID, value); }
        }

        /// <summary>
        /// This method will re-get the summary account information for the 
        /// given session.  If the account information does not already exist
        /// in the session (i.e., the user has not been authenticated), this
        /// method will throw an InvalidOperationException.
        /// </summary>
        /// <returns>The refreshed summary account information.</returns>
        public static GetAccountInfoResponse RefreshAccountInfo()
        {
            GetAccountInfoResponse info = AccountInfo;

            if(info == null)
                throw new InvalidOperationException("The session has not been initialized.");

            // The user ID will never modified during a session.
            info = DomainServiceCustomerOperations.GetAccountInfoByUserId(info.CustomerUserIdDataSet.AccountInfo.UserId);
            AccountInfo = info;
            return info;
        }

        public static LoanApplicationDataSet GetApplicationData(int? applicationId)
        {
            LoanApplicationDataSet lads = (LoanApplicationDataSet)HttpContext.Current.Session[APPLICATION_SESSION_KEY];

            if (applicationId.HasValue)
            {
                if (lads == null || lads.Application[0].ApplicationId != applicationId.Value )
                {
                    lads = DomainServiceLoanApplicationOperations.GetLoanApplicationByApplicationId(applicationId.Value);
                    HttpContext.Current.Session[APPLICATION_SESSION_KEY] = lads;
                }
            }

            return lads;
        }

        public static LoanApplicationDataSet ReloadApplicationData(int applicationId)
        {
            return ReloadApplicationData(HttpContext.Current.Session, applicationId);
        }

        public static LoanApplicationDataSet ReloadApplicationData(HttpSessionState session, int applicationId)
        {
            LoanApplicationDataSet lads = DomainServiceLoanApplicationOperations.GetLoanApplicationByApplicationId(applicationId);
            DataSetUtility.ConfigureDataSetForHighEfficiencySerialization(lads);
            session[APPLICATION_SESSION_KEY] = lads;

            return lads;
        }
        


        #region LoanOffer

        /// <summary>
        /// The Loan Agreement that is returned from the correspondence engine
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public static LoanAgreementHtml GetLoanAgreement()
        {
            HttpSessionState session = HttpContext.Current.Session;
            return (LoanAgreementHtml)session[LOAN_AGREEMENT_KEY];
        }

        public static void SetLoanAgreement(LoanAgreementHtml loanAgreement)
        {
            HttpSessionState session = HttpContext.Current.Session;
            session[LOAN_AGREEMENT_KEY] = loanAgreement;
        }

        
        /// <summary>
        /// This property was added as part of an intermediate (pre v2.0 Loan Acceptance) solution to address
        /// the issue that pertains to a customer who signed their agreement in the past and should not
        /// have to re-sign it when they return.  Requested by business 10/26/06, refer to "Incomplete Loan
        /// Acceptance -- Interim Plan" specification.
        /// </summary>
        public static bool FoundActiveLoanAgreementOnFile
        {
            get { return Get<bool>(FOUND_ACTIVE_LOAN_AGREEMENT_ON_FILE_KEY, false); }
            set { Set(FOUND_ACTIVE_LOAN_AGREEMENT_ON_FILE_KEY, value); }
        }

        public static bool FoundPartiallySignedLoanAgreementOnFile
        {
            get { return Get<bool>(FOUND_PARTIALLY_SIGNED_LOAN_AGREEMENT_ON_FILE_KEY, false); }
            set { Set(FOUND_PARTIALLY_SIGNED_LOAN_AGREEMENT_ON_FILE_KEY, value); }
        }

        public static bool FoundApplicantSignatureOnFile
        {
            get { return Get<bool>(FOUND_APPLICANT_SIGNATURE_ON_FILE_KEY, false); }
            set { Set(FOUND_APPLICANT_SIGNATURE_ON_FILE_KEY, value); }
        }

        public static bool FoundCoApplicantSignatureOnFile
        {
            get { return Get<bool>(FOUND_COAPPLICANT_SIGNATURE_ON_FILE_KEY, false); }
            set { Set(FOUND_COAPPLICANT_SIGNATURE_ON_FILE_KEY, value); }
        }

        /// <summary>
        /// Gets the LoanOffer DataSet
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        [Obsolete]
        public static LoanOfferDataSet GetLoanOfferDataSet()
        {
            return GetLoanOfferDataSet(HttpContext.Current.Session);
        }

        /// <summary>
        /// Gets the LoanOffer DataSet
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        [Obsolete]
        public static LoanOfferDataSet GetLoanOfferDataSet(HttpSessionState session)
        {
            return (LoanOfferDataSet)session[LOAN_OFFER_KEY];
        }

        [Obsolete]
        public static void SetLoanOfferDataSet(FirstAgain.Domain.SharedTypes.LoanApplication.LoanOfferDataSet value)
        {
            DataSetUtility.ConfigureDataSetForHighEfficiencySerialization(value);
            Set(LOAN_OFFER_KEY, value);
        }

        #endregion


        #region Account Services

        public static void SetUpAccountServicesData(string userName = null)
        {
            ServicingTypes.BusinessCalendarDataSet bcds;
            AccountServicesDataSet ads;
            DocumentStoreDataSet dds;

            if (userName == null)
            {
                userName = HttpContext.Current.User.Identity.Name;
            }
            ServicingClient.LoanServicingOperations.GetAccountServicingDataByUserId(userName, out bcds, out ads, out dds);

            AccountServicesData = ads;
            BusinessCalendar = bcds;
            DocumentStore = dds;
        }

        public static AccountServicesDataSet AccountServicesData
        {
            get { return Get<AccountServicesDataSet>(ACCOUNT_SERVICES_DATA_KEY, null); }
            set
            {
                DataSetUtility.ConfigureDataSetForHighEfficiencySerialization(value);
                Set(ACCOUNT_SERVICES_DATA_KEY, value);
            }
        }

        public static bool AcceptedPreviousTerms
        {
            get { return Get<bool>(ACCEPTED_PREVIOUS_TERMS, false); }
            set { Set(ACCEPTED_PREVIOUS_TERMS, value); }
        }

        public static DocumentStoreDataSet DocumentStore
        {
            get { return Get<DocumentStoreDataSet>(DOCUMENT_STORE_DATA_KEY, null); }
            set
            {
                DataSetUtility.ConfigureDataSetForHighEfficiencySerialization(value);
                Set(DOCUMENT_STORE_DATA_KEY, value);
            }
        }

        [Obsolete]
        public static int? ActiveApplicationId
        {
            get { return Get<int?>(ACTIVE_APPLICATION_ID, null); }
            set { Set(ACTIVE_APPLICATION_ID, value); }
        }


        public static FirstAgain.LoanServicing.SharedTypes.BusinessCalendarDataSet BusinessCalendar
        {
            get
            {
                return Get<FirstAgain.LoanServicing.SharedTypes.BusinessCalendarDataSet>(BUSINESS_CALENDAR_KEY, null);
            }
            set
            {
                DataSetUtility.ConfigureDataSetForHighEfficiencySerialization(value);
                Set(BUSINESS_CALENDAR_KEY, value);
            }
        }

        /// <summary>
        /// This property was originally created to keep track of the fact that the customer
        /// logged in, but has decided to apply for a new loan.  Since the application is
        /// primarily for those folks who are not yet customers, we need to know if the
        /// transition occurred in order to modify the look and feel.
        /// </summary>
        public static bool IsAccountServices
        {
            get { return Get<bool>(IS_ACCOUNT_SERVICES_KEY, false); }
            set { Set(IS_ACCOUNT_SERVICES_KEY, value); }
        }

        #endregion

        #region GenericPostingPartner
        public static bool IsGenericPostingPartner
        {
            get { return Get<bool>(IS_GENERIC_POSTING_PARTNER_KEY, false); }
            set { Set(IS_GENERIC_POSTING_PARTNER_KEY, value); }
        }
        #endregion

        public static void CleanUpSessionForRefresh()
        {
            HttpSessionState session = HttpContext.Current.Session;
            if (session == null)
            {
                throw new InvalidOperationException("The session has not been initialized.");
            }
            session.Remove(ADD_CO_APPLICANT);
            session.Remove(ACCEPTED_PREVIOUS_TERMS);
            session.Remove(CUSTOMER_IDENTIFICATION_APPLICANT);
            session.Remove(SIGANTURE_TEXT_KEY);
            session.Remove(LOAN_AGREEMENT_KEY);
            session.Remove(PRIMARY_SIGNATURE_TSTAMP_KEY);
            session.Remove(SECONDARY_SIGNATURE_TSTAMP_KEY);
            session.Remove(PRIMARY_SIGNATURE_IMAGE_BYTES_KEY);
            session.Remove(SECONDARY_SIGNATURE_IMAGE_BYTES_KEY);
            session.Remove(FOUND_ACTIVE_LOAN_AGREEMENT_ON_FILE_KEY);
            session.Remove(FOUND_PARTIALLY_SIGNED_LOAN_AGREEMENT_ON_FILE_KEY);
            session.Remove(FOUND_APPLICANT_SIGNATURE_ON_FILE_KEY);
            session.Remove(FOUND_COAPPLICANT_SIGNATURE_ON_FILE_KEY);
        }

        public static bool AddCoApplicant
        {
            get { return Get<bool>(ADD_CO_APPLICANT, false); }
            set { Set(ADD_CO_APPLICANT, value); }
        }
        
        public static int? CMSWebContentId
        {
            get { return Get<int?>(CMS_WEB_CONTENT_ID, null); }
            set { Set(CMS_WEB_CONTENT_ID, value); }
        }

        public static int? CMSRevision
        {
            get { return Get<int?>(CMS_REVISION, null); }
            set { Set(CMS_REVISION, value); }
        }

        public static ApplicantTypeLookup.ApplicantType? CustomerIdentificationApplicant
        {
            get { return Get<ApplicantTypeLookup.ApplicantType?>(CUSTOMER_IDENTIFICATION_APPLICANT, null); }
            set { Set(CUSTOMER_IDENTIFICATION_APPLICANT, value); }
        }

        public static T Get<T>(string key, T defaultValue, bool initialize = false)
        {
            HttpSessionState session = HttpContext.Current.Session;
            if (session == null)
                return defaultValue;

            object item = session[key];

            if(initialize && item == null)
            {
                Set(key, defaultValue);
            }

            return item == null ? defaultValue : (T)item;
        }

     
        public static void Set(string key, object value)
        {
            HttpSessionState session = HttpContext.Current.Session;
            if (session == null)
            {
                throw new InvalidOperationException("The session has not been initialized.");
            }

            session[key] = value;
        }

        public static bool IsDuplicateSubmission(NativeLoanApplicationModel model, string cookie)
        {
            try
            {
                var cache = GetAppSubmissionCache();
                if (cache == null)
                {
                    return false;
                }

                var key = new SubmittedApplicationKey(model, cookie);

                RetrieveOptions options = new RetrieveOptions
                {
                    LockingMode = ReadLockingMode.LockOnRead,
                    CreateHandler = delegate (CachedObjectId id, object argument) { return new SubmittedApplicationResult(); },
                    CreateArgument = null,
                    CreatePolicy = new CreatePolicy(TimeSpan.FromMinutes(15))
                };

                object cacheItem = null;
                try
                {
                    cacheItem = cache.Retrieve(key.ToString(), options);

                    var existingKey = cacheItem as SubmittedApplicationResult;

                    if (existingKey != null)
                    {
                        if(!existingKey.IsSubmitted)
                        {
                            existingKey.IsSubmitted = true;
                            cache.Update(key.ToString(), existingKey, false);
                            return false;
                        }
                        if(existingKey.SubmitFailed)
                        {
                            existingKey.SubmitFailed = false;
                            cache.Update(key.ToString(), existingKey, false);
                            return false;
                        }
                        else
                        {                          
                            return true;
                        }
                    }
                    return false;
                }
                finally
                {
                    if(cacheItem != null)
                    {
                        cache.ReleaseLock(key.ToString());
                    }
                }
            }
            catch(Exception e)
            {
                LightStreamLogger.WriteWarning(e);
                return false;
            }
        }

        public static void FailAppSubmission(NativeLoanApplicationModel model, string cookie)
        {
            try
            {
                var cache = GetAppSubmissionCache();
                if (cache == null)
                {
                    return;
                }

                var key = new SubmittedApplicationKey(model, cookie);

                object cacheItem = null;
                try
                {
                    cacheItem = cache.Retrieve(key.ToString(), true);

                    var existingKey = cacheItem as SubmittedApplicationResult;

                    if(existingKey != null && !existingKey.SubmitFailed)
                    {
                        existingKey.SubmitFailed = true;
                        cache.Update(key.ToString(), existingKey, false);
                    }
                }
                finally
                {
                    if (cacheItem != null)
                    {
                        cache.ReleaseLock(key.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                LightStreamLogger.WriteWarning(e);
            }
        }

        private static NamedCache GetAppSubmissionCache()
        {
            var cache = CacheFactory.GetCache("AppSubmitCache");

            if (cache == null)
            {
                LightStreamLogger.WriteWarning("Could not create AppSubmit Cache");
            }

            return cache;
        }

        private static HttpSessionState Session
        {
            get
            {
                HttpSessionState session = HttpContext.Current.Session;
                if (session == null)
                    throw new InvalidOperationException("The session has not been initialized.");
                return session;
            }
        }

        /// <summary>
        /// This is a "nicer" version of Session.Abandon/Session.Clear which will keep 
        /// the items added to sessionItemsToKeep.
        /// </summary>
        public static void CleanUpSession()
        {
            Session.Clear();
        }

    }
}
