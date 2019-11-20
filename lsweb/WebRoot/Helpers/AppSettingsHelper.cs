using System;
using System.Configuration;
using Ninject;
using LightStreamWeb.Models.Middleware;

namespace LightStreamWeb.Helpers
{
    public static class AppSettingsHelper
    {
         public static bool IsRatchMatchOfferEnabled()
        {
            return !string.IsNullOrEmpty(ConfigurationManager.AppSettings["EnableRateMatch"]) &&
                   ConfigurationManager.AppSettings["EnableRateMatch"].Equals("true", StringComparison.CurrentCultureIgnoreCase);
        }

        public static string GetCDNBaseUrl()
        {
            return App_Start.NinjectWebCommon.Bootstrapper.Kernel.Get<IAppSettings>().CdnBaseUrl;
        }
    }
}