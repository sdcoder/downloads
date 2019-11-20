using FirstAgain.Common.Logging;
using ImpromptuInterface;
using LightStreamWeb.Middleware.Providers.Http;
using LightStreamWeb.Middleware.Services.Affiliates;
using LightStreamWeb.Middleware.Services.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace LightStreamWeb.Middleware.Providers.Affiliates
{
    public class TuneHttpService : ITuneHttpService
    {
        private readonly HttpClient httpClient;
        public ITuneApiConfig ApiConfig { get; private set; }

        public TuneHttpService()
        {
            this.httpClient = new HttpClient();
        }

        public TuneHttpService(ITuneApiConfig apiConfig)
            : this()
        {
            this.ApiConfig = apiConfig;
        }

        /// <summary>
        /// Only used for unit testing
        /// </summary>
        /// <param name="httpResponseHandler"></param>
        public TuneHttpService(IResponseHandlerOptions httpResponseHandler)
        {
            this.httpClient = new HttpClient(new HttpResponseHandler(httpResponseHandler));
        }

        public async Task<IAffiliate> GetAffiliate(string affiliateId)
        {
            string url = null;

            try
            {
                url = $"{this.ApiConfig.BaseUrl}/Apiv3/json?NetworkToken={this.ApiConfig.ApiKey}&Target=Affiliate&Method=findById&id={affiliateId}";

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                using (var response = await httpClient.GetAsync(url))
                {
                    var content = await response.Content.ReadAsAsync<dynamic>();

                    if (content.response.status == 1 && content.response["data"] != null && content.response["data"]["Affiliate"] != null)
                    {
                        IAffiliate affiliate = Impromptu.ActLike(content.response.data.Affiliate);
                        return affiliate;
                    }
                    else
                    {
                            LightStreamLogger.WriteError("{Application} {Status} {AffiliateId} {Error} {Url}", "Tune", "GET AFFILIATE NAME ERROR", affiliateId, content.response.errorMessage.Value.ToString(), url);
                    }
                }
            }
            catch(Exception ex)
            {
                LightStreamLogger.WriteError(ex, "{Application} {Status} {AffiliateId} {Url}", "Tune", "GET AFFILIATE NAME HTTP ERROR", affiliateId, url);
            }

            return null;
        }
    }
}