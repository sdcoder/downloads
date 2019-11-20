using FirstAgain.Common.Wcf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.App_State
{
    public class Endpoints : IEndpoints
    {
        public Endpoints()
        {
            Initialize();
        }

        public string CustomerDataVerificationUrl { get; set; }
        public string CacheServiceUrl { get; set; }

        private void Initialize()
        {
            CustomerDataVerificationUrl = WebServiceRegistrar.GetUrl("LightStream.CustomerDataVerification");
            CacheServiceUrl = WebServiceRegistrar.GetUrl("LightStream.Service.Cache");
        }
    }
}