using Microsoft.Owin.Extensions;
using Owin;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.IO;
using Microsoft.Owin;
using System.Threading.Tasks;
using System.Net;

using FirstAgain.Common.Caching;
using FirstAgain.Common.Logging;
using FirstAgain.Domain.Common;
using FirstAgain.Domain.SharedTypes.ContentManagement;
using LightStreamWeb.Caching;
using HttpCache.Abstractions;
using SharpReverseProxy;

namespace LightStreamWeb
{
    public partial class Startup
    {
        private const string ReverseProxyAppName = "ReverseProxy";
        private const string HttpCacheAppName = "HttpCache";

        private string _kenticoUrl
        {
            get
            {
                return MachineCache.Get<BusinessConstants>("BusinessConstants").KenticoURL;
            }
        }

        private ReverseProxyUrl[] _reverseProxyUrls
        {
            get
            {
                return MachineCache.Get<ReverseProxyUrl[]>("ReverseProxyUrls");
            }
        }

        private string[] _bannedContentTypes
        {
            get
            {
                return (MachineCache.Get<BusinessConstants>("BusinessConstants").KenticoBannedContentTypes ?? String.Empty).Split(';');
            }
        }

        private string[] _bannedExtensions
        {
            get
            {
                return (MachineCache.Get<BusinessConstants>("BusinessConstants").KenticoBannedUrlExtensions ?? String.Empty).Split(';');
            }
        }

        public void ConfigureReverseProxy(IAppBuilder app)
        {
            app.UseProxy(new ProxyOptions
            {
                BackChannelMessageHandler = HttpMessageHandlerConfig(),
                GetProxyRules = () =>
                {
                    List<ProxyRule> proxyRules = new List<ProxyRule>();
                    string kenticoUrl = _kenticoUrl;

                    try
                    {
                        proxyRules.AddRange(_reverseProxyUrls.Where(i => i.IsProxied).Select(i => new ProxyRule
                        {
                            RequiresAuthentication = false,
                            BanRequest = (request, context) => ProxyBanRequest(request, context),
                            BanResponse = response => ProxyBanResponse(response),
                            Matcher = uri => ProxyUriMatcher(uri, i),
                            Modifier = (request, user) => ProxyRequestModifier(request, user, kenticoUrl),
                            ResponseModifier = ProxyResponseModifier
                        }));
                    }
                    catch (Exception ex)
                    {
                        LightStreamLogger.WriteError(ex, "{Application} Request failed using {KenticoUrl}", ReverseProxyAppName, kenticoUrl);
                    }

                    return proxyRules;
                },
                Reporter = proxyResult => ProxyReporter(proxyResult)
            });

            app.UseStageMarker(PipelineStage.MapHandler);
        }

        private HttpMessageHandler HttpMessageHandlerConfig()
        {
            return new HttpCacheResponseHandler(ScaleOutHttpCache.Instance, new ResponseHandlerOptions(HttpCacheReporter))
            {
                AllowAutoRedirect = true,
            };
        }

        private bool BypassProxyForLegacyCMS(Uri uri, ReverseProxyUrl proxyUrl)
        {
            var requestFromCMS = false; //set to false by default

            var validEnvironments = new[] { "www.lightstream.com:903", "stgweb.lightstream.com", "test2.lightstream.com", "test.lightstream.com", "tstdweb.lightstream.com" };
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var hasOldCMSParams = query.Get("revision") != null && query.Get("id") != null && query.Get("legacyCMS") != null; //if query contains both "id" & "revision"
            
            //If request is from a valid environment and contains cms parameters, allow old CMS version
            if ((validEnvironments.Any(uri.Authority.Contains) || uri.Host.Equals("localhost")) && hasOldCMSParams)
            {
                requestFromCMS = true;
            }
            return requestFromCMS;
        }

        private bool ProxyUriMatcher(Uri uri, ReverseProxyUrl proxyUrl)
        {
            var matchesUrl = false;

            // if legacy cms bypass
            if (BypassProxyForLegacyCMS(uri, proxyUrl))
            {
                matchesUrl = false;
            }
            // if regex is present try to match it and either match or bypass
            else if (String.IsNullOrEmpty(proxyUrl.UrlRegEx))
            {
                matchesUrl = uri.AbsolutePath.Equals($"/{proxyUrl.Url.ToLower()}");
            }
            // otherwise check against a literal string
            else
            {
                matchesUrl = Regex.IsMatch(uri.AbsolutePath, proxyUrl.UrlRegEx, RegexOptions.IgnoreCase);
            }

            return matchesUrl;
        }

        private void ProxyRequestModifier(HttpRequestMessage request, ClaimsPrincipal users, string kenticoUrl)
        {
            request.CreateResponse(HttpStatusCode.BadRequest);

            request.RequestUri = new Uri($"{kenticoUrl}{request.RequestUri.PathAndQuery.Remove(0, 1)}");
            request.Headers.Add("X-ProxyWebServer", Environment.MachineName);
        }

        private Task ProxyResponseModifier(HttpResponseMessage response, IOwinContext owinContext)
        {
            var isFailedResponse = false;
            var logMessage = new
            {
                Format = "{Application} Request failed {OriginalUrl} {ProxiedUrl} {ResponseStatusCode} {ResponseReasonPhrase}",
                Args = new object[]
                {
                    ReverseProxyAppName,
                    owinContext.Request.Uri.AbsoluteUri,
                    response.RequestMessage.RequestUri.AbsoluteUri,
                    owinContext.Response.StatusCode,
                    owinContext.Response.ReasonPhrase
                }
            };

            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    isFailedResponse = true;
                    LightStreamLogger.WriteWarning(logMessage.Format, logMessage.Args);
                    break;
                case HttpStatusCode.ServiceUnavailable:
                case HttpStatusCode.InternalServerError:
                case HttpStatusCode.BadRequest:
                case HttpStatusCode.BadGateway:
                case HttpStatusCode.Forbidden:
                    isFailedResponse = true;
                    LightStreamLogger.WriteError(logMessage.Format, logMessage.Args);
                    break;
                default:
                    break;
            }

            if(isFailedResponse)
            {
                owinContext.Response.StatusCode = 302;
                owinContext.Response.Headers.Set("Location", "/error/general");
            }

            return Task.CompletedTask;
        }

        private bool ProxyBanResponse(HttpResponseMessage response)
        {
            var isContentTypeMissing = response?.Content?.Headers?.ContentType?.MediaType == null;

            if (isContentTypeMissing)
                LightStreamLogger.WriteWarning("{Application} Response content type missing {Url}", "ReverseProxy",
                    ReverseProxyAppName, response.RequestMessage.RequestUri);

            if (!isContentTypeMissing &&
                _bannedContentTypes.Any(i => i.Equals(response.Content.Headers.ContentType.MediaType, StringComparison.OrdinalIgnoreCase)))
                return true;

            return false;
        }

        private bool ProxyBanRequest(HttpRequestMessage request, IOwinContext owinContext)
        {
            var urlExtension = Path.GetExtension(request.RequestUri.GetLeftPart(UriPartial.Path));

            if (_bannedExtensions.Any(i => $".{i}".Equals(urlExtension, StringComparison.OrdinalIgnoreCase)) ||
                (request.Method == HttpMethod.Post && _bannedContentTypes.Any(i => i.Equals(request.Content.Headers.ContentType.MediaType, StringComparison.OrdinalIgnoreCase))))
            {
                owinContext.Response.StatusCode = 302;
                owinContext.Response.Headers.Set("Location", "/error/general");
                return true;
            }

            return false;
        }

        private void ProxyReporter(ProxyResult proxyResult)
        {
            // used for KPI
            //LightStreamLogger.WriteInfo("{Application} {ProxyStatus} {Elapsed} {OriginalUrl} {NewUrl} {Status}", 
            //    ReverseProxyAppName, proxyResult.ProxyStatus, proxyResult.Elapsed, proxyResult.OriginalUri, proxyResult.ProxiedUri.AbsoluteUri, proxyResult.HttpStatusCode);

#if DEBUG_SINGLE_MACHINE || DEBUG_LOCALHOST
            System.Diagnostics.Debug.WriteLine($"{ReverseProxyAppName}: {proxyResult.ProxyStatus} Url: {proxyResult.OriginalUri} Time: {proxyResult.Elapsed}");

            if (proxyResult.ProxyStatus == ProxyStatus.Proxied)
            {
                System.Diagnostics.Debug.WriteLine($"        New Url: {proxyResult.ProxiedUri.AbsoluteUri} Status: {proxyResult.HttpStatusCode}");
            }
#endif

            if(proxyResult.ProxyStatus == ProxyStatus.InformationLogging)
            {
                LightStreamLogger.WriteInfo(proxyResult.Message, proxyResult.Args);
            }

        }

        private void HttpCacheReporter(ResponseHandlerResult status)
        {
            switch (status.ResponseHandlerStatus)
            {
                case ResponseHandlerStatus.UsingStaleCachedResponse:
                    LightStreamLogger.WriteWarning("{Application} {Status} {Url} {Ttl} {StatusCode} {Message}",
                        HttpCacheAppName,
                        status.ResponseHandlerStatus,
                        status.Uri,
                        status.Ttl,
                        status.StatusCode,
                        status.Message);
                    break;
                case ResponseHandlerStatus.FailedToSendRequest:
                case ResponseHandlerStatus.Error:
                    LightStreamLogger.WriteError(status.Exception, "{Application} {Status} {Url} {Message}",
                        HttpCacheAppName,
                        status.ResponseHandlerStatus,
                        status.Uri,
                        status.Message);
                    break;
                default:
#if DEBUG_SINGLE_MACHINE || DEBUG_LOCALHOST
                    System.Diagnostics.Debug.WriteLine($"{HttpCacheAppName}: {status.ResponseHandlerStatus} Url: {status.Uri} Ttl: {status.Ttl.ToLocalTime()}{Environment.NewLine}" +
                                    $"        StatusCode: {status.StatusCode} Message: {status.Message}");
#endif
                    break;
            }
        }
    }
}