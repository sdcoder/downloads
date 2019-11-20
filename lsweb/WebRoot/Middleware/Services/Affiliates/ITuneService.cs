using LightStreamWeb.Middleware.Services.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace LightStreamWeb.Middleware.Services.Affiliates
{
    public interface ITuneService
    {
        Task SetTuneCookieAsync(TuneMiddlewareOptions cookie);
    }
}