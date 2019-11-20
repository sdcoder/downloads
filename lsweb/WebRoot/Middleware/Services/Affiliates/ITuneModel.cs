using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Middleware.Services.Affiliates
{
    public interface ITuneModel
    {
        string TTId { get; set; }
        string TUrl { get; set; }
        string Fact { get; set; }
        string TAId { get; set; }
        string AffiliateName { get; set; }
        string SessionApplyCookie { get; set; }
    }
}