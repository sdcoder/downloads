using FirstAgain.Common;
using FirstAgain.Common.Logging;
using FirstAgain.Web.Cookie;
using LightStreamWeb.Filters;
using LightStreamWeb.Models.SignIn;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LightStreamWeb.Controllers
{
    public class SuntrustSignInController : Controller
    {
        private const string TEST_COOKIE_NAME = "TestTeamMateCookie";

        //
        // GET: /SuntrustSignIn/
        public ActionResult Index()
        {
            // set a test cookie
            Response.Cookies.Set(new HttpCookie(TEST_COOKIE_NAME, "1"));

            return View(new SignInModel());
        }

        //
        // GET: /SuntrustSignIn/CookiesRequired
        public ActionResult CookiesRequired()
        {
            TempData["Alert"] = "For security purposes (yours and ours), you need to enable cookies in order to sign in.  Thank you.";
            return View("Index", new SignInModel());
        }

        //
        // POST: /SignIn/
        [HttpPost]
        [RequireCookie(TEST_COOKIE_NAME, "/SuntrustTeamMate/CookiesRequired")]
        public ActionResult Index([Bind(Include = "UserId, UserPassword")]SignInModel model)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.UserId))
                {
                    TempData["Alert"] = Resources.FAMessages.SignInUserId_Val;
                    return View(model);
                }
                if (string.IsNullOrEmpty(model.UserPassword))
                {
                    TempData["Alert"] = Resources.FAMessages.SignInPassword_Val;
                    return View(model);
                }

                string correctUserId = ConfigurationManager.AppSettings["SuntrustTeamMateUserId"];
                string correctPassword = ConfigurationManager.AppSettings["SuntrustTeamMatePassword"];
                Guard.AgainstNullOrEmpty(correctUserId, "SuntrustTeamMateUserId");
                Guard.AgainstNullOrEmpty(correctPassword, "SuntrustTeamMatePassword");

                if (model.UserId.Equals(correctUserId, StringComparison.InvariantCultureIgnoreCase) && model.UserPassword.Equals(correctPassword, StringComparison.InvariantCultureIgnoreCase))
                {
                    var url = GetRedirectUrl();
                    if (url == AllowedSuntrustUrls.NOT_FOUND)
                    {
                        TempData["Alert"] = "Redirect not found. Please enter the URL of the application in your browser's address bar above";
                        return View(model);
                    }
                    CookieUtility.SetCookie(RequireSuntrustTeamMateLoginAttribute.COOKIE_NAME, "1", TimeSpan.FromDays(30));

                    return Redirect(url.ToString() + "?z=" + DateTime.Now.Ticks.ToString());
                }
                else
                {
                    TempData["Alert"] = "Your user name or password is incorrect";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ExceptionUtility.AddObjectStateToExceptionData(ex, model);
                LightStreamLogger.WriteError(ex);
                throw ex;
            }
        }

        enum AllowedSuntrustUrls
        {
            mcs,
            branch,
            premierbanking,
            pwm,
            chanop,
            NOT_FOUND
        }

        private AllowedSuntrustUrls GetRedirectUrl()
        {
            string url = (string)Session[RequireSuntrustTeamMateLoginAttribute.REDIRECT_SESSION_KEY];
            if (string.IsNullOrEmpty(url))
            {
                if (Request.Cookies[RequireSuntrustTeamMateLoginAttribute.REDIRECT_SESSION_KEY] != null)
                {
                    url = Request.Cookies[RequireSuntrustTeamMateLoginAttribute.REDIRECT_SESSION_KEY].Value;
                }
            }

            AllowedSuntrustUrls result = AllowedSuntrustUrls.NOT_FOUND;
            if (!string.IsNullOrEmpty(url))
            {
                if (!Enum.TryParse<AllowedSuntrustUrls>(url.ToLower(), out result))
                {
                    if (!Enum.TryParse<AllowedSuntrustUrls>(url.Substring(1).ToLower(), out result))
                    {
                        LightStreamLogger.WriteWarning("Unknown URL supplied to SunTrust Sign In Page {URL}", url);
                    }
                }
            }

            return result;
        }
    }
}
