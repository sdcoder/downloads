using ImpromptuInterface;
using LightStreamWeb.Middleware.Business.Http;
using LightStreamWeb.Middleware.Providers.Http;
using LightStreamWeb.Middleware.Services;
using LightStreamWeb.Middleware.Services.Http;
using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace LightStreamWeb.Middleware
{
    public class SessionApplyMiddleware
    {
        private AppFunc next;
        private readonly CookieBusiness cookieBusiness;

        public SessionApplyMiddleware(AppFunc next, string rootDomain, ICookie cookie)
        {
            this.next = next;
            this.cookieBusiness = new CookieBusiness(rootDomain, cookie, new CookieService());
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var context = new OwinContext(environment);

            this.cookieBusiness.CreateCookie(context.Request.Uri, Guid.NewGuid().ToString());

            await next(environment);
        }
    }
}
