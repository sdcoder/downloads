using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using FirstAgain.Common.Logging;
using ImpromptuInterface;
using LightStreamWeb.Middleware.Providers.Http;
using LightStreamWeb.Middleware.Services.Affiliates;
using LightStreamWeb.Middleware.Services.Http;
using Newtonsoft.Json;

namespace LightStreamWeb.Middleware.Business.Affiliates
{
    public class TuneBusiness : ITuneService
    {
        public ICookieService CookieService { get; set; }
        public IQueryStringParamService QueryStringParamService { get; set; }
        public ITuneHttpService HttpService { get; set; }

        public async Task SetTuneCookieAsync(TuneMiddlewareOptions options)
        {
            try
            {
                if (this.QueryStringParamService.Get(options.QueryStringParams.TransactionId) != null)
                {
                    await this.CreateTuneCookie(options.TuneCookieOptions, options.SessionApplyCookieOptions, options.QueryStringParams);
                }
                else if (this.CookieService.Exists(options.TuneCookieOptions))
                {
                    this.UpdateTuneCookie(options.TuneCookieOptions, options.SessionApplyCookieOptions, options.QueryStringParams);
                }
            }
            catch(Exception ex)
            {
                LightStreamLogger.WriteError(ex,
                    "{Application} {Status} {Url} {QueryStringParameters}",
                    "Tune",
                    "ERROR",
                    this.QueryStringParamService.Url,
                    this.QueryStringParamService.ParamValues
                );
            }
        }

        private async Task CreateTuneCookie(ICookie tuneCookie, ICookie sessionApplyCookie, ITuneQueryStringParams queryStringParams)
        {
            var affiliateId = this.QueryStringParamService.Get(queryStringParams.AffiliateId);

            var cookieValue = new
            {
                TTId = this.QueryStringParamService.Get(queryStringParams.TransactionId),
                TUrl = this.QueryStringParamService.Get(queryStringParams.Referer),
                Fact = this.QueryStringParamService.Get(queryStringParams.FactId),
                TAId = affiliateId,
                AffiliateName = (await this.HttpService.GetAffiliate(affiliateId))?.company,
                SessionApplyCookie = this.CookieService.Get(sessionApplyCookie)?.Value,
            }.ActLike<ITuneModel>();

            tuneCookie.Value = this.CookieService.Serialize(cookieValue);

            this.CookieService.Create(tuneCookie);

            LightStreamLogger.WriteInfo("{Application} {Status} {Url} {QueryStringParameters} {Cookie}",
                "Tune",
                "COOKIE CREATED",
                this.QueryStringParamService.Url,
                this.QueryStringParamService.ParamValues,
                JsonConvert.SerializeObject(cookieValue.UndoActLike())
            );
        }

        private void UpdateTuneCookie(ICookie tuneCookie, ICookie sessionApplyCookie, ITuneQueryStringParams queryStringParams)
        {
            var existingCookie = this.CookieService.Deserialize<ITuneModel>(this.CookieService.Get(tuneCookie)?.Value);
            var sessionApply = this.CookieService.Get(sessionApplyCookie);

            if (existingCookie.SessionApplyCookie != sessionApply.Value)
            {
                existingCookie.SessionApplyCookie = sessionApply.Value;

                var updatedCookieValue = this.CookieService.Serialize<ITuneModel>(existingCookie);
                var cookie = tuneCookie;

                cookie.Value = updatedCookieValue;

                this.CookieService.Update(cookie);

                LightStreamLogger.WriteInfo("{Application} {Status} {Url} {QueryStringParameters} {Cookie}",
                    "Tune",
                    "COOKIE UPDATED",
                    this.QueryStringParamService.Url,
                    this.QueryStringParamService.ParamValues,
                    JsonConvert.SerializeObject(cookie.UndoActLike())
                );
            }
        }
    }
}