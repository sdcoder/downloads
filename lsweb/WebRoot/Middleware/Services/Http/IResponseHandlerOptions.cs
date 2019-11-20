using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace LightStreamWeb.Middleware.Services.Http
{
    public interface IResponseHandlerOptions
    {
        Func<HttpResponseMessage, CancellationToken, Task<HttpResponseMessage>> SendAsyncOverride { get; set; }
        HttpResponseMessage ResponseOverride { get; set; }
    }
}