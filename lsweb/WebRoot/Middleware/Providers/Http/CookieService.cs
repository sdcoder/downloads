using ImpromptuInterface;
using LightStreamWeb.Middleware.Services.Http;
using Microsoft.Owin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Middleware.Providers.Http
{
    public class CookieService : ICookieService
    {
        public string[] AllResponseKeys => HttpContext.Current.Response.Cookies.AllKeys;

        public IEnumerable<ICookie> RequestCookies => (from i in HttpContext.Current.Request.Cookies.AllKeys
                                                       select Impromptu.ActLike<ICookie>(HttpContext.Current.Request.Cookies.Get(i))).ToArray();

        public IEnumerable<ICookie> ResponseCookies => (from i in HttpContext.Current.Response.Cookies.AllKeys
                                                        select Impromptu.ActLike<ICookie>(HttpContext.Current.Response.Cookies.Get(i))).ToArray();

        public void Create(ICookie cookie)
        {
            cookie.Value = HttpUtility.UrlEncode(cookie.Value);

            HttpContext.Current.Response.Cookies.Add(cookie.UndoActLike());
        }

        public T Deserialize<T>(string value)
        {
            return JsonConvert.DeserializeObject(HttpUtility.UrlDecode(value)).ActLike(typeof(T));
        }

        public string Serialize<T>(T value)
        {
            return JsonConvert.SerializeObject(value.UndoActLike());
        }

        public bool Exists(ICookie cookie)
        {
            var existingCookie = this.Get(cookie);

            return existingCookie != null;
        }

        public ICookie Get(ICookie cookie)
        {
            if (Array.IndexOf(this.AllResponseKeys, cookie.Name) >= 0)
            {
                return this.ResponseCookies.FirstOrDefault(i => i.Name.Equals(cookie.Name, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                return this.RequestCookies.FirstOrDefault(i => i.Name.Equals(cookie.Name, StringComparison.OrdinalIgnoreCase));
            }
        }

        public void Update(ICookie cookie)
        {
            cookie.Value = HttpUtility.UrlEncode(cookie.Value);

            HttpContext.Current.Response.Cookies.Add(cookie.UndoActLike());
        }

        public string GetDomain(string rootDomain, Uri uri)
        {
            //use a domain cookie for the root domain (lightstream.com or www.lightstream.com)
            if ((uri.Host.Equals(rootDomain, StringComparison.OrdinalIgnoreCase)) || 
                (uri.Host.Equals($"www.{rootDomain}", StringComparison.OrdinalIgnoreCase)))
            {
                return rootDomain;
            }
            else
            {
                return null;
            }
        }
    }
}