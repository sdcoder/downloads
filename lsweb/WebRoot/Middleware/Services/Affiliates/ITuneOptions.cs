using LightStreamWeb.Middleware.Services;
using LightStreamWeb.Middleware.Services.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Middleware.Services.Affiliates
{
    public interface TuneMiddlewareOptions
    {
        ITuneApiConfig ApiConfig { get; }
        ITuneQueryStringParams QueryStringParams  { get; }
        ICookie TuneCookieOptions { get; set; }
        ICookie SessionApplyCookieOptions { get; set; }
    }
}