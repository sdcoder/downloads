using LightStreamWeb.Middleware.Services.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Middleware.Business.Http
{
    public class CookieBusiness
    {
        public string RootDomain { get; private set; }
        public ICookie Cookie { get; private set; }
        public ICookieService CookieService { get; private set; }

        public CookieBusiness(string rootDomain, ICookie cookie, ICookieService cookieService)
        {
            this.RootDomain = rootDomain;
            this.Cookie = cookie;
            this.CookieService = cookieService;
        }

        public void CreateCookie(Uri requestUri, string value)
        {
            this.Cookie.Domain = this.CookieService.GetDomain(this.RootDomain, requestUri);

            if (!this.CookieService.Exists(this.Cookie))
            {
                this.Cookie.Value = value;
                this.Cookie.Domain = this.CookieService.GetDomain(this.RootDomain, requestUri);

                this.CookieService.Create(this.Cookie);
            }
        }
    }
}