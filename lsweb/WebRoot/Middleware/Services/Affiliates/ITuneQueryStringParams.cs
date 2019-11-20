using LightStreamWeb.Middleware.Services.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Middleware.Services.Affiliates
{
    public interface ITuneQueryStringParams
    {
        string TransactionId { get; }
        string AffiliateId { get; }
        string FactId { get; }
        string Referer { get; }
    }
}