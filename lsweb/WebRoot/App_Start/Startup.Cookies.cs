using Owin;
using LightStreamWeb.Middleware;
using LightStreamWeb.Middleware.Services;
using FirstAgain.Web.Cookie;
using System.Configuration;
using Microsoft.Owin.Extensions;
using ImpromptuInterface;
using LightStreamWeb.Middleware.Services.Affiliates;
using LightStreamWeb.Middleware.Services.Http;
using System.Web;
using System;
using Microsoft.Owin;

namespace LightStreamWeb
{
    public partial class Startup
    {
        public void ConfigureCookies(IAppBuilder app)
        {
            app.Use<SessionApplyMiddleware>(
                ConfigurationManager.AppSettings["RootDomain"],
                new HttpCookie(CookieUtility.SESSION_APPLY_COOKIE_NAME)
                {
                    Path = "/",
                    HttpOnly = true,
                    Expires = DateTime.Now.Add(TimeSpan.FromDays(CookieUtility.NEVER_EXPIRE_DAYS))
                }.ActLike<ICookie>()
            );

            app.Use<TuneMiddleware>(
                new
                {
                    ApiConfig = new
                    {
                        ApiKey = ConfigurationManager.AppSettings[CookieUtility.TUNE_API_KEY_NAME],
                        BaseUrl = ConfigurationManager.AppSettings[CookieUtility.TUNE_API_URL],
                    }.ActLike<ITuneApiConfig>(),
                    QueryStringParams = new
                    {
                        TransactionId = "TTID",
                        AffiliateId = "TAID",
                        Referer = "TURL",
                        FactId = "Fact"
                    }.ActLike<ITuneQueryStringParams>(),
                    TuneCookieOptions = new HttpCookie(CookieUtility.TUNE_COOKIE_NAME)
                    {
                        Path = "/",
                        HttpOnly = true,
                        Expires = DateTime.Now.Add(TimeSpan.FromDays(CookieUtility.NEVER_EXPIRE_DAYS))
                    }.ActLike<ICookie>(),
                    SessionApplyCookieOptions = new HttpCookie(CookieUtility.SESSION_APPLY_COOKIE_NAME)
                    {
                        Path = "/",
                        HttpOnly = true,
                        Expires = DateTime.Now.Add(TimeSpan.FromDays(CookieUtility.NEVER_EXPIRE_DAYS))
                    }.ActLike<ICookie>()
                }.ActLike<TuneMiddlewareOptions>()
            );

            app.UseStageMarker(PipelineStage.MapHandler);
        }
    }
}