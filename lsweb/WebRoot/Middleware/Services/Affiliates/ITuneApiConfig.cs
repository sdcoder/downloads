using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Middleware.Services.Affiliates
{
    public interface ITuneApiConfig
    {
        string ApiKey { get; }
        string BaseUrl { get; }
    }
}