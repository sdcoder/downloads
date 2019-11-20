using FirstAgain.Common.Logging;
using Microsoft.Owin;
using Owin;
using System;

[assembly: OwinStartup(typeof(LightStreamWeb.Startup))]
namespace LightStreamWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureStaticFiles(app);
            ConfigureCookies(app);
            ConfigureReverseProxy(app);
            ConfigureCacheBusting();
        }
    }
}