using System.Security.Principal;
using System.Threading;
using System.Web.Hosting;
using FirstAgain.Common.Logging;
using System;
using System.Text;
using System.Web;
using System.Web.Optimization;
using System.Web.Mvc;
using LightStreamWeb.App_Start;

namespace LightStreamWeb
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            LightStreamLogger.Title = "PublicSite";

            BundleConfig.RegisterBundles(BundleTable.Bundles);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(System.Web.Routing.RouteTable.Routes);
            ModelBinderConfig.RegisterModelBinders();
            AutoMapperConfig.Initialize();

            // supress anti-forgery checks when switching between anonymous and logged in users
            System.Web.Helpers.AntiForgeryConfig.SuppressIdentityHeuristicChecks = true;
            // supress x-frame-option-headers, it is added on case by base basis in custom XFrameOptions filter
            System.Web.Helpers.AntiForgeryConfig.SuppressXFrameOptionsHeader = true;

            // remove Mvc Version from response headers
            MvcHandler.DisableMvcResponseHeader = true;
        }

        void Application_BeginRequest(object sender, EventArgs e)
        {
            FirstAgain.Web.Cookie.CookieUtility.SetUniqueCookie(false);
            FirstAgain.Web.Cookie.CookieUtility.SetSplitTestCookie(false);
        }

        void Application_End(object sender, EventArgs e)
        {
            //  Code that runs on application shutdown
        }

        void Application_Error(object sender, EventArgs e)
        {
            bool logError = true;
            int? httpErrorCode = null;

            if (Request.Url.ToString().ToLower().Contains("localhost"))
            {
                HttpException httpEx = null;
                Exception ex = Server.GetLastError();
                if (ex is HttpException)
                {
                    httpEx = (HttpException)ex;
                }

                if (System.Diagnostics.Debugger.IsAttached)
                {
                    var stackTrace = ex.StackTrace;
                    System.Diagnostics.Debugger.Break();
                }
                return;
            }

            // Can not allow any exception to be thrown from here.
            try
            {
                Exception ex = Server.GetLastError();
                Response.TrySkipIisCustomErrors = true;

                if (ex is HttpException)
                {
                    HttpException httpEx = (HttpException)ex;
                    httpErrorCode = httpEx.GetHttpCode();

                    if (httpErrorCode == 404  //Page not found
                        || httpErrorCode == 400) // bad request
                    {
                        logError = false; //Don't log page not found errors
                    }
                }

                if (ex is ArgumentException && ex.Message.Contains("Invalid JSON primitive"))
                {
                    logError = false;
                    httpErrorCode = 400; // Bad request
                }

                if (ex is HttpRequestValidationException)
                {
                    LogWarning(ex);
                    logError = false;
                }

                if (ex != null && ex.InnerException is HttpException && ex.InnerException.Message == "Maximum request length exceeded.")
                {
                    LogWarning(ex.InnerException);
                    logError = false;
                }

                if (logError && ex != null)
                {
                    LogError(ex);
                }

                Server.ClearError();

                if (httpErrorCode == 400)
                {
                    Response.Status = "400 Bad Request";
                }
                else if (httpErrorCode == 404)
                {
                    LogGeneral(ex);
                    Response.Status = "404 Not Found";
                    Server.Transfer("~/Error/NotFound");
                }
                else
                {
                    Response.Status = "500 Internal Server Error";

                    //if the error was generated from the SignIn page, display the custom 
                    //message for that instead of the generic one.
                    if (Request.Path.Contains("LogIn.aspx"))
                    {
                        HttpContext.Current.Items["IsError"] = "true";
                        Server.Transfer("~/SignIn/SignInMaintenance.aspx");
                    }
                    else
                    {
                        if (!Request.Path.Contains("error/general"))
                        {
                            Server.Transfer("~/error/general");
                        }
                    }
                }

            }
            catch (Exception)
            {
                if (!Request.Path.Contains("error/general"))
                    Response.Redirect("~/error/general");
            }
        }

        private void LogError(Exception ex)
        {
            var user = GetCurrentUserName();
            if(user != null)
            {
                ex.Data["User ID"] = user;
            }
            ex.Data["IP Address"] = Request.Headers["X-Forwarded-For"] ?? Request.UserHostAddress;
            ex.Data["User Agent"] = Request.UserAgent;
            ex.Data["URL"] = Request.Url;
            LightStreamLogger.WriteError(ex);

        }

        private void LogWarning(Exception ex)
        {
            StringBuilder msg = new StringBuilder();
            msg.Append(ex.Message);
            msg.AppendLine(string.Format("IP Address: {0}", Request.Headers["X-Forwarded-For"] ?? Request.UserHostAddress));
            msg.AppendLine(string.Format("User Agent: {0}", Request.UserAgent));
            msg.AppendLine(string.Format("URL: {0}", Request.Url));
            LightStreamLogger.WriteWarning(msg.ToString());
        }

        private void LogGeneral(Exception ex)
        {
            StringBuilder msg = new StringBuilder();
            msg.Append(ex.Message);
            msg.AppendLine(string.Format("IP Address: {0}", Request.Headers["X-Forwarded-For"] ?? Request.UserHostAddress));
            msg.AppendLine(string.Format("User Agent: {0}", Request.UserAgent));
            msg.AppendLine(string.Format("URL: {0}", Request.Url));
            LightStreamLogger.WriteInfo(msg.ToString());
        }

        private static string GetCurrentUserName()
        {
            try
            {
                if (HostingEnvironment.IsHosted)
                {
                    HttpContext current = HttpContext.Current;
                    if (current != null)
                        return current.User.Identity.Name;
                }

                IPrincipal currentPrincipal = Thread.CurrentPrincipal;
                if (currentPrincipal == null || currentPrincipal.Identity == null)
                    return string.Empty;

                return currentPrincipal.Identity.Name;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}