using LightStreamWeb.Middleware.Business.Affiliates;
using LightStreamWeb.Middleware.Providers.Affiliates;
using LightStreamWeb.Middleware.Providers.Http;
using LightStreamWeb.Middleware.Services.Affiliates;
using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace LightStreamWeb.Middleware
{ 
    public class TuneMiddleware
    {
        private AppFunc next;
        private readonly TuneMiddlewareOptions options;
        private readonly TuneBusiness business;

        public TuneMiddleware(AppFunc next, TuneMiddlewareOptions options)
        {
            this.next = next;
            this.options = options;
            this.business = new TuneBusiness
            {
                CookieService = new CookieService(),
                QueryStringParamService = new QueryStringParamService(),
                HttpService = new TuneHttpService(options.ApiConfig)
            };
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            await this.business.SetTuneCookieAsync(options);

            await next(environment);
        }
    }
}