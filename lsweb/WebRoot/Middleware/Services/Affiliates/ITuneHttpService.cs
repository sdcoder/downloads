﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace LightStreamWeb.Middleware.Services.Affiliates
{
    public interface ITuneHttpService
    {
        ITuneApiConfig ApiConfig { get; }

        Task<IAffiliate> GetAffiliate(string affiliateId);
    }
}