using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Middleware.Services.Http
{
    public interface ICookieService
    {
        string[] AllResponseKeys { get; }
        IEnumerable<ICookie> RequestCookies { get; }
        IEnumerable<ICookie> ResponseCookies { get; }

        void Create(ICookie cookie);
        void Update(ICookie cookie);
        ICookie Get(ICookie options);
        bool Exists(ICookie options);
        T Deserialize<T>(string value);
        string Serialize<T>(T existingCookie);
        string GetDomain(string rootDomain, Uri uri);
    }
}