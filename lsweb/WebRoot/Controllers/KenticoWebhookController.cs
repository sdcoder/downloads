using FirstAgain.Domain.Lookups.FirstLook;
using HttpCache.Abstractions;
using LightStreamWeb.Caching;
using LightStreamWeb.Extensions;
using LightStreamWeb.Filters;
using LightStreamWeb.Models.Webhooks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json.Schema;
using FirstAgain.Common.Caching;
using FirstAgain.Domain.Common;
using FirstAgain.Common.Logging;

namespace LightStreamWeb.Controllers
{
    public class KenticoWebhookController : ApiController
    {
        private readonly string hmacKey;
        private readonly string kenticoUrl;

        private const string KenticoHmacHeader = "X-Kentico-Hmac";

        public KenticoWebhookController()
        {
            hmacKey = ConfigurationManager.AppSettings["Webhook:PrivateKey"];
            kenticoUrl = MachineCache.Get<BusinessConstants>("BusinessConstants").KenticoURL;
        }

        [HttpPost]
        [Route("api/webhooks/incoming/kentico")]
        public async Task<HttpResponseMessage> Post([FromBody] KenticoWebhookModel model)
        {
            try
            {
                LightStreamLogger.WriteInfo("{Application} {Status} {Event}  {PostData}", "Webhooks", "DETAILS", model.Event, JsonConvert.SerializeObject(model));

                if (!model.IsHmacValid(GetHmacFromHeader(), hmacKey))
                {
                    LightStreamLogger.WriteWarning("{Application} {Status} {Event}  {PostData}", "Webhooks", "UNAUTHORIZED", model.Event, JsonConvert.SerializeObject(model));
                    return await Task.FromResult<HttpResponseMessage>(Request.CreateResponse(HttpStatusCode.Unauthorized));
                }

                switch (model.Event)
                {
                    case "ItemPublished":
                        await ItemPublished(new KenticoItemModel
                        {
                            CacheKey = model.Payload.Url
                        });
                        break;
                    case "ItemDeleted":
                        await ItemDeleted(new KenticoItemModel
                        {
                            CacheKey = model.Payload.Url
                        });
                        break;
                    case "SiteDeleted":
                        await SiteDeleted();
                        break;
                    default:
                        break;
                }

                LightStreamLogger.WriteInfo("{Application} {Status} {Event}  {PostData}", "Webhooks", "SUCCESS", model.Event, JsonConvert.SerializeObject(model));
            }
            catch(Exception ex)
            {
                LightStreamLogger.WriteError(ex, "{Application} {Status} {Event}  {PostData}", "Webhooks", "EXCEPTION", model.Event, JsonConvert.SerializeObject(model));
            }

            return await Task.FromResult<HttpResponseMessage>(Request.CreateResponse(HttpStatusCode.OK));
        }

        private async Task ItemPublished(KenticoItemModel model)
        {
            var cacheKey = GetCacheKey(model.CacheKey);

            HttpCacheItem item = await ScaleOutHttpCache.Instance.GetAsync(cacheKey) as HttpCacheItem;

            if (item == null)
            {
                LightStreamLogger.WriteWarning("{Application} {Status} {PostData}", "Webhooks", "HTTP CACHE ITEM NOTFOUND", JsonConvert.SerializeObject(model));
                return;
            }
                

            item.Ttl = item.Ttl.AddMinutes(-1);

            await ScaleOutHttpCache.Instance.SaveAsync(cacheKey, item);

            LightStreamLogger.WriteInfo("{Application} {Status} {HttpCacheItem}", "Webhooks", "UPDATED HTTP CACHE ITEM", JsonConvert.SerializeObject(new
            {
                item.Ttl,
                item.Headers,
                item.ContentHeaders,
                item.CachedResponseMessage.StatusCode
            }));
        }

        private async Task ItemDeleted(KenticoItemModel model)
        {
            LightStreamLogger.WriteInfo("{Application} {Status} {PostData}", "Webhooks", "HTTP CACHE ITEM DELETED", JsonConvert.SerializeObject(model));
            await ScaleOutHttpCache.Instance.DeleteAsync(model.CacheKey);
        }

        private async Task SiteDeleted()
        {
            LightStreamLogger.WriteInfo("{Application} {Status}", "Webhooks", "HTTPCACHE DELETED");
            await ScaleOutHttpCache.Instance.DeleteAllAsync();
        }

        private string GetCacheKey(string urlPath)
        {
            string normalizedPath = string.Empty;

            if (String.IsNullOrEmpty(urlPath))
                return null;

            if (urlPath.First() != '/')
                return urlPath;

            normalizedPath = urlPath.Substring(1, urlPath.Length - 1);

            return $"GET-{kenticoUrl}{normalizedPath}";
        }
        
        private string GetHmacFromHeader()
        {
            if (!Request.Headers.Contains(KenticoHmacHeader))
                return string.Empty;

            return Request.Headers.GetValues(KenticoHmacHeader).First();
        }

        private bool VerifyHmac(string dataToHash, string hmacHeader)
        {
            var keyBytes = Encoding.UTF8.GetBytes(hmacKey);
            var dataBytes = Encoding.UTF8.GetBytes(dataToHash);

            var hmac = new HMACSHA256(keyBytes);
            var hmacBytes = hmac.ComputeHash(dataBytes);

            var createSignature = BitConverter.ToString(hmacBytes).Replace("-", string.Empty).ToLower(); //Convert.ToBase64String(hmacBytes);

            LightStreamLogger.WriteInfo("{Application} {Status} {HmacHeader} {PayloadHmac}", "Webhooks", "HMAC VERIFIED", hmacHeader, createSignature);

            return hmacHeader == createSignature;
        }
    }
}