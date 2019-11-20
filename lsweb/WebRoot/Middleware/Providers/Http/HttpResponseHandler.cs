using LightStreamWeb.Middleware.Services.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace LightStreamWeb.Middleware.Providers.Http
{
    public class HttpResponseHandler : HttpClientHandler
    {
        private readonly IResponseHandlerOptions options;

        public HttpResponseHandler(IResponseHandlerOptions options)
        {
            this.options = options;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // grab the resource
            if (this.options.SendAsyncOverride == null)
                return await base.SendAsync(request, cancellationToken);
            else// only used for unit testing
                return await this.options.SendAsyncOverride.Invoke(this.options.ResponseOverride, cancellationToken);
        }
    }
}