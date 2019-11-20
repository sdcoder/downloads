using FirstAgain.Common.Encryption;
using FirstAgain.Common.Extensions;
using FirstAgain.Domain.Common;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Contracts.MessageContracts.CustomerOperations;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.ServiceModel.Client;
using LightStreamWeb.ServerState;
using LightStreamWeb.App_State;
using LightStreamWeb.Controllers.Shared;
using LightStreamWeb.Filters;
using LightStreamWeb.Helpers;
using LightStreamWeb.Models.SignIn;
using Ninject;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using FirstAgain.Common.Web;
using FirstAgain.Domain.SharedTypes.ContentManagement;
using FirstAgain.Common.Logging;
using FirstAgain.Web.UI;
using FirstAgain.Web.Cookie;

namespace LightStreamWeb.Controllers
{
    public class SignInController : BaseController
    {
        private const string TEST_COOKIE_NAME = "LS-Test-Cookie";

        [Inject]
        public SignInController(ICurrentUser user)
            : base(user)
        {
        }

        // GET: /SignIn/
        [MaintenanceModeCheck()]
        public ActionResult Index(bool? jumpToApply, bool? inProcess, string status, string UID, int? partnerId, decimal? loanAmount, int? loanTermMonths)
        {
            if (FailedLoginCounter.Get() > FailedLoginCounter.MAX_LOGIN_ATTEMPTS)
            {
                FirstAgain.Common.Logging.LightStreamLogger.WriteWarning(string.Format("{0} exceeded max login attempts from IP {1}", UID, this.WebUser.IPAddress));
                return RedirectToAction("LoginFailed");
            }

            // save the "jump to apply" value
            if (jumpToApply.GetValueOrDefault(false))
            {
                Session["JumpToApply"] = true;
            }

            SetTestCookie();

            // kill off any dangling session information (can happen if someone backs up to this page after signing in and re-signs in)            
            WebUser.SignOut();
            FormsAuthentication.SignOut();

            var model = new SignInModel(inProcess)
            {
                Canonical = "https://www.lightstream.com/customer-sign-in"
            };

            model.Title = "LightStream Loans- Customer Sign In";
            model.MetaDescription = "LightStream Customer Sign In. Access your account information. LightStream online lending offers unsecured personal loans at low rates for those with good credit.";

            if (status == "timeout")
                TempData["Timeout"] = TimeoutMessage;

            if (status == "logout")
                model.IsSignOut = true;

            PartnerReferralSignInModel partnerRefSignInModel;

            bool isLendingTree;
            bool.TryParse(Request.QueryString["IsLendingTree"], out isLendingTree);
            
            if (Request.QueryString["AID"].IsNotNull())
            {
                WebUser.AID = Request.QueryString["AID"];
            }

            if (UID.IsNotNull())
            {
                var symmetricEncryptor = new SymmetricEncryptor();
                var urlDecode = HttpUtility.UrlDecode(UID);
                if (urlDecode != null)
                {
                    var decryptedUserId = symmetricEncryptor.Decrypt(urlDecode.Replace(' ', '+'));

                    if (decryptedUserId.IsNotNullOrEmpty())
                    {
                        var tokens = decryptedUserId.Split(",".ToCharArray());
                        if (tokens.Length > 0)
                        {
                            // if Offer Amount & Term exists then set Cookie for later use
                            if (loanTermMonths.IsNotNull() && loanAmount.IsNotNull())
                            {
                                WebUser.LoanAmount = loanAmount;
                                WebUser.LoanTerm = loanTermMonths;
                                WebUser.SelectedLoanTerm = loanTermMonths;
                            }

                            var authPageContent = new ContentManager().Get<AuthenticationPage>();
                            if (authPageContent != null)
                            {
                                if(tokens.Length == 3)
                                {
                                    partnerId = int.Parse(tokens[2]);
                                }

                                try { 
                                    var appId = DomainServiceCustomerOperations.GetAccountInfoByCustomerUserId(tokens[0]).ApplicationsDates.ViewableApplications.FirstOrDefault();

                                    partnerRefSignInModel = new PartnerReferralSignInModel(partnerId, isLendingTree, authPageContent, appId)
                                    {
                                        UserId = tokens[0],
                                        DisplayPasswordSSNPrompt = true,
                                        IsTempUserId = true
                                    };
                                    
                                    return View("PartnerReferralSignIn", partnerRefSignInModel);
                                }
                                catch (Exception ex)
                                {
                                    //handle if the inquiry is completed. 
                                    if (!ex.Message.Contains("Invalid CustomerUserId"))
                                    {
                                        //error rather than "cannot find the userID"
                                        LightStreamLogger.WriteWarning(string.Format("CustomerUserID: {0}, Exception info: {1}", tokens[0], ex));
                                    }

                                    //TempData["Alert"] = "The temporary ID is not valid anymore. please use your new user ID and password to sign in.";
                                    //move forward to login page. 
                                }
                            }
                            else
                            {
                                model = new SignInModel { UserId = tokens[0], DisplayPasswordSSNPrompt = true };
                            }
                        }
                    }
                }
            }

            if (Request.QueryString["TempUserId"].IsNotNull())
            {
                var authPageContent = new ContentManager().Get<AuthenticationPage>();

                try
                {
                    var appId = 0;
                    if (Request.QueryString["TempUserId"] == "previewUserId")
                    {
                        // return jonathan consumer app id for cms preview.
                        if (FirstAgain.Domain.Common.BusinessConstants.Instance.Environment == FirstAgain.Domain.Lookups.FirstLook.EnvironmentLookup.Environment.QA)
                        {
                            appId = 86437799; //qa jonathan consumer - expired
                        }
                        else if (FirstAgain.Domain.Common.BusinessConstants.Instance.Environment == FirstAgain.Domain.Lookups.FirstLook.EnvironmentLookup.Environment.QA2)
                        {
                            appId = 37656491; //qa2 jonathan consumer - expired
                        }
                        else if (FirstAgain.Domain.Common.BusinessConstants.Instance.Environment == FirstAgain.Domain.Lookups.FirstLook.EnvironmentLookup.Environment.Staging)
                        {
                            appId = 11141310; //staging jonathan consumer - expired
                        }
                        else if (FirstAgain.Domain.Common.BusinessConstants.Instance.Environment == FirstAgain.Domain.Lookups.FirstLook.EnvironmentLookup.Environment.Production)
                        {
                            appId = 86462509; //production jonathan consumer - expired
                        }
                    }
                    else
                    {
                        appId = DomainServiceCustomerOperations.GetAccountInfoByCustomerUserId(Request.QueryString["TempUserId"]).ApplicationsDates.ViewableApplications.FirstOrDefault();
                    }

                    if (authPageContent != null)
                    {
                        partnerRefSignInModel = new PartnerReferralSignInModel(partnerId, isLendingTree, authPageContent, appId)
                        {
                            Canonical = "https://www.lightstream.com/customer-sign-in",
                            IsTempUserId = true,
                            DisplayPasswordSSNPrompt = true
                        };

                        partnerRefSignInModel.UserId = Request.QueryString["TempUserId"];

                        return View("PartnerReferralSignIn", partnerRefSignInModel);
                    }
                    else
                    {
                        model.UserId = Request.QueryString["TempUserId"];
                    }
                }
                catch (Exception ex)
                {
                    //handle if the inquiry is completed. 
                    if (!ex.Message.Contains("Invalid CustomerUserId"))
                    {
                        //error rather than "cannot find the userID"
                        LightStreamLogger.WriteWarning(string.Format("CustomerUserID: {0}, Exception info: {1}", Request.QueryString["TempUserId"], ex));
                    }

                    //TempData["Alert"] = "The temporary ID is not valid anymore. please use your new user ID and password to sign in.";
                    //move forward to login page. 
                }
            }
            return View(model);
        }

        private void SetTestCookie()
        {
            // set a test cookie
            Response.Cookies.Set(new HttpCookie(TEST_COOKIE_NAME, "1"));
        }

        const string TimeoutMessage = "For your security, your session was ended. Please log in to resume your activity.";
        //
        // POST: /customer-sign-in
        [HttpPost]
        [RequireCookie(TEST_COOKIE_NAME, "/signin/cookiesrequired")]
        public ActionResult Index([Bind(Include= "UserId,UserPassword,InProcess,isTimeout")]SignInModel model)
        {
            try
            {
                if (BruteForceCounter.ShouldLockOutIpAddress(this.WebUser))
                {
                    LightStreamLogger.WriteWarning($"Possible brute force detected from {this.WebUser.IPAddress} {this.WebUser.UserAgent}");
                }

                if (model.isTimeout)
                {
                    TempData["Timeout"] = TimeoutMessage;
                }

                var result = model.Login(new FirstAgainMembershipProvider());
                switch (result)
                {
                    case SignInModel.LoginResult.UserIdRequired:
                        TempData["Alert"] = Resources.FAMessages.SignInUserId_Val;
                        return View("Index", model);

                    case SignInModel.LoginResult.UserPasswordRequired:
                        TempData["Alert"] = Resources.FAMessages.SignInPassword_Val;
                        return View("Index", model);

                    case SignInModel.LoginResult.InvalidUserNameOrPassword:
                        if (FailedLoginCounter.Increment() > FailedLoginCounter.MAX_LOGIN_ATTEMPTS)
                        {
                            LightStreamLogger.WriteWarning(string.Format("{0} failed login exceeded max login attempts from IP {1}", model.UserId, this.WebUser.IPAddress));
                            return RedirectToAction("LoginFailed");
                        }

                        // anti-XSS
                        model.UserId = string.Empty;

                        // another option that works is this:
                        // model.UserId = Encoder.JavaScriptEncode(model.UserId);

                        TempData["Alert"] = "Your user name or password is incorrect";
                        FormsAuthentication.SignOut();
                        return View("Index", model);

                    case SignInModel.LoginResult.Success:
                        FailedLoginCounter.Reset();

                        CookieUtility.SetCookie("LS-FirstTimeSignedIn", "true", null, false);

                        return RedirectBasedOnStatus(model.UserId);

                    default:
                        throw new ArgumentException(result.ToString());
                }

            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
                TempData["Alert"] = Resources.FAMessages.GenericError;
                return View("Index", model);
            }
        }

        private ActionResult RedirectBasedOnStatus(string userName)
        {
            var accountData = new SignInModel().GetAccountStatus(userName);
            InitializeSessionObjects(accountData);

            EventAuditLogHelper.Submit(accountData.CustomerData, WebUser, EventTypeLookup.EventType.LoggedIn, null);

            switch (accountData.Result)
            {
                case SignInModel.LoginAccountStatusType.AccountLock:
                    return RedirectToAction("AccountLock", "SignIn");

                case SignInModel.LoginAccountStatusType.EmailLock:
                    return RedirectToAction("AccountLock", "SignIn");

                case SignInModel.LoginAccountStatusType.Fraud:
                case SignInModel.LoginAccountStatusType.Freeze:
                case SignInModel.LoginAccountStatusType.BoardingFailed:
                    ViewData["LoginAccountStatusType"] = accountData.Result;
                    return View("AccountNotAvailable", new SignInModel());

                case SignInModel.LoginAccountStatusType.Failed:
                    TempData["Alert"] = accountData.CustomerMessage;
                    return View(new SignInModel());

                case SignInModel.LoginAccountStatusType.ConfirmEmail:
                    FormsAuthentication.SetAuthCookie(userName, false);
                    return new RedirectResult(string.Format("~/SignIn/ConfirmEmailAddress?ctx={0}", WebSecurityUtility.Scramble(accountData.ApplicationId)));

                case SignInModel.LoginAccountStatusType.ActiveApplication:
                    FormsAuthentication.SetAuthCookie(userName, false);
                    return new RedirectResult(StatusRedirect.GetRedirectBasedOnStatus(accountData.CustomerData, accountData.ApplicationId));

                case SignInModel.LoginAccountStatusType.ExceptionAfterDecision:
                    FormsAuthentication.SetAuthCookie(userName, false);
                    return new RedirectResult("~/appstatus/inreview");

                case SignInModel.LoginAccountStatusType.HasInactiveFundedAccount:
                    ViewData["LoginAccountStatusType"] = accountData.Result;
                    return View("AccountNotAvailable", new SignInModel());

                case SignInModel.LoginAccountStatusType.HasFundedAccount:
                    StartAccountServicesSession(userName);

                    // This is the account services application. Ensure we link to the "old" one for now. New one will be under /AccountServices/Apply - or something like that
                    if ((bool?)Session["JumpToApply"] == true)
                    {
                        Session["JumpToApply"] = null;
                        return new RedirectResult("~/Apply");
                    }
                    return new RedirectResult(string.Format("~/Account?ctx={0}", WebSecurityUtility.Scramble(accountData.ApplicationId)));

                case SignInModel.LoginAccountStatusType.InformationReqestRequired:
                    StartAccountServicesSession(userName);
                    return new RedirectResult(string.Format("~/Account/InformationRequest?ctx={0}", WebSecurityUtility.Scramble(accountData.ApplicationId)));

                default:
                    throw new ArgumentException(accountData.Result.ToString());
            }

            // not possible?
            throw new NotImplementedException();
        }


        private static void StartAccountServicesSession(string userName)
        {
            FormsAuthentication.SetAuthCookie(userName, false);
            SessionUtility.SetUpAccountServicesData(userName);
            SessionUtility.IsAccountServices = true;
        }

        //
        // GET: /SignIn/LoginFailed
        public ActionResult LoginFailed()
        {
            return View("InvalidSecurityInfo", new InvalidSecurityInfoModel() { PageTitle = "Customer Sign In" });
        }

        //
        // GET: /SignIn/Maintenance
        public ActionResult Maintenance()
        {
            Response.Headers.Add("X-In-Maintenance-Mode", "true");

            return View(new SignInModel());
        }
        //
        // GET: /SignIn/CookiesRequired
        public ActionResult CookiesRequired()
        {
            SetTestCookie();

            TempData["Alert"] = "For security purposes (yours and ours), you need to enable cookies in order to sign in.  Thank you.";
            return View("Index", new SignInModel());
        }

        [HttpGet]
        [MaintenanceModeCheck]
        public ActionResult RecoverUserID()
        {
            return View(new RecoverUserIDModel());
        }

        [HttpPost]
        public ActionResult RecoverUserID([Bind(Include= RecoverUserIDModel.BIND_FIELDS)]RecoverUserIDModel model)
        {
            if (model == null)
            {
                return RedirectToAction("RecoverUserID");
            }
            SessionUtility.AccountInfo = null;
            ActionResult actionResult;
            GetAccountInfoResponse info = null;
            try
            {
                if (model.AccountNumberOrSSN.SelectedType == EnterAccountNumberOrSSNModel.RecoveryType.AccountNumber)
                {
                    info = DomainServiceCustomerOperations.GetAccountInfoByApplicationId(model.AccountNumberOrSSN.AccountNumber);
                }
                else if (model.AccountNumberOrSSN.SelectedType == EnterAccountNumberOrSSNModel.RecoveryType.SocialSecurityNumber)
                {
                    info = DomainServiceCustomerOperations.GetAccountInfoBySSN(model.AccountNumberOrSSN.SocialSecurityNumber);
                }

                if (info != null && !info.CustomerUserIdDataSet.IsEmpty)
                {
                    FailedLoginCounter.Reset();

                    SessionUtility.AccountInfo = info;
                    CustomerUserIdDataSet customerAccounts = info.CustomerUserIdDataSet;

                    SelectAccountModel selectAccountModel = SelectAccountModelBuilder.GetSelectAccountModelFromDataSet(customerAccounts);

                    // If have multiple apps related to the user id,
                    // need to display all of them for the user to select one
                    if (selectAccountModel.UserAccountInfo.Count > 1)
                    {
                        // Instantiate a model and show view to select correct application...
                        actionResult = View("SelectAccount", selectAccountModel);

                    }
                    else
                    {
                        // Store selected user id in session on the server, not in the client
                        int selectedUserId = selectAccountModel.UserAccountInfo[0].UserId;
                        SessionUtility.AccountUserId = selectedUserId;
                        if (customerAccounts.CustomerUserId[0].IsTemporary
                            && customerAccounts.Application[0].ApplicationStatusType.IsOneOf(ApplicationStatusTypeLookup.ApplicationStatusType.Inquiry,
                                                                                             ApplicationStatusTypeLookup.ApplicationStatusType.Incomplete,
                                                                                             ApplicationStatusTypeLookup.ApplicationStatusType.Terminated))
                        {
                            // If recovering a userid for an inquiry or incomplete app,
                            // redirect back to the login page now with the recovered user id
                            actionResult = View("Index", new SignInModel() { UserId = customerAccounts.CustomerUserId[0].CustomerUserId, DisplayPasswordSSNPrompt = true });
                        }
                        else
                        {
                            // Instantiate a model and show the security question view...
                            SecurityQuestionModel sqModel = new SecurityQuestionModel();
                            sqModel.SecurityQuestion = SecurityQuestionLookup.GetCaption(customerAccounts.CustomerUserIdSecurityQuestion.First(csq => csq.UserId == selectedUserId).SecurityQuestion);
                            actionResult = View("SecurityQuestion", sqModel);
                        }
                    }
                }
                else
                {
                    actionResult = _handleInvalidSecurityInfo(ForgotCredentialType.ForgotUserId, "Recover User ID", model);
                }
            }
            catch (Exception)
            {
                actionResult = _handleInvalidSecurityInfo(ForgotCredentialType.ForgotUserId, "Recover User ID", model);
            }

            return actionResult;
        }

        [HttpPost]
        public ActionResult SecurityAnswer([Bind(Include="SecurityQuestion,SecurityAnswer,ChangePasswordRequired")]SecurityQuestionModel model)
        {
            try
            {
                if (model.IsNull() || (model.SecurityQuestion.IsNullOrEmpty() && model.SecurityAnswer.IsNullOrEmpty()))
                {
                    TempData["Alert"] = "Security question or answer not found";
                    return View("SecurityQuestion", model);
                }

                CustomerUserIdDataSet customerAccounts = SessionUtility.CustomerUserIdDataSet;
                int? selectedUserId = SessionUtility.AccountUserId;
                ActionResult actionResult = RedirectToAction("RecoverUserId");

                if (customerAccounts != null && selectedUserId.HasValue)
                {
                    CustomerUserIdDataSet.CustomerUserIdSecurityQuestionRow secQuestionRow = customerAccounts.CustomerUserIdSecurityQuestion.First(csq => csq.UserId == selectedUserId);
                    if (secQuestionRow == null)
                    {
                        TempData["Alert"] = "Your security question could not be located. Please try again.";
                        return View("SecurityQuestion", model);
                    }

                    if (string.Compare(model.SecurityAnswer.Trim(), secQuestionRow.SecurityAnswer.Trim(), true) == 0)
                    {
                        if (model.ChangePasswordRequired)
                        {
                            actionResult = View("ResetPassword", new ResetPasswordModel() { CustomerUserID = customerAccounts.CustomerUserId.First(cui => cui.UserId == selectedUserId).CustomerUserId });
                        }
                        else
                        {
                            actionResult = View("Index", new SignInModel() { UserId = customerAccounts.CustomerUserId.First(cui => cui.UserId == selectedUserId).CustomerUserId, DisplayRecoveredUserIdText = true });
                        }
                    }
                    else
                    {
                        model.SecurityQuestion = SecurityQuestionLookup.GetCaption(secQuestionRow.SecurityQuestion);
                        model.SecurityAnswer = string.Empty;
                        if (FailedLoginCounter.Increment() > 3)
                        {
                            actionResult = View("InvalidSecurityInfo", new InvalidSecurityInfoModel() { PageTitle = model.ChangePasswordRequired ? "Forgot Password" : "Recover User ID" });
                        }
                        else
                        {
                            TempData["Alert"] = "Your answer is incorrect. Please try again.";
                            actionResult = View("SecurityQuestion", model);
                        }
                    }
                }

                return actionResult;
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex, "Error finding security answer {0} - {1}", SessionUtility.AccountUserId, model.SecurityAnswer?.Length);
                throw;
            }
        }

        [HttpPost]
        public ActionResult SelectAccount([Bind(Include= "SelectedUserId")]SelectAccountModel model)
        {
            ActionResult actionResult = null;
            CustomerUserIdDataSet customerAccounts = SessionUtility.CustomerUserIdDataSet;
            if (model.SelectedUserId > 0)
            {
                SessionUtility.AccountUserId = model.SelectedUserId;

                CustomerUserIdDataSet.CustomerUserIdRow cuiRow = customerAccounts.CustomerUserId.Where(c => c.UserId == model.SelectedUserId).FirstOrDefault();
                if (cuiRow == null)
                {
                    return RedirectToAction("RecoverUserId");
                }
                CustomerUserIdDataSet.ApplicationRow appRow = customerAccounts.CustomerUserIdXrefApplication
                                                                              .Where(cxa => cxa.UserId == model.SelectedUserId)
                                                                              .Join(customerAccounts.Application, cxa => cxa.ApplicationId, app => app.ApplicationId, (cxa, app) => app)
                                                                              .First();

                // Need to check if the selected app pertains to an inquiry/incomplete app or a dead application for which a security question has not yet been selected
                if (cuiRow.IsTemporary && (appRow.ApplicationStatusType.IsOneOf(ApplicationStatusTypeLookup.ApplicationStatusType.Inquiry,
                                                                               ApplicationStatusTypeLookup.ApplicationStatusType.Incomplete) || appRow.ApplicationStatusType.IsDeadApplication()))
                {
                    // If recovering a userid for an inquiry or incomplete app,
                    // redirect back to the login page now with the recovered user id
                    // login page handles the dead application.
                    actionResult = View("Index", new SignInModel() { UserId = cuiRow.CustomerUserId, DisplayPasswordSSNPrompt = true });
                }
                else
                {
                    SecurityQuestionModel sqModel = new SecurityQuestionModel();
                    sqModel.SecurityQuestion = SecurityQuestionLookup.GetCaption(customerAccounts.CustomerUserIdSecurityQuestion.FirstOrDefault(csq => csq.UserId == model.SelectedUserId).SecurityQuestion);
                    actionResult = View("SecurityQuestion", sqModel);
                }
            }
            else
            {
                TempData["Alert"] = "Please select one of the accounts below.";
                SelectAccountModel selectAccountModel = SelectAccountModelBuilder.GetSelectAccountModelFromDataSet(customerAccounts);
                actionResult = View("SelectAccount", selectAccountModel);
            }

            return actionResult;
        }

        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View("ForgotPassword", new RecoverUserIDModel());
        }

        [HttpPost]
        public ActionResult ForgotPassword([Bind(Include = RecoverUserIDModel.BIND_FIELDS)]RecoverUserIDModel model)
        {
            SessionUtility.AccountInfo = null;
            GetAccountInfoResponse info = null;
            ActionResult actionResult = null;
            try
            {
                if (model.AccountNumberOrSSN.SelectedType == EnterAccountNumberOrSSNModel.RecoveryType.AccountNumber)
                {
                    info = DomainServiceCustomerOperations.GetAccountInfoByCuidAndApplicationId(model.UserID, model.AccountNumberOrSSN.AccountNumber);
                }
                else if (model.AccountNumberOrSSN.SelectedType == EnterAccountNumberOrSSNModel.RecoveryType.SocialSecurityNumber)
                {
                    info = DomainServiceCustomerOperations.GetAccountInfoByCuidAndSSN(model.UserID, model.AccountNumberOrSSN.SocialSecurityNumber);
                }

                if (info != null && info.CustomerUserIdDataSet != null)
                {
                    FailedLoginCounter.Reset();
                    CustomerUserIdDataSet customerAccounts = info.CustomerUserIdDataSet;

                    if (customerAccounts.CustomerUserId[0].IsTemporary
                        && customerAccounts.Application[0].ApplicationStatusType.IsOneOf(ApplicationStatusTypeLookup.ApplicationStatusType.Inquiry, ApplicationStatusTypeLookup.ApplicationStatusType.Incomplete))
                    {
                        actionResult = View("Index", new SignInModel() { UserId = customerAccounts.CustomerUserId[0].CustomerUserId, DisplayPasswordSSNPrompt = true });
                    }
                    else
                    {
                        SessionUtility.AccountInfo = info;
                        SessionUtility.AccountUserId = customerAccounts.CustomerUserId[0].UserId;
                        actionResult = View("SecurityQuestion", new SecurityQuestionModel()
                        {
                            PageHeader = "Change Password",
                            SecurityQuestion = SecurityQuestionLookup.GetCaption(customerAccounts.CustomerUserIdSecurityQuestion[0].SecurityQuestion),
                            ChangePasswordRequired = true
                        });
                    }
                }
                else
                {
                    actionResult = _handleInvalidSecurityInfo(ForgotCredentialType.ForgotPassword, "Change Password", model);
                }

            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && !(ex.InnerException is ArgumentException))
                {
                    throw;
                }
                else
                {
                    actionResult = _handleInvalidSecurityInfo(ForgotCredentialType.ForgotPassword, "Change Password", model);
                }
            }

            return actionResult;
        }

        public ActionResult Extend()
        {
            return new EmptyResult();
        }

        [HttpGet]
        public ActionResult ResetPassword()
        {
            return new HttpNotFoundResult();
        }

        [HttpPost]
        [InjectCustomerUserIdDataSet]
        public ActionResult ResetPassword(CustomerUserIdDataSet customerData, [Bind(Include=ResetPasswordModel.BIND_FIELDS)]ResetPasswordModel model)
        {
            ActionResult actionResult = null;
            if (customerData == null || SessionUtility.AccountUserId == null)
            {
                TempData["Alert"] = "Your request has expired. Please try again";
                return RedirectToAction("Index");
            }
            int selectedUserId = SessionUtility.AccountUserId.Value;
            string selectedCustomerUserId = customerData.CustomerUserId.First(cui => cui.UserId == selectedUserId).CustomerUserId;

            ChangePasswordResultEnum result = DomainServiceCustomerOperations.ChangePassword(selectedUserId, model.Password.Trim());
            switch (result)
            {
                case ChangePasswordResultEnum.Success:
                    {
                        actionResult = View("Index", new SignInModel() { UserId = selectedCustomerUserId });
                        EventAuditLogHelper.Submit(customerData, WebUser, EventTypeLookup.EventType.ForgotPassword, null);
                        break;
                    }
                case ChangePasswordResultEnum.FailsRegEx:
                case ChangePasswordResultEnum.UsesMnemonic:
                    {
                        actionResult = View("ResetPassword", new ResetPasswordModel() { CustomerUserID = selectedCustomerUserId });
                        break;
                    }
            }
            // Return to main log in page
            return actionResult;
        }

        private enum ForgotCredentialType
        {
            ForgotUserId = 1,
            ForgotPassword
        }

        private ViewResult _handleInvalidSecurityInfo(ForgotCredentialType forgotType, string invalidSecurityInfoPageTitle, object model)
        {
            ViewResult view = null;
            if (FailedLoginCounter.Increment() > 3)
            {
                view = View("InvalidSecurityInfo", new InvalidSecurityInfoModel() { PageTitle = invalidSecurityInfoPageTitle });
            }
            else
            {
                TempData["Alert"] = "An account could not be found for the social security number or reference number entered";

                switch (forgotType)
                {
                    case (ForgotCredentialType.ForgotUserId):
                        {
                            view = View("RecoverUserID", model);
                            break;
                        }
                    case (ForgotCredentialType.ForgotPassword):
                        {
                            view = View("ForgotPassword", model);
                            break;
                        }
                }
            }
            return view;
        }

        private void InitializeSessionObjects(SignInModel.LoginAccountStatusResult accountData)
        {
            if (accountData != null && accountData.CustomerData != null)
            {
                SessionUtility.AccountInfo = accountData.AccountInfo;
            }

            if (accountData != null && accountData.ApplicationId > 0)
            {
                WebUser.ApplicationId = accountData.ApplicationId;

                if (accountData != null && accountData.LoanOfferDataSet != null && accountData.CustomerData != null)
                {
                    SessionUtility.SetLoanOfferDataSet(accountData.LoanOfferDataSet);
                    SessionUtility.SetCurrentApplicationData(accountData.ApplicationId, DataSetToSessionStateMapper.Map(accountData.ApplicationId, accountData.CustomerData, accountData.LoanOfferDataSet));
                }
                else if (accountData != null && accountData.CustomerData != null)
                {
                    SessionUtility.SetCurrentApplicationData(accountData.ApplicationId, DataSetToSessionStateMapper.Map(accountData.ApplicationId, accountData.AccountInfo));
                }
            }
        }

        [HttpGet]
        public ActionResult LogOut(bool timeout = false)
        {
            SessionUtility.CleanUpSession();
            FormsAuthentication.SignOut();

            if (timeout)
                TempData["Timeout"] = TimeoutMessage;

            return View("Index", new SignInModel() { IsSignOut = true, isTimeout = timeout });
        }

        [HttpGet]
        [InjectCustomerUserIdDataSet]
        public ActionResult ConfirmEmailAddress(CustomerUserIdDataSet customerData)
        {
            var model = new ConfirmEmailAddressModel();
            model.Populate(customerData.ContactInfo);
            return View("ConfirmEmailAddress", model);
        }

        [HttpPost]
        [InjectCustomerUserIdDataSet]
        public ActionResult ConfirmEmailAddress(CustomerUserIdDataSet customerData, [Bind(Include= ConfirmEmailAddressModel.BIND_FIELDS)]ConfirmEmailAddressModel model)
        {
            if (ModelState.IsValid)
            {

                if (model.Submit(customerData, WebUser))
                {
                    return new RedirectResult("/appstatus/refresh");
                }
            }
            else
            {
                model.Populate(customerData.ContactInfo);
                model.ErrorMessage = string.Join("; ", ModelState.Values
                                        .SelectMany(x => x.Errors)
                                        .Select(x => x.ErrorMessage));
            }

            return View("ConfirmEmailAddress", model);
        }

        [InjectCustomerUserIdDataSet]
        public ActionResult AccountLock(CustomerUserIdDataSet customerData)
        {
            if (customerData == null)
            {
                return RedirectToAction("Index", "SignIn");
            }
            return View(new AccountLockPageModel());
        }

        [HttpPost]
        [InjectCustomerUserIdDataSet]
        public ActionResult AccountLock(CustomerUserIdDataSet customerData, AccountLockPageModel accountLockModel)
        {
            if (customerData == null)
            {
                return RedirectToAction("Index", "SignIn");
            }

            var result = DomainServiceCustomerOperations.SetPasscode(customerData.UserId, accountLockModel.EmailAddress);
            if (result)
            {
                return RedirectToAction("PassCodeLock", "SignIn");
            }
            TempData["Alert"] = Resources.FAMessages.EmailAddressNotFound;
            return View(accountLockModel);
        }

        [InjectCustomerUserIdDataSet]
        public ActionResult UpdateEmail(CustomerUserIdDataSet customerData)
        {
            if (customerData == null)
            {
                return RedirectToAction("Index", "SignIn");
            }
            return View(new UpdateEmailPageModel());
        }

        [HttpPost]
        [InjectCustomerUserIdDataSet]
        public ActionResult UpdateEmail(CustomerUserIdDataSet customerData, [Bind(Include=UpdateEmailPageModel.BIND_FIELDS)] UpdateEmailPageModel updateEmailModel)
        {
            if (customerData == null)
            {
                return RedirectToAction("Index", "SignIn");
            }

            var contactInfo = customerData.ContactInfo;
            contactInfo.ApplicantInfo.HomeEmailAddress.EmailAddress = updateEmailModel.NewEmailAddress;

            var populateEventAuditLogDataSet = EventAuditLogHelper.PopulateEventAuditLogDataSet(customerData, WebUser, EventTypeLookup.EventType.UpdateEmailAddress, null);
            var success = DomainServiceCustomerOperations.UpdateCustomerEmailAddress(contactInfo, updateEmailModel.SecurityAnswer, updateEmailModel.OldEmailAddress, populateEventAuditLogDataSet);

            if (success == false)
            {
                if (FailedLoginCounter.Increment() > 3)
                {
                    FailedLoginCounter.Reset();
                    TempData["Alert"] = string.Format("We are unable to find any accounts that match the information entered. Please contact Customer Service at {0} for assistance.", BusinessConstants.Instance.PhoneNumberMain);
                    return RedirectToAction("LoginFailed", "SignIn");
                }
                TempData["Alert"] = "We are unable to find any accounts that match the information entered. Please review and re-enter your information.";
            }
            else
            {
                SessionUtility.RefreshAccountInfo();
                FailedLoginCounter.Reset();

                DomainServiceCustomerOperations.SetPasscode(customerData.UserId, updateEmailModel.NewEmailAddress);
                return RedirectToAction("PassCodeLock", "SignIn");
            }
            return View(updateEmailModel);
        }

        [InjectCustomerUserIdDataSet]
        public ActionResult PassCodeLock(CustomerUserIdDataSet customerData)
        {
            if (customerData == null)
            {
                return RedirectToAction("Index", "SignIn");
            }
            return View(new PassCodeLockPageModel());
        }

        [HttpPost]
        [InjectCustomerUserIdDataSet]
        public ActionResult PassCodeLock(CustomerUserIdDataSet customerData, [Bind(Include= PassCodeLockPageModel.BIND_FIELDS)]PassCodeLockPageModel passCodeModel)
        {
            if (customerData == null)
            {
                return RedirectToAction("Index", "SignIn");
            }

            if (DomainServiceCustomerOperations.IsPasscodeValid(customerData.UserId, passCodeModel.PassCode))
            {
                FailedLoginCounter.Reset();
                var customerUserId = customerData.AccountInfo.CustomerUserId;
                FormsAuthentication.SetAuthCookie(customerUserId, false);
                SessionUtility.SetUpAccountServicesData(customerData.AccountInfo.CustomerUserId);
                SessionUtility.RefreshAccountInfo();
                return RedirectToAction("Index", "AccountServices");
            }

            if (FailedLoginCounter.Increment() >= 3)
            {
                TempData["Alert"] = string.Format("The passcode you have entered is invalid. Please contact Customer Service at {0} if you need further assistance. Thank you.", BusinessConstants.Instance.PhoneNumberMain);
                return RedirectToAction("LoginFailed", "SignIn");
            }
            return View(passCodeModel);
        }
    }
}